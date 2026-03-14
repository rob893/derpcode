# UX Research — 2026-03-14

## Executive Summary

DerpCode's frontend is well-structured with consistent HeroUI component adoption and strong dark-mode theming, but suffers from several critical UX issues: raw HTML `<button>` elements bypass HeroUI styling and accessibility in the header and account navigation; hardcoded colors (scrollbar grays, `text-white`, `text-gray-*`) break the design-token contract and will fail under future theme changes; light-mode-specific semantic color shades (`bg-warning-50`, `text-danger-700`) render with poor or invisible contrast on the dark `#0a0a0a` background, making the WarningBanner and all alert callouts nearly unreadable. The XP/gamification feedback loop—the platform's signature hook—is text-only and buried, missing the celebratory punch that drives engagement. Mobile navigation is absent entirely, with nav links hidden on small screens and no hamburger menu provided.

## Previous Plan Status

No prior UX plan exists in `.docs/plans/`. This is the **initial UX audit**. An XP progression system plan (`xp-system.md`) exists but is backend-focused with no UX/UI recommendations.

---

## Findings

### 1. Light-Mode Semantic Color Shades Render Poorly in Dark Mode
- **Category:** Consistency / Dark Mode Compliance
- **Description:** Across the app, numerous alert/warning/danger callout boxes use HeroUI's numbered semantic color shades designed for light backgrounds (e.g., `bg-warning-50`, `text-warning-700`, `text-warning-800`, `bg-danger-50`, `text-danger-700`, `bg-success-50`, `text-success-700`). On the dark `#0a0a0a` background, these `-50` backgrounds are near-white and the `-700`/`-800` text is dark — creating unreadable, eye-searing light rectangles floating on a dark page. This affects the WarningBanner (always visible!), all account section callouts, the SyncModal success/error states, and the ProblemList tags error banner. The `ApiErrorDisplay` component already uses the correct dark-mode-friendly pattern (`bg-danger/10`, `border-danger/20`), proving the correct approach is known.
- **Location:**
  - `src/components/WarningBanner.tsx` — lines 5, 10 (`bg-warning-50`, `text-warning-800`)
  - `src/components/AccountSection.tsx` — lines 322, 325-326, 487, 490, 542, 544-545, 577, 579, 684, 686, 689, 698, 700, 703, 711, 713, 717 (all `bg-warning-50`, `bg-danger-50`, `text-warning-700/800`, `text-danger-700`)
  - `src/components/SyncModal.tsx` — lines 48-49, 58-59 (`bg-success-50`, `bg-danger-50`, `text-success-700`, `text-danger-700`)
  - `src/components/ProblemList.tsx` — lines 282-283 (`bg-warning-50`, `text-warning-800`)
  - `src/components/ProblemView/TestCaseDetails.tsx` — line 30 (`border-success-200`, `border-danger-200`)
  - `src/components/ProblemView/ProblemDescription.tsx` — line 223 (`text-warning-700 dark:text-warning-300` — partial fix)
