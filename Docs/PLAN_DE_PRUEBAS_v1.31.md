# 📋 Plan de Pruebas - TrackingApp v1.31

**Fecha:** 20 de Octubre, 2025  
**APK:** com.trackingapp.nutrition-Signed.apk (17 MB)  
**Versión:** 1.31 (Fix crítico de cálculo de dosis)

---

## 🎯 Objetivo de las Pruebas

Validar que el fix aplicado al cálculo de próximas dosis funcione correctamente en tres escenarios:

1. ✅ **Con historial confirmado** → Usa última dosis confirmada
2. ✅ **Sin historial, FirstDoseTime pasado** → Calcula dosis transcurridas con Math.Ceiling
3. ✅ **Sin historial, FirstDoseTime futuro** → Usa FirstDoseTime directamente

---

## 🧪 Casos de Prueba

### **TEST 1: Simeticona - Sin Historial, Tiempo Pasado** ⭐ CRÍTICO

**Objetivo:** Verificar que cuando FirstDoseTime ya pasó y no hay historial, calcula correctamente las dosis intermedias.

**Pasos:**
1. Instalar APK en dispositivo Android
2. Abrir la aplicación
3. Agregar nuevo medicamento:
   - **Nombre:** Simeticona
   - **Dosis:** 40 mg
   - **Primera dosis:** 12:05 AM (medianoche con 5 minutos)
   - **Frecuencia:** 8 horas
   - **Duración:** 7 días
4. Guardar medicamento
5. Observar las próximas dosis generadas

**Resultado Esperado:**
```
Si la hora actual es, por ejemplo, 2:30 PM (14:30):

Tiempo transcurrido desde 12:05 AM = 14 horas 25 minutos = 865 min
Dosis transcurridas = Math.Ceiling(865 / 480) = Math.Ceiling(1.8) = 2
Próxima dosis = 12:05 AM + (2 × 8h) = 4:05 PM

CALENDARIO ESPERADO:
✅ 4:05 PM (HOY)      ← Próxima pendiente
✅ 12:05 AM (MAÑANA)
✅ 8:05 AM (MAÑANA)
✅ 4:05 PM (MAÑANA)
...
```

**Criterios de Éxito:**
- ✅ NO debe mostrar 12:05 AM del día siguiente como próxima dosis
- ✅ DEBE calcular 4:05 PM como próxima dosis
- ✅ Las dosis siguientes mantienen el patrón cada 8 horas

---

### **TEST 2: Ibuprofeno - Con Historial Confirmado** ⭐ CRÍTICO

**Objetivo:** Verificar que usa la última dosis confirmada del historial para calcular próximas dosis.

**Pasos:**
1. Agregar medicamento:
   - **Nombre:** Ibuprofeno
   - **Dosis:** 400 mg
   - **Primera dosis:** 6:00 AM
   - **Frecuencia:** 6 horas
   - **Duración:** 5 días
2. Confirmar la dosis de 6:00 AM ✅
3. Confirmar la dosis de 12:00 PM ✅
4. **NO confirmar** la dosis de 6:00 PM (dejarla pendiente)
5. Cerrar completamente la aplicación
6. Esperar 5 minutos
7. Reabrir la aplicación

**Resultado Esperado:**
```
HISTORIAL:
✅ 6:00 AM  - Confirmada
✅ 12:00 PM - Confirmada (ÚLTIMA CONFIRMADA)

CÁLCULO:
Última confirmada = 12:00 PM
Próxima dosis = 12:00 PM + 6h = 6:00 PM

CALENDARIO AL REABRIR:
⏰ 6:00 PM (HOY)      ← Atrasada/Pendiente
🔔 12:00 AM (MAÑANA)
🔔 6:00 AM (MAÑANA)
...
```

**Criterios de Éxito:**
- ✅ Próxima dosis DEBE ser 6:00 PM (no 12:00 AM del día siguiente)
- ✅ Historial muestra las 2 dosis confirmadas
- ✅ Dosis de 6:00 PM aparece como pendiente/atrasada

---

### **TEST 3: Paracetamol - Cruce de Medianoche**

**Objetivo:** Verificar manejo correcto de dosis que cruzan la medianoche.

