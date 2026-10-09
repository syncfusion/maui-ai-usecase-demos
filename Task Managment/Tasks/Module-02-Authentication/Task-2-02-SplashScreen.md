# Task 2-02: Create Splash Screen (SplashPage)

**Module:** 2 - Authentication  
**Phase:** 2  
**Priority:** High  
**Duration:** 0.5 days  
**Dependencies:** Task 1-04, 1-06, 2-01  
**Status:** ⬜ Not Started

---

## 📋 TASK DESCRIPTION

Create the splash/loading screen that displays on app launch while checking authentication status and loading resources.

## 🎯 OBJECTIVES

- [ ] Create SplashPage.xaml with logo and loading indicator
- [ ] Create SplashPageViewModel with startup logic
- [ ] Implement authentication check on page load
- [ ] Route to TaskListPage if authenticated
- [ ] Route to SignInPage if not authenticated
- [ ] Handle loading states and timeouts
- [ ] Test on all platforms

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Splash page displays on app launch
- [ ] Logo centered with loading animation
- [ ] Auth check completes within 2 seconds
- [ ] Authenticated users navigate to TaskList
- [ ] Unauthenticated users navigate to SignIn
- [ ] Timeout after 5 seconds shows error
- [ ] Loading indicator displays during check
- [ ] No hardcoded delays

---

## 🧪 TESTING

### Manual Tests
- [ ] Launch app, splash displays
- [ ] Wait for auth check (should auto-navigate)
- [ ] Logout from app, relaunch shows splash
- [ ] Network error displays gracefully
- [ ] Timeout scenario handled

---

## 📊 DEFINITION OF DONE

- [ ] SplashPage created and compiles
- [ ] Navigation works correctly
- [ ] Loading UI feels responsive
- [ ] All timeout scenarios handled
- [ ] Manual tests pass

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
