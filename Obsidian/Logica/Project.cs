using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Obsidian.Logica
{
	internal class Project
	{
		public Dictionary<string, Asimilacion> Asimilations { get; private set; }
		public List<WorkBlock> Blocks { get; private set; }
		private List<Train> mcolTrainsOdd;
		private List<Train> mcolTrainsEven;
		public Project(string topoPath, string rautaPath, string planId, string freq, PlanRestrictions specs)
		{
			Asimilations = new Dictionary<string, Asimilacion>();
			ImportAssimilations(topoPath);			
			Blocks = new List<WorkBlock>();
			mcolTrainsOdd = new List<Train>();
			mcolTrainsEven = new List<Train>();
			LoadTrains(rautaPath, planId, freq);
			EmparejarTrenes(specs);
		}

		public string DataReport()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"Cargadas {Asimilations.Count} asimilaciones.");
			sb.AppendLine($"Procesadas {Blocks.Count} cargas de trabajo.");
			sb.AppendLine($"Quedan {mcolTrainsEven.Count} trenes pares desemparejados:");
			foreach (Train tren in mcolTrainsEven)
				sb.AppendLine(tren.ToString());
			sb.AppendLine($"Quedan {mcolTrainsOdd.Count} trenes impares desemparejados:");
			foreach (Train tren in mcolTrainsOdd)
				sb.AppendLine(tren.ToString());
			sb.AppendLine();
			sb.AppendLine("Bloques:");
			foreach (WorkBlock bloque in Blocks)
			{
				sb.AppendLine(bloque.ToString());
			}
			return sb.ToString();
		}

		private void ImportAssimilations(string ruta)
		{
			Asimilations = new Dictionary<string, Asimilacion>();
			var doc = XDocument.Load(ruta);
			var asimilacionSection = doc.Root.Element("asimilation");
			if (asimilacionSection == null)
				throw new Exception("No se encontró la sección <asimilation> en el XML.");

			foreach (var item in asimilacionSection.Elements("item"))
			{
				string id = item.Attribute("id")?.Value;
				if (string.IsNullOrEmpty(id)) continue;

				TimeSpan total = TimeSpan.Zero;
				foreach (var trip in item.Elements("trip"))
				{
					var timeStr = trip.Attribute("time")?.Value;
					var stopStr = trip.Attribute("stop")?.Value;
					if (!string.IsNullOrEmpty(timeStr))
						total += TimeSpan.Parse(timeStr);
					if (!string.IsNullOrEmpty(stopStr))
						total += TimeSpan.Parse(stopStr);
				}
				Asimilations[id] = new Asimilacion { Id = id, Duracion = total };
			}
		}

		private void LoadTrains(string ruta, string planId, string freq)
		{
			var doc = XDocument.Load(ruta);
			var plansSection = doc.Root.Element("plans");
			if (null == plansSection)
				throw new Exception("No encontró la sección <plans> en el XML");

			foreach (var item in plansSection.Elements("plan"))
			{
				string id = item.Attribute("id")?.Value;
				if (string.IsNullOrEmpty(id)) continue;
				if (id.Equals(planId))
					LoadTrainsFromPlan(item, freq);
			}
		}
		private void LoadTrainsFromPlan(XElement root, string freqFilter)
		{
			foreach (XElement conjunto in root.Elements("circulations"))
			{
				foreach (XElement item in conjunto.Elements())
				{
					if (item.Name == "cir")
						LoadTrain(freqFilter, item);
					else if (item.Name == "block")
					{
						foreach (XElement child in item.Elements())
							LoadTrain(freqFilter, child, item.Attribute("asm")?.Value, item.Attribute("freq")?.Value);
					}
				}
			}
		}
		private void LoadTrain(string freqFilter, XElement root, string? asimilationId = null, string? freq = null)
		{
			string? xAsimilationId = null == asimilationId ? root.Attribute("asm")?.Value : asimilationId;
			string? xFreq = null == freq ? root.Attribute("freq")?.Value : freq;
			if (null != xFreq && xFreq.Equals(freqFilter) && null != xAsimilationId && Asimilations.ContainsKey(xAsimilationId))
			{
				Train tren = new Train();
				tren.Asimilacion = Asimilations[xAsimilationId];
				tren.Id = root.Attribute("id")?.Value ?? "";
				tren.HoraSalida = TimeSpan.Parse(root.Attribute("dep")?.Value ?? "00:00:00");
				if (tren.IsOdd)
					mcolTrainsOdd.Add(tren);
				else
					mcolTrainsEven.Add(tren);
			}
		}

		/// <summary>
		/// Empareja trenes impares y pares según la lógica de prefijo y hora.
		/// Si el gap entre impar y par es demasiado grande, crea dos bloques separados.
		/// </summary>
		public void EmparejarTrenes(PlanRestrictions specs)
		{
			// Ordenar por hora de salida
			mcolTrainsOdd.Sort((a, b) => a.HoraSalida.CompareTo(b.HoraSalida));
			mcolTrainsEven.Sort((a, b) => a.HoraSalida.CompareTo(b.HoraSalida));

			var nuevosBlocks = new List<WorkBlock>();
			var imparesRestantes = new List<Train>(mcolTrainsOdd);
			var paresRestantes = new List<Train>(mcolTrainsEven);

			// Emparejar impares con pares
			for (int i = 0; i < imparesRestantes.Count;)
			{
				var impar = imparesRestantes[i];
				string prefijo = impar.Id.Length >= 2 ? impar.Id.Substring(0, 2) : "";

				var par = paresRestantes
					.Where(t => t.Id.StartsWith(prefijo) && t.HoraSalida > impar.HoraLlegada)
					.OrderBy(t => t.HoraSalida)
					.FirstOrDefault();

				if (par != null)
				{
					TimeSpan gap = par.HoraSalida - impar.HoraLlegada;

					if (gap <= specs.MaxTrainBlockBreakingTime)   // ← NUEVA LÓGICA
					{
						// Gap aceptable → bloque combinado
						nuevosBlocks.Add(new WorkBlock(impar, par));
					}
					else
					{
						// Gap demasiado grande → dos bloques independientes
						nuevosBlocks.Add(new WorkBlock(impar));
						nuevosBlocks.Add(new WorkBlock(par));
					}

					paresRestantes.Remove(par);
					imparesRestantes.RemoveAt(i); // No incrementar i
				}
				else
				{
					nuevosBlocks.Add(new WorkBlock(impar));
					imparesRestantes.RemoveAt(i);
				}
			}

			// Agregar pares que nunca encontraron impar
			foreach (var par in paresRestantes)
			{
				nuevosBlocks.Add(new WorkBlock(par));
			}

			// Actualizar estado
			mcolTrainsOdd = imparesRestantes;
			mcolTrainsEven = paresRestantes;
			Blocks = nuevosBlocks;
		}

		/// <summary>
		/// Asigna los bloques a los maquinistas de forma secuencial, respetando que un maquinista solo puede tomar un bloque si ha terminado el anterior.
		/// </summary>
		public PlanResult Optimize(PlanRestrictions specs)
		{
			Planner planner = new Planner();
			planner.Blocks = Blocks;
			return planner.Optimize(specs);
		}
	}
}