**Pasos:**
1. Agregar medicamento:
   - **Nombre:** Paracetamol
   - **Dosis:** 500 mg
   - **Primera dosis:** 11:45 PM
   - **Frecuencia:** 4 horas
   - **Duración:** 3 días
2. Observar calendario generado

**Resultado Esperado:**
```
CALENDARIO (desde 11:45 PM):
🔔 11:45 PM (HOY)
🔔 3:45 AM  (MAÑANA) ← Cruza medianoche correctamente
🔔 7:45 AM  (MAÑANA)
🔔 11:45 AM (MAÑANA)
🔔 3:45 PM  (MAÑANA)
...
```

**Criterios de Éxito:**
- ✅ Las horas NO se reinician a 12:00 AM
- ✅ Mantiene correctamente los 45 minutos en cada dosis
- ✅ Transición de PM a AM es correcta

---

### **TEST 4: Vitamina D - Primera Dosis Futura**

**Objetivo:** Verificar que si FirstDoseTime está en el futuro, no hace cálculos innecesarios.

**Pasos:**
1. Agregar medicamento:
   - **Nombre:** Vitamina D
   - **Dosis:** 1000 UI
   - **Primera dosis:** 9:00 AM (de mañana)
   - **Frecuencia:** 24 horas
   - **Duración:** 7 días
2. Observar calendario

**Resultado Esperado:**
```
CALENDARIO:
🔔 9:00 AM (MAÑANA)   ← Primera dosis, en el futuro
🔔 9:00 AM (PASADO MAÑANA)
🔔 9:00 AM (3 días después)
...
```

**Criterios de Éxito:**
- ✅ Primera dosis respeta la fecha/hora configurada
- ✅ No genera dosis para "hoy"
- ✅ Patrón cada 24 horas es correcto

---

### **TEST 5: Medicina Bebé - Frecuencia Mixta (4h 30min)**

**Objetivo:** Verificar cálculo correcto con frecuencias que incluyen minutos.

**Pasos:**
1. Agregar medicamento:
   - **Nombre:** Medicina Bebé
   - **Dosis:** 2.5 ml
   - **Primera dosis:** 3:15 PM
   - **Frecuencia:** 4 horas 30 minutos
   - **Duración:** 2 días
2. Observar calendario

**Resultado Esperado:**
```
CALENDARIO (desde 3:15 PM):
🔔 3:15 PM  (HOY)
🔔 7:45 PM  (HOY)      ← +4h 30min
🔔 12:15 AM (MAÑANA)   ← +4h 30min
🔔 4:45 AM  (MAÑANA)
🔔 9:15 AM  (MAÑANA)
...
```

**Criterios de Éxito:**
- ✅ Incremento correcto de 4h 30min entre dosis
- ✅ Los minutos (15, 45, 15, 45...) se mantienen correctamente
- ✅ No hay redondeos incorrectos

---

### **TEST 6: Reapertura de App - Persistencia**

**Objetivo:** Verificar que al cerrar y reabrir la app, los cálculos se mantienen correctos.

**Pasos:**
1. Usar cualquier medicamento creado en tests anteriores
2. Confirmar 2-3 dosis ✅
3. Cerrar COMPLETAMENTE la aplicación (Force Stop)
4. Esperar 2-3 minutos
5. Reabrir la aplicación
6. Verificar:
   - Historial mantiene dosis confirmadas
   - Próximas dosis son correctas
   - Horas NO se reinician a 12:00 AM

**Resultado Esperado:**
- ✅ Historial intacto
- ✅ Próximas dosis calculadas desde última confirmada
- ✅ Base de datos SQLite funciona correctamente

---

### **TEST 7: Eliminación de Medicamento - Limpieza de Historial**

**Objetivo:** Verificar que al eliminar un medicamento, también se elimina su historial.

**Pasos:**
1. Crear medicamento de prueba
2. Confirmar 3-4 dosis
3. Ir a historial y verificar que aparecen las dosis
4. Eliminar el medicamento
5. Ir a historial nuevamente

**Resultado Esperado:**
- ✅ Medicamento eliminado de la lista
- ✅ Historial de ese medicamento también eliminado
- ✅ Otros medicamentos NO afectados

---

### **TEST 8: Logs de Debug** 🔍

**Objetivo:** Verificar que los logs de debug muestran información útil.

