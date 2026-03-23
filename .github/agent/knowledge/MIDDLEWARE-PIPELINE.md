# TrackingApp.MAUI — Middleware & App Startup Pipeline

> **TL;DR**: TrackingApp uses the standard .NET MAUI `MauiAppBuilder` pipeline. There is no ASP.NET Core HTTP middleware. The "pipeline" is the MAUI app-startup sequence: builder configuration → host build → platform integration → window creation → Shell navigation. DI, fonts, and debug logging are the only customizations registered in `MauiProgram.cs`.

**Updated:** 2026-03-23 00:00 -0600

---

## Clarification: "Middleware" in a MAUI App

Unlike ASP.NET Core, .NET MAUI does not use HTTP request middleware (`app.Use(...)` / `app.Run(...)`). Instead, the equivalent concept is the **app startup pipeline** — the ordered sequence of builder registrations and lifecycle hooks that run when the application launches. This document describes that pipeline.

---

## Startup Pipeline — Ordered Steps

### Step 1 — Platform Host Entry Point

Each platform has its own entry point that bootstraps the MAUI host:

| Platform | File | Entry Method |
|----------|------|--------------|
| Android | `Platforms/Android/MainActivity.cs` | `Activity.OnCreate` → calls `MauiApp.CreateMauiApp()` |
| iOS / macCatalyst | `Platforms/iOS/AppDelegate.cs` | `UIApplicationDelegate.FinishedLaunching` → calls `MauiApp.CreateMauiApp()` |
| Windows | `Platforms/Windows/App.xaml.cs` | `Microsoft.UI.Xaml.Application.OnLaunched` → calls `MauiApp.CreateMauiApp()` |

All platforms converge on `MauiProgram.CreateMauiApp()`.

---

### Step 2 — `MauiProgram.CreateMauiApp()` (configured in `TrackingApp/MauiProgram.cs`)

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();

    builder
        .UseMauiApp<App>()           // [1] Register App class as MAUI application
        .ConfigureFonts(fonts =>     // [2] Register custom fonts
        {
            fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf",  "OpenSansSemibold");
        });

#if DEBUG
    builder.Logging.AddDebug();      // [3] Debug logger (DEBUG builds only)
#endif

    return builder.Build();          // [4] Build and return MauiApp
}
```

#### Registered Components (in order)

| # | Registration | Location | Effect |
|---|-------------|----------|--------|
| 1 | `.UseMauiApp<App>()` | `MauiProgram.cs` | Registers `App` as the MAUI application type; wires MAUI's built-in services (handlers, renderers, etc.) |
| 2 | `.ConfigureFonts(...)` | `MauiProgram.cs` | Adds OpenSans Regular and Semibold as named font resources available via `FontFamily` in XAML |
| 3 | `builder.Logging.AddDebug()` | `MauiProgram.cs` | Adds debug output logging — only active in `#if DEBUG` builds |
| 4 | `builder.Build()` | `MauiProgram.cs` | Finalizes the DI container and creates the `MauiApp` instance |

> **Note**: No additional services (ViewModels, DataService, DatabaseService) are registered in the DI container. The app currently uses static singletons (`AppServices.DataService`, `DatabaseService.Instance`) instead of constructor injection. This is a known deviation from idiomatic MAUI/DI patterns.

---

### Step 3 — MAUI Platform Integration

After `builder.Build()`, the MAUI host:
1. Resolves platform-specific handler mappings (controls → native views)
2. Configures the platform window (Activity on Android, UIWindow on iOS)
3. Calls `App.CreateWindow(IActivationState?)` to create the top-level window

---

### Step 4 — `App.CreateWindow()` (configured in `TrackingApp/App.xaml.cs`)

```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    return new Window(new AppShell());
}
```

Returns a `Window` whose root is `AppShell`. This is the only place `AppShell` is instantiated.

---

### Step 5 — `App.xaml` Resource Dictionary Loading

When `App` is initialized (`InitializeComponent()`), the merged resource dictionaries are loaded:

| Order | Resource | Content |
|-------|----------|---------|
| 1 | `Resources/Styles/Colors.xaml` | Color palette resource dictionary (e.g., `#4CAF50`, `#2a3d66`) |
| 2 | `Resources/Styles/Styles.xaml` | Global control style overrides |

