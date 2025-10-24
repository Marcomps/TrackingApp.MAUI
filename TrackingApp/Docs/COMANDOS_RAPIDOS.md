# ⚡ COMANDOS RÁPIDOS

## 🌐 Versión Web

### Abrir la aplicación
```powershell
cd "c:\Users\PC\Desktop\Traking food"
start index.html
```

---

## 📱 Versión MAUI - Android

### Instalación inicial (solo una vez)
```powershell
# Instalar workload de MAUI
dotnet workload install maui

# Instalar workload de Android
dotnet workload install android
```

### Compilar el proyecto
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet build
```

### Ejecutar en Android
```powershell
# En emulador o dispositivo conectado
dotnet build -t:Run -f net8.0-android
```

### Compilar APK Debug
```powershell
dotnet build -f net8.0-android -c Debug
```

### Compilar APK Release (firmado)
```powershell
dotnet publish -f net8.0-android -c Release
```

### Compilar AAB para Play Store
```powershell
dotnet publish -f net8.0-android -c Release -p:AndroidPackageFormat=aab
```

---

## 🔧 Android Debug Bridge (ADB)

### Ver dispositivos conectados
```powershell
adb devices
```

### Instalar APK manualmente
```powershell
adb install ruta\al\archivo.apk
```

### Ver logs en tiempo real
```powershell
adb logcat
```

### Limpiar logs y ver solo la app
```powershell
adb logcat -c  # Limpiar
adb logcat | findstr "TrackingApp"  # Filtrar
```

### Desinstalar la app
```powershell
adb uninstall com.tuempresa.trackingapp
```

### Reiniciar ADB (si no detecta dispositivos)
```powershell
adb kill-server
adb start-server
```

---

## 🔐 Generar Keystore (solo una vez)

```powershell
keytool -genkey -v -keystore tracking-app.keystore -alias trackingapp -keyalg RSA -keysize 2048 -validity 10000
```

**⚠️ IMPORTANTE:** Guarda el archivo .keystore y las contraseñas en un lugar seguro.

---

## 🎯 Emulador Android

### Listar emuladores disponibles
```powershell
emulator -list-avds
```

### Iniciar un emulador
```powershell
emulator -avd nombre_del_emulador
```

### Crear emulador (desde Android Studio)
1. Abrir Android Studio
2. Tools > Device Manager
3. Create Device
4. Seleccionar dispositivo y sistema

---

## 🛠️ Solución de Problemas

### Reparar workloads de .NET
```powershell
dotnet workload repair
dotnet workload update
```

### Limpiar build y reconstruir
```powershell
dotnet clean
dotnet build
```

### Ver información de .NET instalado
```powershell
dotnet --info
```

### Ver workloads instalados
```powershell
dotnet workload list
```

### Actualizar .NET SDK
```powershell
# Descargar desde: https://dotnet.microsoft.com/download
```

---

## 📂 Navegación Rápida

### Ir a carpeta del proyecto Web
```powershell
cd "c:\Users\PC\Desktop\Traking food"
```

### Ir a carpeta del proyecto MAUI
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
```

### Abrir en Visual Studio Code
```powershell
code .
```

### Abrir en Explorador de Windows
```powershell
explorer .
```

---

## 🗂️ Ubicaciones de Archivos Compilados

### APK Debug
```
bin\Debug\net8.0-android\
```

### APK Release
```
bin\Release\net8.0-android\publish\
```

### AAB (Android App Bundle)
```
bin\Release\net8.0-android\publish\
```

---

## 📊 Información del Proyecto

### Ver tamaño del APK
```powershell
dir bin\Release\net8.0-android\publish\*.apk
```

### Ver estructura del proyecto
```powershell
tree /F
```

### Buscar archivos específicos
```powershell
dir /s /b *.cs    # Archivos C#
dir /s /b *.xaml  # Archivos XAML
```

---

## 🚀 Workflow Completo de Desarrollo

