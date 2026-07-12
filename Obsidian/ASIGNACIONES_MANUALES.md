# Asignaciones Manuales de Trenes a Maquinistas

## 📋 ¿Para qué sirve?

Esta funcionalidad permite **pre-asignar manualmente** ciertos trenes a maquinistas específicos antes de ejecutar el algoritmo de optimización automática. Esto es útil cuando:

- Necesitas que un conductor específico lleve ciertos trenes por razones operativas
- Quieres forzar ciertas combinaciones por experiencia del conductor
- Tienes restricciones externas (vacaciones, turnos específicos, etc.)

## 🚀 Cómo usar

### Opción 1: Directamente en código

```csharp
PlanRestrictions specs = new PlanRestrictions();
specs.MaxPayload = new TimeSpan(9, 10, 0);

// Asignar trenes manualmente
specs.ManualAssignments.Add(new ManualAssignment("Conductor01", "31901"));
specs.ManualAssignments.Add(new ManualAssignment("Conductor01", "31902"));
specs.ManualAssignments.Add(new ManualAssignment("Conductor02", "31903"));

Project proyecto = new Project("Data/toposfm227.xml", "Data/rautasfm227.xml", "Agosto26V4", "lab", specs);
PlanResult salida = proyecto.Optimize(specs);
```

### Opción 2: Desde archivo XML

1. Crea un archivo XML con el siguiente formato:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manualAssignments>
	<assignment driver="Conductor01" train="31901"/>
	<assignment driver="Conductor01" train="31902"/>
	<assignment driver="Conductor02" train="31903"/>
</manualAssignments>
```

2. Carga las asignaciones antes de crear el proyecto:

```csharp
PlanRestrictions specs = new PlanRestrictions();
Project.LoadManualAssignments("Data/manual_assignments.xml", specs);

Project proyecto = new Project("Data/toposfm227.xml", "Data/rautasfm227.xml", "Agosto26V4", "lab", specs);
PlanResult salida = proyecto.Optimize(specs);
```

## ⚙️ Comportamiento

1. **Primero se aplican las asignaciones manuales**
   - Se crean los maquinistas especificados (si no existen)
   - Se asignan los bloques que contienen los trenes especificados
   - Se valida que las asignaciones cumplan las restricciones (MaxPayload, MaxDrivingTime, etc.)

2. **Luego se ejecuta la optimización automática**
   - Solo se asignan los bloques restantes
   - Se respetan las asignaciones manuales previas
   - Los maquinistas manuales pueden recibir más bloques automáticamente

## ⚠️ Validaciones y Advertencias

El sistema mostrará mensajes en consola:

- ✓ **Asignación exitosa**: La asignación manual cumple todas las restricciones
- ⚠️ **Tren no encontrado**: El ID del tren no existe en el plan
- ⚠️ **Asignación inválida**: La asignación viola restricciones (jornada máxima, tiempo de conducción, etc.)

**Nota**: Si una asignación manual es inválida, el tren se asignará automáticamente en su lugar.

## 📊 Ejemplo de Salida

```
📋 Asignación cargada: Conductor01 → 31901
📋 Asignación cargada: Conductor01 → 31902
📋 Asignación cargada: Conductor02 → 31903
✓ Total de 3 asignaciones manuales cargadas

=== DATOS DEL PROYECTO ===
Cargadas 42 asimilaciones.
Procesadas 28 cargas de trabajo.

✓ Asignación manual: Conductor01 → Bloque con tren 31901
✓ Asignación manual: Conductor01 → Bloque con tren 31902
✓ Asignación manual: Conductor02 → Bloque con tren 31903

=== RESULTADO DE LA OPTIMIZACIÓN ===
Maquinista Conductor01:
  31901 (06:00 - 08:30)
  31902 (09:15 - 11:45)
  ...

Maquinista Conductor02:
  31903 (07:00 - 09:30)
  ...
```

## 🔧 Archivos de Ejemplo

Consulta `Data/manual_assignments_example.xml` para ver un ejemplo completo de formato XML.
