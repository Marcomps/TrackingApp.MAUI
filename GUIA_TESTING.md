# 🧪 GUÍA DE TESTING - Tracking App

## Cómo Probar la Aplicación

Esta guía te muestra todas las formas de probar tu aplicación de tracking en diferentes entornos.

---

## 🎯 Opciones de Testing

Tienes **3 opciones principales**:

1. ✅ **Visual Studio 2022** (Recomendado para MAUI)
2. ✅ **Visual Studio Code** (Ligero y rápido)
3. ✅ **Línea de comandos** (Sin IDE)

---

## 📱 OPCIÓN 1: Visual Studio 2022 (Recomendado)

### ¿Por qué Visual Studio 2022?
- ✅ **Soporte completo** para .NET MAUI
- ✅ **Emulador Android integrado**
- ✅ **Debugging visual** con breakpoints
- ✅ **Hot Reload** (recarga en caliente)
- ✅ **Diseñador XAML** incluido

### Requisitos Previos
```
✅ Visual Studio 2022 (versión 17.3 o superior)
✅ Workload: ".NET Multi-platform App UI development"
✅ Android SDK (se instala automáticamente)
```

### Paso 1: Abrir el Proyecto

1. Abre **Visual Studio 2022**
2. Clic en **"Abrir un proyecto o solución"**
3. Navega a:
   ```
   c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp\TrackingApp.csproj
   ```
4. Clic en **Abrir**

### Paso 2: Configurar el Target

En la barra superior de Visual Studio:

```
┌─────────────────────────────────────────────────┐
│ Debug ▼ │ net9.0-android ▼ │ Pixel 5 - API 33 ▼ │
└─────────────────────────────────────────────────┘
```

1. **Configuración:** Selecciona `Debug`
2. **Framework:** Selecciona `net9.0-android` (o `net8.0-android`)
3. **Dispositivo:** Selecciona un emulador Android

### Paso 3: Crear/Seleccionar Emulador

#### Si NO tienes emulador:

1. Clic en el dropdown de dispositivos
2. Selecciona **"Android Emulator Manager"**
3. Clic en **"+ Crear"**
4. Selecciona un dispositivo (recomendado: **Pixel 5**)
5. Selecciona una imagen del sistema (recomendado: **Android 13 - API 33**)
6. Clic en **Crear**
7. Espera a que se descargue (puede tardar 5-10 minutos)

#### Si ya tienes emulador:

1. Selecciónalo directamente del dropdown
2. Ejemplo: `Pixel 5 - API 33`

### Paso 4: Ejecutar la App

**Opción A: Con F5 (Debugging)**
```
Presiona F5 o clic en el botón verde ▶️
```
- Inicia con debugging
- Puedes poner breakpoints
- Puedes inspeccionar variables

**Opción B: Sin Debugging (Ctrl+F5)**
```
Presiona Ctrl+F5
```
- Inicia más rápido
- Sin debugging
- Mejor para pruebas rápidas

### Paso 5: Verificar la App

✅ **El emulador se abrirá automáticamente**
✅ **La app se instalará** (puede tardar 1-2 minutos la primera vez)
✅ **Verás el splash screen verde** con tu logo
✅ **La app se abrirá** lista para usar

### Debugging en Visual Studio 2022

#### Poner Breakpoints:
1. Abre un archivo .cs (ejemplo: `MainViewModel.cs`)
2. Clic en el margen izquierdo (línea gris)
3. Aparece un círculo rojo 🔴
4. Cuando el código llegue ahí, se pausará

#### Inspeccionar Variables:
- Pasa el mouse sobre variables para ver valores
- Usa la ventana **"Autos"** o **"Locals"**
- Usa la ventana **"Watch"** para vigilar variables específicas

#### Hot Reload:
- Modifica el código XAML o C#
- Visual Studio lo actualizará automáticamente
- **No necesitas recompilar** (en la mayoría de casos)

