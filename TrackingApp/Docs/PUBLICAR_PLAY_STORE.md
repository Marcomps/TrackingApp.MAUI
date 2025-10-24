# Guía para Publicar en Google Play Store

Esta guía te ayudará a preparar y publicar tu aplicación de Tracking en Google Play Store.

---

## Requisitos Previos

- [ ] Cuenta de Google Play Console ($25 USD pago único)
- [ ] Aplicación funcionando correctamente
- [ ] Íconos y recursos gráficos
- [ ] Política de privacidad (requerida)

---

## Paso 1: Preparar la Aplicación

### 1.1 Actualizar información de la app

Edita `TrackingApp.csproj` y actualiza:

```xml
<PropertyGroup>
    <ApplicationTitle>Tracking App</ApplicationTitle>
    <ApplicationId>com.tuempresa.trackingapp</ApplicationId>
    <ApplicationVersion>1</ApplicationVersion>
    <ApplicationDisplayVersion>1.0.0</ApplicationDisplayVersion>
    <AndroidVersionCode>1</AndroidVersionCode>
</PropertyGroup>
```

**Importante:** 
- `ApplicationId`: Debe ser único (ej: com.tunombre.trackingapp)
- `AndroidVersionCode`: Incrementa con cada actualización
- `ApplicationDisplayVersion`: Versión visible para usuarios

### 1.2 Crear íconos

Necesitas íconos en múltiples tamaños. Colócalos en:
```
Resources/AppIcon/
└── appicon.svg  (o archivos PNG en diferentes tamaños)
```

Tamaños requeridos:
- 48x48 (mdpi)
- 72x72 (hdpi)
- 96x96 (xhdpi)
- 144x144 (xxhdpi)
- 192x192 (xxxhdpi)
- 512x512 (para Play Store)

### 1.3 Crear Splash Screen

```
Resources/Splash/
└── splash.svg
```

---

## Paso 2: Generar Certificado de Firma

### 2.1 Crear keystore

```powershell
keytool -genkey -v -keystore tracking-app.keystore -alias trackingapp -keyalg RSA -keysize 2048 -validity 10000
```

Guarda el archivo `.keystore` en un lugar seguro y **NO LO PIERDAS**.

Información que te pedirá:
- Contraseña del keystore (¡guárdala!)
- Nombre y apellido
- Unidad organizativa
- Organización
- Ciudad
- Estado/Provincia
- Código de país

### 2.2 Configurar firma en el proyecto

Edita `TrackingApp.csproj`:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <AndroidKeyStore>true</AndroidKeyStore>
    <AndroidSigningKeyStore>tracking-app.keystore</AndroidSigningKeyStore>
    <AndroidSigningKeyAlias>trackingapp</AndroidSigningKeyAlias>
    <AndroidSigningKeyPass>TU_CONTRASEÑA</AndroidSigningKeyPass>
    <AndroidSigningStorePass>TU_CONTRASEÑA</AndroidSigningStorePass>
</PropertyGroup>
```

⚠️ **Seguridad:** No subas el archivo `.keystore` ni las contraseñas a repositorios públicos.

---

## Paso 3: Compilar APK/AAB de Release

### Opción A: APK (Instalación directa)
```powershell
dotnet publish -f net8.0-android -c Release
```

### Opción B: AAB (Google Play - Recomendado)
```powershell
dotnet publish -f net8.0-android -c Release -p:AndroidPackageFormat=aab
```

El archivo se generará en:
```
bin\Release\net8.0-android\publish\
```

---

## Paso 4: Probar el APK/AAB

### Instalar y probar localmente:
```powershell
adb install bin\Release\net8.0-android\publish\com.tuempresa.trackingapp-Signed.apk
```

### Verificar que funcione correctamente:
- [ ] La app se instala sin errores
- [ ] Todas las funcionalidades trabajan
- [ ] No hay crashes
- [ ] Los datos persisten correctamente

---

## Paso 5: Preparar Recursos para Play Store

### 5.1 Capturas de pantalla
Necesitas al menos 2 capturas (máximo 8):
- Resolución mínima: 320px
- Resolución máxima: 3840px
- Formato: PNG o JPEG
- Sin transparencias

Recomendaciones:
- Captura la pantalla principal
- Captura el registro de alimentos
- Captura el registro de medicamentos
- Captura el calendario de medicamentos

### 5.2 Gráfico destacado
- Tamaño: 1024 x 500 px
- Formato: PNG o JPEG
- Sin transparencias

### 5.3 Ícono de alta resolución
- Tamaño: 512 x 512 px
- Formato: PNG (32 bits con transparencia)

### 5.4 Descripción de la app

**Título corto (máx 30 caracteres):**
```
Tracking de Alimentos
```

**Título completo (máx 50 caracteres):**
```
Tracking de Alimentos y Medicamentos
```

**Descripción corta (máx 80 caracteres):**
```
Registra alimentos y medicamentos para bebés, adultos y mascotas
```

**Descripción completa (máx 4000 caracteres):**
```
📱 Tracking de Alimentos y Medicamentos

Aplicación diseñada para llevar un control detallado de alimentos y medicamentos para bebés, adultos y animales.

✨ CARACTERÍSTICAS PRINCIPALES:

🍼 REGISTRO DE ALIMENTOS
• Registra tipo de alimento
• Cantidad consumida
• Unidades de medida (oz, ml, g, taza, cucharada)
• Hora exacta de consumo
• Historial completo

💊 GESTIÓN DE MEDICAMENTOS
• Registro de medicamentos con dosis
• Frecuencia personalizada (cada X horas)
• Calendario automático de próximas dosis
• Recordatorios visuales

