# 🚂 Obsidian - Sistema de Asignación de Turnos para Maquinistas

Sistema de optimización para asignar **WorkBlocks** (bloques de trabajo/trenes) a **Maquinistas**, respetando restricciones operativas y regulatorias.

## 🎯 Funcionalidades Principales

### 1. **Carga de Datos**
- Importa asimilaciones desde archivos XML (topologías, rutas)
- Procesa planes de circulación con diferentes frecuencias
- Empareja trenes impares y pares automáticamente

### 2. **Optimización Automática**
- Asigna bloques de trabajo a maquinistas minimizando cantidad de conductores
- Respeta restricciones:
  - ⏱️ **Jornada máxima** (MaxPayload: 9h 10min por defecto)
  - 🚗 **Tiempo de conducción continua** (MaxDrivingTime: 5h por defecto)
  - ☕ **Tiempo mínimo de descanso** (MinIddleTime: 45min por defecto)
  - 🔗 **Tiempo máximo entre trenes de un bloque** (MaxTrainBlockBreakingTime: 2h)

### 3. **🔧 Asignaciones Manuales** (¡NUEVO!)
Permite pre-asignar trenes específicos a conductores antes de ejecutar la optimización automática.

#### **Dos formas de uso:**

**Opción A: Código directo**
```csharp
PlanRestrictions specs = new PlanRestrictions();
specs.ManualAssignments.Add(new ManualAssignment("JuanPerez", "4701"));
specs.ManualAssignments.Add(new ManualAssignment("MariaLopez", "4901"));
```

**Opción B: Archivo XML**
```csharp
Project.LoadManualAssignments("Data/manual_assignments.xml", specs);
```

Formato del XML:
```xml
<?xml version="1.0" encoding="utf-8"?>
<manualAssignments>
	<assignment driver="JuanPerez" train="4701"/>
	<assignment driver="MariaLopez" train="4901"/>
</manualAssignments>
```

#### **Comportamiento:**
✅ Las asignaciones manuales se aplican **primero**, antes del algoritmo  
✅ Se **valida** que cumplan todas las restricciones  
⚠️ Las asignaciones inválidas se muestran como advertencias y se ignoran  
✅ Los conductores manuales pueden recibir **más bloques automáticamente**  
✅ Los bloques restantes se asignan de forma **óptima**  

### 4. **Generación de Reportes**
- Informe de texto con detalles de asignaciones
- Generación de informes LaTeX profesionales
- Métricas de uso y eficiencia

## 🚀 Uso Rápido

```csharp
// 1. Configurar restricciones
PlanRestrictions specs = new PlanRestrictions();
specs.MaxPayload = new TimeSpan(9, 10, 0);

// 2. (Opcional) Cargar asignaciones manuales
Project.LoadManualAssignments("Data/manual_assignments.xml", specs);

// 3. Cargar proyecto
Project proyecto = new Project(
	"Data/toposfm227.xml",      // Topología
	"Data/rautasfm227.xml",     // Rutas
	"Agosto26V4",               // ID del plan
	"lab",                      // Frecuencia
	specs
);

// 4. Optimizar
PlanResult resultado = proyecto.Optimize(specs);

// 5. Generar informe
Console.WriteLine(resultado.Report);
var latex = LatexReportGenerator.GenerateLatexReport(proyecto, resultado, specs);
File.WriteAllText("informe.tex", latex);
```

## 📁 Estructura del Proyecto

```
Obsidian/
├── Logica/
│   ├── Basics.cs           # Clases base (Train, WorkBlock, Maquinista, etc.)
│   ├── Project.cs          # Carga de datos y gestión del proyecto
│   ├── Planner.cs          # Algoritmo de optimización
│   └── LatexReportGenerator.cs
├── Data/
│   ├── toposfm227.xml      # Asimilaciones (tiempos de trayectos)
│   ├── rautasfm227.xml     # Planes de circulación
│   ├── manual_assignments_example.xml   # Ejemplo con IDs reales
│   └── manual_assignments_valid.xml     # Ejemplo válido simple
├── Program.cs              # Punto de entrada
└── ASIGNACIONES_MANUALES.md  # Documentación detallada
```

## 📖 Documentación Adicional

- **[INICIO_RAPIDO.md](Obsidian/INICIO_RAPIDO.md)** - ⚡ Comienza en 5 minutos
- **[ASIGNACIONES_MANUALES.md](Obsidian/ASIGNACIONES_MANUALES.md)** - Guía completa sobre asignaciones manuales
- **[CASOS_DE_USO.md](Obsidian/CASOS_DE_USO.md)** - Ejemplos prácticos de casos reales
- **[RESUMEN_CAMBIOS.md](Obsidian/RESUMEN_CAMBIOS.md)** - Detalles técnicos de la implementación

## 🛠️ Tecnologías

- **.NET 10** (C# 14.0)
- **LINQ** para procesamiento de datos
- **XML LINQ** para parsing de archivos de configuración

## 🤝 Contribuir

Este proyecto sirve para optimizar la asignación de turnos respetando normativas laborales y operativas. Las mejoras son bienvenidas.

---

**Repositorio:** https://github.com/laretics/Obsidian
