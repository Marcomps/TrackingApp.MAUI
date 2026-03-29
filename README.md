# TrackingApp — Aplicación MAUI de Salud y Nutrición

> [!IMPORTANT]
> **🛑 PUNTO DE ESTABILIDAD (v1.16)**
> Este código representa la versión estable y segura del proyecto.
> **NO** mezclar con código de versiones posteriores inestables sin pasar por tests.
> Si vas a crear una nueva rama, hazlo a partir del tag: `stable-v1.16-baseline`.
> Ver detalles en: [ESTADO_ESTABLE.md](ESTADO_ESTABLE.md)

## Descripción

Aplicación móvil .NET 10 MAUI para Android, iOS y Windows orientada al seguimiento de salud y nutrición de bebés, adultos y animales. Implementa el patrón **MVVM + Capa de Servicios** con persistencia local SQLite. La lógica de negocio reside en `TrackingApp.Core` (librería sin dependencias MAUI) para garantizar cobertura completa de tests.

---

## Estado de funcionalidades

| Módulo / Característica | Estado | Notas |
|---|---|---|
| Registro de alimentos (cantidad, unidad, hora) | ✅ Implementado | v1.16 estable |
| Registro de medicamentos con dosis y frecuencia | ✅ Implementado | v1.16 estable |
| Calendario de medicamentos con estados visuales | ✅ Implementado | v1.16 estable |
| Confirmación de dosis + recálculo automático | ✅ Implementado | v1.16 estable |
| Historial de registros con filtros | ✅ Implementado | v1.16 estable |
| Citas médicas (crear, confirmar) | ✅ Implementado | v1.16 estable |
| Persistencia SQLite | ✅ Implementado | v1.16 estable |
| Perfiles completos (nombre, foto, datos) | ✅ Implementado | v2.0 RF-001 |
| Módulo Crecimiento (peso, talla, IMC) | ✅ Implementado | v2.0 RF-002 |
| Servicio centralizado de unidades | ✅ Implementado | v2.0 RF-005 |
| Mejoras módulo Alimento | ✅ Implementado | v2.0 RF-003 |
| Mejoras módulo Citas | ✅ Implementado | v2.0 RF-004 |
| Gráficas de alimentos (tendencia semanal) | ✅ Implementado | v2.0 LiveCharts2 |
| Módulo Crecimiento (UI: lista + formulario) | ✅ Implementado | v2.0 RF-002 |
| Selectores de unidad (kg/lb/cm/in…) en formularios | ✅ Implementado | v2.0 RF-005 |
| Página de preferencias (métrico / imperial) | ✅ Implementado | v2.0 RF-005 |
| Sistema de perfiles (UI: lista + formulario + selección activo) | ✅ Implementado | v2.0 RF-001 |
| Índices SQLite para gráficas personalizadas | ✅ Implementado | v2.0 Performance |
| Notificaciones locales | 🔲 Pendiente | v2.0 |
| Gráficas de crecimiento | 🔲 Pendiente | v2.0 RF-002 |
| Exportar historial a PDF | 🔲 Pendiente | Fase siguiente |

---

## Arquitectura

