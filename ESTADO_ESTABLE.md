# 🛡️ PUNTO DE ESTABILIDAD Y GESTIÓN DE RAMAS

## 🚨 IMPORTANTE: LEER ANTES DE CONTINUAR

Esta versión del código (**v1.16**) ha sido identificada como el **Punto de Estabilidad Máxima**. 

Debido a problemas críticos encontrados en versiones posteriores (crashes en APK, errores de lógica en dosis), hemos retrocedido el repositorio a este estado.

### 📌 Estado Actual
- **Tag:** `stable-v1.16-baseline`
- **Rama Principal de Desarrollo:** `IA/dev-IA`
- **Estado:** Funcional, estable, sin crashes críticos en APK.

### 🌳 Política de Ramas (Branching Policy)

Cualquier nueva funcionalidad, experimento o intento de avanzar hacia versiones superiores (v1.17+) **DEBE** seguir estas reglas:

1.  **NUNCA** trabajar directamente sobre `master` o ramas antiguas inestables.
2.  **SIEMPRE** crear nuevas ramas partiendo de este punto estable.

#### Cómo crear una nueva rama segura:

```bash
# 1. Asegúrate de estar en el punto estable
git checkout stable-v1.16-baseline

# 2. Crea tu nueva rama
git checkout -b feature/nueva-funcionalidad
```

### ⚠️ Advertencia
Si el APK comienza a fallar o la aplicación se cierra inesperadamente, **descarta los cambios y vuelve a este punto**. No intentes parchar sobre una base inestable.

---
**Referencia:** Para volver a implementar las mejoras perdidas de forma segura, consulta: `GUIA_IMPLEMENTACION_DESDE_v1.16.md`
