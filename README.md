# Tracking App - Aplicación MAUI para Android

> [!IMPORTANT]
> **🛑 PUNTO DE ESTABILIDAD (v1.16)**
> Este código representa la versión estable y segura del proyecto.
> **NO** mezclar con código de versiones posteriores inestables sin pasar por tests.
> Si vas a crear una nueva rama, hazlo a partir del tag: `stable-v1.16-baseline`.
> Ver detalles en: [ESTADO_ESTABLE.md](ESTADO_ESTABLE.md)

## Descripción
Aplicación móvil para tracking de alimentos y medicamentos para bebés, adultos y animales.

## Características
- ✅ Registro de alimentos con cantidad, unidad y hora
- ✅ Registro de medicamentos con dosis y frecuencia
- ✅ Calendario de medicamentos organizado por día
- ✅ Estados visuales (próximo, atrasado, confirmado)
- ✅ Edición de horarios de dosis
- ✅ Filtrado por tipo de usuario (Bebé, Adulto, Animal)
- ✅ Soporte para múltiples medicamentos con filtro individual
- ✅ Persistencia de datos con SQLite
- ✅ Historial de registros con página dedicada

## Requisitos
- .NET 10 SDK o superior
- Visual Studio 2022 (v17.12+) con carga de trabajo de MAUI
- Para Android: Android SDK (API 21 o superior)

## Estructura del Proyecto
```
TrackingApp.MAUI/
├── TrackingApp/              # Proyecto MAUI principal
│   ├── ViewModels/           # MVVM ViewModels (ObservableObject)
│   │   ├── MainViewModel.cs
│   │   └── HistoryViewModel.cs
│   ├── Services/             # Servicios de infraestructura MAUI
│   │   ├── AppServices.cs
│   │   └── DatabaseService.cs
│   ├── Converters/           # Convertidores XAML
│   ├── Platforms/            # Código específico por plataforma
│   ├── Resources/            # Imágenes, fuentes, assets
│   ├── MainPage.xaml         # Vista principal
│   ├── HistoryPage.xaml      # Vista de historial
│   └── AppShell.xaml         # Navegación Shell
├── TrackingApp.Core/         # Lógica de negocio (sin deps MAUI)
│   ├── Models/               # Entidades del dominio
│   │   ├── FoodEntry.cs
│   │   ├── Medication.cs
│   │   └── MedicationDose.cs
│   └── Services/             # Servicios de negocio (testeables)
│       └── DataService.cs
└── TrackingApp.Tests/        # Tests unitarios (xUnit)
    ├── Services/
    │   └── DataServiceTests.cs
    └── ViewModels/
```

## Instalación

### 1. Verificar instalación de .NET MAUI
```powershell
dotnet workload install maui
```

### 2. Compilar la solución completa
```powershell
dotnet build TrackingApp.MAUI.sln
```

### 3. Ejecutar en Android (Emulador o dispositivo)
```powershell
# Para emulador Android
dotnet build -t:Run -f net10.0-android

# Para dispositivo físico conectado por USB
dotnet build -t:Run -f net10.0-android /p:AndroidDebugUseFastDeploy=true
```

### 4. Ejecutar los tests
```powershell
dotnet test TrackingApp.Tests/TrackingApp.Tests.csproj
```

## Compilar APK para distribución
```powershell
# Usando el script incluido
.\Build-APK.ps1

# O manualmente
dotnet publish -f net10.0-android -c Release
```

El APK se generará en: `bin\Release\net10.0-android\publish\`

## Uso de la Aplicación

### Registrar Alimentos
1. Selecciona el tipo de usuario (Bebé/Adulto/Animal)
2. Ingresa el tipo de alimento (ej: "Leche")
3. Ingresa la cantidad y selecciona la unidad (oz, ml, g, etc.)
4. Selecciona la hora
5. Presiona "Agregar Alimento"

### Registrar Medicamentos
1. Ingresa el nombre del medicamento
2. Ingresa la dosis (ej: "5ml")
3. Ingresa la frecuencia en horas (ej: "6" para cada 6 horas)
4. Selecciona la hora de la primera dosis
5. Presiona "Agregar Medicamento"

### Calendario de Medicamentos
- El calendario muestra las próximas dosis organizadas por día
- Puedes filtrar por días (1, 2, 3, 5, 7 días)
- Puedes filtrar por medicamento específico
- Estados visuales:
  - **Verde**: Dosis confirmada
  - **Amarillo**: Próxima dosis (menos de 30 min)
  - **Rojo**: Dosis atrasada (más de 30 min)
  - **Gris**: Dosis programada

### Confirmar/Editar Dosis
- Presiona "Confirmar" para marcar una dosis como administrada
- Presiona "Editar" para cambiar la hora de una dosis específica

## Problemas Comunes

### Error al compilar para Android
Asegúrate de tener instalado Android SDK:
```powershell
dotnet workload repair
dotnet workload install android
```

### No se detecta el emulador
Abre Android Studio y verifica que tengas un AVD (Android Virtual Device) creado.

### Error de permisos en dispositivo físico
Habilita "Depuración USB" en las opciones de desarrollador de tu dispositivo Android.

## Próximas Mejoras
- [ ] Notificaciones push para recordatorios de dosis
- [ ] Gráficos de consumo y estadísticas
- [ ] Exportar historial a PDF
- [ ] Soporte para múltiples perfiles (varios bebés/mascotas)
- [ ] Integración con APIs de salud (Google Fit, Apple Health)

## Autor
Aplicación creada para tracking de alimentos y medicamentos.
