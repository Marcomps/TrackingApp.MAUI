# 💾 Persistencia de Datos con SQLite

## ✅ ¡Implementación Completada!

La aplicación ahora guarda **automáticamente** todo el historial en una base de datos local SQLite en cada dispositivo Android.

---

## 🎯 ¿Qué se guarda?

### ✅ Alimentos
- Tipo de alimento
- Cantidad y unidad
- Hora de consumo
- Tipo de usuario

### ✅ Medicamentos
- Nombre
- Dosis
- Frecuencia (horas)
- Hora de primera dosis
- Tipo de usuario

### ✅ Dosis de Medicamentos
- Hora programada
- Hora real (si fue editada)
- Estado de confirmación
- Si fue editada manualmente

---

## 📍 Ubicación de la Base de Datos

La base de datos se almacena en:
```
/data/data/com.tuempresa.trackingapp/files/tracking.db3
```

**Importante:** Esta ubicación es privada y solo accesible por la aplicación.

---

## 🚀 Funcionamiento Automático

### Al Iniciar la App
1. Se carga automáticamente el historial de alimentos
2. Se cargan todos los medicamentos registrados
3. Se restauran las dosis de medicamentos con su estado

### Al Agregar Datos
✅ **Alimentos:** Se guardan inmediatamente en la base de datos
✅ **Medicamentos:** Se guardan junto con su calendario de dosis
✅ **Confirmaciones:** Se actualizan instantáneamente
✅ **Ediciones:** Se persisten automáticamente

### Al Cerrar la App
✅ **Todos los datos permanecen guardados**
✅ No se pierde nada al reiniciar el dispositivo
✅ Los datos persisten incluso si cierras la app forzosamente

---

## 🔄 Ventajas de SQLite

| Característica | Beneficio |
|----------------|-----------|
| **Offline** | Funciona sin internet |
| **Rápido** | Acceso instantáneo a datos |
| **Confiable** | Base de datos probada y estable |
| **Local** | Privacidad total, datos en tu dispositivo |
| **Eficiente** | Poco consumo de espacio y batería |

---

## 📊 Cambios Técnicos Realizados

### 1. Paquetes NuGet Agregados
```
✅ sqlite-net-pcl (versión 1.9.172)
✅ SQLitePCLRaw.bundle_green (versión 2.1.2)
```

### 2. Modelos Actualizados

Todos los modelos ahora tienen:
- `[PrimaryKey, AutoIncrement]` - ID autoincremental
- `[Ignore]` - Propiedades calculadas que no se guardan en BD
- Inicializadores por defecto para propiedades string

**Archivos modificados:**
- `Models/FoodEntry.cs`
- `Models/Medication.cs`
- `Models/MedicationDose.cs`

### 3. Nuevo Servicio: DatabaseService

**Ubicación:** `Services/DatabaseService.cs`

**Funciones principales:**
```csharp
// Alimentos
await GetAllFoodEntriesAsync()
await SaveFoodEntryAsync(entry)
await DeleteFoodEntryAsync(entry)

// Medicamentos
await GetAllMedicationsAsync()
await SaveMedicationAsync(medication)
await DeleteMedicationAsync(medication)

// Dosis
await GetAllDosesAsync()
await SaveDoseAsync(dose)
await DeleteDoseAsync(dose)

// Utilidades
await ClearAllDataAsync()
await GetDatabasePathAsync()
await GetDatabaseSizeAsync()
```

### 4. DataService Actualizado

Ahora usa métodos `async` para todas las operaciones:
- `AddFoodEntryAsync()`
- `AddMedicationAsync()`
- `GenerateDosesForMedicationAsync()`
- `ConfirmDoseAsync()`
- `EditDoseTimeAsync()`
- `RegenerateDosesAsync()`

### 5. ViewModel Actualizado

Todos los comandos ahora son asíncronos y guardan en BD automáticamente.

---

## 🎮 Uso para el Usuario

**¡No cambia nada!** La app funciona exactamente igual, pero ahora:

✅ Los datos se guardan automáticamente
✅ Al cerrar y abrir la app, todo permanece
✅ No necesitas hacer nada especial
✅ Es completamente transparente