```
TrackingApp.MAUI/
├── TrackingApp/                        # Proyecto MAUI principal (UI)
│   ├── ViewModels/                     # MVVM — ObservableObject, comandos
│   │   ├── MainViewModel.cs            # ~1500 líneas, lógica pantalla principal
│   │   ├── HistoryViewModel.cs         # ~500 líneas, filtros e historial
│   │   ├── AlimentoGraficasViewModel.cs # [v2.0] Gráficas de tendencia de alimentos
│   │   ├── CrecimientoViewModel.cs     # [v2.0] Lista de registros de crecimiento
│   │   ├── CrecimientoFormViewModel.cs # [v2.0] Formulario add/edit con selectores de unidad
│   │   ├── PerfilesViewModel.cs        # [v2.0] Lista de perfiles + cambio de perfil activo
│   │   ├── PerfilFormViewModel.cs      # [v2.0] Formulario crear/editar perfil
│   │   └── PreferenciasViewModel.cs    # [v2.0] Preferencia de sistema métrico/imperial
│   ├── Services/                       # Servicios de infraestructura MAUI
│   │   ├── AppServices.cs              # Singleton DataService accesible globalmente
│   │   └── DatabaseService.cs          # Implementación SQLite de IDatabaseService
│   ├── Converters/                     # 10 convertidores IValueConverter
│   ├── Platforms/                      # Android / iOS / Windows / MacCatalyst
│   ├── Resources/
│   │   ├── Styles/                     # Colors.xaml, Styles.xaml
│   │   ├── Fonts/                      # OpenSans Regular y Semibold
│   │   └── Images/                     # Assets PNG
│   ├── MainPage.xaml                   # Pantalla principal
│   ├── HistoryPage.xaml                # Historial
│   ├── AlimentoGraficasPage.xaml       # [v2.0] Gráficas de alimentos (LiveCharts2)
│   ├── CrecimientoPage.xaml            # [v2.0] Lista de registros de crecimiento
│   ├── CrecimientoFormPage.xaml        # [v2.0] Formulario nuevo/editar registro
│   ├── PerfilesPage.xaml               # [v2.0] Lista de perfiles y selector de perfil activo
│   ├── PerfilFormPage.xaml             # [v2.0] Formulario crear/editar perfil
│   ├── PreferenciasPage.xaml           # [v2.0] Configuración de unidades (métrico/imperial)
│   └── AppShell.xaml                   # Navegación Shell Tab-Bar
│
├── TrackingApp.Core/                   # Librería .NET 10 (sin dependencias MAUI)
│   ├── Models/
│   │   ├── AppEnums.cs                 # Todos los enums del dominio
│   │   ├── FoodEntry.cs
│   │   ├── Medication.cs
│   │   ├── MedicationDose.cs
│   │   ├── MedicationHistory.cs
│   │   ├── MedicationEvent.cs
│   │   ├── MedicalAppointment.cs
│   │   ├── Perfil.cs                   # [v2.0] Perfil de usuario completo
│   │   └── RegistroCrecimiento.cs      # [v2.0] Registro de peso/talla
│   ├── Services/
│   │   ├── IDatabaseService.cs         # Contrato de acceso a datos
│   │   ├── DataService.cs              # Lógica de negocio + ObservableCollections
│   │   ├── IUnitService.cs             # [v2.0] Contrato de conversión de unidades
│   │   └── UnitService.cs              # [v2.0] Conversiones métricas/imperiales
│   └── Helpers/
│       └── NumericParser.cs            # Parseo numérico (punto y coma como decimal)
│
└── TrackingApp.Tests/                  # Tests unitarios xUnit (net10.0)
    ├── Services/
    │   └── DataServiceTests.cs         # 40+ tests de lógica de dosis
    ├── Helpers/
    │   └── NumericParserTests.cs
    └── Mocks/
        └── MockDatabaseService.cs      # IDatabaseService en memoria para tests
```

> **Regla clave**: `TrackingApp.Core` **nunca** debe referenciar ensamblados MAUI. Toda la lógica de negocio testeable vive ahí.

---

## Requisitos

| Herramienta | Versión |
|-------------|---------|
| .NET SDK | 10.0+ |
| Visual Studio | 2022 v17.12+ con carga de trabajo MAUI |
| Android SDK | API 21+ (Android 5.0+) |

---

## Instalación y compilación

```powershell
# Instalar o reparar workload MAUI
dotnet workload install maui
dotnet workload repair

# Compilar toda la solución
dotnet build TrackingApp.MAUI.sln

# Compilar solo Android (debug)
dotnet build TrackingApp/TrackingApp.csproj -f net10.0-android
```

## Ejecutar en dispositivo / emulador

```powershell
# Emulador Android
dotnet build -t:Run -f net10.0-android

# Dispositivo físico (deploy rápido)
dotnet build -t:Run -f net10.0-android /p:AndroidDebugUseFastDeploy=true
```

## Tests

```powershell
# Ejecutar todos los tests unitarios
dotnet test TrackingApp.Tests/TrackingApp.Tests.csproj

# Con cobertura de código
dotnet test TrackingApp.Tests/TrackingApp.Tests.csproj --collect:"XPlat Code Coverage"
```

Los tests usan `MockDatabaseService` (en memoria) — no requieren emulador ni dispositivo.

## Publicar APK

```powershell
# Script incluido (firma con keystore configurado)
.\Build-APK.ps1

# Publicación manual
dotnet publish TrackingApp/TrackingApp.csproj -f net10.0-android -c Release
```

El AAB firmado se genera en: `publish_output/com.trackingapp.nutrition-Signed.aab`

---

## Uso de la Aplicación

### Registrar Alimentos
1. Selecciona el perfil activo
2. Ingresa el tipo de alimento (ej: "Leche")
3. Ingresa la cantidad y selecciona la unidad (oz, ml, g, etc.)
4. Selecciona la hora y presiona "Agregar Alimento"

### Ver Gráficas de Alimentos
1. En la pantalla principal, pulsa **📊 Ver Gráficas de Alimentos**
2. Selecciona el período: Hoy / 7 días / 30 días
3. La gráfica muestra tendencias por tipo: Fórmula, Lactancia y Sólidos

