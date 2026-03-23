# TrackingApp.MAUI — Codebase Overview

> **TL;DR**: TrackingApp.MAUI is a three-project .NET 10 solution. `TrackingApp` is the MAUI UI layer; `TrackingApp.Core` holds all platform-independent business logic and models; `TrackingApp.Tests` provides xUnit tests for the core. The app tracks food entries, medications (with dose scheduling), and medical appointments using a local SQLite database.

**Updated:** 2026-03-23 00:00 -0600

---

## High-Level Purpose

The application helps users track health-related data for one or more profiles (Bebé, Adulto, Animal):

| Feature | Description |
|---------|-------------|
| **Food tracking** | Record food type, amount, unit, and time per profile |
| **Medication management** | Register medications with dose amounts and recurrence frequency |
| **Dose calendar** | Auto-generated scheduled doses grouped by day; 1–7 day window |
| **Dose confirmation** | Mark doses as taken; recalculates future schedule from actual time |
| **Medication history** | Immutable log of all confirmed doses |
| **Medical appointments** | Schedule, confirm, and track doctor visits |
| **History page** | Filterable view of all food and medication events with statistics |

---

## Project Structure

```
TrackingApp.MAUI/
│
├── TrackingApp/                        # MAUI UI project (net10.0-android/ios/maccatalyst/windows)
│   ├── MauiProgram.cs                  # App bootstrap & DI
│   ├── App.xaml / App.xaml.cs          # Application class, creates Window → AppShell
│   ├── AppShell.xaml / .cs             # Shell tab-bar navigation
│   ├── MainPage.xaml / .cs             # Primary page (food entry + medication scheduling)
│   ├── HistoryPage.xaml / .cs          # History page (filterable log)
│   │
│   ├── ViewModels/
│   │   ├── MainViewModel.cs            # ~1500 lines — main screen logic, all commands
│   │   └── HistoryViewModel.cs         # ~500 lines — history filtering & statistics
│   │
│   ├── Services/
│   │   ├── AppServices.cs              # Singleton DataService accessor
│   │   └── DatabaseService.cs          # SQLite implementation of IDatabaseService
│   │
│   ├── Converters/                     # IValueConverter implementations (10 files)
│   ├── Extensions/
│   │   └── MedicationDoseExtensions.cs
│   ├── Platforms/                      # Android / iOS / Windows / MacCatalyst entry points
│   └── Resources/
│       ├── AppIcon/                    # App icon (SVG source → PNG at build)
│       ├── Fonts/                      # OpenSans-Regular, OpenSans-Semibold
│       ├── Styles/                     # Colors.xaml, Styles.xaml
│       ├── Images/                     # PNG image assets
│       └── Raw/                        # Raw assets (served as-is)
│
├── TrackingApp.Core/                   # Class library (net10.0 — no MAUI dependencies)
│   ├── Models/
│   │   ├── FoodEntry.cs
│   │   ├── Medication.cs
│   │   ├── MedicationDose.cs
│   │   ├── MedicationHistory.cs
│   │   ├── MedicationEvent.cs          # Unified view: history + pending dose
│   │   └── MedicalAppointment.cs
│   ├── Services/
│   │   ├── IDatabaseService.cs         # Interface for all data access operations
│   │   └── DataService.cs              # Core business logic (ObservableCollections)
│   └── Helpers/
│       └── NumericParser.cs            # Locale-aware numeric parser (dot/comma)
│
└── TrackingApp.Tests/                  # xUnit test project (net10.0)
    ├── Services/
    │   └── DataServiceTests.cs         # 40+ tests covering dose logic
    ├── Helpers/
    │   └── NumericParserTests.cs
    ├── Models/                         # (reserved for model tests)
    └── Mocks/
        └── MockDatabaseService.cs      # In-memory IDatabaseService for tests
```

---

## Entry Points

| Entry Point | Path | Description |
|-------------|------|-------------|
| `MauiProgram.CreateMauiApp()` | `TrackingApp/MauiProgram.cs` | Called by the platform host; builds the MAUI app |
| `App.CreateWindow()` | `TrackingApp/App.xaml.cs` | Returns `Window(new AppShell())` |
| `AppShell` | `TrackingApp/AppShell.xaml` | Tab bar: **Inicio** (`MainPage`) and **Historial** (`HistoryPage`) |
| `MainPage.OnAppearing()` | `TrackingApp/MainPage.xaml.cs` | Reloads all data from DB on every navigation |

---

## Key Services

### `DataService` (`TrackingApp.Core/Services/DataService.cs`)

The central business-logic service. Holds all `ObservableCollection<T>` instances that ViewModels bind to.

