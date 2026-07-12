# ⚡ Guía Rápida - 5 Minutos

## 1️⃣ Crear tu archivo de asignaciones

Crea `Data/mis_asignaciones.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manualAssignments>
	<assignment driver="TuNombre" train="4701"/>
</manualAssignments>
```

## 2️⃣ Activar en Program.cs

Descomenta esta línea en `Program.cs`:

```csharp
Project.LoadManualAssignments("Data/mis_asignaciones.xml", specs);
```

## 3️⃣ Ejecutar

```bash
cd Obsidian
dotnet run
```

## 4️⃣ Buscar tu conductor en la salida

```
Maquinista TuNombre:
  4701 (05:35 - 06:35)    ← Tu asignación manual
  4704 (07:10 - 08:08)    ← Asignado automáticamente
  ...
```

---

## 💡 IDs de Trenes Disponibles (Plan Agosto26V4, Frecuencia "lab")

### Turno Matutino (05:00 - 14:00)
- `4701`, `4901`, `4703`, `4903`, `4705`, `4905`, `4707`, `4907`, `4709`
- `5001` a `5034` (trenes cortos)

### Turno Vespertino (14:00 - 23:00)  
- `4719`, `4919`, `4721`, `4721`, `4723`, `4923`, `4725`, `4925`, `4727`, `4927`, `4729`
- `5035` a `5054` (trenes cortos)

### Turno Nocturno (20:00 - 00:00)
- `4731`, `4931`, `4733`, `4933`, `4501`

---

## ⚠️ Recuerda

- Un conductor no puede trabajar más de **9 horas 10 minutos** por día
- No puede conducir más de **5 horas continuas** sin descanso
- Necesita al menos **45 minutos** de descanso entre bloques

¡Eso es todo! Para más detalles, consulta:
- **[ASIGNACIONES_MANUALES.md](ASIGNACIONES_MANUALES.md)** - Documentación completa
- **[CASOS_DE_USO.md](CASOS_DE_USO.md)** - Ejemplos prácticos
