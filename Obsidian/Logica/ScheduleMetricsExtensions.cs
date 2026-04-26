using System;
using System.Collections.Generic;
using System.Text;

namespace Obsidian.Logica
{
	public static class ScheduleMetricsExtensions
	{
		/// <summary>
		/// Calcula todas las métricas de un maquinista usando la clase ScheduleMetrics
		/// </summary>
		public static ScheduleMetrics GetMetrics(this Maquinista maq, PlanRestrictions specs)
		{
			var metrics = new ScheduleMetrics
			{
				DriverId = maq.Id
			};

			if (maq.TrenesAsignados.Count == 0)
				return metrics;

			// Ordenar trenes cronológicamente (importante)
			var trenes = maq.TrenesAsignados
							.OrderBy(t => t.HoraSalida)
							.ToList();

			// === Datos básicos del turno ===
			metrics.ScheduleBegin = trenes.First().HoraSalida;
			metrics.ScheduleEnd = trenes.Last().HoraLlegada;
			metrics.TrainsCount = trenes.Count;

			// === Tiempo de conducción total ===
			metrics.DrivingTime = trenes.Aggregate(TimeSpan.Zero,
				(total, tren) => total + (tren.HoraLlegada - tren.HoraSalida));

			// === Cálculo de pausas (gaps) y conducción continua ===
			TimeSpan maxContinuaActual = TimeSpan.Zero;
			TimeSpan continuaActual = TimeSpan.Zero;
			TimeSpan sumaPausas = TimeSpan.Zero;
			int gapsCount = 0;

			metrics.IddleMin = TimeSpan.MaxValue;
			metrics.IddleMax = TimeSpan.Zero;

			for (int i = 0; i < trenes.Count; i++)
			{
				var trenActual = trenes[i];
				var duracion = trenActual.HoraLlegada - trenActual.HoraSalida;
				continuaActual += duracion;

				if (i < trenes.Count - 1)
				{
					var trenSiguiente = trenes[i + 1];
					var gap = trenSiguiente.HoraSalida - trenActual.HoraLlegada;

					if (gap > TimeSpan.Zero)
					{
						gapsCount++;
						sumaPausas += gap;

						if (gap < metrics.IddleMin) metrics.IddleMin = gap;
						if (gap > metrics.IddleMax) metrics.IddleMax = gap;
					}

					// Reiniciar conducción continua si el descanso es suficiente
					if (gap >= specs.MinIddleTime)  // Usa el valor de las restricciones
					{
						if (continuaActual > maxContinuaActual)
							maxContinuaActual = continuaActual;

						continuaActual = TimeSpan.Zero;
					}
				}
			}

			// Último tramo de conducción continua
			if (continuaActual > maxContinuaActual)
				maxContinuaActual = continuaActual;

			metrics.MaxDrivingTime = maxContinuaActual;
			metrics.GapsCount = gapsCount;
			metrics.IddleAverage = gapsCount > 0
				? sumaPausas / gapsCount
				: TimeSpan.Zero;

			// === Distribución de carga por hora ===
			var distribucion = new Dictionary<int, double>(); // hora → minutos de conducción

			foreach (var tren in trenes)
			{
				int hora = tren.HoraSalida.Hours;
				double minutos = (tren.HoraLlegada - tren.HoraSalida).TotalMinutes;

				if (distribucion.ContainsKey(hora))
					distribucion[hora] += minutos;
				else
					distribucion[hora] = minutos;
			}

			metrics.AssignationsByHour = distribucion
				.OrderBy(x => x.Key)
				.Select(x => (new TimeSpan(x.Key, 0, 0), x.Value))
				.ToList();

			// BlocksCount → por ahora aproximamos (puedes mejorarlo después)
			metrics.BlocksCount = trenes.Count; // Temporal: igual a trenes. Luego puedes pasar info de bloques.

			return metrics;
		}
	}
}