| Responsibility | Methods |
|----------------|---------|
| Food CRUD | `AddFoodEntryAsync`, `UpdateFoodEntryAsync`, `DeleteFoodEntryAsync` |
| Medication CRUD | `AddMedicationAsync`, `UpdateMedicationAsync`, `DeleteMedicationAsync` |
| Dose generation | `GenerateDosesForMedicationAsync(medication, days)` |
| Dose confirmation | `ConfirmDoseAndRecalculateAsync(dose, days)` ← **critical path** |
| Dose recalculation | `RecalculateNextDosesFromConfirmedTimeAsync`, `RecalculateNextDosesFromLastConfirmedAsync` |
| Unified events | `RebuildCombinedEvents()` — merges history + pending into `CombinedMedicationEvents` |
| Appointments | `AddAppointmentAsync`, `UpdateAppointmentAsync`, `DeleteAppointmentAsync`, `ConfirmAppointmentAsync` |
| Data reload | `ReloadAllDataAsync()` — called on page appear |
| Data reset | `ResetAllDataAsync()` — deletes ALL data |

### `DatabaseService` (`TrackingApp/Services/DatabaseService.cs`)

SQLite persistence layer implementing `IDatabaseService`. Uses `SQLiteAsyncConnection` (sqlite-net-pcl). Database file: `FileSystem.AppDataDirectory/tracking.db3`.

Tables created at startup:
- `FoodEntry`
- `Medication`
- `MedicationDose`
- `MedicationHistory`
- `MedicalAppointment`

Pattern: lazy `InitializeAsync()` call before each operation, singleton via `DatabaseService.Instance`.

### `AppServices` (`TrackingApp/Services/AppServices.cs`)

Static wrapper that holds the singleton `DataService`, initialized with `DatabaseService.Instance`. ViewModels access services via `AppServices.DataService`.

---

## Data Models

| Model | SQLite Table | Key Fields |
|-------|-------------|------------|
| `FoodEntry` | `FoodEntry` | Id, FoodType, Amount, Unit, Time, UserType |
| `Medication` | `Medication` | Id, Name, Dose, FrequencyHours, FrequencyMinutes, FirstDoseTime, UserType |
| `MedicationDose` | `MedicationDose` | Id, MedicationId, ScheduledTime, ActualTime, IsConfirmed, IsEdited |
| `MedicationHistory` | `MedicationHistory` | Id, MedicationId, MedicationName, Dose, AdministeredTime, UserType |
| `MedicalAppointment` | `MedicalAppointment` | Id, Title, Description, AppointmentDate, Doctor, IsConfirmed |
| `MedicationEvent` | _(in-memory only)_ | Unified view combining `MedicationHistory` + `MedicationDose` |

---

## ViewModels

### `MainViewModel`

- Extends `ObservableObject` (CommunityToolkit.Mvvm pattern implied)
- Exposes all UI-bound collections and commands
- Accesses data exclusively through `AppServices.DataService`
- **Does not** hold its own state — delegates all persistence to `DataService`

Key collections exposed:
| Property | Type | Purpose |
|----------|------|---------|
| `FilteredFoodEntries` | `ObservableCollection<FoodEntry>` | Date-filtered food records |
| `FilteredMedications` | `ObservableCollection<Medication>` | All registered medications |
| `GroupedDoses` | Grouped by `DateTime.Date` | Dose calendar view |
| `PendingDoses` | `ObservableCollection<MedicationDose>` | Unconfirmed future doses |
| `ConfirmedDoses` | `ObservableCollection<MedicationDose>` | Already-confirmed doses |
| `FilteredMedicationHistory` | `ObservableCollection<MedicationHistory>` | Confirmed dose log |
| `FilteredAppointments` | `ObservableCollection<MedicalAppointment>` | Appointment list |

### `HistoryViewModel`

- History-only view: no dose confirmation, no medication creation
- Applies multi-dimensional filters (medication name, food type, unit, profile, date range)
- Computes statistics: total confirmed doses, total food entries, total food amount, most frequent food type

---

## Converters (10 total)

All converters implement `IValueConverter` and are registered globally in `App.xaml`:

| Converter | Maps |
|-----------|------|
| `BoolToTextConverter` | `true` → `"✓"` / `false` → `"Confirmar"` |
| `BoolToColorConverter` | `true` (history) → Green / `false` (pending) → Blue |
| `InverseBoolConverter` | Inverts boolean (bidirectional) |
| `BoolToConfirmButtonColorConverter` | Button color for confirm state |
| `BoolToHistoryBackgroundConverter` | Background for history row |
| `BoolToHistoryTextColorConverter` | Text color for history row |
| `BoolToNextDoseBackgroundConverter` | Highlight color for next dose |
| `BoolToNextDoseColorConverter` | Text color for next dose |
| `BoolToNextDoseTextConverter` | Label for next dose row |
| `StringIsNotNullOrEmptyConverter` | `!string.IsNullOrEmpty(value)` |

---

## Tests

40+ unit tests in `TrackingApp.Tests/` covering:
- Dose confirmation flow (saves history, deletes original dose, recalculates)
- Dose generation & midnight crossings
- Dose status computation (Confirmado / Atrasado / Próximo / Programado)
- Frequency → minutes calculations
- Recalculation from confirmed or last-history time
- `NumericParser` accepting dots and commas

See [ARCHITECTURE-MAP.md](ARCHITECTURE-MAP.md) for data flow diagrams.