### Registrar Crecimiento
1. En la pantalla principal, pulsa **📈 Ver registros de crecimiento**
2. Pulsa **+** para agregar un nuevo registro
3. Ingresa el peso y la talla — selecciona la unidad deseada con el Picker (kg/g/lb/oz y cm/in/ft/m)
4. El IMC se calcula automáticamente en tiempo real
5. Pulsa **Guardar** — los valores se almacenan siempre en unidades base (g y cm)

### Gestionar Perfiles
1. En la pantalla principal, pulsa **👤 Ver y gestionar perfiles** (sección Perfiles)
2. Los perfiles existentes se muestran en tarjetas. Toca uno para **marcarlo como activo** (borde azul)
3. Desliza una tarjeta hacia la izquierda para **Editar** o hacia la derecha para **Eliminar**
4. Pulsa **+** en la barra superior para crear un nuevo perfil
5. En el formulario define: nombre, tipo (Bebé/Adulto), sexo, fecha de nacimiento, semanas de gestación (solo bebés), sistema de unidades y notas

### Configurar Unidades por Defecto
1. En la pantalla principal, pulsa **⚖️ Unidades de medida** (sección Configuración)
2. Elige **Métrico** o **Imperial**
3. Pulsa **Guardar preferencia** — el formulario de Crecimiento usará esas unidades por defecto

### Registrar Medicamentos
1. Ingresa el nombre y dosis del medicamento (ej: "5ml")
2. Define la frecuencia en horas y/o minutos
3. Selecciona la hora de la primera dosis
4. Presiona "Agregar Medicamento"

### Calendario de Medicamentos
- Estados visuales de dosis:
  - **Verde**: Dosis confirmada
  - **Amarillo**: Próxima (menos de 30 min)
  - **Rojo**: Atrasada (más de 30 min)
  - **Gris**: Programada
- Filtros: por días (1–7) y por medicamento específico
- Confirmar → recalcula automáticamente las dosis futuras desde la hora real de administración

---

## Documentación técnica (base de conocimientos)

Generada automáticamente en `.github/agent/knowledge/`:

| Documento | Contenido |
|-----------|-----------|
| [README.md](.github/agent/knowledge/README.md) | Guía de uso del código, comandos |
| [CODEBASE-OVERVIEW.md](.github/agent/knowledge/CODEBASE-OVERVIEW.md) | Estructura, servicios, modelos, ViewModels |
| [DEPENDENCIES.md](.github/agent/knowledge/DEPENDENCIES.md) | Paquetes NuGet con versiones y propósito |
| [ARCHITECTURE-MAP.md](.github/agent/knowledge/ARCHITECTURE-MAP.md) | Capas, flujos de trabajo, interfaces |
| [MIDDLEWARE-PIPELINE.md](.github/agent/knowledge/MIDDLEWARE-PIPELINE.md) | Pipeline de arranque MAUI paso a paso |

---

## Solución de problemas comunes

### Error al compilar para Android
```powershell
dotnet workload repair
dotnet workload install android
```

### No se detecta el emulador
Abre Android Studio → AVD Manager → verifica que haya un Android Virtual Device creado y en ejecución.

### Error de permisos en dispositivo físico
Habilita **Depuración USB** en las opciones de desarrollador del dispositivo.

---

## Requerimientos v2.0 (en implementación)

Ver documento completo: `TrackingApp_Requerimientos_v2.md`

| # | RF | Prioridad | Descripción |
|---|---|---|---|
| 1 | RF-005 | ✅ Listo | Servicio centralizado de unidades (IUnitService + UnitService) |
| 2 | RF-001 | ✅ Listo | Sistema de perfiles completos (Perfil model + CRUD + UI) |
| 3 | RF-002 | ✅ Listo | Módulo Crecimiento (peso, talla, IMC — data layer + UI) |
| 4 | RF-005 | ✅ Listo | Selectores de unidad en formularios + PreferenciasPage |
| 5 | RF-003 | ✅ Listo | Mejoras módulo Alimento — AlimentoListPage + AlimentoFormPage (TipoAlimentacion, PerfilId, campos condicionales) |
| 6 | RF-004 | ✅ Listo | Mejoras módulo Citas — CitasListPage + CitaFormPage (Categoria, Estado, NotasPostCita, Recordatorio, PerfilId) |
| 7 | RF-003 | ✅ Listo | Gráficas de alimentos (LiveCharts2 — tendencia semanal) |
| 8 | RF-001 | ✅ Listo | UI Sistema de perfiles (PerfilesPage + PerfilFormPage + activo) |
| 9 | — | ✅ Listo | Índices SQLite para gráficas personalizadas (12 índices) |
| 10 | RF-002 | 🔲 Pendiente | Gráficas de crecimiento (LiveCharts2) |
| 11 | — | 🟢 Baja | Notificaciones locales para citas y medicamentos |

---

## Autor
Aplicación para seguimiento de salud, alimentación y medicamentos — bebés, adultos y animales.
