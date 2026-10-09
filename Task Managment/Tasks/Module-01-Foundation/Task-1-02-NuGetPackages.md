# Task 1-02: Install and Verify NuGet Packages

**Module:** 1 - Foundation & Setup  
**Phase:** 1  
**Priority:** Critical  
**Duration:** 0.5 days  
**Dependencies:** Task 1-01  
**Status:** ⬜ Not Started

---

## 📋 TASK DESCRIPTION

Add all required NuGet packages to the project for Syncfusion controls, MVVM framework, database, and core MAUI functionality.

## 🎯 OBJECTIVES

- [ ] Install all Syncfusion packages
- [ ] Install MVVM Toolkit
- [ ] Install database packages (SQLite)
- [ ] Verify all packages install without conflicts
- [ ] Project builds successfully

---

## 📦 PACKAGES TO INSTALL

### Syncfusion Packages
```
Package Name                         Version      Purpose
Syncfusion.Maui.ListView            24.1.x       Task list display
Syncfusion.Maui.Calendar            24.1.x       Date picker
Syncfusion.Maui.Core                24.1.x       Core framework
Syncfusion.Maui.Buttons             24.1.x       Button controls (future)
```

### MVVM & Binding
```
CommunityToolkit.MVVM               8.4.x        ObservableProperty, RelayCommand
```

### Database & Storage
```
sqlite-net-pcl                      1.8.x        SQLite ORM
SQLitePCLRaw.bundle_green           2.1.x        SQLite native bindings
```

### Core MAUI (Should Already Be Present)
```
Microsoft.Maui.Controls             8.0.x        Framework
Microsoft.Maui.Controls.Hosting     8.0.x        DI integration
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] All Syncfusion packages installed (24.1.x versions)
- [ ] CommunityToolkit.MVVM 8.4.x installed
- [ ] SQLite packages installed
- [ ] Project builds without errors
- [ ] No compiler warnings related to packages
- [ ] No package version conflicts
- [ ] IntelliSense works for all packages

---

## 🛠️ INSTALLATION STEPS

### Option 1: Using Package Manager UI (Visual Studio)
```
1. Right-click project → "Manage NuGet Packages"
2. Search for "Syncfusion.Maui.ListView"
   - Select version 24.1.x
   - Click Install
3. Repeat for each package above
4. Review and accept license terms
5. Wait for installation to complete
```

### Option 2: Using Package Manager Console
```powershell
# Syncfusion Packages
Install-Package Syncfusion.Maui.ListView -Version 24.1.0
Install-Package Syncfusion.Maui.Calendar -Version 24.1.0
Install-Package Syncfusion.Maui.Core -Version 24.1.0
Install-Package Syncfusion.Maui.Buttons -Version 24.1.0

# MVVM
Install-Package CommunityToolkit.MVVM -Version 8.4.0

# Database
Install-Package sqlite-net-pcl -Version 1.8.0
Install-Package SQLitePCLRaw.bundle_green -Version 2.1.0
```

### Option 3: Using dotnet CLI
```bash
dotnet add package Syncfusion.Maui.ListView --version 24.1.0
dotnet add package Syncfusion.Maui.Calendar --version 24.1.0
dotnet add package Syncfusion.Maui.Core --version 24.1.0
dotnet add package Syncfusion.Maui.Buttons --version 24.1.0
dotnet add package CommunityToolkit.MVVM --version 8.4.0
dotnet add package sqlite-net-pcl --version 1.8.0
dotnet add package SQLitePCLRaw.bundle_green --version 2.1.0

# Restore
dotnet restore
```

---

## 🧪 VERIFICATION STEPS

### Step 1: Check Project File
- [ ] Open `.csproj` file in text editor
- [ ] Verify all package references listed under `<ItemGroup>`
- [ ] Verify versions match requirements

### Step 2: Build Project
```bash
dotnet build
# OR use Visual Studio → Build → Build Solution
```

**Expected Result:**
- ✅ Build succeeds
- ✅ No errors
- ✅ No warnings related to packages

### Step 3: Check IntelliSense
- [ ] Open a C# file
- [ ] Type: `using Syncfusion.Maui.ListView;`
- [ ] Verify IntelliSense recognizes the namespace
- [ ] Try: `SfListView` - should show in IntelliSense
- [ ] Try: `[ObservableProperty]` - should show MVVM attribute

### Step 4: Verify No Conflicts
```bash
dotnet package search Syncfusion.Maui.ListView
# Should show version 24.1.x as installed
```

---

## 📊 DEFINITION OF DONE

**Code Quality:**
- [ ] All packages installed from approved sources
- [ ] Version numbers match requirements
- [ ] No pre-release versions installed
- [ ] Licenses accepted/reviewed

**Testing:**
- [ ] Project builds successfully
- [ ] No build errors or warnings
- [ ] IntelliSense works for new packages
- [ ] Can reference Syncfusion namespaces

**Documentation:**
- [ ] Task marked complete
- [ ] Package versions documented
- [ ] Any issues/workarounds noted

---

## 🔍 TROUBLESHOOTING

### Issue: Package Not Found
**Solution:**
- Verify NuGet.org is configured as package source
- Check package name spelling
- Ensure version number exists on NuGet.org

### Issue: Version Conflicts
**Solution:**
- Use Package Manager Console: `Update-Package -Reinstall`
- Delete `packages` folder and restore
- Check for conflicting dependencies

### Issue: Syncfusion License Warning
**Solution:**
- This is normal for trial/community editions
- Continue with implementation
- License can be configured in MauiProgram.cs later

### Issue: SQLite Version Conflicts
**Solution:**
- Ensure `SQLitePCLRaw.bundle_green` matches sqlite-net-pcl version
- Try: `Install-Package SQLitePCLRaw.bundle_green -DependencyVersion Highest`

---

## 📚 RELATED TASKS

**Previous Tasks:**
- Task 1-01: Create project folder structure ✓

**Next Tasks:**
- Task 1-03: Implement base MVVM classes
- Task 1-05: Setup MauiProgram and DI

---

## 🔗 REFERENCES

- **Quick Reference:** QUICK_REFERENCE.md → 📦 NUGET PACKAGES
- **Implementation Guide:** HARNESS.md → MODULE 1 → 1.1 Project Structure & NuGet Dependencies
- **Syncfusion Docs:** https://help.syncfusion.com/maui/introduction/getting-started

---

## 📝 NOTES

**Important:**
1. Do NOT use preview/beta versions unless specified
2. Ensure internet connection for package download
3. Some packages may need to download native bindings
4. First build may take longer (compilation + native bindings)

**Tips:**
- Keep Package Manager Console visible to watch installation progress
- If installation stalls, check NuGet.org status
- Can install packages incrementally and build after each

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
