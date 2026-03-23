# TrackingApp.MAUI — Dependencies

> **TL;DR**: The solution uses three distinct dependency sets: the MAUI UI project (`TrackingApp`) depends on MAUI controls, SQLite, and logging; the core library (`TrackingApp.Core`) depends only on sqlite-net-pcl; and the test project (`TrackingApp.Tests`) depends on xUnit, FluentAssertions, and test runners. No external cloud SDKs or analytics frameworks are present.

**Updated:** 2026-03-23 00:00 -0600

---

## TrackingApp (MAUI UI project)

Target frameworks: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`

### Direct NuGet References

| Package | Version | Usage in Project | Purpose |
|---------|---------|------------------|---------|
| `Microsoft.Maui.Controls` | `$(MauiVersion)` (10.x) | All XAML pages, controls | Core MAUI UI framework — pages, layouts, controls, navigation, XAML |
| `Microsoft.Extensions.Logging.Debug` | `10.0.0-preview.1.25080.5` | `MauiProgram.cs` (DEBUG only) | Routes `ILogger` output to the debug output window during development |
| `sqlite-net-pcl` | `1.9.172` | `DatabaseService.cs` | Lightweight ORM for SQLite — `SQLiteAsyncConnection`, `[PrimaryKey]`, `[AutoIncrement]`, `[Ignore]` attributes |
| `SQLitePCLRaw.bundle_green` | `2.1.11` | Implicit (sqlite-net-pcl requires it) | Native SQLite binaries bundled for Android, iOS, Windows, Mac — the "green" bundle links the system SQLite on iOS/Mac and ships a static lib for Android/Windows |

### Project References

| Reference | Purpose |
|-----------|---------|
| `TrackingApp.Core` | Provides all models (`FoodEntry`, `Medication`, etc.), `IDatabaseService` interface, `DataService`, and `NumericParser` |

### Notable Transitive Dependencies (from `Microsoft.Maui.Controls`)

| Transitive Package | Origin | Role |
|--------------------|--------|------|
| `Microsoft.Maui.Core` | MAUI | Platform abstraction layer |
| `Microsoft.Maui.Essentials` | MAUI | `FileSystem`, `SecureStorage`, `Preferences`, `MainThread`, etc. |
| `Microsoft.Extensions.DependencyInjection` | ASP.NET Core | DI container used by `MauiAppBuilder` |
| `Microsoft.Extensions.Logging` | ASP.NET Core | `ILogger<T>` abstractions |
| `CommunityToolkit.Mvvm` (pulled in by MAUI) | Community Toolkit | `ObservableObject`, `[RelayCommand]`, `[ObservableProperty]` |

---

## TrackingApp.Core (class library)

Target framework: `net10.0`

### Direct NuGet References

| Package | Version | Usage in Project | Purpose |
|---------|---------|------------------|---------|
| `sqlite-net-pcl` | `1.9.172` | `Models/` (ORM attributes) | Provides `[PrimaryKey]`, `[AutoIncrement]`, `[Ignore]` attributes on model classes so `DatabaseService` can use them without the Core project directly referencing a native SQLite driver |

> **Why sqlite-net-pcl in Core?** The model attributes (`[PrimaryKey]`, `[Ignore]`) are defined in this package. The actual database connection (`SQLiteAsyncConnection`) is only used in `TrackingApp/Services/DatabaseService.cs`.

---

## TrackingApp.Tests (xUnit test project)

Target framework: `net10.0`

### Direct NuGet References

| Package | Version | Usage in Project | Purpose |
|---------|---------|------------------|---------|
| `xunit` | `2.9.2` | All test files | Test framework — `[Fact]`, `[Theory]`, `[InlineData]` attributes |
| `xunit.runner.visualstudio` | `2.8.2` | VS Test Explorer | Adapter that exposes xUnit tests to Visual Studio's Test Explorer and `dotnet test` |
| `Microsoft.NET.Test.Sdk` | `17.11.1` | Test project infra | MSBuild integration for test discovery and execution |
| `FluentAssertions` | `6.12.0` | Assertion style | Expressive assertions — `.Should().Be()`, `.Should().HaveCount()`, `.Should().NotBeNull()`, etc. |
| `coverlet.collector` | `6.0.2` | CI coverage | Code coverage collector for `dotnet test --collect:"XPlat Code Coverage"` |

### Project References

| Reference | Purpose |
|-----------|---------|
| `TrackingApp.Core` | Unit-tests `DataService`, models, and `NumericParser` without any MAUI runtime |

---

## Version Constraints & Notes

- The Android minimum API is **21** (Android 5.0 Lollipop) — set in `TrackingApp.csproj`.
- iOS minimum version is **15.0**.
- `SQLitePCLRaw.bundle_green` is explicitly pinned at `2.1.11` to avoid conflicts with the version pulled transitively by `sqlite-net-pcl`. Do not upgrade independently.
- `Microsoft.Extensions.Logging.Debug` should stay at the same major/minor as the MAUI SDK (`10.0.x-preview`). Mismatches can cause binding redirect issues.
- `FluentAssertions` `6.x` requires .NET 4.7.2+ or .NET Core 3.1+; `net10.0` is compatible.
- No third-party analytics, crash-reporting, or cloud-storage SDKs are present in this version.

---

## Upgrade Guidance

| Package | Risk When Upgrading | Notes |
|---------|---------------------|-------|
| `Microsoft.Maui.Controls` | High — breaking changes between major versions | Test all platforms after upgrade |
| `sqlite-net-pcl` | Medium — schema migrations may be needed | Test DB operations on device after upgrade |
| `SQLitePCLRaw.bundle_green` | Medium | Must be compatible with `sqlite-net-pcl` version; check release notes |
| `xunit` + runners | Low | Upgrade together to avoid version conflicts |
| `FluentAssertions` | Low | Review changelog if upgrading from 6.x to 7.x (breaking assertion changes) |