After styles, all **value converters** are instantiated as global singleton resources:

```xml
<converters:BoolToTextConverter              x:Key="BoolToTextConverter" />
<converters:InverseBoolConverter             x:Key="InverseBoolConverter" />
<converters:StringIsNotNullOrEmptyConverter  x:Key="StringIsNotNullOrEmptyConverter" />
<converters:BoolToColorConverter             x:Key="BoolToColorConverter" />
<converters:BoolToHistoryBackgroundConverter x:Key="BoolToHistoryBackgroundConverter" />
<converters:BoolToHistoryTextColorConverter  x:Key="BoolToHistoryTextColorConverter" />
<converters:BoolToNextDoseColorConverter     x:Key="BoolToNextDoseColorConverter" />
<converters:BoolToNextDoseBackgroundConverter x:Key="BoolToNextDoseBackgroundConverter" />
<converters:BoolToNextDoseTextConverter      x:Key="BoolToNextDoseTextConverter" />
<converters:BoolToConfirmButtonColorConverter x:Key="BoolToConfirmButtonColorConverter" />
```

---

### Step 6 — `AppShell` Initialization (`TrackingApp/AppShell.xaml`)

`AppShell` defines a `TabBar` with two tabs. Both pages are loaded lazily via `ContentTemplate`:

```xml
<TabBar>
    <ShellContent Title="Inicio"
                  ContentTemplate="{DataTemplate local:MainPage}"
                  Route="MainPage" />
    <ShellContent Title="Historial"
                  ContentTemplate="{DataTemplate local:HistoryPage}"
                  Route="HistoryPage" />
</TabBar>
```

Shell routes registered: `MainPage`, `HistoryPage`  
`Shell.FlyoutBehavior="Disabled"` — no hamburger flyout menu.

---

### Step 7 — First `MainPage` Appearance

When `MainPage` first appears:

```csharp
// MainPage.xaml.cs
protected override async void OnAppearing()
{
    base.OnAppearing();
    await AppServices.DataService.ReloadAllDataAsync(); // Loads all DB data
    if (BindingContext is MainViewModel viewModel)
        viewModel.NotifyAllDataChanged();               // Triggers collection refresh
}
```

This is the **last step** in the startup pipeline. After this, the app is fully loaded and interactive.

---

## Complete Startup Sequence Summary

```
Platform Entry Point (Android MainActivity / iOS AppDelegate / Windows App)
    │
    ▼
MauiProgram.CreateMauiApp()
    ├─ UseMauiApp<App>()
    ├─ ConfigureFonts(OpenSans Regular + Semibold)
    ├─ Logging.AddDebug()  [DEBUG only]
    └─ builder.Build()
         │
         ▼
    MAUI Platform Integration
    (handler mappings, native window setup)
         │
         ▼
    App.CreateWindow()
    └─ new Window(new AppShell())
         │
         ▼
    App.xaml InitializeComponent()
    ├─ Load Colors.xaml
    ├─ Load Styles.xaml
    └─ Register 10 value converters as global resources
         │
         ▼
    AppShell.xaml
    └─ TabBar: MainPage (Inicio) | HistoryPage (Historial)
         │
         ▼
    MainPage.OnAppearing()
    ├─ DataService.ReloadAllDataAsync()  ← SQLite DB read
    └─ MainViewModel.NotifyAllDataChanged()
         │
         ▼
    App is running & interactive
```

---

## What Is NOT Present

| Pattern | Status |
|---------|--------|
| HTTP middleware pipeline | Not applicable — this is a mobile app, not a web server |
| ASP.NET Core request pipeline | Not present |
| Custom MAUI handlers | Not present in this version (default MAUI handlers used) |
| Background services (`IHostedService`) | Not present |
| Push notification middleware | Not present (planned — see README "Próximas Mejoras") |
| Authentication middleware | Not present |

---

## Cross-References

- [ARCHITECTURE-MAP.md](ARCHITECTURE-MAP.md) — Layer diagram and data flows
- [DEPENDENCIES.md](DEPENDENCIES.md) — Full NuGet package list including logging packages
- [CODEBASE-OVERVIEW.md](CODEBASE-OVERVIEW.md) — File structure and entry points
