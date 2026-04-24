using System;
using System.Collections.Generic;
using System.Text;

namespace Obsidian.Logica
{
	public class Asimilacion
	{
		public string Id { get; set; }
		public TimeSpan Duracion { get; set; }
	}

	public class WorkBlock
	{
		public List<Train> Trains { get; private set; }
		public WorkBlock(Train train)
		{
			Trains = new List<Train>();
			Trains.Add(train);
		}
		public WorkBlock(Train train1, Train train2)
		{
			Trains = new List<Train>();
			Trains.Add(train1);
			Trains.Add(train2);
		}
		public override string ToString()
		{
			if (Trains.Count > 1)
				return string.Format("Bloque {0} {1}", Trains[0].ToString(), Trains[1].ToString());
			return string.Format("Single {0}", Trains[0].ToString());
		}
	}

	public class Train
	{
		public string Id { get; set; }
		public Asimilacion Asimilacion { get; set; } = new Asimilacion();
		public TimeSpan HoraSalida { get; set; } // Hora relativa o absoluta según tu modelo
		public TimeSpan HoraLlegada { get => HoraSalida.Add(Asimilacion.Duracion); }
		public bool IsOdd
		{
			get
			{
				if (string.IsNullOrEmpty(Id)) return false;
				char last = Id[Id.Length - 1];
				return char.IsDigit(last) && ((last - '0') % 2 == 1);
			}
		}
		public override string ToString()
		{
			return $"{Id} ({HoraSalida:hh\\:mm} - {HoraLlegada:hh\\:mm})";
		}
	}


	public class Maquinista
	{
		public string Id { get; set; }
		public List<Train> TrenesAsignados { get; set; } = new List<Train>();
	}

	public class PlanRestrictions
	{	
		public bool ConsumeUmpaired { get; set; } = true;
		public TimeSpan MaxPayload { get; set; } = new TimeSpan(9, 0, 0);
		public TimeSpan MaxDrivingTime { get; set; } = new TimeSpan(3, 0, 0);
		public TimeSpan MinIddleTime { get; set; } = new TimeSpan(0, 45, 0);

	}
	public class PlanResult
	{
		public List<Maquinista> Schedules { get; set; } = new();
		public List<WorkBlock> Unassigned { get; set; } = new();

		public string Report
		{
			get
			{
				var sb = new StringBuilder();
				sb.AppendLine("=== ASIGNACIÓN DE MAQUINISTAS ===");
				foreach (var maq in Schedules)
				{
					sb.AppendLine($"Maquinista {maq.Id}:");
					if (maq.TrenesAsignados.Count == 0)
					{
						sb.AppendLine("  (Sin trenes asignados)");
					}
					else
					{
						foreach (var tren in maq.TrenesAsignados)
							sb.AppendLine($"  {tren}");
					}
					sb.AppendLine();
				}
				if (Unassigned != null && Unassigned.Count > 0)
				{
					sb.AppendLine("=== BLOQUES NO ASIGNADOS ===");
					foreach (var bloque in Unassigned)
						sb.AppendLine(bloque.ToString());
				}
				return sb.ToString();
			}
		}
	}

	
}
