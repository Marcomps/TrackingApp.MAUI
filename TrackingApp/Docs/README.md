# 📚 Base de Conocimiento - TrackingApp# Tracking App - Aplicación MAUI para Android



Esta carpeta contiene toda la documentación del proyecto TrackingApp, una aplicación de seguimiento de alimentación y medicamentos para bebés.## Descripción

Aplicación móvil para tracking de alimentos y medicamentos para bebés, adultos y animales.

## 📋 Índice de Documentación

## Características

### Documentación Principal- ✅ Registro de alimentos con cantidad, unidad y hora

- `README.md` - Este archivo, índice de la documentación- ✅ Registro de medicamentos con dosis y frecuencia

- `INDICE.md` - Índice detallado de todas las funcionalidades- ✅ Calendario de medicamentos organizado por día

- ✅ Estados visuales (próximo, atrasado, confirmado)

### Guías de Desarrollo- ✅ Edición de horarios de dosis

- `GENERAR_APK.md` - Instrucciones para generar APK de producción- ✅ Filtrado por tipo de usuario (Bebé, Adulto, Animal)

- `PUBLICAR_PLAY_STORE.md` - Guía para publicar en Play Store- ✅ Soporte para múltiples medicamentos con filtro individual

- `COMANDOS_RAPIDOS.md` - Comandos útiles para desarrollo

## Requisitos

## 🏗️ Arquitectura del Proyecto- .NET 8 SDK o superior

- Visual Studio 2022 con carga de trabajo de MAUI

```- Para Android: Android SDK (API 21 o superior)

TrackingApp/

├── Models/              - Entidades de datos## Instalación

├── ViewModels/          - Lógica de presentación MVVM

├── Views/               - Páginas XAML (UI)### 1. Verificar instalación de .NET MAUI

├── Services/            - Servicios y lógica de negocio```powershell

├── Converters/          - Conversores para bindingdotnet workload install maui

├── Resources/           - Recursos visuales```

├── Docs/                - Esta carpeta

└── Platforms/           - Código específico### 2. Compilar el proyecto

``````powershell

cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"

## 🧪 Pruebas Unitarias (25+ Tests)dotnet build

```

```

TrackingApp.Tests/### 3. Ejecutar en Android (Emulador o dispositivo)

├── Models/              - Tests de entidades```powershell

├── Services/            - Tests de lógica# Para emulador Android

├── ViewModels/          - Tests de VMsdotnet build -t:Run -f net8.0-android

└── Helpers/             - Tests de utilidades

```# Para dispositivo físico conectado por USB

dotnet build -t:Run -f net8.0-android /p:AndroidDebugUseFastDeploy=true

### Ejecutar Pruebas```

```bash

dotnet test## Compilar APK para distribución

``````powershell

dotnet publish -f net8.0-android -c Release

## 📱 Funcionalidades```



1. **Medicamentos** - Gestión completa con cálculo automático de dosisEl APK se generará en: `bin\Release\net8.0-android\publish\`

2. **Alimentación** - Registro con filtros avanzados

3. **Historial** - Vista combinada con estadísticas## Estructura del Proyecto

4. **UI/UX** - Material Design con TimePickers visuales```

TrackingApp/

## 🔧 Stack Tecnológico├── Models/              # Modelos de datos

│   ├── FoodEntry.cs

- .NET MAUI 9.0│   ├── Medication.cs

- SQLite│   └── MedicationDose.cs

- MVVM Pattern├── ViewModels/          # Lógica de presentación

- xUnit + FluentAssertions + Moq│   └── MainViewModel.cs

├── Views/               # Vistas XAML

---│   └── MainPage.xaml

**Versión:** 1.30 | **Fecha:** Octubre 2025├── Services/            # Servicios de datos

│   └── DataService.cs
└── Converters/          # Convertidores XAML
    └── BoolToTextConverter.cs
```

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
- [ ] Persistencia de datos (SQLite)
- [ ] Notificaciones push para recordatorios
- [ ] Gráficos de consumo
- [ ] Exportar historial a PDF
- [ ] Soporte para múltiples perfiles (varios bebés/mascotas)

## Autor
Aplicación creada para tracking de alimentos y medicamentos.
