# Análisis de Problemas - Medicamentos v1.8
**Fecha:** 17 de octubre, 2025  
**Problemas reportados por usuario**

---

## 🔴 Problemas Identificados

### 1. **Dosis no se confirman correctamente**
   - **Síntoma:** Al hacer clic en "Confirmar", la dosis no se marca como confirmada
   - **Causa probable:** 
     - El evento click funciona pero el DisplayAlert puede estar bloqueando
     - O la dosis se confirma pero no se actualiza el UI correctamente

### 2. **Historial no se muestra**
   - **Síntoma:** No aparecen las dosis confirmadas en el historial
   - **Causa probable:**
     - El historial se crea correctamente (línea 786-792 MainViewModel)
     - Pero el filtro `FilteredMedicationHistory` puede estar excluyendo los registros

### 3. **Eventos muestra más horas de las seleccionadas**
   - **Síntoma:** Si selecciona 3 días, muestra 5 o 7 días de eventos
   - **Causa CONFIRMADA:** 
     - **Línea 88 DataService.cs:** `await GenerateDosesForMedicationAsync(medication, 3); // 3 días por defecto`
     - El valor está **hardcodeado a 3** en lugar de usar `SelectedDays`
     - Cuando el usuario cambia el selector de días, no afecta la generación inicial

---

## 🔍 Código Problemático

### Problema 3: Días hardcodeados
**Archivo:** `Services/DataService.cs`  
**Línea:** 88

```csharp
public async Task AddMedicationAsync(Medication medication)
{
    medication.UserType = CurrentUserType;
    await _databaseService.SaveMedicationAsync(medication);
    Medications.Add(medication);
    await GenerateDosesForMedicationAsync(medication, 3); // ❌ HARDCODEADO A 3 DÍAS
}
```

**Solución:**
```csharp
public async Task AddMedicationAsync(Medication medication, int days)
{
    medication.UserType = CurrentUserType;
    await _databaseService.SaveMedicationAsync(medication);
    Medications.Add(medication);
    await GenerateDosesForMedicationAsync(medication, days); // ✅ Usar parámetro
}
```

---

## 🔍 Investigación Adicional Necesaria

### Problema 1 & 2: Confirmación y Historial

Necesito verificar:

1. **FilteredMedicationHistory** - ¿Está filtrando correctamente?
   ```csharp
   // Buscar este property en MainViewModel
   public ObservableCollection<MedicationHistory> FilteredMedicationHistory
   ```

2. **GroupedDoses** - ¿Se actualiza después de confirmar?
   ```csharp
   // Verificar si se llama OnPropertyChanged
   OnPropertyChanged(nameof(GroupedDoses));
   ```

3. **UI Binding** - ¿El CollectionView está bound correctamente?
   ```xaml
   <!-- En MainPage.xaml, buscar binding de historial -->
   <CollectionView ItemsSource="{Binding FilteredMedicationHistory}" ...>
   ```

---

## 📝 Plan de Acción

### Fix Inmediato: Problema 3 (Días hardcodeados)

1. ✅ Modificar `AddMedicationAsync` para aceptar parámetro `days`
2. ✅ En `MainViewModel.AddMedication()` pasar `SelectedDays` al servicio
3. ✅ Testing: Cambiar selector de días y verificar que genera las dosis correctas

### Investigación: Problemas 1 & 2 (Confirmación e Historial)

1. ⏳ Leer código de `FilteredMedicationHistory`
2. ⏳ Verificar binding en XAML
3. ⏳ Agregar más logging para debugging
4. ⏳ Probar en dispositivo y verificar Output Window

---

## 🛠️ Próximos Pasos

1. **Implementar fix para días hardcodeados** (alta prioridad)
2. **Investigar filtro de historial** (alta prioridad)
3. **Verificar confirmación de dosis** (alta prioridad)
4. **Build y test en dispositivo real**

---

**Estado:** En progreso - Implementando fixes
