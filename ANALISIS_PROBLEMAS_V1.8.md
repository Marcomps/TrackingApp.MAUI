# Análisis de Problemas - Versión 1.6
**Fecha:** 17 de octubre, 2025  
**Reportado por:** Usuario  

---

## 🔴 Problemas Reportados

### 1. **Solo se muestran 2 eventos de medicación (debería mostrar todas las horas)**
   - **Descripción:** El CollectionView de eventos combinados solo muestra 2 elementos
   - **Ubicación:** MainPage.xaml línea 260 (`FilteredCombinedEvents`)
   - **Causa Probable:** 
     - `MaximumHeightRequest` en ScrollView o CollectionView demasiado pequeño
     - Filtro de fechas muy restrictivo
     - No se están generando todas las dosis programadas

### 2. **Citas médicas no se muestran a pesar de estar registradas**
   - **Descripción:** CollectionView de citas aparece vacío aunque se agregó una cita
   - **Ubicación:** MainPage.xaml línea 364 (`FilteredAppointments`)
   - **Causa Probable:**
     - Filtro de fechas en `FilteredAppointments` (línea 842-854 MainViewModel.cs)
     - `GetDateRange()` no incluye la fecha de la cita registrada
     - Cita no se guardó correctamente en la base de datos
     - No se llama a `OnPropertyChanged(nameof(FilteredAppointments))` después de agregar

### 3. **Botón "Confirmar" resuelto pero puede afectar eventos**
   - **Estado:** ✅ Resuelto en v1.6
   - **Nota:** Verificar que el fix no afecte la generación de eventos

---

## 🔍 Análisis del Código

### FilteredCombinedEvents (Eventos de Medicación)
```csharp
// MainViewModel.cs líneas 728-746
public ObservableCollection<MedicationEvent> FilteredCombinedEvents
{
    get
    {
        var (startDate, endDate) = GetDateRange(); // ⚠️ Filtro de fechas
        var filtered = CombinedMedicationEvents
            .Where(e => e.EventTime >= startDate && e.EventTime <= endDate);

        // Aplicar filtro de medicamento si hay uno seleccionado
        if (SelectedMedicationId.HasValue)
        {
            filtered = filtered.Where(e => e.MedicationId == SelectedMedicationId.Value);
        }

        var ordered = filtered.OrderByDescending(e => e.EventTime).ToList();
        return new ObservableCollection<MedicationEvent>(ordered);
    }
}
```

**Problema:** El filtro de fechas puede estar limitando a solo HOY, y si las dosis programadas son para horas futuras del día de hoy, solo mostraría las próximas 2.

### FilteredAppointments (Citas Médicas)
```csharp
// MainViewModel.cs líneas 842-854
public ObservableCollection<MedicalAppointment> FilteredAppointments
{
    get
    {
        var (startDate, endDate) = GetDateRange(); // ⚠️ Mismo filtro de fechas
        var filtered = Appointments
            .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
            .OrderBy(a => a.AppointmentDate)
            .ToList();
        
        return new ObservableCollection<MedicalAppointment>(filtered);
    }
}
```

**Problema:** Si la cita fue programada para una fecha futura y el filtro está en "Hoy", NO se mostrará.

### SelectedHistoryRange
```csharp
private string _selectedHistoryRange = "Hoy"; // ⚠️ Por defecto solo HOY
```

**Problema:** El filtro predeterminado es "Hoy", lo que limita eventos y citas a solo el día actual.

---

## 🛠️ Soluciones Propuestas

### Solución 1: Aumentar HeightRequest de CollectionView de Eventos
- Cambiar `MaximumHeightRequest="500"` en ScrollView de eventos (línea 259)
- O remover la restricción de altura completamente

### Solución 2: Cambiar filtro predeterminado a "Esta semana" o "Todo"
- En MainViewModel.cs línea ~16: `private string _selectedHistoryRange = "Esta semana";`
- O modificar GetDateRange() para incluir más días por defecto

### Solución 3: Agregar OnPropertyChanged para FilteredAppointments
- Verificar que AddAppointmentCommand llame a `OnPropertyChanged(nameof(FilteredAppointments))`
- Verificar que la cita se esté guardando en la base de datos

### Solución 4: Verificar generación de MedicationDoses
- Confirmar que cuando se agrega un medicamento, se generan todas las dosis programadas
- Verificar que RebuildCombinedEvents() se llame correctamente

---

## 📋 Pasos de Debugging

1. **Verificar datos en colecciones:**
   - Agregar logging en FilteredCombinedEvents para ver cuántos elementos hay
   - Agregar logging en FilteredAppointments para ver cuántas citas hay

2. **Verificar filtro de fechas:**
   - Revisar qué rango devuelve GetDateRange() actualmente
   - Cambiar temporalmente el filtro a "Todo" y verificar si aparecen más elementos

3. **Verificar base de datos:**
   - Confirmar que las citas se guardan correctamente
   - Confirmar que las dosis programadas se generan correctamente

---

## ✅ Plan de Acción

1. [ ] Cambiar filtro predeterminado de "Hoy" a "Esta semana"
2. [ ] Remover o aumentar MaximumHeightRequest de eventos
3. [ ] Agregar logging para debug en FilteredCombinedEvents
4. [ ] Agregar logging para debug en FilteredAppointments
5. [ ] Verificar AddAppointmentCommand notifica cambios
6. [ ] Rebuild y testing

---

**Próximo paso:** Implementar las soluciones y verificar funcionamiento.
