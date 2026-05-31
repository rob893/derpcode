# Security Research — 2026-03-14

## Executive Summary
DerpCode has several good baseline controls, including global auth-by-default, CSRF protection on refresh, hashed refresh tokens, and network-disabled runner containers. The highest-risk gaps are concentrated in code execution and deployment: untrusted submissions run as root in Docker with a writable host mount, execution containers are missing process-safety limits, and the Azure VM template exposes a public host while still permitting password-based administration. I also found exploitable weaknesses around rate-limit bypass, verbose exception leakage, plaintext local secrets, and missing browser hardening headers.

## Previous Plan Status
No previous plan exists. This is the initial security audit.

## Findings

### 1. Untrusted code executes as root with a writable host bind mount
- **Status:** Fixed on 2026-05-31.
- **Description:** The code runner starts user submissions with `User = "root"` and bind-mounts the host temp directory into the container as read-write. This overrides the non-root `runner` user defined in the language images and materially increases the impact of any container breakout, permission abuse, or runner bug.
- **Location:** `DerpCode.API\Services\Core\CodeExecutionService.cs:106-120`
- **Impact:** Critical
- **Effort:** Medium
- **Dependencies:** Docker-based code execution for runs and submissions
- **Breaking Changes:** Yes
- **Recommendation:** Run the container as the image’s non-root user, mount only the minimum writable output path, and add hardening such as `ReadonlyRootfs`, `CapDrop = ALL`, `no-new-privileges`, and a restrictive seccomp/AppArmor profile.
- **Example Attack Scenario:** A malicious Python or Rust submission exploits a container or kernel vulnerability from a root context, then uses the writable mount to tamper with result files or pivot toward host compromise.
- **Resolution:**
    - Removed the `User = "root"` override in `CodeExecutionService.ExecuteInContainerAsync` so containers run as each image's default non-root `runner` user.
    - Added `CapDrop = ["ALL"]` and `SecurityOpt = ["no-new-privileges:true"]` to the `HostConfig`.
    - Added `IFileSystemService.SetUnixFileMode` and chmod the submission tempDir to `0777` after creation so the non-root container UID can still write `results.json`/`output.txt`/`error.txt` back to the host bind mount on Linux (no-op on Windows).
    - Added `RUN chown -R runner:runner /home/runner` to `Docker/CSharp/Dockerfile` (the `App/` dir is built as root by `dotnet new console`) and `Docker/TypeScript/Dockerfile` (esbuild needs to write outputs/caches alongside the root-installed `node_modules`).
    - `ReadonlyRootfs` was intentionally not enabled because the runner scripts write to `/home/runner/App`, `/home/runner/src`, `/home/runner/target`, `/home/runner/dist`, etc. Enabling it would require remounting `/home/runner` as tmpfs and is deferred as a follow-up.
    - The writable submission mount was kept because the runner uses it to deliver results back to the API. The trusted driver code (curated by us) is what writes `results.json`, so user-controlled tampering is bounded by the driver's correctness rather than the mount mode.

### 2. Execution containers have no PID/process limit, enabling fork-bomb denial of service
- **Status:** Fixed on 2026-05-31.
- **Description:** The runner constrains memory and CPU, but it does not set `PidsLimit` or equivalent process-count controls on the container host config. That leaves the platform vulnerable to submissions that rapidly spawn processes until the VM exhausts its PID table.
- **Location:** `DerpCode.API\Services\Core\CodeExecutionService.cs:106-113`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** Access to code execution endpoints
- **Breaking Changes:** No
- **Recommendation:** Set `HostConfig.PidsLimit`, add sane `ulimit` values, and consider per-language watchdogs that kill process trees rather than only the entrypoint.
- **Example Attack Scenario:** An attacker submits `while True: os.fork()` or the equivalent in another language; the container stays within memory limits long enough to starve the host of PIDs and destabilize the API, Docker daemon, and SSH access.
- **Resolution:**
    - Set `HostConfig.PidsLimit = 512` on the container host config — cgroups-enforced and scoped per-container.
    - Added `Ulimits` for `nofile` (1024 soft/hard) and `fsize` (64 MB soft/hard).
    - Deliberately did not set an `nproc` ulimit because `RLIMIT_NPROC` is enforced per-UID across the host, which could collide between concurrent containers running as the same UID; `PidsLimit` covers per-container process count more correctly.

