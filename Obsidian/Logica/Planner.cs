using System;
using System.Collections.Generic;
using System.Text;

namespace Obsidian.Logica
{
	public class Planner
	{
		public List<WorkBlock> Blocks { get; set; } = new();

		public PlanResult Optimize(PlanRestrictions specs)
		{
			var bloquesOrdenados = new List<WorkBlock>(Blocks);
			bloquesOrdenados.Sort((a, b) => a.Trains[0].HoraSalida.CompareTo(b.Trains[0].HoraSalida));

			int minMaquinistas = 1;
			PlanResult mejorResultado = null;

			while (minMaquinistas <= bloquesOrdenados.Count)
			{
				var maquinistas = new List<Maquinista>();
				for (int i = 0; i < minMaquinistas; i++)
					maquinistas.Add(new Maquinista { Id = $"M{i + 1}" });

				var bloquesPendientes = new List<WorkBlock>(bloquesOrdenados);

				// Asignar bloques de un solo tren par al inicio si ConsumeUmpaired
				if (specs.ConsumeUmpaired)
				{
					var bloquesUnTrenPar = bloquesPendientes.Where(b => b.Trains.Count == 1 && !b.Trains[0].IsOdd).ToList();
					for (int i = 0; i < bloquesUnTrenPar.Count && i < maquinistas.Count; i++)
					{
						foreach (var tren in bloquesUnTrenPar[i].Trains)
							maquinistas[i].TrenesAsignados.Add(tren);
						bloquesPendientes.Remove(bloquesUnTrenPar[i]);
					}
				}

				// Estado de conducción continua para cada maquinista
				var drivingTimes = new TimeSpan[maquinistas.Count];
				var lastEndTimes = new TimeSpan[maquinistas.Count];

				foreach (var (maq, idx) in maquinistas.Select((m, i) => (m, i)))
				{
					if (maq.TrenesAsignados.Count > 0)
						lastEndTimes[idx] = maq.TrenesAsignados[^1].HoraLlegada;
				}

				while (bloquesPendientes.Count > 0)
				{
					bool asignado = false;
					for (int i = 0; i < maquinistas.Count; i++)
					{
						var maq = maquinistas[i];
						TimeSpan finUltimo = maq.TrenesAsignados.Count > 0
							? maq.TrenesAsignados[^1].HoraLlegada
							: TimeSpan.Zero;

						var bloque = bloquesPendientes.FirstOrDefault(b =>
						{
							if (b.Trains[0].HoraSalida < finUltimo) return false;

							// Jornada máxima
							TimeSpan nuevaInicio = maq.TrenesAsignados.Count > 0 ? maq.TrenesAsignados[0].HoraSalida : b.Trains[0].HoraSalida;
							TimeSpan nuevaFin = b.Trains[^1].HoraLlegada;
							if (maq.TrenesAsignados.Count > 0 && maq.TrenesAsignados[^1].HoraLlegada > nuevaFin)
								nuevaFin = maq.TrenesAsignados[^1].HoraLlegada;
							TimeSpan jornada = nuevaFin - nuevaInicio;
							if (jornada > specs.MaxPayload) return false;

							// Calcular conducción continua
							TimeSpan driving = drivingTimes[i];
							TimeSpan gap = b.Trains[0].HoraSalida - finUltimo;
							if (maq.TrenesAsignados.Count == 0 || gap >= specs.MinIddleTime)
								driving = TimeSpan.Zero;
							driving += b.Trains[^1].HoraLlegada - b.Trains[0].HoraSalida; // tiempo del bloque
							if (maq.TrenesAsignados.Count > 0)
								driving += b.Trains[0].HoraSalida - maq.TrenesAsignados[^1].HoraLlegada; // tiempo entre bloques

							if (driving > specs.MaxDrivingTime)
								return false;

							return true;
						});

						if (bloque != null)
						{
							// Actualizar conducción continua y fin
							TimeSpan gap = bloque.Trains[0].HoraSalida - finUltimo;
							if (maq.TrenesAsignados.Count == 0 || gap >= specs.MinIddleTime)
								drivingTimes[i] = TimeSpan.Zero;
							drivingTimes[i] += bloque.Trains[^1].HoraLlegada - bloque.Trains[0].HoraSalida;
							if (maq.TrenesAsignados.Count > 0)
								drivingTimes[i] += bloque.Trains[0].HoraSalida - maq.TrenesAsignados[^1].HoraLlegada;

							foreach (var tren in bloque.Trains)
								maq.TrenesAsignados.Add(tren);
							lastEndTimes[i] = bloque.Trains[^1].HoraLlegada;
							bloquesPendientes.Remove(bloque);
							asignado = true;
						}
					}
					if (!asignado) break;
				}

				if (bloquesPendientes.Count == 0)
				{
					mejorResultado = new PlanResult
					{
						Schedules = maquinistas,
						Unassigned = new List<WorkBlock>()
					};
					break;
				}
				minMaquinistas++;
			}

			// Si no se pudo asignar todo, devolver el mejor intento con todos los bloques no asignados
			if (mejorResultado == null)
			{
				mejorResultado = new PlanResult
				{
					Schedules = new List<Maquinista>(),
					Unassigned = new List<WorkBlock>(Blocks)
				};
			}
			return mejorResultado;
		}
	}
}
