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
        // Ejemplo de carga de asimilaciones desde toposfm227.xml
        Project proyecto = new Project("Data/toposfm227.xml", "Data/rautasfm227.xml", "Inv2026", "lab");

        Console.WriteLine(proyecto.DataReport());

        PlanRestrictions restrictions = new PlanRestrictions();
        restrictions.MaxPayload = new TimeSpan(9, 10, 0);

        PlanResult salida = proyecto.Optimize(restrictions);
        Console.WriteLine(salida.Report);

        Console.ReadLine();
    }
}

    // Importa asimilaciones desde toposfm227.xml, sección <layout><asimilation>
    

    

// Ejemplo de XML para asimilaciones (asimilaciones.xml):
/*
<Asimilaciones>
  <Asimilacion Id="A1" Duracion="01:30:00" />
  <Asimilacion Id="A2" Duracion="00:45:00" />
</Asimilaciones>
*/

// Ejemplo de XML para bloques de trabajo (bloques.xml):
/*
<BloquesTrabajo>
  <Bloque Id="B1">
    <Tren Id="T1" AsimilacionId="A1" HoraSalida="08:00:00" />
    <Tren Id="T2" AsimilacionId="A2" HoraSalida="10:00:00" />
  </Bloque>
  <Bloque Id="B2">
    <Tren Id="T3" AsimilacionId="A1" HoraSalida="12:00:00" />
  </Bloque>
</BloquesTrabajo>
*/