### 3. The Rust runner can compile indefinitely because the build phase is not time-boxed
- **Status:** Fixed on 2026-05-31.
- **Description:** The API waits for container completion without applying its own timeout, and the Rust runner script wraps execution in `timeout 20s` but leaves `cargo build --release --quiet` unbounded. A deliberately pathological Rust submission can therefore occupy a worker far longer than intended.
- **Location:** `DerpCode.API\Services\Core\CodeExecutionService.cs:123-124`; `Docker\Rust\run.sh:12-17`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** Access to Rust problem execution
- **Breaking Changes:** No
- **Recommendation:** Apply an API-side timeout around container wait operations and wrap the Rust build step in its own timeout, with cleanup that force-removes the container when compilation exceeds budget.
- **Example Attack Scenario:** An attacker submits Rust code designed to trigger extreme compile-time expansion; each request pins a container and ties up an API execution slot until the host’s worker capacity is exhausted.
- **Resolution:**
    - Added a configurable outer 60s timeout around `WaitContainerAsync` via a linked `CancellationTokenSource`. The timeout is injectable through a new `CodeExecutionService` constructor for testing.
    - On timeout, the service best-effort calls `StopContainerAsync` and `RemoveContainerAsync(Force = true)` using a fresh `CancellationTokenSource(10s)` (a rubber-duck pass caught that reusing the canceled wait token would skip cleanup), then throws `TimeoutException` whose message surfaces to the user.
    - Caller cancellation also triggers the same best-effort cleanup before rethrowing `OperationCanceledException`.
    - The Rust `cargo build` was already wrapped with `timeout 15s` in `Docker/Rust/run.sh` at the time of the fix. The previously-unbounded `esbuild` step in `Docker/TypeScript/run.sh` was also wrapped with `timeout 15s` for the same defense-in-depth.

### 4. Rate limiting trusts spoofable `X-Forwarded-For` values
- **Status:** Fixed on 2026-05-31.
- **Description:** The rate-limiter partition key prefers the raw `X-Forwarded-For` header over `RemoteIpAddress`, and the application also enables forwarded-header handling without constraining trusted proxies in code. An attacker can therefore rotate fake source IPs in headers to evade per-IP throttles on login, registration, and other sensitive routes.
- **Location:** `DerpCode.API\ApplicationStartup\ServiceCollectionExtensions\RateLimiterServiceCollectionExtensions.cs:122-130`; `DerpCode.API\Program.cs:139-142`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** Ability to reach the API directly or through a proxy that forwards user-supplied headers
- **Breaking Changes:** No
- **Recommendation:** Derive the partition key from `RemoteIpAddress` after properly configuring `KnownProxies` and `KnownNetworks`, and ignore raw forwarding headers from untrusted clients.
- **Example Attack Scenario:** A bot trying to brute-force accounts sends `X-Forwarded-For: 1.1.1.1`, then `2.2.2.2`, then `3.3.3.3`, effectively resetting the rate-limit bucket on each request.
- **Resolution:**
    - Rewrote `RateLimiterServiceCollectionExtensions.GetIpAddress` to read **only** `Connection.RemoteIpAddress` and added a code comment explaining why raw forwarding headers are intentionally never consulted.
    - Made `GetIpAddress` and `GetPartitionKey` `internal` and added `InternalsVisibleTo("DerpCode.API.Tests")` so the trust boundary can be regression-tested.
    - Replaced the implicit `ForwardedHeadersOptions` block in `Program.cs` with an explicit `BuildForwardedHeadersOptions` helper. The framework's default trust list is cleared and rebuilt from code as loopback only (`127.0.0.0/8`, `::1/128`), with `ForwardLimit = 1`. Additional trusted proxies/networks can be added via the new `ForwardedHeaders:KnownProxies` and `ForwardedHeaders:KnownNetworks` configuration keys without redeploying the binary (`Models/Settings/ForwardedHeadersSettings.cs`, `Constants/ConfigurationKeys.cs`).
    - Migrated to the .NET 10 `KnownIPNetworks` / `System.Net.IPNetwork` APIs since `KnownNetworks` and `Microsoft.AspNetCore.HttpOverrides.IPNetwork` are deprecated.
    - Added `DerpCode.API.Tests/ApplicationStartup/ServiceCollectionExtensions/RateLimiterServiceCollectionExtensionsTests.cs` with coverage for: spoofed `X-Forwarded-For` being ignored, IPv4/IPv6 `RemoteIpAddress`, authenticated user winning over IP, anonymous fallback, null context, and unnamed authenticated identities falling back to IP.

### 5. Production error responses expose raw exception messages and inner-exception chains
- **Description:** The global exception handler serializes thrown exceptions into `ProblemDetailsWithErrors`, and that class copies the top-level message plus every inner exception message into the client-visible response. This leaks internal implementation details such as database errors, schema names, dependency failures, and troubleshooting clues that should remain server-side.
- **Location:** `DerpCode.API\Middleware\GlobalExceptionHandlerMiddleware.cs:31-52`; `DerpCode.API\Core\ProblemDetailsWithErrors.cs:58-81`; `DerpCode.API\Core\ProblemDetailsWithErrors.cs:105-111`
- **Impact:** Medium
- **Effort:** Low
- **Dependencies:** Any request path that can trigger an unhandled exception
- **Breaking Changes:** No
- **Recommendation:** Return a generic 500/504 message to clients, keep full exception details in structured logs only, and use the correlation ID as the support/debugging handle.
- **Example Attack Scenario:** An attacker intentionally sends malformed data until Entity Framework or PostgreSQL throws, then harvests the returned error chain to map table names, constraints, and internal service behavior.

