# Script para generar APK - TrackingApp

# VERSION - Cambia este valor para personalizar el nombre del APK
$VERSION = "v2.0.1"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Generador de APK - TrackingApp" -ForegroundColor Green
Write-Host "  Version: $VERSION" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Ir al directorio del proyecto
Write-Host "[1/4] Navegando al directorio del proyecto..." -ForegroundColor Yellow
Set-Location "c:\Users\PC\Desktop\TrackingApp.MAUI\TrackingApp.MAUI\TrackingApp"
Write-Host "      OK" -ForegroundColor Green
Write-Host ""

# Limpiar compilacion anterior
Write-Host "[2/4] Limpiando compilacion anterior..." -ForegroundColor Yellow
dotnet clean | Out-Null
Write-Host "      OK" -ForegroundColor Green
Write-Host ""

# Compilar y generar APK
Write-Host "[3/4] Compilando aplicacion (esto puede tardar varios minutos)..." -ForegroundColor Yellow
dotnet publish -f net9.0-android -c Release

# Buscar el APK generado
Write-Host ""
Write-Host "[4/4] Buscando APK generado..." -ForegroundColor Yellow
$apkPath = Get-ChildItem -Path "bin\Release\net9.0-android\publish\" -Filter "*-Signed.apk" -ErrorAction SilentlyContinue | Select-Object -First 1

if ($apkPath) {
    # Copiar a Escritorio con nombre personalizado
    $destinoAPK = "$env:USERPROFILE\Desktop\TrackingApp-$VERSION.apk"
    Copy-Item $apkPath.FullName $destinoAPK -Force
    
    Write-Host "      OK" -ForegroundColor Green
    Write-Host ""
    Write-Host "======================================" -ForegroundColor Green
    Write-Host "  APK GENERADO EXITOSAMENTE" -ForegroundColor Green
    Write-Host "======================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ubicacion: $destinoAPK" -ForegroundColor Cyan
    Write-Host "Tamano: $([math]::Round($apkPath.Length/1MB, 2)) MB" -ForegroundColor Cyan
    Write-Host "Version: $VERSION" -ForegroundColor Cyan
    Write-Host ""
    
    # Abrir carpeta del Escritorio
    Start-Process "explorer.exe" "$env:USERPROFILE\Desktop"
    
    Write-Host "PROXIMOS PASOS:" -ForegroundColor Yellow
    Write-Host "1. Transfiere TrackingApp-$VERSION.apk a tu celular" -ForegroundColor White
    Write-Host "2. Habilita 'Origenes desconocidos' en Ajustes" -ForegroundColor White
    Write-Host "3. Abre el APK y toca 'Instalar'" -ForegroundColor White
    Write-Host ""
    
} else {
    Write-Host "      ERROR" -ForegroundColor Red
    Write-Host ""
    Write-Host "No se encontro el APK generado" -ForegroundColor Red
    Write-Host "Revisa los errores de compilacion arriba" -ForegroundColor Red
}

Write-Host ""
Write-Host "TIP: Para cambiar la version, edita la linea 4 del script" -ForegroundColor Magenta
