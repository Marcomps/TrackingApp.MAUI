# 📦 GENERAR APK - Tracking App

## Cómo Generar APK para Instalar en tu Celular

Esta guía te muestra cómo crear un archivo APK para instalar la aplicación directamente en tu teléfono Android.

---

## 🎯 ¿Qué es un APK?

**APK** = Android Package Kit

Es el archivo instalable de Android, como un `.exe` en Windows o `.dmg` en Mac.

**Dos tipos de APK:**

1. **Debug APK** 🔧
   - Para pruebas y desarrollo
   - Sin firmar oficialmente
   - Más rápido de generar
   - **👉 Usaremos este**

2. **Release APK** 📦
   - Para publicación en Play Store
   - Firmado digitalmente
   - Optimizado y ofuscado
   - Para distribución oficial

---

## 🚀 MÉTODO 1: Generar Debug APK (Rápido)

### Paso 1: Generar el APK

Abre PowerShell y ejecuta:

```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"

# Generar APK de Debug
dotnet build -t:Run -f net9.0-android -c Debug
```

O si prefieres net8.0:

```powershell
dotnet build -t:Run -f net8.0-android -c Debug
```

### Paso 2: Encontrar el APK

El APK se genera en:

```
TrackingApp/bin/Debug/net9.0-android/
```

Busca el archivo:
```
com.trackingapp.nutrition-Signed.apk
```

O:
```
TrackingApp-Signed.apk
```

### Paso 3: Copiar el APK

Copia el APK a un lugar fácil:

```powershell
# Copiar a Escritorio
Copy-Item "bin\Debug\net9.0-android\*-Signed.apk" "$env:USERPROFILE\Desktop\TrackingApp.apk"
```

### Paso 4: Transferir a tu Celular

**Opción A: USB**
1. Conecta tu celular con cable USB
2. Copia `TrackingApp.apk` a la carpeta de Descargas del celular

**Opción B: Email**
1. Envíate el APK por email
2. Abre el email en tu celular
3. Descarga el APK

**Opción C: Google Drive / Dropbox**
1. Sube el APK a Drive/Dropbox
2. Descárgalo desde tu celular

**Opción D: Bluetooth**
1. Envía el APK por Bluetooth a tu celular

### Paso 5: Instalar en tu Celular

1. **Habilitar instalación de fuentes desconocidas:**
   - Ajustes → Seguridad
   - Activar "Orígenes desconocidos" o "Instalar apps desconocidas"

2. **Instalar el APK:**
   - Abre el archivo APK desde Descargas
   - Toca "Instalar"
   - Espera unos segundos
   - Toca "Abrir"

✅ **¡Listo! La app está instalada**

---

## 🏆 MÉTODO 2: Generar Release APK (Optimizado)

Para una versión más optimizada (recomendado para uso real):

### Paso 1: Generar Release APK

```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"

# Generar APK Release
dotnet publish -f net9.0-android -c Release
```

### Paso 2: Encontrar el APK

El APK se genera en:

```
TrackingApp/bin/Release/net9.0-android/publish/
```

Nombre del archivo:
```
com.trackingapp.nutrition-Signed.apk
```

### Paso 3: Copiar a Escritorio

```powershell
Copy-Item "bin\Release\net9.0-android\publish\*-Signed.apk" "$env:USERPROFILE\Desktop\TrackingApp-Release.apk"
```

### Paso 4: Instalar (mismo proceso que Debug)

---

## 📱 MÉTODO 3: Generar e Instalar Directamente (Con dispositivo conectado)

Si tienes tu celular conectado por USB:

### Paso 1: Habilitar Depuración USB

En tu celular:
1. Ajustes → Acerca del teléfono
2. Toca 7 veces en "Número de compilación"
3. Vuelve → Opciones de desarrollador
4. Activa "Depuración USB"

### Paso 2: Conectar USB

1. Conecta el cable USB
2. Acepta la autorización en el celular
3. Verifica la conexión:

```powershell
# Si tienes adb instalado:
adb devices
```

Deberías ver tu dispositivo listado.

### Paso 3: Compilar e Instalar Directamente

```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"

# Esto compilará e instalará directamente
dotnet build -t:Run -f net9.0-android
```