### 6. The Azure VM template exposes a public host while still permitting password-based administration
- **Status:** Fixed on 2026-05-31.
- **Description:** The infrastructure template provisions a public IP, accepts an administrator password, and explicitly leaves password authentication enabled on the Linux VM. The same template does not define a network security group, and cloud-init adds the admin user to the Docker group, turning a successful login into near-root host control.
- **Location:** `CI\Azure\infrastructure.bicep:39-47`; `CI\Azure\infrastructure.bicep:71-85`; `CI\Azure\cloud-init.sh:14-23`
- **Impact:** Critical
- **Effort:** Medium
- **Dependencies:** Deployment of infrastructure from the checked-in Bicep/cloud-init assets
- **Breaking Changes:** Yes
- **Recommendation:** Remove password-based login, require SSH keys or Bastion, attach an NSG that only exposes necessary ports, and avoid granting Docker-group membership to the primary admin account.
- **Example Attack Scenario:** An attacker targets the public VM with password spraying; once inside, Docker-group membership lets them start privileged containers, mount the host filesystem, and take over the server.
- **Resolution:**
    - Removed the `@secure() param adminPassword` and the `disablePasswordAuthentication: false` block from `CI/Azure/infrastructure.bicep`. Added a required `adminSshPublicKey` parameter and a `linuxConfiguration.ssh.publicKeys` entry. `disablePasswordAuthentication` is now `true`.
    - Added a new `Microsoft.Network/networkSecurityGroups` resource with four rules: SSH inbound restricted to a required `sshSourceAddressPrefix` operator CIDR, HTTP/HTTPS inbound bound to a configurable `httpSourceAddressPrefix` (defaults to `Internet`), and an explicit catch-all deny so loose rules cannot silently expose ports. The NSG is wired into the subnet so it applies to all NICs in the default subnet.
    - Removed the broken `usermod -aG docker ${adminUsername}` line from `CI/Azure/cloud-init.sh` (the `${adminUsername}` substitution never happened because `customData` is loaded verbatim, so it would have failed at runtime — and even when working, it would have granted the interactive admin Docker-group membership which is effectively root). Replaced it with a dedicated `derpcode` system user (`useradd --system --create-home --shell /bin/bash`) that owns Docker group membership, leaving the admin user to use `sudo` for ad-hoc privileged operations.
    - Verified the resulting Bicep compiles with `az bicep build`. No live deployment currently uses this template, so the breaking parameter changes do not affect any running environment.

### 7. Sensitive local credentials are stored in plaintext next to the source tree and read by test code
- **Description:** `appsettings.Local.json` contains plaintext development secrets including database credentials, OAuth client secrets, a JWT signing key, a GitHub PAT, and test-user credentials. Although the file is gitignored, the UI E2E helper reads it directly from the repository root, which keeps live-looking secrets in a predictable location for malware, accidental commits, or overly broad file sharing.
- **Location:** `DerpCode.API\appsettings.Local.json:7-40`; `.gitignore:308-310`; `derpcode-ui\e2e\utils\credentials.ts:15-31`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** Workstation compromise, repository archive leakage, or accidental source-control exposure
- **Breaking Changes:** No
- **Recommendation:** Move local secrets into a dedicated secrets store or developer-specific environment mechanism, rotate the currently exposed values, and add automated secret scanning or pre-commit protection.
- **Example Attack Scenario:** Malware on a developer laptop or a mistakenly shared workspace archive yields the file, giving an attacker reusable database, OAuth, GitHub, and application credentials in one place.

### 8. The application does not set modern anti-clickjacking or anti-injection response headers
- **Description:** The HTTP pipeline enables HSTS but does not add headers such as `Content-Security-Policy`, `X-Frame-Options` or `frame-ancestors`, `X-Content-Type-Options`, `Referrer-Policy`, or `Permissions-Policy`. That leaves the SPA and API responses without several standard browser-enforced mitigations.
- **Location:** `DerpCode.API\Program.cs:134-148`; `derpcode-ui\index.html:1-27`
- **Impact:** Low
- **Effort:** Medium
- **Dependencies:** Browser-based access to the UI or API
- **Breaking Changes:** Yes
- **Recommendation:** Add an explicit security-header middleware at the API edge and define a restrictive CSP for the SPA, validating any third-party script or style requirements before rollout.
- **Example Attack Scenario:** A phishing site frames the DerpCode UI in an invisible iframe to trick users into interacting with real controls, or a future third-party script compromise executes more freely because there is no CSP to contain it.