- **Impact:** Critical
- **Effort:** Medium (1-4hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Replace all light-mode numbered shades with opacity-based token patterns. `bg-warning-50` → `bg-warning/10`; `text-warning-800` → `text-warning`; `border-warning-200` → `border-warning/20`. This pattern is already used correctly in `ApiErrorDisplay` and hint display. Adopt it universally.

### 2. Mobile Navigation Missing — Nav Links Hidden With No Alternative
- **Category:** Layout / Responsive Design
- **Description:** The AppHeader hides the main navigation links ("Problems", "Create Problem", "Sync Problems") behind `hidden sm:flex` at mobile widths. There is no hamburger menu, drawer, or any alternative. Mobile users can only navigate via the avatar dropdown (which only has "Account" and "Sign Out") or by entering URLs directly. The "Welcome, username" and level chip are also hidden below `lg` (1024px) with `hidden lg:flex`.
- **Location:**
  - `src/components/AppHeader.tsx` — line 57 (`hidden sm:flex` for nav links)
  - `src/components/AppHeader.tsx` — line 82 (`hidden sm:block` for divider)
  - `src/components/AppHeader.tsx` — line 85 (`hidden lg:flex` for welcome text + level)
- **Impact:** Critical
- **Effort:** Medium (1-4hr) — HeroUI Navbar has built-in mobile menu support via `NavbarMenuToggle` + `NavbarMenu`
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Implement HeroUI's `NavbarMenuToggle` and `NavbarMenu` components for a collapsible mobile menu. Include all nav links, user greeting, and admin actions. Alternatively, add these links into the existing Avatar dropdown for authenticated users.

### 3. Raw HTML `<button>` Elements Bypass HeroUI and Accessibility
- **Category:** Component / Accessibility
- **Description:** Six instances of raw HTML `<button>` elements are used instead of HeroUI `<Button>`. These lack HeroUI's built-in focus ring, ripple effect, keyboard handling, and consistent styling. The AppHeader nav links, AccountPage sidebar tabs, and the LandingPage footer privacy link all use unstyled native buttons.
- **Location:**
  - `src/components/AppHeader.tsx` — lines 58, 66, 72 (nav links)
  - `src/pages/AccountPage.tsx` — lines 35, 45 (sidebar nav buttons)
  - `src/pages/LandingPage.tsx` — line 118 (footer privacy link)
- **Impact:** High
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Replace AppHeader buttons with HeroUI `<Button variant="light">` or proper `NavbarItem` components. Replace AccountPage sidebar with HeroUI `<Tabs variant="light" isVertical>` or `<Listbox>`. Replace footer link with React Router `<Link>`.

### 4. No Visible Focus Indicators on Problem List Cards
- **Category:** Accessibility
- **Description:** Problem list cards use `role="button"` and `tabIndex={0}` with keyboard handlers, which is good. However, the wrapper `<div>` has `className="outline-none"`, which explicitly removes the browser's default focus outline with no replacement, making it impossible for keyboard users to see which card is focused.
- **Location:**
  - `src/components/ProblemList.tsx` — line 624 (`className="outline-none"`)
- **Impact:** High
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Replace `outline-none` with `outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 focus-visible:ring-offset-background rounded-lg`.

### 5. No Toast/Notification System for Mutation Feedback
- **Category:** Flow / Component
- **Description:** Multiple mutation operations (clone, toggle published, favorite/unfavorite, delete problem, update password, update username) log errors to `console.error` with comments like `"Error handling could be enhanced with toast notifications"`. There is no visual feedback for success or failure. Users have no way to know if an action succeeded.
- **Location:**
  - `src/components/ProblemView/ProblemView.tsx` — lines 183, 199-200, 209-210 (comments noting missing toasts)
  - `src/components/ArticleComments/ArticleComments.tsx` — line 61 (`console.error` only)
  - `src/components/ArticleComments/CommentItem.tsx` — line 99 (`console.error` only)
- **Impact:** High
- **Effort:** Medium (1-4hr)
- **Dependencies:** Choose a toast library (e.g., `react-hot-toast`, `sonner`)
- **Breaking Changes:** No
- **Recommendation:** Install a toast notification library and create a centralized `useToast` hook. Add toasts for all mutation success/failure cases. Style to match dark theme with green (success) and red (error) accents.

### 6. Problem List Filter Bar Not Responsive
- **Category:** Layout / Responsive Design
- **Description:** The ProblemList action bar uses `flex justify-between items-center gap-4` with inline elements (Random Problem button, `w-80` fixed-width search, page size selector, sort selector, filters button) that overflow on screens below ~1200px. The `w-80` search bar doesn't shrink on narrow viewports.
- **Location:**
  - `src/components/ProblemList.tsx` — lines 288-376 (action bar)
  - `src/components/ProblemList.tsx` — line 303 (`w-80` fixed width search)
- **Impact:** High
- **Effort:** Medium (1-4hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Change `w-80` to `w-full md:w-80`. Stack the action bar vertically on small screens with `flex-col sm:flex-row`. Consider collapsing sort/page-size selectors into the filter panel on mobile.

### 7. XP Feedback After Submission Is Underwhelming
- **Category:** Gamification / Flow
- **Description:** The core gamification loop — submit → result → XP gain — is the platform's key differentiator, yet XP feedback is a single-line paragraph: `"+{xp} XP gained. Total XP: {total}. Level {level} ({current}/{needed} to next level)."` No animation, no level-up celebration, no progress bar. The "no XP increase" message is equally flat. The level chip in the header is only visible above 1024px.
- **Location:**
  - `src/components/ProblemView/ProblemSubmissionResult.tsx` — lines 51-66 (XP feedback area)
  - `src/components/AppHeader.tsx` — lines 88-91 (level chip, `hidden lg:flex`)
- **Impact:** High
- **Effort:** High (4-8hr)
- **Dependencies:** XP system backend (already implemented per `xp-system.md`)
- **Breaking Changes:** No
- **Recommendation:** Create an `<XPGainAnimation>` component with: (1) prominent "+{xp} XP" badge with scale-in animation; (2) progress bar showing level progress before → after; (3) special celebration (confetti, snarky congratulations) on level-up; (4) always-visible level/XP in the header across all breakpoints.

### 8. Empty States Lack Brand Personality
- **Category:** Brand / Empty States
- **Description:** Most empty states use generic, corporate-sounding copy: "No Explanation Available", "Coming Soon", "No submissions yet", "No comments yet", "No problems found". Compare with the excellent snarky tone in the login modal ("You thought I'd let you use my compute resources without signing in?") and landing page copy. The brand voice is snarky in a few spots, sterile everywhere else.
- **Location:**
  - `src/components/ProblemView/ProblemDescription.tsx` — lines 240-244, 260-265, 279-285
  - `src/components/ProblemView/ProblemSubmissions.tsx` — lines 159-165
  - `src/components/ArticleComments/ArticleComments.tsx` — lines 144-149
  - `src/components/ProblemList.tsx` — lines 597-609
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Rewrite with DerpCode's snarky voice. E.g. "No submissions yet" → "Your submission history is emptier than your GitHub contribution graph. Time to fix that." / "Coming Soon" → "Solutions section is under construction. Unlike your code, it'll actually work when it's done."

### 9. Logo/Brand Click Uses Non-Interactive `<div>`
- **Category:** Accessibility
- **Description:** The DerpCode logo in the AppHeader is a `<div>` with `onClick` and `cursor-pointer`, but no `role`, `tabIndex`, or `onKeyDown`. Screen readers and keyboard users cannot activate it.
- **Location:**
  - `src/components/AppHeader.tsx` — lines 47-53
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Wrap in a React Router `<Link to="/problems">` for semantic navigation, browser status bar URL preview, and right-click support.

### 10. Missing Skip-to-Content Link
- **Category:** Accessibility
- **Description:** No "Skip to main content" link exists. Keyboard users must tab through the entire navigation on every page load.
- **Location:**
  - `src/layouts/AppLayout.tsx` — no skip link
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Add visually hidden skip link as first focusable element: `<a href="#main-content" className="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50 focus:bg-primary focus:text-white focus:px-4 focus:py-2 focus:rounded">Skip to main content</a>`. Add `id="main-content"` to `<main>`.

### 11. No ARIA Live Region for Submission Results
- **Category:** Accessibility
- **Description:** When a user submits code and receives results (pass/fail, XP gained), there is no ARIA live region to announce these changes to screen readers.
- **Location:**
  - `src/components/ProblemView/ProblemView.tsx` — lines 341, 393
  - `src/components/ProblemView/ProblemSubmissionResult.tsx` — entire component
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Wrap the submission result area in `aria-live="polite"` or add `role="status"` to the result card.

### 12. Comment Vote Buttons Are Non-Functional Without Indication
- **Category:** Flow / Accessibility
- **Description:** Upvote/downvote buttons in `CommentItem.tsx` render as interactive HeroUI Buttons but have no `onPress` handler and no `aria-label`. They do nothing when clicked and are inaccessible to screen readers.
- **Location:**
  - `src/components/ArticleComments/CommentItem.tsx` — lines 143-148
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** Backend voting API (not yet implemented)
- **Breaking Changes:** No
- **Recommendation:** Add `isDisabled`, `aria-label="Upvote comment"` / `aria-label="Downvote comment"`, and a tooltip: "Voting coming soon!" On-brand: "Voting™ is still being coded by unpaid interns."

### 13. `disabled` Prop Used Instead of `isDisabled` on HeroUI Buttons
- **Category:** Component / Accessibility
- **Description:** In the SyncModal, two HeroUI `<Button>` components use the native HTML `disabled` attribute instead of HeroUI's `isDisabled` prop, bypassing HeroUI's proper styling and ARIA attributes.
- **Location:**
  - `src/components/SyncModal.tsx` — line 80 (`disabled={isLoading}`)
  - `src/components/SyncModal.tsx` — line 83 (`disabled={error !== null}`)
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Change `disabled=` to `isDisabled=`.

### 14. Inconsistent Container Max-Width Across Pages
- **Category:** Layout / Consistency
- **Description:** Different pages use different `max-w-*` values: navbar and ProblemList use `max-w-7xl`, AccountPage uses `max-w-6xl`, PrivacyPolicy uses `max-w-4xl`. The AppLayout has no `max-w` at all (`w-full px-4`). Content width jumps when navigating between pages.
- **Location:**
  - `src/components/AppHeader.tsx` — line 43 (`max-w-7xl`)
  - `src/components/ProblemList.tsx` — line 275 (`max-w-7xl`)
  - `src/pages/AccountPage.tsx` — line 28 (`max-w-6xl`)
  - `src/layouts/AppLayout.tsx` — line 15 (no `max-w`)
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Add `max-w-7xl mx-auto` to `AppLayout.tsx` content wrapper as default.

### 15. ProblemView Mobile Layout — Editor Buried Below Long Description
- **Category:** Layout / Responsive Design
- **Description:** On mobile (`lg:hidden`), the ProblemView stacks description and code editor vertically. The editor uses `editorHeight="79vh"`, so users must scroll past the entire problem description to reach a nearly-full-viewport-height editor. No way to toggle between panels.
- **Location:**
  - `src/components/ProblemView/ProblemView.tsx` — lines 314-361
  - `src/components/ProblemView/ProblemCodeEditor.tsx` — line 431 (`editorHeight="79vh"`)
- **Impact:** High
- **Effort:** High (4-8hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** On mobile, use a tab-based approach to switch between "Description" and "Code Editor" views, or a collapsible panel. Add a "scroll to editor" FAB.

### 16. Fullscreen Code Editor Uses Hardcoded `top` Offset
- **Category:** Layout
- **Description:** The fullscreen overlay uses `top-[114px]` — a magic number for navbar + warning banner height. Will break if the banner is removed, resized, or at different viewport conditions.
- **Location:**
  - `src/components/ProblemView/ProblemCodeEditor.tsx` — line 109 (`fixed top-[114px]`)
- **Impact:** Medium
- **Effort:** Medium (1-4hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Use `inset-0` with a higher z-index (standard fullscreen pattern), or measure header height dynamically via CSS variable.

### 17. Code Editor Action Buttons Use `absolute` Centering That Clips
- **Category:** Layout
- **Description:** Run/Submit/Reset buttons are positioned with `absolute left-1/2 transform -translate-x-1/2`, which can overflow and overlap with adjacent controls when the container is narrow (especially in the resizable split-pane).
- **Location:**
  - `src/components/ProblemView/ProblemCodeEditor.tsx` — lines 156, 318
- **Impact:** Medium
- **Effort:** Medium (1-4hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Replace absolute positioning with flexbox: three sections (left `flex-1`, center, right `flex-1 justify-end`).

### 18. Massive Code Duplication in ProblemCodeEditor (Normal vs. Fullscreen)
- **Category:** Component / Code Quality
- **Description:** `ProblemCodeEditor.tsx` duplicates its entire UI twice — once for fullscreen (lines 108-273) and once for normal (lines 275-434). ~165 lines of header, action buttons, language selector, error display, and editor are copy-pasted.
- **Location:**
  - `src/components/ProblemView/ProblemCodeEditor.tsx` — lines 108-434
- **Impact:** Medium
- **Effort:** Medium (1-4hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Extract shared UI elements into sub-components, then compose differently for each mode.

### 19. ResizableSplitter Lacks Touch/Keyboard Support and ARIA
- **Category:** Accessibility
- **Description:** The `ResizableSplitter` only handles mouse events. No touch events for tablets, no keyboard support for arrow-key resizing. The divider has no `role="separator"`, `aria-valuenow`, or `tabIndex`.
- **Location:**
  - `src/components/ResizableSplitter.tsx` — lines 27-69 (mouse-only), lines 80-96 (divider without ARIA)
- **Impact:** Medium
- **Effort:** Medium (1-4hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Add `role="separator"`, `aria-orientation="vertical"`, `aria-valuenow`/`min`/`max`, `tabIndex={0}`, keyboard handlers (Left/Right arrows), and touch event handlers.

### 20. Hardcoded `text-gray-*` in PWABanner
- **Category:** Consistency
- **Description:** PWABanner uses raw Tailwind `text-gray-300` and `text-gray-500` instead of HeroUI's `text-default-*` theme tokens.
- **Location:**
  - `src/components/PWABanner.tsx` — line 84 (`text-gray-300`), line 175 (`text-gray-500`), line 203 (`text-gray-500`)
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Replace with `text-default-300` and `text-default-500`.

### 21. Hardcoded `text-white` in CreateEditProblem
- **Category:** Consistency
- **Description:** Page header uses `text-white` instead of `text-foreground`. Works now but breaks the token contract.
- **Location:**
  - `src/components/CreateEditProblem.tsx` — line 303
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Replace `text-white` with `text-foreground`.

### 22. Hardcoded Scrollbar Colors in App.css
- **Category:** Consistency
- **Description:** Custom scrollbar styles use hardcoded hex values (`#1a1a1a`, `#4a4a4a`, `#5a5a5a`) bypassing theme tokens.
- **Location:**
  - `src/App.css` — lines 19-30
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Use `hsl(var(--heroui-content1))` for track and `hsl(var(--heroui-content3))`/`hsl(var(--heroui-content4))` for thumb.

### 23. No 404 Page — Silent Redirect
- **Category:** Flow / Brand
- **Description:** The catch-all route silently redirects to `/` with no error page. Users who mistype URLs have no idea what happened. Missed opportunity for brand personality.
- **Location:**
  - `src/App.tsx` — line 95
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Create a `NotFoundPage` with snarky messaging (e.g., "404: This page derped out of existence. Maybe try typing better? 🤷") and a button to `/problems`.

### 24. Theme Preference UI Offers Non-Functional Light/Custom Themes
- **Category:** Flow / Consistency
- **Description:** PreferencesSection offers Dark, Light, and Custom themes, but the app is hardcoded to dark mode (`<html class="dark">`). Selecting "Light" or "Custom" saves to the API but has zero visual effect.
- **Location:**
  - `src/components/PreferencesSection.tsx` — lines 210-214
  - `index.html` — line 2 (`class="dark"`)
- **Impact:** Medium
- **Effort:** Medium (1-4hr to disable; Very High to implement)
- **Dependencies:** Full light theme design if implementing
- **Breaking Changes:** No
- **Recommendation:** Short term: disable "Light" and "Custom" with "Coming soon" tooltip. Long term: implement theme switching.

### 25. Problem Card Uses `<div role="button">` Instead of Semantic Link
- **Category:** Accessibility
- **Description:** Each problem card is a `<div role="button" tabIndex={0}>` with custom click/keyboard handlers. Semantically it navigates, so it should be an `<a>`/`<Link>` for proper screen reader announcement, right-click "open in new tab", and status bar URL preview.
- **Location:**
  - `src/components/ProblemList.tsx` — lines 613-624
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Wrap each card in a React Router `<Link to={`/problems/${problem.id}`}>` with `className="block no-underline"`.

### 26. Inconsistent Loading States (Spinner vs. Plain Text)
- **Category:** Consistency
- **Description:** `ProtectedRoute` shows a HeroUI `Spinner`, but `AdminRoute` shows plain "Loading..." text. `AccountPage` also uses plain text.
- **Location:**
  - `src/components/AdminRoute.tsx` — lines 23-26
  - `src/pages/AccountPage.tsx` — lines 12-15
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Use `<Spinner size="lg" color="primary" label="Loading..." />` everywhere.

### 27. No Loading Skeletons — Bare Spinners Only
- **Category:** Component / Flow
- **Description:** Every async loading state uses a centered `<Spinner>` creating full-content-replacement. Modern UX prefers skeleton/shimmer loading states that preserve layout shape.
- **Location:**
  - `src/components/ProblemList.tsx` — lines 253-258
  - `src/components/ProblemView/ProblemView.tsx` — lines 284-289
  - `src/components/ProblemView/ProblemSubmissions.tsx` — lines 142-148
  - `src/components/ArticleComments/ArticleComments.tsx` — lines 79-84
- **Impact:** Medium
- **Effort:** High (4-8hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Create skeleton components using HeroUI's `<Skeleton>` for ProblemList and ProblemView. Prioritize ProblemList.

### 28. Duplicate Utility Functions
- **Category:** Code Quality
- **Description:** `getDifficultyColor`/`getDifficultyLabel` defined both in `ProblemList.tsx` (lines 209-239) and `utilities.ts` (lines 12-42).
- **Location:**
  - `src/components/ProblemList.tsx` — lines 209-239
  - `src/utils/utilities.ts` — lines 12-42
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Delete inline versions; import from `utilities.ts`.

### 29. Editor Settings Flame Toggle Doesn't Persist to Server
- **Category:** Flow
- **Description:** Code editor settings modal has a "Flame Effects" toggle that only updates local state. The Preferences page has a separate toggle that persists. The two can get out of sync.
- **Location:**
  - `src/components/ProblemView/ProblemCodeEditor.tsx` — lines 68, 450
  - `src/components/PreferencesSection.tsx` — lines 296-314
- **Impact:** Medium
- **Effort:** Medium (1-4hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Wire editor toggle to server preferences, or remove it and direct to Preferences.

### 30. PWABanner Dismiss Buttons Missing ARIA Labels
- **Category:** Accessibility
- **Description:** Both PWABanner dismiss buttons are `isIconOnly` with no `aria-label`.
- **Location:**
  - `src/components/PWABanner.tsx` — lines 186, 214
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Add `aria-label="Dismiss install banner"` and `aria-label="Dismiss update banner"`.

### 31. Missing `aria-hidden="true"` on Google OAuth SVG in LoginPage
- **Category:** Accessibility
- **Description:** Google OAuth SVG on Login page lacks `aria-hidden="true"`, while GitHub SVG has it. Register page's Google SVG has it correctly.
- **Location:**
  - `src/pages/LoginPage.tsx` — line 116 (missing `aria-hidden`)
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Add `aria-hidden="true"` to the Google OAuth SVG.

### 32. Heading Hierarchy Issues
- **Category:** Accessibility / Typography
- **Description:** AppHeader uses `<h1>` for the brand name (should be a `<span>`). LandingPage starts at `<h2>` with no `<h1>`.
- **Location:**
  - `src/components/AppHeader.tsx` — line 52 (`<h1>`)
  - `src/pages/LandingPage.tsx` — lines 25, 51
- **Impact:** Medium
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Change AppHeader brand from `<h1>` to `<span>`. Make each page's primary heading `<h1>`.

### 33. `<br />` Tags Used for Spacing
- **Category:** Typography / Code Quality
- **Description:** Login modal, email verification modal, and AccountSection use `<br />` for spacing instead of proper elements or Tailwind classes.
- **Location:**
  - `src/components/ProblemView/ProblemModals.tsx` — lines 62-67
  - `src/components/AccountSection.tsx` — line 482
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Replace with `<p>` elements in `space-y-4` containers or `mt-6`.

### 34. Login/Register Pages Missing Logo Icon
- **Category:** Consistency
- **Description:** ForgotPassword, ResetPassword, ConfirmEmail, OAuthCallback pages show favicon + "DerpCode". Login and Register only show text.
- **Location:**
  - `src/pages/LoginPage.tsx` — line 55
  - `src/pages/RegisterPage.tsx` — line 97
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Add favicon icon to Login and Register pages.

### 35. Auth Page Background Gradient Inconsistency
- **Category:** Consistency
- **Description:** Login/Register use `from-background to-content1`. ConfirmEmail/OAuthCallback use `from-background via-content1 to-background`.
- **Location:**
  - `src/pages/LoginPage.tsx` — line 52
  - `src/pages/ConfirmEmailPage.tsx` — line 50
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Standardize on one gradient pattern.

### 36. Copyright Year Hardcoded to 2025
- **Category:** Brand / Content
- **Description:** Landing page footer has hardcoded `© 2025 DerpCode`.
- **Location:**
  - `src/pages/LandingPage.tsx` — line 116
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Use `{new Date().getFullYear()}`.

### 37. Debug Comment Left in ProblemList
- **Category:** Code Quality
- **Description:** Stray `//asdfsadf` comment on line 37.
- **Location:**
  - `src/components/ProblemList.tsx` — line 37
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Remove the comment.

### 38. PWABanner Auto-Dismiss Comment Discrepancy
- **Category:** Code Quality
- **Description:** `AUTO_DISMISS_DURATION = 15000` but comment says "10 seconds".
- **Location:**
  - `src/components/PWABanner.tsx` — line 7
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Fix comment to "15 seconds" or change value to 10000.

### 39. WarningBanner Has No Dismiss Mechanism
- **Category:** Flow
- **Description:** The development warning banner is always visible with no dismiss. Wastes vertical space for returning users and contributes to the hardcoded `top-[114px]` fullscreen issue.
- **Location:**
  - `src/components/WarningBanner.tsx`
  - `src/layouts/AppLayout.tsx` — line 13
- **Impact:** Low
- **Effort:** Medium (1-4hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Add dismiss with localStorage TTL, or make environment-conditional.

### 40. Duplicate Pagination Controls with Inconsistent Sizing
- **Category:** Consistency
- **Description:** ProblemList has top pagination (`size="sm"`) and bottom pagination (`size="lg"`). Visually jarring.
- **Location:**
  - `src/components/ProblemList.tsx` — lines 581-593, 728-740
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Use consistent sizing. Consider removing top pagination.

### 41. `prose` Double-Nesting in Comment MarkdownRenderer
- **Category:** Typography
- **Description:** `CommentItem.tsx` wraps content in `prose prose-sm` which contains `<MarkdownRenderer>` that itself has `prose prose-slate`. Double-nesting causes unexpected margin/padding.
- **Location:**
  - `src/components/ArticleComments/CommentItem.tsx` — line 137
  - `src/components/MarkdownRenderer.tsx` — line 14
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Remove outer `prose` in `CommentItem.tsx`; let `MarkdownRenderer` handle it.

### 42. `bg-default-50` Used Without Dark Override
- **Category:** Consistency / Dark Mode
- **Description:** Several components use `bg-default-50` which may render too bright in dark mode. MarkdownRenderer correctly overrides it; Submissions table header and ResetPasswordPage don't.
- **Location:**
  - `src/components/ProblemView/ProblemSubmissions.tsx` — line 205
  - `src/pages/ResetPasswordPage.tsx` — line 180
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Replace with `bg-content2` or add `dark:bg-default-100/50`.

### 43. Editor Height Uses Arbitrary `79vh`
- **Category:** Consistency
- **Description:** Code editor has `79vh` (normal), `calc(100vh - 300px)` (fullscreen), `70vh` (default). The `79vh` is arbitrary and doesn't account for surrounding chrome.
- **Location:**
  - `src/components/ProblemView/ProblemCodeEditor.tsx` — line 431
  - `src/components/CodeEditor.tsx` — line 185
- **Impact:** Low
- **Effort:** Low (< 1hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Use consistent `calc()` approach accounting for header + banner height.

### 44. No Onboarding for First-Time Users
- **Category:** Flow
- **Description:** New users are dropped into the problem list with no tour, XP system explanation, or "try your first problem" prompt.
- **Location:**
  - `src/App.tsx` — routing setup
- **Impact:** Medium
- **Effort:** Very High (> 8hr)
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Lightweight onboarding: welcome modal after first login explaining XP/levels, highlight first easy problem, tooltip tour via `react-joyride`.

### 45. Google OAuth SVG Hardcoded Brand Colors (Intentional — No Change Needed)
- **Category:** Consistency
- **Description:** Google OAuth SVGs use hardcoded brand colors (`#4285F4`, `#34A853`, `#FBBC05`, `#EA4335`). Correct per Google's brand guidelines.
- **Location:** LoginPage, RegisterPage, OAuthCallbackPage, AccountSection
- **Impact:** N/A (correct as-is)
- **Effort:** N/A
- **Dependencies:** None
- **Breaking Changes:** N/A
- **Recommendation:** No change needed.
