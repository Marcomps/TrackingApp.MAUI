<#
.SYNOPSIS
    Compila y genera la APK de TrackingApp con un número de versión específico.

.DESCRIPTION
    Este script:
    1. Actualiza la versión en el .csproj
    2. Ejecuta los unit tests
    3. Compila la APK en modo Release con firma (keystore)
    4. Copia la APK al Escritorio con el nombre TrackingApp_v{VERSION}.apk

.PARAMETER Version
    Número de versión (ej: 1.7, 1.8, 2.0). Se usa como ApplicationDisplayVersion.

.EXAMPLE
    .\Build-APK.ps1 -Version 1.7
    # Genera: C:\Users\PC\Desktop\TrackingApp_v1.7.apk

.EXAMPLE
    .\Build-APK.ps1 -Version 1.8
    # Genera: C:\Users\PC\Desktop\TrackingApp_v1.8.apk
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

# ============================================================
# CONFIGURACIÓN
# ============================================================
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Join-Path $ScriptDir "TrackingApp"
$CsprojPath = Join-Path $ProjectDir "TrackingApp.csproj"
$TestsDir = Join-Path $ScriptDir "TrackingApp.Tests"
$PublishOutput = Join-Path $ScriptDir "publish_output"
$DesktopPath = [Environment]::GetFolderPath("Desktop")
$OutputApkName = "TrackingApp_v$Version.apk"
$OutputApkPath = Join-Path $DesktopPath $OutputApkName

# Calcular ApplicationVersion (entero) a partir de la versión
# Ej: 1.7 -> 17, 2.0 -> 20, 1.10 -> 110
$VersionParts = $Version -split '\.'
if ($VersionParts.Length -ge 2) {
    $AppVersion = [int]$VersionParts[0] * 10 + [int]$VersionParts[1]
} else {
    $AppVersion = [int]$VersionParts[0]
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TrackingApp APK Builder" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Version:     $Version" -ForegroundColor White
Write-Host "  AppVersion:  $AppVersion" -ForegroundColor White
Write-Host "  Output:      $OutputApkPath" -ForegroundColor White
Write-Host ""

# ============================================================
# PASO 1: Actualizar versión en el .csproj
# ============================================================
Write-Host "[1/4] Actualizando versión en .csproj..." -ForegroundColor Yellow

$csprojContent = Get-Content $CsprojPath -Raw

# Actualizar ApplicationDisplayVersion
$csprojContent = $csprojContent -replace '(<ApplicationDisplayVersion>)[^<]+(</ApplicationDisplayVersion>)', "`${1}$Version`${2}"

# Actualizar ApplicationVersion
$csprojContent = $csprojContent -replace '(<ApplicationVersion>)[^<]+(</ApplicationVersion>)', "`${1}$AppVersion`${2}"

Set-Content -Path $CsprojPath -Value $csprojContent -NoNewline

Write-Host "  ✅ Versión actualizada a $Version (build $AppVersion)" -ForegroundColor Green
Write-Host ""

# ============================================================
# PASO 2: Ejecutar Unit Tests
# ============================================================
Write-Host "[2/4] Ejecutando unit tests..." -ForegroundColor Yellow

Push-Location $ScriptDir
try {
    $testResult = dotnet test $TestsDir --verbosity minimal 2>&1
    $testOutput = $testResult -join "`n"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ❌ Los tests fallaron. Abortando compilación." -ForegroundColor Red
        Write-Host $testOutput
        Pop-Location
        exit 1
    }
    
    # Extraer resumen de tests
    $summaryLine = $testOutput | Select-String "Test summary:" | Select-Object -Last 1
    if ($summaryLine) {
        Write-Host "  ✅ $($summaryLine.Line.Trim())" -ForegroundColor Green
    } else {
        Write-Host "  ✅ Tests pasados correctamente" -ForegroundColor Green
    }
} finally {
    Pop-Location
}
Write-Host ""

# ============================================================
# PASO 3: Compilar APK
# ============================================================
Write-Host "[3/4] Compilando APK en modo Release..." -ForegroundColor Yellow
Write-Host "  (Esto puede tardar varios minutos...)" -ForegroundColor DarkGray

Push-Location $ProjectDir
try {
    # Limpiar output previo
    if (Test-Path $PublishOutput) {
        Remove-Item "$PublishOutput\*.apk" -Force -ErrorAction SilentlyContinue
    }

    $buildResult = dotnet publish -f net10.0-android -c Release -o $PublishOutput 2>&1
    $buildOutput = $buildResult -join "`n"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ❌ La compilación falló." -ForegroundColor Red
        Write-Host $buildOutput
        Pop-Location
        exit 1
    }
    
    Write-Host "  ✅ Compilación exitosa" -ForegroundColor Green
} finally {
    Pop-Location
}
Write-Host ""

# ============================================================
# PASO 4: Copiar APK al Escritorio
# ============================================================
Write-Host "[4/4] Copiando APK al Escritorio..." -ForegroundColor Yellow

$signedApk = Get-ChildItem -Path $PublishOutput -Filter "*-Signed.apk" -Recurse | Select-Object -First 1

if (-not $signedApk) {
    # Fallback: buscar cualquier APK
    $signedApk = Get-ChildItem -Path $PublishOutput -Filter "*.apk" -Recurse | Select-Object -First 1
}

if (-not $signedApk) {
    Write-Host "  ❌ No se encontró el archivo APK en la salida de compilación." -ForegroundColor Red
    exit 1
}

Copy-Item $signedApk.FullName $OutputApkPath -Force

$apkSizeMB = [math]::Round((Get-Item $OutputApkPath).Length / 1MB, 2)

Write-Host "  ✅ APK copiada exitosamente" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ¡LISTO!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "  📦 Archivo: $OutputApkPath" -ForegroundColor White
Write-Host "  📏 Tamaño:  $apkSizeMB MB" -ForegroundColor White
Write-Host "  🏷️  Versión: $Version (build $AppVersion)" -ForegroundColor White
Write-Host ""
Write-Host "  Para instalar en tu teléfono:" -ForegroundColor DarkGray
Write-Host "  adb install -r `"$OutputApkPath`"" -ForegroundColor DarkGray
Write-Host ""
