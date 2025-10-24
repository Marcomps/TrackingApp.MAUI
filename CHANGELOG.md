# 📝 CHANGELOG - TrackingApp

## [v1.31] - 2025-10-20

### 🔥 CRÍTICO - Fix de Cálculo de Dosis

#### 🐛 Bug Corregido
**Problema:** Cuando `FirstDoseTime` ya había pasado y no existía historial de dosis confirmadas, el sistema simplemente movía la próxima dosis al día siguiente en lugar de calcular las dosis intermedias que deberían haber ocurrido.

**Ejemplo del problema:**
```
Medicamento: Simeticona
Primera dosis configurada: 12:05 AM
Frecuencia: 8 horas
Hora actual: 2:30 PM (14:30)

COMPORTAMIENTO ANTERIOR (BUG):
Próxima dosis: 12:05 AM (mañana) ❌

COMPORTAMIENTO CORRECTO (v1.31):
Cálculo: 14h 25min transcurridos ÷ 8h = 1.8 dosis → Math.Ceiling = 2 dosis
Próxima dosis: 12:05 AM + (2 × 8h) = 4:05 PM (HOY) ✅
```

#### ✅ Solución Implementada

**Archivo modificado:** `Services/DataService.cs`  
**Método:** `RecalculateNextDosesFromLastConfirmedAsync`  
**Líneas:** 285-320

**Cambios:**
```csharp
// ANTES (v1.30):
if (nextDoseTime < now) {
    nextDoseTime = nextDoseTime.AddDays(1);
}

// DESPUÉS (v1.31):
if (nextDoseTime < now) {
    var elapsedMinutes = (now - nextDoseTime).TotalMinutes;
    var dosesElapsed = Math.Ceiling(elapsedMinutes / medication.TotalFrequencyInMinutes);
    nextDoseTime = medication.FirstDoseTime.AddMinutes(dosesElapsed * medication.TotalFrequencyInMinutes);
}
```

**Consistencia con app.js:** Este cambio alinea el comportamiento de la app MAUI con la lógica original de `app.js` que usaba `Math.ceil()` para calcular dosis transcurridas.

---

### ✨ Nuevas Funcionalidades

#### 📊 Priorización de Historial
El método `RecalculateNextDosesFromLastConfirmedAsync` ahora implementa una jerarquía clara de prioridades:

1. **PRIORIDAD 1:** Si existe historial confirmado → usa última dosis confirmada
2. **PRIORIDAD 2:** Si no hay historial y FirstDoseTime pasó → calcula con Math.Ceiling
3. **PRIORIDAD 3:** Si FirstDoseTime está en el futuro → usa directamente

**Beneficio:** Garantiza que las dosis se calculen correctamente independientemente del estado de la aplicación.

#### 🔍 Logs de Debug Mejorados
Agregados logs detallados para debugging:
```
🔄 RecalculateNextDoses: Medication={Name}, Frequency={Minutes}min
  ✅ Último historial encontrado: {DateTime}
  ➡️ Cálculo: {Time} + {Frequency}min = {NextDose}
  
  // O en caso de no haber historial:
  ℹ️ No hay historial. FirstDoseTime: {DateTime}
  ⏱️ Tiempo transcurrido: {Minutes} min, Dosis transcurridas: {Count}
  ➡️ Próxima dosis calculada: {DateTime}
```

---

### 🧪 Pruebas Agregadas

**5 nuevos tests de edge cases** en `NextDoseCalculationEdgeCasesTests.cs`:

1. ✅ `FirstDosePassedSameDay_CalculatesCorrectNextDose`
   - Valida cálculo cuando FirstDose fue hoy pero ya pasó
   
2. ✅ `FirstDoseWasYesterday_CalculatesCorrectNextDose`
   - Valida cálculo cuando FirstDose fue ayer
   
3. ✅ `FirstDoseInFuture_UsesFirstDoseDirectly`
   - Valida que no hace cálculos innecesarios si FirstDose está en el futuro
   
4. ✅ `FrequencyWithMinutes_CalculatesCorrectly`
   - Valida cálculos con frecuencias mixtas (4h 30min, etc.)
   
