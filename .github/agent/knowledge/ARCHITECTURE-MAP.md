# TrackingApp.MAUI — Architecture Map

> **TL;DR**: The solution follows a strict three-layer MVVM + Service architecture. The View layer (XAML) binds to ViewModels; ViewModels delegate all persistence and business logic to `DataService`; `DataService` depends on `IDatabaseService` (implemented by `DatabaseService` using SQLite). A clean boundary exists between the MAUI UI project and the testable `TrackingApp.Core` library.

**Updated:** 2026-03-23 00:00 -0600

---

## Layer Overview

```
┌─────────────────────────────────────────────────────────────┐
│                   TrackingApp (MAUI UI)                      │
│                                                              │
│  ┌─────────────┐    ┌──────────────────┐                    │
│  │  Views       │◄──│   ViewModels      │                    │
│  │  (XAML)      │   │  MainViewModel    │                    │
│  │  MainPage    │   │  HistoryViewModel │                    │
│  │  HistoryPage │   └────────┬─────────┘                    │
│  └─────────────┘            │ AppServices.DataService        │
│                              │                               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              AppServices (static singleton)          │    │
│  └─────────────────────────────────────────────────────┘    │
│                              │                               │
│  ┌───────────────────────────▼───────────────────────────┐  │
│  │  DatabaseService (SQLite)  implements IDatabaseService │  │
│  └───────────────────────────────────────────────────────┘  │
└──────────────────────────────┬──────────────────────────────┘
                               │ project reference
┌──────────────────────────────▼──────────────────────────────┐
│                  TrackingApp.Core (net10.0)                  │
│                                                              │
│  ┌────────────┐   ┌────────────────┐   ┌─────────────────┐  │
│  │  Models    │   │  DataService   │   │  IDatabaseService│  │
│  │  FoodEntry │   │  (business     │──►│  (interface)    │  │
│  │  Medication│   │   logic)       │   └─────────────────┘  │
│  │  Dose      │   └────────────────┘                        │
│  │  History   │   ┌────────────────┐                        │
│  │  Event     │   │  NumericParser │                        │
│  │  Appt.     │   │  (helper)      │                        │
│  └────────────┘   └────────────────┘                        │
└─────────────────────────────────────────────────────────────┘
                               │ project reference
┌──────────────────────────────▼──────────────────────────────┐
│                  TrackingApp.Tests (net10.0)                 │
│                                                              │
│  ┌──────────────────┐   ┌────────────────────────────────┐  │
│  │  DataServiceTests│   │  MockDatabaseService           │  │
│  │  NumericParser   │   │  (in-memory IDatabaseService)  │  │
│  │  Tests           │   └────────────────────────────────┘  │
│  └──────────────────┘                                       │
└─────────────────────────────────────────────────────────────┘
```

---

## Layers

### 1. View Layer — `TrackingApp/` (XAML Pages)

- **Files**: `MainPage.xaml`, `HistoryPage.xaml`
- **Rules**: No logic in code-behind except lifecycle hooks (`OnAppearing`). All bindings use `x:DataType` for compile-time safety.
- **Navigation**: Shell tab-bar (`AppShell.xaml`) — no `NavigationPage` or `TabbedPage`.
- **Dependency on**: ViewModels only (via `BindingContext`)

### 2. ViewModel Layer — `TrackingApp/ViewModels/`

- **Files**: `MainViewModel.cs`, `HistoryViewModel.cs`
- **Pattern**: MVVM with `ObservableObject` and property-change notification
- **Dependency on**: `AppServices.DataService` (indirectly `DataService` from Core)
- **No** MAUI lifecycle dependencies — could be tested if `AppServices` were injectable

### 3. Infrastructure Service — `TrackingApp/Services/`

| File | Responsibility |
|------|---------------|
| `AppServices.cs` | Static singleton holder for `DataService`. Bridges MAUI and Core. |
| `DatabaseService.cs` | SQLite implementation of `IDatabaseService`. Lazy-initializes DB. |

### 4. Business Logic — `TrackingApp.Core/Services/`

| File | Responsibility |
|------|---------------|
| `DataService.cs` | All business operations on `ObservableCollection<T>` instances used for UI binding |
| `IDatabaseService.cs` | Contract for all CRUD operations — injectable for testing |

### 5. Domain Models — `TrackingApp.Core/Models/`

Pure C# POCOs. SQLite-decorated but contain no business logic beyond computed `[Ignore]` display properties.

### 6. Test Layer — `TrackingApp.Tests/`

| File | Responsibility |
|------|---------------|
| `DataServiceTests.cs` | 40+ tests for `DataService` business logic |
| `NumericParserTests.cs` | Parsing edge cases |
| `MockDatabaseService.cs` | In-memory substitute for `IDatabaseService` |

---

## Main Workflows

### A. Adding a Food Entry

```
User fills form → MainViewModel.AddFoodCommand
  → NumericParser.TryParseDouble(FoodAmount)
  → DataService.AddFoodEntryAsync(entry)
    → DatabaseService.SaveFoodEntryAsync(entry)
    → FoodEntries.Insert(0, entry)  [ObservableCollection → UI updates]
```

### B. Adding a Medication

```
User fills form → MainViewModel.AddMedicationCommand
  → NumericParser validates frequency hours/minutes
  → DataService.AddMedicationAsync(medication, days)
    → DatabaseService.SaveMedicationAsync(medication)
    → Medications.Add(medication)
    → DataService.GenerateDosesForMedicationAsync(medication, days)
      → Loop: FirstDoseTime + N * Frequency while < endDate
      → DatabaseService.SaveDoseAsync(dose) for each
      → MedicationDoses.Add(dose)
    → RebuildCombinedEvents()
```

