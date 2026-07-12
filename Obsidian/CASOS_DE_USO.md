# 💡 Casos de Uso - Asignaciones Manuales

Este documento muestra ejemplos prácticos de cuándo y cómo usar las asignaciones manuales.

---

## 🎯 Caso 1: Conductor Senior en Trenes Específicos

**Situación:** Juan tiene más experiencia y debe llevar el primer tren del día (tramo difícil).

```xml
<manualAssignments>
	<assignment driver="JuanPerez_Senior" train="4701"/>
</manualAssignments>
```

**Resultado:** Juan llevará el tren 4701, y el sistema le asignará automáticamente más trenes después.

---

## 🎯 Caso 2: Turnos Fijos por Preferencia Laboral

**Situación:** María solo puede trabajar turno matutino, Carlos turno vespertino.

```xml
<manualAssignments>
	<!-- María - Turno matutino (empieza a las 6:00) -->
	<assignment driver="Maria" train="4901"/>

	<!-- Carlos - Turno vespertino (empieza a las 14:35) -->
	<assignment driver="Carlos" train="4719"/>
</manualAssignments>
```

**Resultado:** Cada conductor comienza en su horario preferido, el sistema completa su jornada.

---

## 🎯 Caso 3: Múltiples Conductores del Mismo Turno

**Situación:** Necesitas 3 conductores en el turno matutino (6:00-15:00).

```xml
<manualAssignments>
	<assignment driver="Conductor_Matutino_1" train="4701"/>  <!-- 5:35 -->
	<assignment driver="Conductor_Matutino_2" train="4901"/>  <!-- 6:00 -->
	<assignment driver="Conductor_Matutino_3" train="4703"/>  <!-- 6:35 -->
</manualAssignments>
```

**Resultado:** Los 3 conductores empiezan en horarios escalonados del mismo turno.

---

## 🎯 Caso 4: Asignación por Línea/Ruta Específica

**Situación:** Ciertos conductores están certificados solo para ciertas rutas.

```xml
<manualAssignments>
	<!-- Pedro solo puede conducir trenes de la serie 47xx -->
	<assignment driver="PedroRodriguez" train="4701"/>

	<!-- Ana solo puede conducir trenes de la serie 49xx -->
	<assignment driver="AnaGarcia" train="4901"/>
</manualAssignments>
```

**Resultado:** Cada conductor empieza en su línea certificada, el sistema respeta esas series.

---

## 🎯 Caso 5: Conductor de Respaldo/Reemplazo

**Situación:** Un conductor habitual está de vacaciones, asignas su reemplazo.

```xml
<manualAssignments>
	<!-- Conductor temporal que reemplaza a otro -->
	<assignment driver="Reemplazo_Temporal_Juan" train="4701"/>
	<assignment driver="Reemplazo_Temporal_Maria" train="4901"/>
</manualAssignments>
```

**Resultado:** Los reemplazos toman los turnos habituales de los conductores regulares.

---

## 🎯 Caso 6: Minimizar Conductores con Seed Inicial

**Situación:** Quieres que el algoritmo use menos conductores, das un "seed" inicial.

```xml
<manualAssignments>
	<!-- Forzar que estos trenes vayan al mismo conductor (si es posible) -->
	<assignment driver="SuperConductor" train="4701"/>  <!-- Mañana -->
	<!-- El sistema intentará asignarle más bloques al mismo conductor -->
</manualAssignments>
```

**Resultado:** El conductor recibe muchos trenes si las restricciones lo permiten.

---

## 🎯 Caso 7: Conductor en Capacitación

**Situación:** Un conductor nuevo solo puede llevar ciertos trenes bajo supervisión.

```xml
<manualAssignments>
	<!-- Conductor en entrenamiento - solo trenes cortos -->
	<assignment driver="Aprendiz_Luis" train="5001"/>  <!-- Tren corto 06:35-06:48 -->
</manualAssignments>
```

**Resultado:** El aprendiz lleva trenes cortos/seguros, el resto se asigna normalmente.

---

## ⚠️ Caso 8: Asignación Inválida (Advertencia)

**Situación:** Intentas asignar trenes que violan restricciones.

```xml
<manualAssignments>
	<!-- ❌ ESTO GENERARÁ ADVERTENCIA: Estos trenes están muy separados para un conductor -->
	<assignment driver="Conductor_Invalido" train="4701"/>  <!-- 05:35 -->
	<assignment driver="Conductor_Invalido" train="4731"/>  <!-- 20:35 - 15 horas después! -->
</manualAssignments>
```

**Resultado:**
```
⚠️ Advertencia: Asignación manual inválida (viola restricciones): Conductor_Invalido → Tren 4731
```

El sistema **rechaza** la segunda asignación porque violaría MaxPayload (9h 10min).

---

## 📊 Buenas Prácticas

### ✅ Recomendado:
- Asignar **1-2 trenes por conductor** manualmente
- Dejar que el algoritmo **complete la jornada** automáticamente
- Usar IDs descriptivos: `"JuanPerez_Senior"`, `"Turno_Matutino_A"`
- Validar que los trenes existan en el plan antes de asignar

### ❌ Evitar:
- Asignar **toda la jornada manualmente** (pierdes los beneficios de la optimización)
- Asignar trenes con **grandes gaps de tiempo** al mismo conductor
- Usar IDs genéricos sin significado: `"M1"`, `"Conductor123"`

---

## 🔍 Verificación de Asignaciones

Después de ejecutar el programa, busca en la consola:

```
✓ Asignación manual: JuanPerez → Bloque con tren 4701
⚠️ Advertencia: Asignación manual inválida (viola restricciones): Carlos → Tren 9999
```

Las asignaciones con ✓ se aplicaron correctamente.  
Las asignaciones con ⚠️ fueron rechazadas (el tren se asignará automáticamente).

---

## 📂 Archivos de Ejemplo Incluidos

- **`manual_assignments_example.xml`** - IDs reales del plan Agosto26V4
- **`manual_assignments_valid.xml`** - Ejemplo simple con 5 conductores (funciona sin advertencias)

Usa estos archivos como plantilla para tus propias asignaciones.
