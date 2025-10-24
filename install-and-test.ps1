# Script de Instalación y Pruebas - TrackingApp v1.31
# Ejecutar desde: TrackingApp.MAUI\

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TrackingApp v1.31 - Test Installation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que ADB esté disponible
Write-Host "🔍 Verificando ADB..." -ForegroundColor Yellow
$adbPath = Get-Command adb -ErrorAction SilentlyContinue

if (-not $adbPath) {
    Write-Host "❌ ERROR: ADB no encontrado. Por favor instala Android SDK Platform Tools." -ForegroundColor Red
    Write-Host "   Descarga: https://developer.android.com/studio/releases/platform-tools" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ ADB encontrado: $($adbPath.Source)" -ForegroundColor Green
Write-Host ""

# Verificar dispositivos conectados
Write-Host "📱 Buscando dispositivos conectados..." -ForegroundColor Yellow
$devices = adb devices | Select-String -Pattern "device$"

if ($devices.Count -eq 0) {
    Write-Host "❌ ERROR: No se encontraron dispositivos Android conectados." -ForegroundColor Red
    Write-Host "   Conecta tu dispositivo por USB y habilita 'Depuración USB'" -ForegroundColor Yellow
    Write-Host "   Luego ejecuta: adb devices" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Dispositivo(s) encontrado(s):" -ForegroundColor Green
adb devices
Write-Host ""

# Ruta del APK
$apkPath = "TrackingApp\bin\Release\net9.0-android\publish\com.trackingapp.nutrition-Signed.apk"

if (-not (Test-Path $apkPath)) {
    Write-Host "❌ ERROR: APK no encontrado en: $apkPath" -ForegroundColor Red
    Write-Host "   Por favor compila el proyecto primero con:" -ForegroundColor Yellow
    Write-Host "   dotnet publish TrackingApp\TrackingApp.csproj -f net9.0-android -c Release" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ APK encontrado: $apkPath" -ForegroundColor Green
$apkInfo = Get-Item $apkPath
Write-Host "   Tamaño: $([math]::Round($apkInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
Write-Host "   Fecha: $($apkInfo.LastWriteTime)" -ForegroundColor Cyan
Write-Host ""

# Preguntar si desea desinstalar versión anterior
Write-Host "❓ ¿Deseas desinstalar la versión anterior? (S/N)" -ForegroundColor Yellow
$uninstall = Read-Host

if ($uninstall -eq 'S' -or $uninstall -eq 's') {
    Write-Host "🗑️  Desinstalando versión anterior..." -ForegroundColor Yellow
    adb uninstall com.trackingapp.nutrition 2>$null
    Write-Host "✅ Versión anterior desinstalada (si existía)" -ForegroundColor Green
    Write-Host ""
}

# Instalar APK
Write-Host "📲 Instalando APK..." -ForegroundColor Yellow
Write-Host "   Esto puede tardar unos segundos..." -ForegroundColor Cyan
Write-Host ""

$installResult = adb install -r $apkPath 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ APK instalado exitosamente!" -ForegroundColor Green
    Write-Host ""
    
    # Opciones de prueba
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Opciones de Prueba" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "1. Abrir aplicación en el dispositivo" -ForegroundColor White
    Write-Host "2. Ver logs en tiempo real (Ctrl+C para detener)" -ForegroundColor White
    Write-Host "3. Limpiar datos de la app (reset completo)" -ForegroundColor White
    Write-Host "4. Salir" -ForegroundColor White
    Write-Host ""
    
    $option = Read-Host "Selecciona una opción (1-4)"
    
    switch ($option) {
        "1" {
            Write-Host "🚀 Abriendo aplicación..." -ForegroundColor Yellow
            adb shell am start -n com.trackingapp.nutrition/crc64e9e7e55a3f2f6c7e.MainActivity
            Write-Host "✅ Aplicación abierta" -ForegroundColor Green
        }
        "2" {
            Write-Host "📋 Mostrando logs (Ctrl+C para detener)..." -ForegroundColor Yellow
            Write-Host "   Filtrando por: RecalculateNextDoses, Medication, DataService" -ForegroundColor Cyan
            Write-Host ""
            adb logcat | Select-String -Pattern "RecalculateNextDoses|Medication|DataService"
        }
        "3" {
            Write-Host "⚠️  ¿Estás seguro? Esto eliminará TODOS los datos de la app (S/N)" -ForegroundColor Red
            $confirm = Read-Host
            if ($confirm -eq 'S' -or $confirm -eq 's') {
                Write-Host "🗑️  Limpiando datos..." -ForegroundColor Yellow
                adb shell pm clear com.trackingapp.nutrition
                Write-Host "✅ Datos eliminados" -ForegroundColor Green
            } else {
                Write-Host "❌ Operación cancelada" -ForegroundColor Yellow
            }
        }
        "4" {
            Write-Host "👋 ¡Hasta luego!" -ForegroundColor Cyan
        }
        default {
            Write-Host "❌ Opción inválida" -ForegroundColor Red
        }
    }
} else {
    Write-Host "❌ ERROR al instalar APK:" -ForegroundColor Red
    Write-Host $installResult -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Información Útil" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "📄 Plan de Pruebas: Docs\PLAN_DE_PRUEBAS_v1.31.md" -ForegroundColor White
Write-Host "📱 Package: com.trackingapp.nutrition" -ForegroundColor White
Write-Host "🔧 Versión: 1.31" -ForegroundColor White
Write-Host ""
Write-Host "Comandos útiles:" -ForegroundColor Yellow
Write-Host "  adb logcat | Select-String 'RecalculateNextDoses'  # Ver logs" -ForegroundColor Cyan
Write-Host "  adb shell pm clear com.trackingapp.nutrition      # Reset app" -ForegroundColor Cyan
Write-Host "  adb uninstall com.trackingapp.nutrition           # Desinstalar" -ForegroundColor Cyan
Write-Host ""
