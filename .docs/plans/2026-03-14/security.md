# Security Research — 2026-03-14

## Executive Summary
DerpCode has several good baseline controls, including global auth-by-default, CSRF protection on refresh, hashed refresh tokens, and network-disabled runner containers. The highest-risk gaps are concentrated in code execution and deployment: untrusted submissions run as root in Docker with a writable host mount, execution containers are missing process-safety limits, and the Azure VM template exposes a public host while still permitting password-based administration. I also found exploitable weaknesses around rate-limit bypass, verbose exception leakage, plaintext local secrets, and missing browser hardening headers.

## Previous Plan Status
No previous plan exists. This is the initial security audit.

## Findings

### 1. Untrusted code executes as root with a writable host bind mount
- **Description:** The code runner starts user submissions with `User = "root"` and bind-mounts the host temp directory into the container as read-write. This overrides the non-root `runner` user defined in the language images and materially increases the impact of any container breakout, permission abuse, or runner bug.
- **Location:** `DerpCode.API\Services\Core\CodeExecutionService.cs:106-120`
- **Impact:** Critical
- **Effort:** Medium
- **Dependencies:** Docker-based code execution for runs and submissions
- **Breaking Changes:** Yes
- **Recommendation:** Run the container as the image’s non-root user, mount only the minimum writable output path, and add hardening such as `ReadonlyRootfs`, `CapDrop = ALL`, `no-new-privileges`, and a restrictive seccomp/AppArmor profile.
- **Example Attack Scenario:** A malicious Python or Rust submission exploits a container or kernel vulnerability from a root context, then uses the writable mount to tamper with result files or pivot toward host compromise.

### 2. Execution containers have no PID/process limit, enabling fork-bomb denial of service
- **Description:** The runner constrains memory and CPU, but it does not set `PidsLimit` or equivalent process-count controls on the container host config. That leaves the platform vulnerable to submissions that rapidly spawn processes until the VM exhausts its PID table.
- **Location:** `DerpCode.API\Services\Core\CodeExecutionService.cs:106-113`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** Access to code execution endpoints
- **Breaking Changes:** No
- **Recommendation:** Set `HostConfig.PidsLimit`, add sane `ulimit` values, and consider per-language watchdogs that kill process trees rather than only the entrypoint.
- **Example Attack Scenario:** An attacker submits `while True: os.fork()` or the equivalent in another language; the container stays within memory limits long enough to starve the host of PIDs and destabilize the API, Docker daemon, and SSH access.

### 3. The Rust runner can compile indefinitely because the build phase is not time-boxed
- **Description:** The API waits for container completion without applying its own timeout, and the Rust runner script wraps execution in `timeout 20s` but leaves `cargo build --release --quiet` unbounded. A deliberately pathological Rust submission can therefore occupy a worker far longer than intended.
- **Location:** `DerpCode.API\Services\Core\CodeExecutionService.cs:123-124`; `Docker\Rust\run.sh:12-17`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** Access to Rust problem execution
- **Breaking Changes:** No
- **Recommendation:** Apply an API-side timeout around container wait operations and wrap the Rust build step in its own timeout, with cleanup that force-removes the container when compilation exceeds budget.
- **Example Attack Scenario:** An attacker submits Rust code designed to trigger extreme compile-time expansion; each request pins a container and ties up an API execution slot until the host’s worker capacity is exhausted.

### 4. Rate limiting trusts spoofable `X-Forwarded-For` values
- **Description:** The rate-limiter partition key prefers the raw `X-Forwarded-For` header over `RemoteIpAddress`, and the application also enables forwarded-header handling without constraining trusted proxies in code. An attacker can therefore rotate fake source IPs in headers to evade per-IP throttles on login, registration, and other sensitive routes.
- **Location:** `DerpCode.API\ApplicationStartup\ServiceCollectionExtensions\RateLimiterServiceCollectionExtensions.cs:122-130`; `DerpCode.API\Program.cs:139-142`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** Ability to reach the API directly or through a proxy that forwards user-supplied headers
- **Breaking Changes:** No
- **Recommendation:** Derive the partition key from `RemoteIpAddress` after properly configuring `KnownProxies` and `KnownNetworks`, and ignore raw forwarding headers from untrusted clients.
- **Example Attack Scenario:** A bot trying to brute-force accounts sends `X-Forwarded-For: 1.1.1.1`, then `2.2.2.2`, then `3.3.3.3`, effectively resetting the rate-limit bucket on each request.

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
- **Description:** The infrastructure template provisions a public IP, accepts an administrator password, and explicitly leaves password authentication enabled on the Linux VM. The same template does not define a network security group, and cloud-init adds the admin user to the Docker group, turning a successful login into near-root host control.
- **Location:** `CI\Azure\infrastructure.bicep:39-47`; `CI\Azure\infrastructure.bicep:71-85`; `CI\Azure\cloud-init.sh:14-23`
- **Impact:** Critical
- **Effort:** Medium
- **Dependencies:** Deployment of infrastructure from the checked-in Bicep/cloud-init assets
- **Breaking Changes:** Yes
- **Recommendation:** Remove password-based login, require SSH keys or Bastion, attach an NSG that only exposes necessary ports, and avoid granting Docker-group membership to the primary admin account.
- **Example Attack Scenario:** An attacker targets the public VM with password spraying; once inside, Docker-group membership lets them start privileged containers, mount the host filesystem, and take over the server.

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