✅ **La app se instalará automáticamente en tu celular**

---

## 🔍 Verificar la Generación del APK

### Ver qué se generó:

```powershell
# Listar APKs en Debug
Get-ChildItem -Path "bin\Debug\net9.0-android\" -Filter "*.apk" -Recurse

# Listar APKs en Release
Get-ChildItem -Path "bin\Release\net9.0-android\" -Filter "*.apk" -Recurse
```

### Ver detalles del APK:

```powershell
# Ver tamaño del archivo
Get-Item "bin\Debug\net9.0-android\*-Signed.apk" | Select-Object Name, Length, LastWriteTime
```

---

## 📊 Información del APK Generado

### Tamaño Aproximado:
- **Debug APK:** ~50-80 MB
- **Release APK:** ~30-50 MB (optimizado)

### Versión:
- **1.0** (configurado en `TrackingApp.csproj`)

### Identificador:
- **com.trackingapp.nutrition**

### Nombre:
- **Tracking App**

### Permisos:
- Internet (si fuera necesario)
- Almacenamiento (para SQLite)

---

## 🎨 Logo Incluido

El APK incluye:
✅ Logo personalizado verde
✅ Splash screen con "Tracking App"
✅ Icono en el launcher
✅ Iconos en todas las resoluciones

---

## ⚙️ Comandos Completos (Script Automatizado)

Guarda esto como `generar-apk.ps1`:

```powershell
# Script para generar APK

Write-Host "🚀 Generando APK de Tracking App..." -ForegroundColor Green

# Ir al directorio del proyecto
Set-Location "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"

# Limpiar compilación anterior
Write-Host "`n🧹 Limpiando compilación anterior..." -ForegroundColor Yellow
dotnet clean

# Compilar y generar APK
Write-Host "`n🔨 Compilando aplicación..." -ForegroundColor Yellow
dotnet publish -f net9.0-android -c Release

# Buscar el APK generado
$apkPath = Get-ChildItem -Path "bin\Release\net9.0-android\publish\" -Filter "*-Signed.apk" | Select-Object -First 1

if ($apkPath) {
    # Copiar a Escritorio
    $destinoAPK = "$env:USERPROFILE\Desktop\TrackingApp.apk"
    Copy-Item $apkPath.FullName $destinoAPK -Force
    
    Write-Host "`n✅ APK generado exitosamente!" -ForegroundColor Green
    Write-Host "📍 Ubicación: $destinoAPK" -ForegroundColor Cyan
    Write-Host "📦 Tamaño: $([math]::Round($apkPath.Length/1MB, 2)) MB" -ForegroundColor Cyan
    
    # Abrir carpeta del Escritorio
    Start-Process "explorer.exe" "$env:USERPROFILE\Desktop"
    
    Write-Host "`n📱 Próximos pasos:" -ForegroundColor Yellow
    Write-Host "1. Transfiere TrackingApp.apk a tu celular" -ForegroundColor White
    Write-Host "2. Habilita 'Orígenes desconocidos' en Ajustes" -ForegroundColor White
    Write-Host "3. Abre el APK y toca 'Instalar'" -ForegroundColor White
    Write-Host "4. ¡Disfruta tu app! 🎉" -ForegroundColor White
    
} else {
    Write-Host "`n❌ Error: No se encontró el APK generado" -ForegroundColor Red
    Write-Host "Revisa los errores de compilación arriba" -ForegroundColor Red
}
```

### Ejecutar el script:

```powershell
# Dar permisos (solo la primera vez)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Ejecutar
.\generar-apk.ps1
```

---

## 🐛 Solución de Problemas

### "No se encuentra el APK"

**Solución:**
```powershell
# Ver todos los archivos generados
Get-ChildItem -Path "bin\Release\net9.0-android\" -Recurse | Where-Object { $_.Extension -eq ".apk" }
```

### "Error de compilación"

**Solución:**
```powershell
dotnet clean
dotnet restore
dotnet publish -f net9.0-android -c Release
```

### "APK no se instala en el celular"

**Causas comunes:**
1. ❌ No habilitaste "Orígenes desconocidos"
2. ❌ APK corrupto (vuelve a generarlo)
3. ❌ Versión Android muy antigua (requiere Android 5.0+)

**Solución:**
- Verifica configuración de seguridad
- Genera APK nuevamente
- Revisa versión de Android en tu celular

### "App se cierra al abrir"

**Solución:**
```powershell
# Genera APK de Debug para ver errores
dotnet build -f net9.0-android -c Debug

