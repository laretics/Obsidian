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
			if (specs == null) throw new ArgumentNullException(nameof(specs));

			var bloques = new List<WorkBlock>(Blocks);
			bloques.Sort((a, b) => a.Trains[0].HoraSalida.CompareTo(b.Trains[0].HoraSalida));

			var schedules = new List<Maquinista>();
			var estados = new List<DriverState>();

			// PASO 1: Aplicar asignaciones manuales
			var bloquesAsignados = AplicarAsignacionesManuales(bloques, schedules, estados, specs);

			// PASO 2: Asignar el resto automáticamente
			foreach (var bloque in bloques)
			{
				if (bloquesAsignados.Contains(bloque))
					continue; // Ya fue asignado manualmente

				// Buscamos el maquinista con MENOR carga que pueda tomar este bloque
				var (mejorMaq, mejorEstado) = EncontrarMaquinistaMenosCargado(bloque, schedules, estados, specs);

				if (mejorMaq == null)
				{
					// Ninguno puede → creamos uno nuevo
					var nuevo = new Maquinista { Id = $"M{schedules.Count + 1}" };
					var nuevoEstado = new DriverState();
					schedules.Add(nuevo);
					estados.Add(nuevoEstado);

					Assign(bloque, nuevo, nuevoEstado, specs);
				}
				else
				{
					Assign(bloque, mejorMaq, mejorEstado, specs);
				}
			}

			return new PlanResult { Schedules = schedules, Unassigned = new List<WorkBlock>() };
		}

		/// <summary>
		/// Aplica las asignaciones manuales definidas en specs.ManualAssignments.
		/// Retorna el conjunto de bloques que fueron asignados manualmente.
		/// </summary>
		private HashSet<WorkBlock> AplicarAsignacionesManuales(
			List<WorkBlock> bloques, 
			List<Maquinista> schedules, 
			List<DriverState> estados, 
			PlanRestrictions specs)
		{
			var asignados = new HashSet<WorkBlock>();
			var maquinistasMap = new Dictionary<string, (Maquinista, DriverState)>();

			foreach (var asignacion in specs.ManualAssignments)
			{
				// Buscar el bloque que contiene el tren especificado
				var bloque = bloques.FirstOrDefault(b => 
					b.Trains.Any(t => t.Id == asignacion.TrainId));

				if (bloque == null)
				{
					Console.WriteLine($"⚠️ Advertencia: No se encontró el tren '{asignacion.TrainId}' para asignación manual");
					continue;
				}

				// Obtener o crear el maquinista
				if (!maquinistasMap.TryGetValue(asignacion.DriverId, out var tupla))
				{
					var maq = new Maquinista { Id = asignacion.DriverId };
					var estado = new DriverState();
					schedules.Add(maq);
					estados.Add(estado);
					tupla = (maq, estado);
					maquinistasMap[asignacion.DriverId] = tupla;
				}

				// Verificar si la asignación es válida
				if (CanAssign(bloque, tupla.Item1, tupla.Item2, specs))
				{
					Assign(bloque, tupla.Item1, tupla.Item2, specs);
					asignados.Add(bloque);
					Console.WriteLine($"✓ Asignación manual: {asignacion.DriverId} → Bloque con tren {asignacion.TrainId}");
				}
				else
				{
					Console.WriteLine($"⚠️ Advertencia: Asignación manual inválida (viola restricciones): {asignacion.DriverId} → Tren {asignacion.TrainId}");
				}
			}

			return asignados;
		}

		private (Maquinista?, DriverState?) EncontrarMaquinistaMenosCargado(
			WorkBlock bloque, List<Maquinista> schedules, List<DriverState> estados, PlanRestrictions specs)
		{
			Maquinista? mejor = null;
			DriverState? mejorEstado = null;
			int menorCarga = int.MaxValue;

			for (int i = 0; i < schedules.Count; i++)
			{
				var maq = schedules[i];
				var estado = estados[i];

				if (CanAssign(bloque, maq, estado, specs))
				{
					int carga = maq.Blocks.Count;                    // puedes cambiar a minutos de jornada
					if (carga < menorCarga)
					{
						menorCarga = carga;
						mejor = maq;
						mejorEstado = estado;
					}
				}
			}
			return (mejor, mejorEstado);
		}

		private bool CanAssign(WorkBlock bloque, Maquinista maq, DriverState estado, PlanRestrictions specs)
		{
			var primer = bloque.Trains[0];
			var ultimo = bloque.Trains[^1];

			TimeSpan finUltimo = maq.Blocks.Count > 0
				? maq.Blocks[^1].Trains[^1].HoraLlegada : TimeSpan.Zero;

			if (primer.HoraSalida < finUltimo) return false;

			TimeSpan inicioJornada = maq.Blocks.Count > 0
				? maq.Blocks[0].Trains[0].HoraSalida : primer.HoraSalida;

			if ((ultimo.HoraLlegada - inicioJornada) > specs.MaxPayload)
				return false;

			TimeSpan gap = primer.HoraSalida - finUltimo;
			TimeSpan nuevaStreak = estado.DrivingStreak;

			if (maq.Blocks.Count == 0 || gap >= specs.MinIddleTime)
				nuevaStreak = TimeSpan.Zero;

			nuevaStreak += ultimo.HoraLlegada - primer.HoraSalida;

			if (maq.Blocks.Count > 0 && gap < specs.MinIddleTime)
				nuevaStreak += gap;

			return nuevaStreak <= specs.MaxDrivingTime;
		}

		private void Assign(WorkBlock bloque, Maquinista maq, DriverState estado, PlanRestrictions specs)
		{
			var primer = bloque.Trains[0];
			var ultimo = bloque.Trains[^1];
			TimeSpan finUltimo = maq.Blocks.Count > 0
				? maq.Blocks[^1].Trains[^1].HoraLlegada : TimeSpan.Zero;

			TimeSpan gap = primer.HoraSalida - finUltimo;

			if (maq.Blocks.Count == 0 || gap >= specs.MinIddleTime)
				estado.DrivingStreak = TimeSpan.Zero;

			estado.DrivingStreak += ultimo.HoraLlegada - primer.HoraSalida;

			if (maq.Blocks.Count > 0 && gap < specs.MinIddleTime)
				estado.DrivingStreak += gap;

			maq.Blocks.Add(bloque);
		}

		private class DriverState
		{
			public TimeSpan DrivingStreak { get; set; } = TimeSpan.Zero;
		}
	}
}