### C. Confirming a Dose (Critical Flow)

```
User taps "Confirmar" → MainViewModel.ConfirmDoseCommand
  → DataService.ConfirmDoseAndRecalculateAsync(dose, days)
    1. dose.IsConfirmed = true; dose.ActualTime = DateTime.Now
    2. Create MedicationHistory record → DatabaseService.SaveMedicationHistoryAsync
    3. DatabaseService.DeleteDoseAsync(dose) + MedicationDoses.Remove(dose)
    4. RecalculateNextDosesFromConfirmedTimeAsync(medicationId, DateTime.Now, days)
       → Delete all remaining pending doses for this medication
       → Generate new doses: confirmedTime + N * Frequency while < endDate
       → DatabaseService.SaveDoseAsync + MedicationDoses.Add for each
    5. RebuildCombinedEvents()
       → Merges MedicationHistory + unconfirmed MedicationDoses
       → CombinedMedicationEvents = ordered merged list
```

> **Key invariant**: Confirmed doses are permanently removed from `MedicationDoses`. Only unconfirmed doses appear in the calendar. History is permanent.

### D. Page Navigation (Shell)

```
AppShell.xaml defines two ShellContent tabs:
  - Route "MainPage"   → MainPage
  - Route "HistoryPage" → HistoryPage

MainPage.OnAppearing():
  → AppServices.DataService.ReloadAllDataAsync()  (reloads all DB data)
  → MainViewModel.NotifyAllDataChanged()          (forces collection refresh)
```

### E. Data Reload on App Resume

```
MainPage.OnAppearing() [called every tab switch to MainPage]
  → DataService.ReloadAllDataAsync()
    → DatabaseService.GetAllFoodEntriesAsync() → FoodEntries.Clear + refills
    → DatabaseService.GetAllMedicationsAsync() → Medications.Clear + refills
    → DatabaseService.GetAllDosesAsync()       → only UNCONFIRMED doses → MedicationDoses
    → DatabaseService.GetAllMedicationHistoryAsync() → MedicationHistory
    → DatabaseService.GetAllAppointmentsAsync() → Appointments
  → RebuildCombinedEvents()
```

---

## Key Interfaces

### `IDatabaseService` (`TrackingApp.Core/Services/IDatabaseService.cs`)

The primary extension point. Swap `DatabaseService` for any implementation (e.g., REST API, other local DB):

```csharp
public interface IDatabaseService
{
    // FoodEntry
    Task<List<FoodEntry>> GetAllFoodEntriesAsync();
    Task<int> SaveFoodEntryAsync(FoodEntry entry);
    Task<int> DeleteFoodEntryAsync(FoodEntry entry);

    // Medication
    Task<List<Medication>> GetAllMedicationsAsync();
    Task<Medication?> GetMedicationAsync(int id);
    Task<int> SaveMedicationAsync(Medication medication);
    Task<int> DeleteMedicationAsync(Medication medication);

    // MedicationDose
    Task<List<MedicationDose>> GetAllDosesAsync();
    Task<List<MedicationDose>> GetDosesByMedicationAsync(int medicationId);
    Task<int> SaveDoseAsync(MedicationDose dose);
    Task<int> DeleteDoseAsync(MedicationDose dose);
    Task<int> DeleteDosesByMedicationAsync(int medicationId);

    // MedicationHistory
    Task<List<MedicationHistory>> GetAllMedicationHistoryAsync();
    Task<List<MedicationHistory>> GetMedicationHistoryByIdAsync(int medicationId);
    Task<int> SaveMedicationHistoryAsync(MedicationHistory history);
    Task<int> DeleteMedicationHistoryAsync(MedicationHistory history);

    // MedicalAppointment
    Task<List<MedicalAppointment>> GetAllAppointmentsAsync();
    Task<int> SaveAppointmentAsync(MedicalAppointment appointment);
    Task<int> DeleteAppointmentAsync(MedicalAppointment appointment);

    // Utilities
    Task<int> ClearAllDataAsync();
    Task<string> GetDatabasePathAsync();
    Task<long> GetDatabaseSizeAsync();
}
```

---

## Extension Points

| What to extend | How |
|----------------|-----|
| New tracking entity | Add model to `Core/Models`, add CRUD methods to `IDatabaseService`, implement in `DatabaseService`, add business logic to `DataService` |
| New platform support | Add platform folder under `TrackingApp/Platforms/`, add `TargetFramework` to `.csproj` |
| Remote sync / cloud | Implement `IDatabaseService` backed by a REST client, inject instead of `DatabaseService.Instance` |
| Push notifications | Add platform-specific notification service; inject via `MauiProgram.cs` DI container |
| New UI page | Add `ContentPage` + ViewModel, register route in `AppShell.xaml` via `<ShellContent>` or `Routing.RegisterRoute()` |
| Replace SQLite | Swap `DatabaseService` with another `IDatabaseService` implementation — `DataService` is unaffected |

---

## Security Notes

- Sensitive tokens must use `SecureStorage` — NOT `Preferences`.
- SQLite DB path is in `FileSystem.AppDataDirectory` (sandboxed, private to the app).
- Keystore password is currently embedded in `TrackingApp.csproj`. Move to environment variables or a CI secrets store before production.
- All user inputs (food type, medication name, dose) pass through ViewModel validation before reaching `DataService`.

---

## Cross-References

- [CODEBASE-OVERVIEW.md](CODEBASE-OVERVIEW.md) — Detailed file-level breakdown
- [DEPENDENCIES.md](DEPENDENCIES.md) — Full NuGet package list
- [MIDDLEWARE-PIPELINE.md](MIDDLEWARE-PIPELINE.md) — App startup and DI pipeline