# Conecta el celular por USB
# Ejecuta para ver logs:
adb logcat | Select-String "TrackingApp"
```

---

## 📋 Checklist de Generación

Antes de generar el APK, verifica:

- [ ] ✅ Proyecto compila sin errores
- [ ] ✅ Logo personalizado está incluido
- [ ] ✅ Persistencia SQLite implementada
- [ ] ✅ Todas las funcionalidades probadas
- [ ] ✅ Versión actualizada en `.csproj`

---

## 🎯 Resumen de Comandos

### Para probar rápido (Debug):
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet build -f net9.0-android -c Debug
Copy-Item "bin\Debug\net9.0-android\*-Signed.apk" "$env:USERPROFILE\Desktop\TrackingApp-Debug.apk"
```

### Para uso real (Release):
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet publish -f net9.0-android -c Release
Copy-Item "bin\Release\net9.0-android\publish\*-Signed.apk" "$env:USERPROFILE\Desktop\TrackingApp.apk"
```

### Abrir carpeta del APK:
```powershell
Start-Process "explorer.exe" "$env:USERPROFILE\Desktop"
```

---

## 📱 Instalación en el Celular (Paso a Paso)

### 1. En tu celular Android:

**Habilitar instalación:**
```
Ajustes 
  → Seguridad y privacidad
    → Más ajustes de seguridad
      → Instalar apps desconocidas
        → Chrome/Archivos
          → Permitir de esta fuente ✅
```

### 2. Transferir el APK:

**Opción más fácil - USB:**
1. Conecta el cable USB al PC y al celular
2. En el celular, selecciona "Transferir archivos"
3. En el PC, abre "Este equipo" → tu celular
4. Copia `TrackingApp.apk` a la carpeta "Download"

### 3. Instalar:

1. Abre la app "Archivos" o "Mis archivos" en tu celular
2. Ve a "Descargas" o "Downloads"
3. Toca `TrackingApp.apk`
4. Toca "Instalar"
5. Espera 5-10 segundos
6. Toca "Abrir"

### 4. Verificar:

✅ Verás el splash screen verde con tu logo
✅ La app se abre correctamente
✅ Puedes registrar alimentos y medicamentos
✅ Los datos persisten al cerrar y reabrir

---

## 🎉 Próximos Pasos Después de Instalar

1. **Prueba todas las funciones:**
   - Registra alimentos
   - Registra medicamentos
   - Confirma dosis
   - Edita horarios
   - Cambia tipo de usuario

2. **Verifica persistencia:**
   - Cierra la app completamente
   - Ábrela de nuevo
   - Los datos deben seguir ahí ✅

3. **Prueba en condiciones reales:**
   - Usa la app durante varios días
   - Registra datos reales
   - Verifica rendimiento

4. **Comparte feedback:**
   - ¿Funciona todo bien?
   - ¿Encuentras bugs?
   - ¿Falta alguna función?

---

## 📚 Documentación Relacionada

- [GUIA_TESTING.md](GUIA_TESTING.md) - Cómo probar la app
- [PERSISTENCIA_SQLITE.md](PERSISTENCIA_SQLITE.md) - Base de datos
- [PUBLICAR_PLAY_STORE.md](PUBLICAR_PLAY_STORE.md) - Para publicar oficialmente

---

## ✅ ¡Todo Listo!

Ahora tienes:
- ✅ APK listo para instalar
- ✅ Instrucciones completas
- ✅ Solución de problemas
- ✅ Logo personalizado incluido
- ✅ Base de datos SQLite funcionando

**¡Disfruta tu app de tracking en tu celular!** 🎉📱

---

**Versión:** 1.0.0  
**Fecha:** Octubre 2025  
**Plataforma:** Android 5.0+ (API 21+)  
**Tamaño:** ~30-80 MB (según configuración)