📅 CALENDARIO INTELIGENTE
• Vista organizada por días
• Estados visuales (próximo, atrasado, confirmado)
• Filtrado por medicamento específico
• Edición de horarios
• Confirmación de dosis administradas

👥 MÚLTIPLES PERFILES
• Modo Bebé
• Modo Adulto
• Modo Animal/Mascota

🎯 IDEAL PARA:
• Padres de recién nacidos
• Cuidadores de adultos mayores
• Dueños de mascotas
• Personal médico
• Cualquier persona que necesite llevar control estricto

⚡ VENTAJAS:
✅ Interfaz intuitiva y fácil de usar
✅ No requiere conexión a internet
✅ Gratis y sin publicidad
✅ Datos privados (almacenados solo en tu dispositivo)
✅ Calendario visual con estados de alerta

Mantén un control preciso de la alimentación y medicación. ¡Descárgala ahora!
```

### 5.5 Política de Privacidad

Necesitas una URL con tu política de privacidad. Puedes usar servicios como:
- GitHub Pages (gratis)
- Google Sites (gratis)
- Tu propio sitio web

Ejemplo de política básica:
```
Esta aplicación no recopila, almacena ni comparte datos personales.
Todos los datos se almacenan localmente en tu dispositivo.
No utilizamos servicios de terceros ni análisis.
No hay publicidad ni rastreadores.
```

---

## Paso 6: Crear Cuenta en Google Play Console

1. Ve a: https://play.google.com/console
2. Regístrate ($25 USD pago único)
3. Completa tu perfil de desarrollador
4. Acepta los términos y condiciones

---

## Paso 7: Crear Nueva Aplicación

1. En Play Console, haz clic en "Crear aplicación"
2. Completa:
   - Nombre de la aplicación
   - Idioma predeterminado: Español
   - Tipo de aplicación: Aplicación
   - Categoría: Medicina (o Salud y bienestar)
   - Gratis o de pago: Gratis

---

## Paso 8: Configurar Ficha de Play Store

### Pestaña "Ficha de Play Store principal":
- [ ] Título de la aplicación
- [ ] Descripción corta
- [ ] Descripción completa
- [ ] Íconos de aplicación (512x512)
- [ ] Gráfico destacado (1024x500)
- [ ] Capturas de pantalla (mínimo 2)
- [ ] Categoría
- [ ] Información de contacto
- [ ] Política de privacidad (URL)

### Pestaña "Clasificación de contenido":
1. Completa el cuestionario
2. Selecciona "Salud" como categoría
3. Especifica que no hay contenido inapropiado

### Pestaña "Público objetivo":
- Edad objetivo: Selecciona rangos apropiados
- Si es para niños: Cumple con COPPA

---

## Paso 9: Subir el AAB

1. Ve a "Producción" o "Prueba interna/cerrada/abierta"
2. Haz clic en "Crear versión"
3. Sube el archivo `.aab`
4. Completa las notas de la versión:
```
Versión 1.0.0
• Lanzamiento inicial
• Registro de alimentos y medicamentos
• Calendario de dosis
• Soporte para bebés, adultos y animales
```

---

## Paso 10: Revisión y Publicación

### Revisión previa:
- [ ] Verifica toda la información
- [ ] Prueba el APK/AAB localmente
- [ ] Lee las políticas de Google Play

### Enviar a revisión:
1. Haz clic en "Enviar para revisión"
2. El proceso puede tomar de 1 a 7 días
3. Recibirás notificaciones por correo

### Estados posibles:
- ✅ **Aprobada**: ¡Tu app está en Play Store!
- ⚠️ **Cambios solicitados**: Corrige y vuelve a enviar
- ❌ **Rechazada**: Lee los motivos y apela si es necesario

---

## Paso 11: Post-Publicación

### Promoción:
- Comparte el enlace de Play Store
- Pide reseñas a usuarios
- Actualiza regularmente

### Actualizaciones:
Para actualizar:
1. Incrementa `AndroidVersionCode` en el .csproj
2. Compila nueva versión
3. Sube nuevo AAB
4. Envía para revisión

---

## Comandos Útiles Resumidos

```powershell
# Generar keystore
keytool -genkey -v -keystore tracking-app.keystore -alias trackingapp -keyalg RSA -keysize 2048 -validity 10000

# Compilar AAB de release
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet publish -f net8.0-android -c Release -p:AndroidPackageFormat=aab

# Instalar APK para pruebas
adb install bin\Release\net8.0-android\publish\*.apk

# Ver logs si hay problemas
adb logcat
```

---

## Checklist Final Antes de Publicar

- [ ] La app funciona sin crashes
- [ ] Todos los permisos necesarios están declarados
- [ ] Íconos y recursos gráficos listos
- [ ] Descripción y capturas de pantalla preparadas
- [ ] Política de privacidad publicada
- [ ] Keystore guardado en lugar seguro
- [ ] AAB firmado correctamente
- [ ] Probado en diferentes dispositivos
- [ ] Versión de prueba revisada
- [ ] Información legal completa

---

## Recursos Adicionales

- [Documentación oficial de Google Play](https://support.google.com/googleplay/android-developer)
- [Políticas de Google Play](https://play.google.com/about/developer-content-policy/)
- [Centro de ayuda de Play Console](https://support.google.com/googleplay/android-developer/answer/9859152)
- [.NET MAUI - Publicación Android](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/)

---

## ¡Buena Suerte! 🚀

Una vez publicada, tu aplicación estará disponible para millones de usuarios en Google Play Store.