**Pasos:**
1. Conectar dispositivo por USB
2. Ejecutar: `adb logcat | findstr "RecalculateNextDoses"`
3. Abrir la app y crear un medicamento
4. Observar logs en consola

**Resultado Esperado:**
```
🔄 RecalculateNextDoses: Medication=Simeticona, Frequency=480min
  ℹ️ No hay historial. FirstDoseTime: 2025-10-20 00:05
  ⏱️ Tiempo transcurrido: 865 min, Dosis transcurridas: 2
  ➡️ Próxima dosis calculada: 2025-10-20 16:05
  🗑️ Eliminando 0 dosis pendientes...
  ✨ Generando 21 nuevas dosis...
```

**Criterios de Éxito:**
- ✅ Logs muestran el flujo completo
- ✅ Cálculos son visibles y verificables
- ✅ No hay errores/excepciones

---

## 📊 Matriz de Resultados

| Test | Descripción | Estado | Notas |
|------|-------------|--------|-------|
| 1 | Simeticona - Sin historial, tiempo pasado | ⬜ | |
| 2 | Ibuprofeno - Con historial confirmado | ⬜ | |
| 3 | Paracetamol - Cruce medianoche | ⬜ | |
| 4 | Vitamina D - Dosis futura | ⬜ | |
| 5 | Medicina Bebé - Frecuencia mixta | ⬜ | |
| 6 | Reapertura - Persistencia | ⬜ | |
| 7 | Eliminación - Limpieza historial | ⬜ | |
| 8 | Logs de debug | ⬜ | |

**Leyenda:**
- ⬜ Pendiente
- ✅ Pasó
- ❌ Falló
- ⚠️ Parcial

---

## 🐛 Bugs Conocidos (Pre-v1.31)

### ❌ BUG #1: Horas se reinician a 12:00 AM (RESUELTO v1.31)
**Descripción:** Al reabrir la app, las dosis mostraban "12:00 AM" en lugar de mantener las horas correctas.  
**Causa:** `RecalculateNextDosesFromLastConfirmedAsync` no calculaba dosis transcurridas cuando no había historial.  
**Fix:** Implementado `Math.Ceiling(elapsedMinutes / frequency)` para calcular dosis intermedias.

### ❌ BUG #2: Historial no se elimina con medicamento (VERIFICAR)
**Descripción:** Al eliminar un medicamento, su historial permanecía en la base de datos.  
**Status:** Pendiente de verificación en TEST 7.

---

## 📱 Información del APK

```
📍 Ruta: TrackingApp\bin\Release\net9.0-android\publish\
📱 Archivo: com.trackingapp.nutrition-Signed.apk
💾 Tamaño: 17 MB
📅 Fecha compilación: 20/10/2025 18:00:11
🔧 Framework: .NET MAUI 9.0
🎯 Target: Android (net9.0-android)
```

---

## 🔍 Comandos Útiles para Pruebas

### Instalar APK:
```bash
adb install -r "TrackingApp\bin\Release\net9.0-android\publish\com.trackingapp.nutrition-Signed.apk"
```

### Ver logs en tiempo real:
```bash
adb logcat | findstr "RecalculateNextDoses"
```

### Limpiar datos de la app (reset completo):
```bash
adb shell pm clear com.trackingapp.nutrition
```

### Ver base de datos SQLite:
```bash
adb shell "run-as com.trackingapp.nutrition cat /data/data/com.trackingapp.nutrition/files/medications.db3" > medications.db3
```

---

## ✅ Checklist Final

Antes de liberar a producción:

- [ ] Todos los tests pasan (8/8)
- [ ] Logs de debug verificados
- [ ] Sin crashes durante uso normal
- [ ] Persistencia de datos confirmada
- [ ] Transiciones de medianoche correctas
- [ ] Historial se elimina con medicamento
- [ ] Rendimiento aceptable (< 2s para generar calendario)
- [ ] Documentación actualizada

---

## 📝 Notas del Tester

_Espacio para notas durante las pruebas..._

```
Fecha: _____________
Dispositivo: _____________
Android Version: _____________

Observaciones:
- 
- 
- 

```

---

**Preparado por:** GitHub Copilot  
**Última actualización:** 20 de Octubre, 2025
