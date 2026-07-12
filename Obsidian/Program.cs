using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Obsidian.Logica;


// Ejemplo de cómo cargar los datos desde XML (asimilaciones y bloques)
// Puedes adaptar la ruta y el formato según tus necesidades
class Program
{
	static void Main(string[] args)
	{
		PlanRestrictions specs = new PlanRestrictions();
		specs.MaxPayload = new TimeSpan(9, 10, 0);

		// ============================================================================
		// 🔧 ASIGNACIONES MANUALES - Dos formas de uso:
		// ============================================================================

		// OPCIÓN 1: Directamente en código (útil para pruebas rápidas)
		// ----------------------------------------------------------------------------
		// specs.ManualAssignments.Add(new ManualAssignment("JuanPerez", "4701"));
		// specs.ManualAssignments.Add(new ManualAssignment("MariaLopez", "4901"));

		// OPCIÓN 2: Desde archivo XML (recomendado para producción)
		// ----------------------------------------------------------------------------
		// Project.LoadManualAssignments("Data/manual_assignments_valid.xml", specs);

		// COMPORTAMIENTO:
		// - Las asignaciones manuales se aplican PRIMERO, antes del algoritmo automático
		// - Se valida que cada asignación cumpla las restricciones (MaxPayload, MaxDrivingTime, etc.)
		// - Si una asignación manual es inválida, se muestra una advertencia y se ignora
		// - Los conductores manuales pueden recibir más bloques automáticamente después
		// - Los bloques no asignados manualmente se distribuyen de forma automática
		// ============================================================================

		// Ejemplo de carga de asimilaciones desde toposfm227.xml
		Project proyecto = new Project("Data/toposfm227.xml", "Data/rautasfm227.xml", "Agosto26V4", "lab",specs);
		Project.LoadManualAssignments("Data/manacor_turnos.xml",specs);

		Console.WriteLine("=== DATOS DEL PROYECTO ===");
		Console.WriteLine(proyecto.DataReport());
		Console.WriteLine();

		PlanResult salida = proyecto.Optimize(specs);

		Console.WriteLine("=== RESULTADO DE LA OPTIMIZACIÓN ===");
		Console.WriteLine(salida.Report);

		var latex = LatexReportGenerator.GenerateLatexReport(proyecto, salida, specs);
		System.IO.File.WriteAllText("informe_latex.tex", latex);
		Console.WriteLine("✓ Informe LaTeX generado: informe_latex.tex");
	}
}