---

## 🔍 Verificar la Base de Datos

### En Android (con adb):
```powershell
# Conectar al dispositivo
adb shell

# Ir a la carpeta de la app
cd /data/data/com.tuempresa.trackingapp/files

# Ver la base de datos
ls -lh tracking.db3

# Opcional: Copiar BD al PC para inspección
adb pull /data/data/com.tuempresa.trackingapp/files/tracking.db3 ./
```

### Herramientas para inspeccionar:
- **DB Browser for SQLite** (https://sqlitebrowser.org/)
- **SQLite Studio** (https://sqlitestudio.pl/)

---

## 📦 Tamaño de la Base de Datos

### Estimaciones:
- **Base de datos vacía:** ~10 KB
- **100 entradas de alimentos:** ~30 KB
- **10 medicamentos con 1 mes de dosis:** ~50 KB
- **Uso intensivo (1 año):** ~2-5 MB

**Conclusión:** Ocupa muy poco espacio 💾

---

## 🛡️ Seguridad y Privacidad

### ✅ Privacidad Total
- Los datos **NUNCA** salen del dispositivo
- No se envían a ningún servidor
- No hay conexión a internet requerida
- Solo la app puede acceder a los datos

### ✅ Respaldo Manual
Si quieres hacer respaldo:
1. Usa `adb` para copiar el archivo `tracking.db3`
2. Guárdalo en tu PC o nube personal
3. Para restaurar, copia el archivo de vuelta

---

## 🔧 Funciones Futuras Sugeridas

### Próximas Mejoras:
- [ ] **Exportar a CSV/Excel** - Para análisis externo
- [ ] **Importar/Exportar BD** - Backup y restauración fácil
- [ ] **Sincronización en la nube** - Google Drive, Dropbox
- [ ] **Estadísticas** - Gráficos de consumo
- [ ] **Limpieza automática** - Borrar datos antiguos
- [ ] **Múltiples perfiles** - Varios bebés/mascotas

---

## 📝 Comandos Útiles

### Compilar con SQLite
```powershell
cd "c:\Users\PC\Desktop\Traking food\TrackingApp.MAUI\TrackingApp"
dotnet build
```

### Ejecutar en Android
```powershell
dotnet build -t:Run -f net8.0-android
```

### Limpiar y reconstruir
```powershell
dotnet clean
dotnet build
```

---

## ❓ Preguntas Frecuentes

### ¿Los datos se borran al desinstalar la app?
❌ **Sí**, al desinstalar la app se pierden todos los datos.
✅ **Solución:** Implementar exportación/importación de datos.

### ¿Puedo acceder a la BD desde otra app?
❌ **No**, Android protege los datos de cada app.
✅ Solo esta app puede acceder a su propia base de datos.

### ¿Qué pasa si actualizo la app?
✅ **Los datos se mantienen** al actualizar la versión de la app.

### ¿Funciona sin internet?
✅ **Sí**, SQLite es completamente local y offline.

### ¿Cuánto espacio ocupa?
💾 Muy poco, aproximadamente 2-5 MB con uso intensivo de 1 año.

### ¿Los datos están encriptados?
⚠️ **No** por defecto, pero Android protege la carpeta de la app.
📌 Se puede agregar encriptación en el futuro si es necesario.

---

## 🎉 Resumen

### Antes (Sin SQLite):
❌ Datos solo en memoria
❌ Se perdían al cerrar la app
❌ No había historial

### Ahora (Con SQLite):
✅ Datos persistentes
✅ Historial completo
✅ Funcionamiento automático
✅ Privacidad total
✅ Sin conexión a internet necesaria

---

## 🚀 ¡Ya está listo!

La persistencia de datos está completamente implementada y funcional. La aplicación ahora guarda automáticamente todo el historial de alimentos y medicamentos en una base de datos local SQLite.

**No se requiere ninguna acción adicional del usuario. Todo funciona automáticamente.**

---

**Última actualización:** Octubre 2025
**Versión:** 1.1.0 (con persistencia SQLite)
