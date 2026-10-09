# Task 2-03: Create Sign In Screen (SignInPage)

**Module:** 2 - Authentication  
**Phase:** 2  
**Priority:** Critical  
**Duration:** 1 day  
**Dependencies:** Task 1-04, 1-06, 2-01  
**Status:** ⬜ Not Started

---

## 📋 TASK DESCRIPTION

Create the sign-in screen where users enter email and password. Implement real-time validation, error display, and navigation to main app on success.

## 🎯 OBJECTIVES

- [ ] Create SignInPage.xaml with email and password fields
- [ ] Create SignInPageViewModel with validation logic
- [ ] Implement real-time email/password validation
- [ ] Show validation errors inline
- [ ] Handle sign-in button (loading state, success/error)
- [ ] Implement "Remember Me" checkbox (optional)
- [ ] Add links to SignUp and ForgotPassword pages
- [ ] Test all validation scenarios

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Email field with keyboard: email
- [ ] Password field masked
- [ ] Both fields required
- [ ] Sign-in button disabled until valid
- [ ] Loading indicator on button click
- [ ] Success navigates to TaskListPage
- [ ] Error shows validation message
- [ ] Remember Me persists auth (optional)
- [ ] SignUp link navigates to signup page
- [ ] ForgotPassword link navigates to reset page
- [ ] Responsive on all screen sizes

---

## 🧪 TESTING

### Manual Tests
- [ ] Empty fields show validation error
- [ ] Invalid email rejected
- [ ] Valid email + password enables button
- [ ] Click sign-in shows loading
- [ ] Wrong credentials show error
- [ ] Correct credentials navigate to main
- [ ] Links navigate correctly
- [ ] Responsive on phone/tablet/desktop

---

## 📊 DEFINITION OF DONE

- [ ] Page layout matches design
- [ ] Validation works correctly
- [ ] Error messages clear
- [ ] Loading states visible
- [ ] Navigation works
- [ ] Responsive verified

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
