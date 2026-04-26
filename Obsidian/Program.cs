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
		// Ejemplo de carga de asimilaciones desde toposfm227.xml
		Project proyecto = new Project("Data/toposfm227.xml", "Data/rautasfm227.xml", "propXavi", "lab",specs);
        PlanResult salida = proyecto.Optimize(specs);
		var latex = LatexReportGenerator.GenerateLatexReport(proyecto, salida, specs);
		System.IO.File.WriteAllText("informe_latex.tex", latex);

    }
}
