# GitHub Copilot Instructions: .NET MAUI

**Prompt Version**: 1.1.0

**Purpose**
Guide Copilot to safely assist with .NET MAUI code changes, adhering to MAUI architecture, conventions, and testing standards.

## 1. Mission
1. Safely accelerate work on this .NET MAUI codebase by generating code that follows our architecture and standards
2. Explain suggested changes and cross-platform implications (Android, iOS, Windows)
3. Point out risks (security, performance, breaking changes, platform-specific behavior)
4. Respect existing patterns over inventing new ones
5. Crate app for traking food, weight, measurements, medications, and other health data. The app should have a clean and intuitive UI, with features such as data visualization, reminders, and integration with health APIs.

## 2. Context You Should Load First
1. .github/agent/knowledge/*.md
2. README.md
3. TrackingApp/TrackingApp.csproj -- target frameworks and NuGet dependencies
4. TrackingApp.Core/TrackingApp.Core.csproj -- shared/testable core logic
5. TrackingApp.Tests/TrackingApp.Tests.csproj -- test project structure
6. TrackingApp/MauiProgram.cs -- DI registration and app bootstrap
7. TrackingApp/AppShell.xaml -- Shell navigation structure
8. TrackingApp/ViewModels/ -- MVVM ViewModels
9. TrackingApp.Core/Services/ -- business logic and data services
> If any are missing and you need them, ask for creation or clarification.

## 3. Architecture & Code Conventions
1. **Architecture Pattern**: MVVM + Service Layer
   - **ViewModels** (`TrackingApp/ViewModels/`): bind to Views; extend `ObservableObject`; no UI dependencies
   - **Services** (`TrackingApp/Services/`, `TrackingApp.Core/Services/`): business logic; injected via DI
   - **Models** (`TrackingApp.Core/Models/`): plain C# classes; no MAUI dependencies
   - **Views** (`TrackingApp/*.xaml`): XAML only; minimal code-behind; bind via `x:DataType`
2. **Testable core logic** lives in `TrackingApp.Core` (no MAUI dependencies)
3. **Naming**: PascalCase for classes; camelCase for locals and parameters
4. **Async methods**: suffix with `Async`; always `await`; never `.Result` or `.Wait()`
5. **Nullability**: treat warnings as errors; prefer non-nullable references
6. **DI**: register all services in `MauiProgram.cs`; inject via constructor

## 4. MAUI UI Rules (CRITICAL -- Never Violate)
1. **NEVER use `ListView`** -- use `CollectionView` instead
2. **NEVER use `TableView`** -- use `Grid` or `VerticalStackLayout`
3. **NEVER use `AndExpand`** layout options -- obsolete
4. **NEVER use `BackgroundColor`** -- use `Background` property
5. **NEVER place `ScrollView` or `CollectionView` inside `StackLayout`** -- breaks scrolling/virtualization
6. **NEVER reference images as `.svg`** -- use `.png` (SVG only for asset generation)
7. **NEVER mix `Shell` with `NavigationPage`/`TabbedPage`/`FlyoutPage`**
8. **NEVER use renderers** -- use Handlers instead
9. **Always use compiled bindings** -- set `x:DataType` on every `ContentPage`, `DataTemplate`, and `ContentView`
10. **Prefer `Border` over `Frame`** -- Frame is legacy (use only for shadows)
11. **Use `VerticalStackLayout`/`HorizontalStackLayout`** instead of `StackLayout`
12. **Shell navigation** is the standard: `await Shell.Current.GoToAsync("route")`

## 5. Testing Rules
1. Use xUnit for unit tests
2. Test project: `TrackingApp.Tests` -- tests for Core services and ViewModels
3. Only test code in `TrackingApp.Core` (no MAUI runtime required)
4. Do not mix unit and integration tests
5. Add unit tests for new logic added to `TrackingApp.Core`
6. Mock external dependencies (e.g., `IDataService`) with interfaces

## 6. Security & Secrets
1. **Never hardcode secrets or connection strings**
2. Use `SecureStorage` for sensitive data (tokens, credentials): `await SecureStorage.SetAsync("key", value)`
3. Use `Preferences` only for non-sensitive user settings
4. Sanitize logs; avoid logging PII
5. Always use HTTPS for external API calls
6. Validate all user inputs before processing

## 7. Generating Changes
1. Explain what you changed and why (inline comment or PR description)
2. Touch minimal files; avoid unrelated refactors
3. Follow existing patterns before adding new libraries or frameworks
4. Add or update unit tests when logic in `TrackingApp.Core` changes
5. Consider cross-platform behavior (Android, iOS) for any UI or platform API change

## 8. What NOT to Do
1. Do not use obsolete MAUI controls: `ListView`, `TableView`, `Frame` (for layout), `StackLayout` with `AndExpand`
2. Do not add MAUI dependencies to `TrackingApp.Core` -- it must remain testable without MAUI runtime
3. Do not create new architectural layers without approval
4. Do not change major package versions silently
5. Do not auto-fix formatting; let CI handle it
6. Do not use string-based bindings -- always use compiled bindings with `x:DataType`
7. Do not update UI from background threads -- use `MainThread.BeginInvokeOnMainThread()` or inject `IDispatcher`

## 9. Pull Request Checklist
- Builds succeed for all targets (`net10.0-android`, `net10.0-ios`, `net10.0-windows10.0.19041.0`)
- Tests in `TrackingApp.Tests` pass
- No MAUI anti-patterns introduced (see Section 4)
- Breaking changes documented
- No secrets or hardcoded values in code or config

## 10. Tools & Commands
- Build (all): `dotnet build TrackingApp.MAUI.sln`
- Build (Android): `dotnet build -t:Run -f net10.0-android`
- Test: `dotnet test TrackingApp.Tests/TrackingApp.Tests.csproj`
- Publish APK: see `Build-APK.ps1`
- Lint/analyzers: refer to `.editorconfig`

## 11. Agent Task & Knowledge Files
1. Knowledge summaries go in `.github/agent/knowledge/`
2. Task lists go in `.github/agent/tasks/`
> Append new knowledge to the right file; don't overwrite without reason.

## 12. How to Ask for Help
If you need more detail, ask:
"I see no docs for the navigation flow. Should I add one or skip documentation?"

## 13. Using MCP tools
1. If the user intent relates to Azure DevOps, make sure to prioritize Azure DevOps MCP server tools.
2. Assume all work is in the "<ADO_Project_Name>" ADO project.
3. When creating Tasks:
   - Always start the title with "Dev - "
   - Set the Activity to "Development"
   - Set the Area and Iteration to the same as the story
4. When creating Pull Requests:
   - Merge into `develop` or `dev` branch; if they do not exist, use `main`
   - Create a relevant title
   - Summarize commit messages as the description
