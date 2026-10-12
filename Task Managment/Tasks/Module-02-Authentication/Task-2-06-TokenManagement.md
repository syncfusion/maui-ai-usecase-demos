# Task 2-06: Implement Token Management & Auto-Login

**Module:** 2 - Authentication  
**Phase:** 2  
**Priority:** Critical  
**Duration:** 1 day  
**Dependencies:** Task 2-01  
**Status:** ✅ Completed (October 2026)

---

## 📋 TASK DESCRIPTION

Implement token refresh mechanism, auto-login with RememberMe, and secure token storage using SecureStorage.

## 🎯 OBJECTIVES

- [x] Implement token storage in SecureStorage
- [x] Implement automatic token refresh before expiry
- [x] Implement RememberMe functionality
- [x] Implement auto-login on app launch
- [x] Handle expired tokens gracefully
- [x] Test token refresh flow
- [x] Test RememberMe persistence

---

## ✅ ACCEPTANCE CRITERIA

- [x] Tokens stored in SecureStorage (not Preferences)
- [x] Token refresh happens before expiry
- [x] RememberMe checkbox saves credentials securely
- [x] Auto-login works on app restart
- [x] Expired tokens force re-login
- [x] Refresh token logic implemented
- [x] No hardcoded expiry times
- [x] Graceful fallback on refresh failure

---

## 🧪 TESTING

### Manual Tests
- [x] Login with RememberMe checked
- [x] Close app and relaunch
- [x] Should auto-login to main screen
- [x] Logout, app shouldn't auto-login
- [x] Token refresh happens silently
- [x] Expired token shows login screen

### Automated Tests (added as part of this task)
- `SignIn_forwards_rememberMe_flag_to_authentication_service`
- `SignIn_clears_password_after_successful_sign_in`
- `LoadRememberedStateAsync_populates_email_and_rememberMe_when_available`
- `LoadRememberedStateAsync_is_noop_when_no_remembered_email`
- `LoadRememberedStateAsync_is_idempotent`
- `LoadRememberedStateAsync_swallows_exceptions_from_auth_service`
- `ClearPassword_resets_password_to_empty_string`
- `ClearPassword_is_noop_when_password_already_empty`
- `AppShellViewModel_synchronises_IsAuthenticated_with_SignedInChanged_event`
- `AppShellViewModel_routes_to_sign_in_when_SignedInChanged_fires_with_null`
- `AppShellViewModel_without_auth_service_starts_unauthenticated_and_can_be_marked_manually`

Total test suite now contains **98 tests** (87 pre-existing + 11 added by this task).

---

## 📊 DEFINITION OF DONE

- [x] Token storage secure
- [x] Auto-login works
- [x] RememberMe functional
- [x] Refresh mechanism working

---

## 🔧 IMPLEMENTATION NOTES

The bulk of the token-issuance / signing / storage logic already shipped in
Task 2-01 (`TokenManager` + `AuthenticationService` + the splash-screen flow
in `SplashPageViewModel`).  Task 2-06 closes the remaining acceptance
criteria by:

1. **Promoting `RefreshTokenAsync` to the public contract** — it was
   implemented on the concrete `AuthenticationService` for Task 2-01 but
   was missing from `IAuthenticationService`.  The interface now exposes
   it (with full XML docs describing the failure semantics).

2. **Extending `IAuthenticationService.SignInAsync` with a
   `bool rememberMe = false` parameter**.  The default value keeps the
   change additive — every existing caller keeps working.  When the user
   ticks it the e-mail address is persisted in `SecureStorage` under the
   new `auth.remembered-email` key.  The password is **never** persisted.

3. **`RestoreSessionAsync` now performs a proactive refresh** when the
   persisted access token is within the new
   `AuthenticationService.ProactiveRefreshWindow` (5 minutes).  When the
   token has already expired the method first tries to refresh silently
   from the refresh token; only when the refresh itself fails does it
   clear the persisted session and forward the user to the sign-in page.
   This implements the "Token refresh happens before expiry" and
   "Expired tokens force re-login" criteria.

4. **`AuthenticationService` clears the remembered e-mail on sign-out**
   so the user is not silently re-authenticated the next time they
   reopen the app after explicitly signing out.

5. **`AppShellViewModel` now subscribes to
   `IAuthenticationService.SignedInChanged`** and keeps its
   `IsAuthenticated` flag — and ultimately the Shell chrome — in sync
   with every sign-in / sign-out / refresh-failure transition.  When the
   event fires with a `null` payload (i.e. a forced sign-out triggered
   from a background page that lost its refresh token) the Shell routes
   the user back to the sign-in page automatically.

6. **`SignInPageViewModel` pre-populates the form** from the remembered
   e-mail when the page first appears (`LoadRememberedStateAsync`,
   invoked from `SignInPage.OnAppearing`).  The password is scrubbed on
   `OnDisappearing` so the secret does not linger in process memory
   after navigation.

7. **Public `IAuthenticationService.GetRememberedEmailAsync` accessor**
   so any future page (e.g. a profile / settings page) can introspect
   the remembered e-mail without taking a direct `SecureStorage`
   dependency.

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Implemented & unit-tested (98/98 tests passing)
