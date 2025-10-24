# ✅ Checklist de Verificación Rápida - v1.31

## 🎯 Test Crítico: Simeticona (5 minutos)

Este test valida el fix principal aplicado en v1.31.

### ⚡ Pasos Rápidos:

1. **Instalar APK**
   ```powershell
   cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI"
   .\install-and-test.ps1
   ```

2. **Abrir la app en el dispositivo**

3. **Agregar Medicamento:**
   - Nombre: `Simeticona`
   - Dosis: `40 mg`
   - Primera dosis: `12:05 AM` (00:05)
   - Frecuencia: `8 horas`
   - Duración: `7 días`

4. **Verificar Calendario Generado:**

   **Si la hora actual es 2:30 PM (14:30):**
   
   ❌ **INCORRECTO (Bug anterior):**
   ```
   12:05 AM (mañana)  ← Primera dosis
   8:05 AM (mañana)
   4:05 PM (mañana)
   ```
   
   ✅ **CORRECTO (Fix v1.31):**
   ```
   4:05 PM (HOY)      ← Próxima dosis calculada correctamente
   12:05 AM (mañana)
   8:05 AM (mañana)
   ```

### 📊 Cálculo Esperado:

```
Hora configurada: 12:05 AM
Hora actual:      2:30 PM (14:30)
Frecuencia:       8 horas (480 minutos)

Tiempo transcurrido = 14h 25min = 865 minutos
Dosis transcurridas = Math.Ceiling(865 / 480) = Math.Ceiling(1.8) = 2
Próxima dosis = 12:05 AM + (2 × 8h) = 4:05 PM ✅
```

### ✅ Criterios de Éxito:

- [ ] Próxima dosis es **4:05 PM** (hoy)
- [ ] NO muestra 12:05 AM del día siguiente como primera
- [ ] Las siguientes dosis mantienen patrón: 12:05 AM, 8:05 AM, 4:05 PM
- [ ] Los **5 minutos** se mantienen en todas las dosis

---

## 🔍 Test de Historial (3 minutos)

Valida que usa última dosis confirmada como referencia.

### ⚡ Pasos Rápidos:

1. **Agregar Medicamento:**
   - Nombre: `Ibuprofeno`
   - Dosis: `400 mg`
   - Primera dosis: `6:00 AM`
   - Frecuencia: `6 horas`

2. **Confirmar dos dosis:**
   - ✅ 6:00 AM
   - ✅ 12:00 PM

3. **Cerrar app completamente** (Force Stop o deslizar en multitarea)

4. **Reabrir app**

5. **Verificar:**
   
   ✅ **CORRECTO:**
   ```
   Próxima dosis: 6:00 PM  ← (12:00 PM + 6h)
   ```
   
   ❌ **INCORRECTO (Bug anterior):**
   ```
   Próxima dosis: 12:00 AM ← Hora incorrecta
   ```

### ✅ Criterios de Éxito:

- [ ] Historial muestra las 2 dosis confirmadas
- [ ] Próxima dosis es **6:00 PM** (no 12:00 AM)
- [ ] Hora se calcula desde última confirmada (12:00 PM)

---

## 🌙 Test de Medianoche (2 minutos)

Valida que las transiciones PM → AM son correctas.

### ⚡ Pasos Rápidos:

1. **Agregar Medicamento:**
   - Nombre: `Paracetamol`
   - Primera dosis: `11:45 PM`
   - Frecuencia: `4 horas`

2. **Verificar Calendario:**

   ✅ **CORRECTO:**
   ```
   11:45 PM (hoy)
   3:45 AM (mañana)    ← Cruza medianoche correctamente
   7:45 AM (mañana)
   11:45 AM (mañana)
   ```

### ✅ Criterios de Éxito:

- [ ] Los **45 minutos** se mantienen en todas las dosis
- [ ] La transición 11:45 PM → 3:45 AM es correcta
- [ ] NO hay horas con :00 minutos

---

## 📋 Resumen de Tests

| # | Test | Tiempo | Prioridad | Estado |
|---|------|--------|-----------|--------|
| 1 | Simeticona - Cálculo con Math.Ceiling | 5 min | 🔴 CRÍTICO | ⬜ |
| 2 | Ibuprofeno - Última dosis confirmada | 3 min | 🔴 CRÍTICO | ⬜ |
| 3 | Paracetamol - Cruce de medianoche | 2 min | 🟡 ALTA | ⬜ |

**Tiempo total estimado:** 10 minutos

---

## 🐛 Qué Buscar (Bugs Potenciales)

### ❌ Síntomas de Bug:

1. **Horas se reinician a 12:00 AM**
   - Todas las dosis muestran 12:00 AM o 12:00 PM
   - Los minutos se pierden (ej: 12:05 AM → 12:00 AM)

2. **Próxima dosis incorrecta cuando FirstDoseTime pasó**
   - Muestra la próxima como "mañana" en lugar de "hoy"
   - Salta dosis intermedias

3. **Historial ignorado**
   - Calcula desde FirstDoseTime aunque haya dosis confirmadas
   - Al reabrir app, las dosis cambian incorrectamente

### ✅ Señales de Éxito:

1. **Cálculos precisos**
   - Próxima dosis tiene sentido basándose en última confirmada
   - Los minutos se mantienen (ej: :05, :15, :30, :45)

2. **Persistencia correcta**
   - Al cerrar/reabrir, las dosis NO cambian
   - Historial se mantiene intacto

3. **Logs informativos**
   - Si conectas con ADB, los logs muestran el cálculo paso a paso

---

## 📱 Instalación Rápida

### Opción 1: Script PowerShell (Recomendado)
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI"
.\install-and-test.ps1
```

### Opción 2: Manual
```powershell
adb install -r "TrackingApp\bin\Release\net9.0-android\publish\com.trackingapp.nutrition-Signed.apk"
adb shell am start -n com.trackingapp.nutrition/crc64e9e7e55a3f2f6c7e.MainActivity
```

---

## 🔧 Comandos Útiles

```powershell
# Ver logs en tiempo real (Ctrl+C para detener)
adb logcat | Select-String "RecalculateNextDoses"

# Reset completo de la app
adb shell pm clear com.trackingapp.nutrition

# Desinstalar
adb uninstall com.trackingapp.nutrition

# Ver dispositivos conectados
adb devices
```

---

## 📝 Reporte de Resultados

**Tester:** _________________  
**Fecha:** _________________  
**Dispositivo:** _________________  
**Android:** _________________  

### Resultados:

**Test 1 - Simeticona:**
- [ ] ✅ Pasó
- [ ] ❌ Falló - Descripción: _______________

**Test 2 - Ibuprofeno:**
- [ ] ✅ Pasó
- [ ] ❌ Falló - Descripción: _______________

**Test 3 - Paracetamol:**
- [ ] ✅ Pasó
- [ ] ❌ Falló - Descripción: _______________

### Bugs Encontrados:
```
1. 
2. 
3. 
```

### Observaciones:
```


```

---

**Versión APK:** 1.31  
**Fecha Compilación:** 20/10/2025 18:00:11  
**Tamaño:** 17 MB
