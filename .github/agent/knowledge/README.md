# TrackingApp.MAUI — Knowledge README

> **TL;DR**: TrackingApp.MAUI is a .NET 10 MAUI cross-platform app for tracking food intake, medications, and medical appointments. This document explains how to build, run, test, and leverage the codebase effectively.

**Updated:** 2026-03-23 00:00 -0600

---

## Overview

TrackingApp is a mobile health-tracking application targeting Android, iOS, macOS Catalyst, and Windows. The primary use case is recording food entries and medication schedules (with dose confirmation and history) for babies, adults, and animals. It stores all data locally using SQLite.

---

## Repository Layout

```
TrackingApp.MAUI/
├── TrackingApp/            # MAUI UI project (views, ViewModels, services wrapper)
├── TrackingApp.Core/       # Pure .NET library — business logic, models, interfaces
├── TrackingApp.Tests/      # xUnit unit test project (no MAUI runtime required)
└── TrackingApp.MAUI.sln    # Solution file
```

> **Rule**: `TrackingApp.Core` must never reference MAUI assemblies. Keep it clean so tests run on plain `net10.0`.

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.0+ |
| Visual Studio | 2022 v17.12+ with MAUI workload |
| Android SDK | API 21+ (for Android target) |

Install or repair the MAUI workload:
```powershell
dotnet workload install maui
dotnet workload repair
```

---

## Build

```powershell
# Build all targets
dotnet build TrackingApp.MAUI.sln

# Build Android debug
dotnet build TrackingApp/TrackingApp.csproj -f net10.0-android

# Build Windows debug
dotnet build TrackingApp/TrackingApp.csproj -f net10.0-windows10.0.19041.0
```

---

## Run

```powershell
# Run on Android emulator
dotnet build -t:Run -f net10.0-android

# Run on connected Android device (fast deploy)
dotnet build -t:Run -f net10.0-android /p:AndroidDebugUseFastDeploy=true
```

---

## Test

```powershell
# Run all unit tests
dotnet test TrackingApp.Tests/TrackingApp.Tests.csproj

# Run with coverage
dotnet test TrackingApp.Tests/TrackingApp.Tests.csproj --collect:"XPlat Code Coverage"
```

Tests live in `TrackingApp.Tests/` and reference only `TrackingApp.Core`. They use `MockDatabaseService` (in-memory) so no device or emulator is needed.

---

## Publish / Distribute APK

```powershell
# Using the helper script
.\Build-APK.ps1

# Manual publish (signed AAB)
dotnet publish TrackingApp/TrackingApp.csproj -f net10.0-android -c Release
```

Output: `publish_output/com.trackingapp.nutrition-Signed.aab`

Keystore details are configured in `TrackingApp/TrackingApp.csproj` under the Android signing `<PropertyGroup>`. **Do not commit actual passwords to source control** — use environment variables or a secrets store.

---

## Key Entry Points

| File | Purpose |
|------|---------|
| `TrackingApp/MauiProgram.cs` | App bootstrap, DI registration, font config |
| `TrackingApp/App.xaml.cs` | Creates the `AppShell` window |
| `TrackingApp/AppShell.xaml` | Tab-bar navigation with two tabs: Inicio / Historial |
| `TrackingApp/MainPage.xaml` | Primary UI — food entry + medication scheduling |
| `TrackingApp/HistoryPage.xaml` | Filterable history of all food + medication events |

---

## Leveraging the Codebase

- **Add new tracked entities**: Add a model to `TrackingApp.Core/Models/`, extend `IDatabaseService`, implement in `DatabaseService`, add business logic to `DataService`.
- **Add new UI pages**: Create a `ContentPage` + ViewModel pair, register a Shell route in `AppShell.xaml`.
- **Add unit tests**: Reference `TrackingApp.Core` only; use `MockDatabaseService` as the `IDatabaseService` implementation.
- **Sensitive data storage**: Use `SecureStorage.SetAsync` / `GetAsync` — never `Preferences` for tokens or credentials.

---

## Related Knowledge Docs

- [CODEBASE-OVERVIEW.md](CODEBASE-OVERVIEW.md) — Structure and key components
- [ARCHITECTURE-MAP.md](ARCHITECTURE-MAP.md) — Layers, data flow, interfaces
- [DEPENDENCIES.md](DEPENDENCIES.md) — All NuGet packages
- [MIDDLEWARE-PIPELINE.md](MIDDLEWARE-PIPELINE.md) — MAUI app startup pipeline
