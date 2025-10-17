# 🎨 Logo y Diseño Visual - Tracking App

## 📱 Logo Personalizado Implementado

Se ha creado un **logo profesional y personalizado** para la aplicación de tracking de alimentos y medicamentos.

---

## 🎨 Diseño del Logo

### Concepto
El logo combina **elementos visuales representativos** de las funcionalidades principales:

1. **🍴 Tenedor** (Fork) - Representa alimentos y nutrición
2. **🥄 Cuchara** (Spoon) - Complementa el concepto de comidas
3. **💊 Pastilla** (Pill) - Representa medicamentos
4. **⏰ Reloj** (Clock) - Indica seguimiento temporal y horarios

### Paleta de Colores

#### Color Principal: Verde Salud
```
Verde claro: #4CAF50
Verde oscuro: #2E7D32
Blanco: #FFFFFF
```

**¿Por qué verde?**
- ✅ Asociado con salud y bienestar
- ✅ Color universal para nutrición
- ✅ Transmite confianza y seguridad
- ✅ Agradable visualmente

---

## 📂 Archivos de Logo

### 1. Icon de Aplicación (`appicon.svg`)
**Ubicación:** `Resources/AppIcon/appicon.svg`

**Características:**
- Fondo degradado verde (#4CAF50 → #2E7D32)
- Bordes redondeados (rx="80")
- Tamaño: 456x456 px
- Formato: SVG vectorial

**Uso:**
- Icono en el launcher de Android
- Icono en ajustes del sistema
- Icono en notificaciones

### 2. Foreground del Icon (`appiconfg.svg`)
**Ubicación:** `Resources/AppIcon/appiconfg.svg`

**Características:**
- Símbolos blancos sobre transparente
- 4 elementos: tenedor, cuchara, pastilla, reloj
- Diseño balanceado y simétrico
- Compatible con Android Adaptive Icons

**Elementos:**
```xml
<!-- Fork (izquierda) -->
Tenedor con 3 dientes y mango largo

<!-- Spoon (derecha) -->
Cuchara con forma ovalada

<!-- Pill (centro abajo) -->
Pastilla bicolor (blanco/verde)

<!-- Clock (esquina superior derecha) -->
Reloj pequeño marcando horario
```

### 3. Splash Screen (`splash.svg`)
**Ubicación:** `Resources/Splash/splash.svg`

**Características:**
- Pantalla de presentación completa
- Mismo fondo degradado verde
- Iconos escalados 1.5x (más grandes)
- **Texto:** "Tracking App"
- **Subtítulo:** "Alimentos & Medicamentos"
- Tipografía: Arial Bold

**Uso:**
- Pantalla inicial al abrir la app
- Primera impresión del usuario
- Identidad de marca

---

## 🔧 Configuración Técnica

### En `TrackingApp.csproj`:

```xml
<!-- App Icon -->
<MauiIcon Include="Resources\AppIcon\appicon.svg" 
          ForegroundFile="Resources\AppIcon\appiconfg.svg" 
          Color="#4CAF50" />

<!-- Splash Screen -->
<MauiSplashScreen Include="Resources\Splash\splash.svg" 
                  Color="#4CAF50" 
                  BaseSize="128,128" />
```

### Tamaños Generados Automáticamente

MAUI genera automáticamente múltiples resoluciones:

#### Android:
- `mipmap-mdpi`: 48x48 px
- `mipmap-hdpi`: 72x72 px
- `mipmap-xhdpi`: 96x96 px
- `mipmap-xxhdpi`: 144x144 px
- `mipmap-xxxhdpi`: 192x192 px

#### iOS:
- `@1x`: 60x60 px
- `@2x`: 120x120 px
- `@3x`: 180x180 px

---

## 🎯 Cambios Realizados

### Antes (Logo por defecto):
❌ Logo genérico de .NET MAUI
❌ Color morado corporativo (#512BD4)
❌ Texto ".NET" sin relación con la app
❌ Sin identidad propia

### Ahora (Logo personalizado):
✅ **Diseño único y personalizado**
✅ **Color verde salud (#4CAF50)**
✅ **Símbolos representativos** (alimentos + medicamentos)
✅ **Identidad profesional**
✅ **Splash screen con nombre de la app**

---

## 🚀 Compilación y Visualización

### Para regenerar los logos:

```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"

# Limpiar compilación anterior
dotnet clean

# Recompilar (regenera logos en todas las resoluciones)
dotnet build
```

### Para ver el logo en acción:

```powershell
# Ejecutar en Android
dotnet build -t:Run -f net8.0-android
```

**El nuevo logo aparecerá:**
1. En el splash screen al iniciar
2. En el drawer/launcher de Android
3. En la multitarea de apps
4. En notificaciones (futura implementación)

---

## 📐 Especificaciones de Diseño

### Dimensiones del Canvas:
- **Width:** 456 px
- **Height:** 456 px
- **ViewBox:** 0 0 456 456

### Posicionamiento de Elementos:

#### Tenedor (Fork):
```
Posición: translate(120, 150)
Ancho mango: 8 px
Alto total: 120 px
Dientes: 3 círculos de r=8
```

#### Cuchara (Spoon):
```
Posición: translate(280, 150)
Ancho mango: 8 px
Alto mango: 90 px
Cuenco: elipse 14x20
```

#### Pastilla (Pill):
```
Posición: translate(200, 280)
Tamaño: elipse 24x18
Línea verde: rect 48x20
```

#### Reloj (Clock):
```
Posición: translate(340, 100)
Radio exterior: 18 px
Radio interior: 14 px
Manecillas: 2 líneas
```

---

## 🎨 Variaciones del Logo (Futuras)

### Posibles extensiones:

1. **Logo Horizontal**
   - Para encabezados y banners
   - Iconos + texto en línea

2. **Logo Monocromático**
   - Versión en blanco/negro
   - Para documentos impresos

3. **Logo Simplificado**
   - Solo el símbolo más icónico
   - Para notificaciones pequeñas

4. **Logo Animado**
   - Para splash screen mejorado
   - Animación Lottie

---

## 🖼️ Archivos Generados

Después de compilar, encontrarás los logos en:

```
TrackingApp/obj/Debug/net9.0-android/resizetizer/
├── r/
│   ├── mipmap-mdpi/appicon.png
│   ├── mipmap-hdpi/appicon.png
│   ├── mipmap-xhdpi/appicon.png
│   ├── mipmap-xxhdpi/appicon.png
│   └── mipmap-xxxhdpi/appicon.png
└── sp/
    ├── drawable-mdpi/splash.png
    ├── drawable-hdpi/splash.png
    ├── drawable-xhdpi/splash.png
    ├── drawable-xxhdpi/splash.png
    └── drawable-xxxhdpi/splash.png
```

---

## ✏️ Personalización Adicional

### Cambiar colores:

En `TrackingApp.csproj`, modifica:
```xml
Color="#4CAF50"  <!-- Color del fondo/tema -->
```

### Modificar elementos visuales:

Edita directamente los archivos SVG:
- `Resources/AppIcon/appicon.svg` - Fondo
- `Resources/AppIcon/appiconfg.svg` - Símbolos
- `Resources/Splash/splash.svg` - Pantalla completa

### Herramientas recomendadas:
- **Inkscape** (gratuito) - Editor SVG profesional
- **Figma** (web) - Diseño colaborativo
- **Adobe Illustrator** - Herramienta profesional

---

## 🎯 Mejores Prácticas

### ✅ Hacer:
- Mantener SVG vectoriales (escalan sin pérdida)
- Usar colores contrastantes para legibilidad
- Probar en diferentes fondos (claro/oscuro)
- Mantener simplicidad en el diseño

### ❌ Evitar:
- Usar imágenes rasterizadas (PNG/JPG)
- Detalles muy pequeños (no se ven en iconos pequeños)
- Demasiados colores (máximo 3-4)
- Texto ilegible en tamaños pequeños

---

## 📱 Visualización en Dispositivo

### Tamaños reales:

| Contexto | Tamaño típico | Archivo |
|----------|---------------|---------|
| Launcher | 48-192 dp | appicon.png |
| Splash | Pantalla completa | splash.png |
| Notificación | 24-48 dp | appicon.png |
| Play Store | 512x512 px | (exportar manual) |

---

## 🚀 Próximos Pasos

### Logo implementado ✅
- [x] Diseño personalizado creado
- [x] Archivos SVG configurados
- [x] Colores actualizados en proyecto
- [x] Documentación completa

### Futuras mejoras sugeridas:
- [ ] Logo para Play Store (512x512 PNG)
- [ ] Feature Graphic (1024x500)
- [ ] Screenshots con logo
- [ ] Assets para marketing

---

## 📞 Información del Logo

**Versión:** 1.0.0
**Fecha:** Octubre 2025
**Formato:** SVG (vectorial escalable)
**Colores:** Verde (#4CAF50, #2E7D32), Blanco (#FFFFFF)
**Estilo:** Flat design, iconografía simple
**Compatibilidad:** Android, iOS, Windows, macOS

---

## 🎉 Resumen

Tu aplicación ahora tiene:
✅ **Logo profesional y único**
✅ **Identidad visual coherente**
✅ **Splash screen personalizado**
✅ **Colores representativos**
✅ **Diseño escalable (SVG)**
✅ **Configuración completa**

El logo representa perfectamente las funcionalidades de la app: **tracking de alimentos y medicamentos con seguimiento temporal**.

---

**¡Listo para compilar y ver tu nuevo logo en acción!** 🚀