5. ✅ `MultipleElapsedDoses_CalculatesCorrectNextDose`
   - Valida cálculo cuando han pasado múltiples dosis

**Total de tests:** 64 → **69 tests** (100% pasando)

---

### 📚 Documentación Agregada

1. **PLAN_DE_PRUEBAS_v1.31.md**
   - Plan completo con 8 casos de prueba detallados
   - Matriz de resultados
   - Comandos útiles para debugging

2. **VERIFICACION_RAPIDA_v1.31.md**
   - Checklist de verificación rápida (10 minutos)
   - 3 tests críticos prioritarios
   - Reporte de resultados

3. **install-and-test.ps1**
   - Script PowerShell para instalación automatizada
   - Menú interactivo con opciones de prueba
   - Validaciones pre-instalación

4. **LISTO_PARA_PRUEBAS.md**
   - Guía de inicio rápido
   - 3 opciones de instalación
   - Comandos útiles

5. **NUEVAS_PRUEBAS_DOSIS.md**
   - Documentación de los 5 tests nuevos agregados anteriormente
   - Ejemplos con medicamentos reales (Simeticona, Ibuprofeno, etc.)

---

### 🔧 Mejoras Técnicas

- ✅ Algoritmo de cálculo de dosis consistente entre JavaScript y C#
- ✅ Mejor manejo de casos edge (primera dosis pasada, sin historial, etc.)
- ✅ Logs de debug más informativos
- ✅ Cobertura de tests aumentada de 64 a 69

---

### 📦 APK Release

**Archivo:** `com.trackingapp.nutrition-Signed.apk`  
**Tamaño:** 17 MB  
**Ubicación:** `TrackingApp\bin\Release\net9.0-android\publish\`  
**Fecha:** 2025-10-20 18:00:11

---

## [v1.30] - 2025-10-20 (Anterior)

### 🐛 Bug Corregido
- ✅ **Dosis mantienen horarios correctos al reabrir app**
  - Método `SyncDosesWithHistoryAsync` corregido
  - Las dosis confirmadas no se regeneraban incorrectamente

### ✨ Nuevas Funcionalidades
- ✅ 5 nuevas pruebas unitarias para cálculo de dosis
  - Simeticona (12:05 AM → 8:05 AM)
  - Ibuprofeno (cada 6h, cruce de medianoche)
  - Medicina bebé (4h 30min)
  - Paracetamol (transición de fecha)
  - Vitamina D (diario por 7 días)

### 📚 Documentación
- ✅ Arquitectura MVVM documentada
- ✅ Documentos .md organizados en carpeta Docs/

---

## [v1.20] - 2025-10-XX (Anterior)

### ✨ Funcionalidades Iniciales
- ✅ Tracking de alimentos
- ✅ Tracking de medicamentos
- ✅ Calendario de dosis
- ✅ Historial de confirmación de dosis
- ✅ Base de datos SQLite local

### 🧪 Pruebas
- ✅ 59 pruebas unitarias iniciales
  - 31 tests de modelos
  - 14 tests de servicios
  - 14 tests de helpers

---

## Resumen de Evolución

| Versión | Tests | Bugs Críticos | APK Size | Fecha |
|---------|-------|---------------|----------|-------|
| v1.20 | 59 | - | 16.5 MB | 2025-10-XX |
| v1.30 | 64 | 1 corregido | 16.8 MB | 2025-10-20 |
| v1.31 | 69 | 1 corregido | 17.0 MB | 2025-10-20 |

---

## Roadmap Futuro (Opcional)

### 🚀 Posibles Mejoras

**Alta Prioridad:**
- [ ] Notificaciones push para recordatorios de dosis
- [ ] Exportar historial a PDF/Excel
- [ ] Modo oscuro

**Media Prioridad:**
- [ ] Gráficos de adherencia al tratamiento
- [ ] Recordatorios personalizables
- [ ] Sincronización en la nube

**Baja Prioridad:**
- [ ] Múltiples perfiles de usuario
- [ ] Integración con wearables
- [ ] Modo familiar

---

**Mantenido por:** Equipo de Desarrollo TrackingApp  
**Última actualización:** 2025-10-20