---

## 💻 OPCIÓN 2: Visual Studio Code

### ¿Por qué VS Code?
- ✅ **Ligero y rápido**
- ✅ **Multiplataforma**
- ✅ **Bueno para edición rápida**
- ⚠️ No tiene emulador integrado (debes configurarlo)

### Requisitos Previos
```
✅ Visual Studio Code
✅ Extensión: C# (Microsoft)
✅ Extensión: .NET MAUI (opcional)
✅ .NET SDK 8 o 9 instalado
✅ Android SDK instalado manualmente
```

### Paso 1: Abrir el Proyecto

1. Abre **Visual Studio Code**
2. Menú: **File → Open Folder**
3. Selecciona la carpeta:
   ```
   c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp
   ```
4. Clic en **Seleccionar carpeta**

### Paso 2: Terminal Integrada

1. Menú: **View → Terminal** (o Ctrl+`)
2. Deberías ver PowerShell en la parte inferior

### Paso 3: Ejecutar con Comandos

#### Opción A: Ejecutar directamente
```powershell
dotnet build -t:Run -f net8.0-android
```

#### Opción B: Primero compilar, luego ejecutar
```powershell
# Compilar
dotnet build -f net8.0-android

# Ejecutar
dotnet build -t:Run -f net8.0-android
```

### Paso 4: Configurar Emulador (Si no tienes)

**Si no tienes Android Studio instalado:**

1. Descarga Android Studio: https://developer.android.com/studio
2. Instala Android Studio
3. Abre Android Studio → Tools → AVD Manager
4. Crea un emulador (Pixel 5, API 33)
5. Inicia el emulador manualmente
6. Luego ejecuta el comando de VS Code

### Debugging en VS Code

#### Configurar launch.json:

1. Crea `.vscode/launch.json`:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET MAUI Android",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build-android",
            "program": "${workspaceFolder}/bin/Debug/net8.0-android/TrackingApp.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "console": "internalConsole",
            "stopAtEntry": false
        }
    ]
}
```

2. Presiona **F5** para iniciar debugging

---

## ⌨️ OPCIÓN 3: Línea de Comandos (Sin IDE)

### Ventajas
- ✅ **Rápido y directo**
- ✅ **No requiere IDE pesado**
- ✅ **Ideal para CI/CD**

### Comandos Básicos

#### 1. Compilar la app:
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet build -f net8.0-android
```

#### 2. Ejecutar en emulador/dispositivo:
```powershell
dotnet build -t:Run -f net8.0-android
```

#### 3. Limpiar y recompilar:
```powershell
dotnet clean
dotnet build -f net8.0-android
```

#### 4. Generar APK:
```powershell
dotnet publish -f net8.0-android -c Release
```

---

## 📱 Tipos de Testing

### 1. Testing en Emulador Android

**Ventajas:**
- ✅ No requiere dispositivo físico
- ✅ Puedes probar diferentes versiones de Android
- ✅ Puedes simular diferentes tamaños de pantalla

**Desventajas:**
- ⚠️ Más lento que dispositivo real
- ⚠️ Consume muchos recursos (RAM, CPU)
- ⚠️ No prueba hardware real (cámara, sensores)

**Recomendado para:**
- Desarrollo inicial
- Pruebas de UI/UX
- Testing de lógica de negocio

### 2. Testing en Dispositivo Físico

**Ventajas:**
- ✅ Rendimiento real
- ✅ Prueba hardware real
- ✅ Experiencia real del usuario

**Cómo conectar dispositivo:**

1. **Habilitar modo desarrollador en Android:**
   - Ajustes → Acerca del teléfono
   - Toca 7 veces en "Número de compilación"
   - Vuelve → Opciones de desarrollador
   - Activa "Depuración USB"

2. **Conectar USB:**
   - Conecta el cable USB
   - Acepta la autorización en el teléfono
   - En Visual Studio, aparecerá tu dispositivo

3. **Ejecutar:**
   - Selecciona tu dispositivo físico
   - Presiona F5 (Visual Studio)
   - O ejecuta: `dotnet build -t:Run -f net8.0-android`

### 3. Testing de Funcionalidades Específicas

#### Probar Registro de Alimentos:
1. Abre la app
2. Selecciona "Bebé" (o Adulto/Mascota)
3. Selecciona tipo de alimento
4. Ingresa cantidad (ejemplo: 120)
5. Selecciona unidad (ml, oz, g)
6. Clic en "Registrar Alimento"
7. ✅ Verifica que aparece en el historial

#### Probar Registro de Medicamentos:
1. Selecciona pestaña "Medicamentos"
2. Ingresa nombre (ejemplo: "Paracetamol")
3. Ingresa dosis (ejemplo: "500mg")
4. Selecciona frecuencia (ejemplo: "Cada 6 horas")
5. Selecciona primera dosis
6. Clic en "Registrar Medicamento"
7. ✅ Verifica que aparece en el calendario

#### Probar Persistencia (SQLite):
1. Registra algunos alimentos y medicamentos
2. **Cierra completamente la app** (desliza hacia arriba en multitarea)
3. **Vuelve a abrir la app**
4. ✅ Verifica que los datos siguen ahí

#### Probar Filtros:
1. Registra varios medicamentos diferentes
2. En el calendario, clic en "Filtrar por medicamento"
3. Selecciona un medicamento específico
4. ✅ Verifica que solo muestra ese medicamento

#### Probar Confirmación de Dosis:
1. En el calendario de medicamentos
2. Busca una dosis pendiente (amarilla)
3. Clic en "Confirmar"
4. ✅ Verifica que cambia a verde

#### Probar Edición de Horario:
1. En el calendario, busca una dosis
2. Clic en "Editar hora"
3. Cambia la hora
4. ✅ Verifica que se actualiza

---

## 🐛 Testing y Debugging Avanzado

### Ver Logs en Tiempo Real

#### En Visual Studio 2022:
1. Menú: **View → Output**
2. Selecciona "Debug" en el dropdown
3. Verás todos los logs de la app

#### En Línea de Comandos (si tienes adb):
```powershell
# Instalar Android SDK Platform-Tools primero
# Luego:
adb logcat | Select-String "TrackingApp"
```

### Inspeccionar Base de Datos SQLite

#### Opción 1: Extraer la base de datos
```powershell
# Con adb instalado:
adb pull /data/data/com.trackingapp.nutrition/files/tracking.db3 ./tracking.db3
```

#### Opción 2: Usar SQLite Browser
1. Descarga DB Browser for SQLite: https://sqlitebrowser.org/
2. Abre el archivo tracking.db3
3. Explora las tablas: FoodEntry, Medication, MedicationDose

### Probar Diferentes Escenarios

#### Escenario 1: Usuario Nuevo
- Instala la app
- Primera vez que abre
- ✅ Verifica que la base de datos se crea
- ✅ Verifica que no hay datos previos

#### Escenario 2: Uso Intensivo
- Registra 50+ alimentos
- Registra 10+ medicamentos
- ✅ Verifica que no hay lag
- ✅ Verifica que el scroll funciona bien

#### Escenario 3: Cambio de Usuario
- Registra datos para "Bebé"
- Cambia a "Adulto"
- Registra más datos
- ✅ Verifica que cada tipo tiene su propio historial

#### Escenario 4: Cambio de Fecha
- Cambia la fecha del sistema
- Abre la app
- ✅ Verifica que el calendario se actualiza

---

## 📊 Checklist de Testing

### Testing de UI ✅
- [ ] Todos los botones funcionan
- [ ] Formularios validan datos correctamente
- [ ] Campos de texto aceptan entrada
- [ ] Pickers muestran opciones correctas
- [ ] Colores se ven correctamente
- [ ] Logo aparece en splash screen
- [ ] Iconos se muestran en launcher

### Testing de Funcionalidad ✅
- [ ] Registro de alimentos funciona
- [ ] Registro de medicamentos funciona
- [ ] Calendario muestra dosis correctas
- [ ] Confirmación de dosis funciona
- [ ] Edición de horario funciona
- [ ] Filtros funcionan correctamente
- [ ] Cambio de tipo de usuario funciona

### Testing de Persistencia ✅
- [ ] Datos persisten al cerrar app
- [ ] Base de datos se crea automáticamente
- [ ] Datos se guardan inmediatamente
- [ ] No hay pérdida de datos

### Testing de Performance ✅
- [ ] App inicia en menos de 3 segundos
- [ ] No hay lag al agregar datos
- [ ] Scroll es fluido
- [ ] No hay memory leaks

---

## 🚀 Comandos Rápidos de Referencia

### Visual Studio 2022:
```
F5          → Iniciar con debugging
Ctrl+F5     → Iniciar sin debugging
Shift+F5    → Detener debugging
F9          → Toggle breakpoint
F10         → Step over
F11         → Step into
```

### VS Code + Terminal:
```powershell
# Compilar
dotnet build -f net8.0-android

# Ejecutar
dotnet build -t:Run -f net8.0-android

# Limpiar
dotnet clean

# Ver ayuda
dotnet build --help
```

---

## ❓ Problemas Comunes

### "No se encuentra ningún dispositivo"
**Solución:**
1. Abre Android Studio
2. Tools → AVD Manager
3. Inicia un emulador manualmente
4. Espera que inicie completamente
5. Vuelve a ejecutar en VS

### "Error de compilación"
**Solución:**
```powershell
dotnet clean
dotnet restore
dotnet build -f net8.0-android
```

### "App se cierra inmediatamente"
**Solución:**
1. Revisa los logs en Output (VS 2022)
2. Busca excepciones
3. Verifica que SQLite está incluido
4. Intenta en modo Release

### "Emulador muy lento"
**Solución:**
1. Habilita aceleración de hardware (HAXM en Intel, Hyper-V en AMD)
2. Asigna más RAM al emulador (AVD Manager → Edit)
3. Cierra otras aplicaciones
4. Usa dispositivo físico en su lugar

---

## 📚 Recursos Adicionales

### Documentación Relacionada:
- [COMANDOS_RAPIDOS.md](COMANDOS_RAPIDOS.md) - Referencia de comandos
- [GUIA_ANDROID.md](GUIA_ANDROID.md) - Guía completa de Android
- [README.md](README.md) - Documentación principal

### Links Útiles:
- .NET MAUI Docs: https://learn.microsoft.com/dotnet/maui/
- Android Emulator: https://developer.android.com/studio/run/emulator
- Visual Studio 2022: https://visualstudio.microsoft.com/

---

## 🎯 Recomendación Final

### Para empezar (más fácil):
**👉 Usa Visual Studio 2022**
- Todo integrado
- Un solo clic para ejecutar
- Debugging visual incluido

### Para desarrollo rápido:
**👉 Usa VS Code + Terminal**
- Más ligero
- Compilación rápida
- Ideal para cambios pequeños

### Para CI/CD y scripts:
**👉 Usa línea de comandos**
- Automatizable
- Sin dependencias de UI
- Perfecto para Jenkins/GitHub Actions

---

## ✅ Próximos Pasos

1. **Elige tu método** (Visual Studio 2022 recomendado)
2. **Abre el proyecto**
3. **Selecciona un emulador** (o conecta dispositivo)
4. **Presiona F5** (o ejecuta comando)
5. **¡Prueba la app!** 🎉

---

**¿Dudas?** Consulta la documentación completa en el índice:
[INDICE.md](INDICE.md)

**Versión:** 1.0.0  
**Fecha:** Octubre 2025  
**Actualizado:** Guía completa de testing
