# Tracking App - Aplicación MAUI para Android

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

## Requisitos
- .NET 8 SDK o superior
- Visual Studio 2022 con carga de trabajo de MAUI
- Para Android: Android SDK (API 21 o superior)

## Instalación

### 1. Verificar instalación de .NET MAUI
```powershell
dotnet workload install maui
```

### 2. Compilar el proyecto
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet build
```

### 3. Ejecutar en Android (Emulador o dispositivo)
```powershell
# Para emulador Android
dotnet build -t:Run -f net8.0-android

# Para dispositivo físico conectado por USB
dotnet build -t:Run -f net8.0-android /p:AndroidDebugUseFastDeploy=true
```

## Compilar APK para distribución
```powershell
dotnet publish -f net8.0-android -c Release
```

El APK se generará en: `bin\Release\net8.0-android\publish\`

## Estructura del Proyecto
```
TrackingApp/
├── Models/              # Modelos de datos
│   ├── FoodEntry.cs
│   ├── Medication.cs
│   └── MedicationDose.cs
├── ViewModels/          # Lógica de presentación
│   └── MainViewModel.cs
├── Views/               # Vistas XAML
│   └── MainPage.xaml
├── Services/            # Servicios de datos
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