### 1. Desarrollo inicial
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet build
```

### 2. Probar en emulador/dispositivo
```powershell
dotnet build -t:Run -f net8.0-android
```

### 3. Hacer cambios en el código
```
# Edita archivos .cs, .xaml, etc.
```

### 4. Recompilar y probar
```powershell
dotnet build -t:Run -f net8.0-android
```

### 5. Preparar para distribución
```powershell
# Actualizar versión en .csproj
# <AndroidVersionCode>2</AndroidVersionCode>
# <ApplicationDisplayVersion>1.0.1</ApplicationDisplayVersion>
```

### 6. Compilar Release
```powershell
dotnet publish -f net8.0-android -c Release -p:AndroidPackageFormat=aab
```

### 7. Subir a Play Store
```
# Seguir pasos en PUBLICAR_PLAY_STORE.md
```

---

## 🎨 Visual Studio 2022

### Abrir proyecto en VS 2022
```powershell
start TrackingApp.sln
```

### Atajos útiles en VS:
- `F5` - Compilar y ejecutar con debugging
- `Ctrl+F5` - Compilar y ejecutar sin debugging
- `Ctrl+Shift+B` - Solo compilar
- `Ctrl+K, Ctrl+D` - Formatear documento
- `Ctrl+.` - Acciones rápidas

---

## 📱 Pruebas en Dispositivo Físico

### 1. Habilitar modo desarrollador en Android
```
Configuración > Acerca del teléfono
Tocar 7 veces en "Número de compilación"
```

### 2. Habilitar depuración USB
```
Configuración > Sistema > Opciones de desarrollador
Activar "Depuración USB"
```

### 3. Conectar y verificar
```powershell
adb devices
# Debe aparecer tu dispositivo
```

### 4. Ejecutar en dispositivo
```powershell
dotnet build -t:Run -f net8.0-android
```

---

## 🔄 Actualización de la App

### Proceso completo:
```powershell
# 1. Actualizar código
# 2. Incrementar versión en .csproj
# 3. Limpiar y reconstruir
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet clean
dotnet build -f net8.0-android -c Release

# 4. Generar AAB
dotnet publish -f net8.0-android -c Release -p:AndroidPackageFormat=aab

# 5. Subir a Play Console
```

---

## 📝 Notas Importantes

### ⚠️ Nunca olvides:
1. Hacer backup del archivo `.keystore`
2. Guardar las contraseñas del keystore
3. Incrementar `AndroidVersionCode` con cada actualización
4. Probar en múltiples dispositivos antes de publicar
5. Leer los logs si algo falla: `adb logcat`

### ✅ Buenas prácticas:
1. Hacer commits frecuentes (si usas Git)
2. Probar en Debug antes de compilar Release
3. Mantener documentación actualizada
4. Tener respaldos de versiones anteriores
5. Leer las políticas de Google Play regularmente

---

## 🆘 Si algo sale mal...

### La app no compila
```powershell
dotnet clean
dotnet workload repair
dotnet build
```

### No detecta el dispositivo
```powershell
adb kill-server
adb start-server
adb devices
```

### Error de firma
```
Verifica que el archivo .keystore existe
Verifica las contraseñas en .csproj
```

### La app se cierra inmediatamente
```powershell
adb logcat > error.log
# Revisa error.log para ver el problema
```

---

## 🎯 Un Solo Comando para Todo

### Script PowerShell completo (crear archivo build.ps1):
```powershell
# build.ps1
Write-Host "🔨 Compilando Tracking App..." -ForegroundColor Cyan
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet clean
dotnet build -f net8.0-android
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Compilación exitosa!" -ForegroundColor Green
    dotnet build -t:Run -f net8.0-android
} else {
    Write-Host "❌ Error en la compilación" -ForegroundColor Red
}
```

### Usar el script:
```powershell
.\build.ps1
```

---

**💡 Tip:** Guarda este archivo como referencia rápida para no tener que recordar todos los comandos.
