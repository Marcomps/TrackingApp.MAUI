# Guía Rápida - Ejecutar en Android

## Opción 1: Usar Visual Studio 2022 (Recomendado)

1. Abre Visual Studio 2022
2. Abre el proyecto: `File > Open > Project/Solution`
3. Selecciona: `c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp\TrackingApp.csproj`
4. En la barra superior, selecciona:
   - Framework: `net8.0-android`
   - Dispositivo: Selecciona un emulador Android o tu dispositivo físico
5. Presiona F5 o haz clic en el botón ▶ (Play/Debug)

## Opción 2: Línea de Comandos

### Paso 1: Instalar workload de Android (si no está instalado)
```powershell
dotnet workload install android
```

### Paso 2: Ver dispositivos Android disponibles
```powershell
dotnet build -t:InstallAndroidDependencies
adb devices
```

### Paso 3: Compilar y ejecutar
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet build -t:Run -f net8.0-android
```

## Opción 3: Crear Emulador Android

### Usando Android Studio:
1. Descarga e instala Android Studio
2. Abre Android Studio
3. Ve a: Tools > Device Manager
4. Haz clic en "Create Device"
5. Selecciona un dispositivo (ej: Pixel 5)
6. Descarga e instala una imagen del sistema (API 30 o superior recomendado)
7. Finaliza la creación del emulador
8. Inicia el emulador
9. Ejecuta el proyecto MAUI

### Usando línea de comandos:
```powershell
# Listar AVDs disponibles
emulator -list-avds

# Iniciar un emulador específico
emulator -avd <nombre_del_avd>
```

## Opción 4: Dispositivo Físico Android

### Paso 1: Habilitar Depuración USB en tu dispositivo
1. Ve a Configuración > Acerca del teléfono
2. Toca 7 veces en "Número de compilación" para habilitar opciones de desarrollador
3. Ve a Configuración > Sistema > Opciones de desarrollador
4. Activa "Depuración USB"

### Paso 2: Conectar dispositivo
1. Conecta tu dispositivo Android por USB
2. Acepta la autorización de depuración USB en tu dispositivo
3. Verifica la conexión:
```powershell
adb devices
```

### Paso 3: Ejecutar en el dispositivo
```powershell
dotnet build -t:Run -f net8.0-android
```

## Generar APK para Instalación

### APK Debug (para pruebas)
```powershell
dotnet build -f net8.0-android -c Debug
```
APK ubicado en: `bin\Debug\net8.0-android\`

### APK Release (para distribución)
```powershell
dotnet publish -f net8.0-android -c Release
```
APK ubicado en: `bin\Release\net8.0-android\publish\`

### Instalar APK manualmente
```powershell
adb install ruta\al\archivo.apk
```

## Solución de Problemas Comunes

### Error: "No Android device/emulator found"
- Asegúrate de que un emulador esté corriendo o un dispositivo esté conectado
- Ejecuta `adb devices` para verificar

### Error: "Android SDK not found"
```powershell
dotnet workload repair
dotnet workload install android
```

### Error: "Unauthorized device"
- Desconecta y reconecta el dispositivo USB
- Revoca las autorizaciones USB en el dispositivo (Opciones de desarrollador)
- Vuelve a conectar y acepta la autorización

### La app se cierra inmediatamente
- Revisa los logs con: `adb logcat`
- O desde Visual Studio: View > Output > Show output from: Debug

## Próximos Pasos

Una vez que la app esté corriendo:

1. **Prueba las funcionalidades básicas:**
   - Cambiar tipo de usuario
   - Registrar alimentos
   - Registrar medicamentos
   - Ver calendario de medicamentos

2. **Personaliza según tus necesidades:**
   - Modifica colores en `Resources/Styles/Colors.xaml`
   - Ajusta layouts en `MainPage.xaml`
   - Agrega nuevas funcionalidades en `MainViewModel.cs`

3. **Mejoras futuras sugeridas:**
   - Agregar SQLite para persistencia de datos
   - Implementar notificaciones locales
   - Agregar gráficos de consumo
   - Exportar datos a PDF o CSV

## Recursos Adicionales

- [Documentación oficial de .NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Guía de Android Development](https://developer.android.com/)
- [Community Toolkit MAUI](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/)
