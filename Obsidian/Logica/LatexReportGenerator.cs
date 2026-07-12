using System;
using System.Collections.Generic;
using System.Text;

namespace Obsidian.Logica
{
	internal static class LatexReportGenerator
	{
		public static string GenerateLatexReport(Project project, PlanResult resultado, PlanRestrictions specs)
		{
			StringBuilder sb = new StringBuilder();
			GenerateLatexHeader(sb);
			sb.AppendLine(@"\begin{document}");
			sb.AppendLine(@"\section*{Informe de Planificación de Turnos de Maquinistas}");

			List<Train> allTrains = project.Blocks.SelectMany(b => b.Trains).Distinct().OrderBy(t => t.HoraSalida).ToList();
			//GenerateTrainsList(allTrains,sb);

			var metricsList = new Dictionary<Maquinista, ScheduleMetrics>();
			foreach (var maq in resultado.Schedules)
				metricsList.Add(maq, maq.GetMetrics(specs));

			GenerateScheduleList(sb,resultado,metricsList);

			// Estadísticas globales
			sb.AppendLine(@"\subsection*{Estadísticas Globales}");
			sb.AppendLine(@"\begin{itemize}");
			sb.AppendLine($@"  \item Número total de trenes: {allTrains.Count}");
			sb.AppendLine($@"  \item Número total de maquinistas: {resultado.Schedules.Count}");
			sb.AppendLine($@"  \item Media de eficiencia: {metricsList.Values.Average(m => m.Efficiency):F1}\%");
			sb.AppendLine($@"  \item Media de trenes por turno: {metricsList.Values.Average(m => m.TrainsCount):F2}");
			sb.AppendLine($@"  \item Media de pausas por turno: {metricsList.Values.Average(m => m.GapsCount):F2}");
			sb.AppendLine($@"  \item Máxima conducción continua global: {metricsList.Values.Max(m => m.MaxDrivingTime):hh\:mm}");
			sb.AppendLine(@"\end{itemize}");

			// Distribución de carga por hora (tabla)
			sb.AppendLine(@"\subsection*{Distribución de Carga por Hora}");
			var cargaPorHora = allTrains
				.GroupBy(t => t.HoraSalida.Hours)
				.OrderBy(g => g.Key)
				.Select(g => new { Hora = g.Key, Minutos = g.Sum(t => (t.HoraLlegada - t.HoraSalida).TotalMinutes) })
				.ToList();

			sb.AppendLine(@"\begin{center}");
			sb.AppendLine(@"\begin{tabular}{c|c}");
			sb.AppendLine(@"Hora & Minutos de conducción \\");
			sb.AppendLine(@"\hline");
			foreach (var h in cargaPorHora)
			{
				sb.AppendLine($"{h.Hora:00}:00 & {h.Minutos:F0} \\\\");
			}
			sb.AppendLine(@"\end{tabular}");
			sb.AppendLine(@"\end{center}");

			// Ejemplo de gráfica con pgfplots (el usuario debe copiar los datos)
			sb.AppendLine(@"
% Ejemplo de gráfica con pgfplots
\begin{figure}[h!]
\centering
\begin{tikzpicture}
\begin{axis}[
    width=0.8\textwidth,
    xlabel={Hora},
    ylabel={Minutos de conducción},
    xtick=data,
    ybar,
    bar width=15pt,
    nodes near coords,
    symbolic x coords={"
				+ string.Join(",", cargaPorHora.Select(h => $"{h.Hora:00}:00")) +
			@"}
]
\addplot coordinates {"
				+ string.Join(" ", cargaPorHora.Select(h => $"({h.Hora:00}:00,{h.Minutos:F0})")) +
			@"};
\end{axis}
\end{tikzpicture}
\caption{Distribución de la carga de conducción por hora}
\end{figure}
");
			sb.AppendLine(@"\end{document}");
			return sb.ToString();
		}

		private static void GenerateLatexHeader(StringBuilder sb)
		{
			sb.AppendLine(@"\documentclass[12pt,a4paper]{article}");
			sb.AppendLine(@"\usepackage[spanish]{babel}");
			sb.AppendLine(@"\usepackage{geometry}");
			sb.AppendLine(@"\geometry{margin=2.5cm}");
			sb.AppendLine(@"\usepackage{graphicx}");
			sb.AppendLine(@"\usepackage{booktabs}");
			sb.AppendLine(@"\usepackage{subcaption}");
			sb.AppendLine(@"\usepackage{float}");
			sb.AppendLine(@"\usepackage{amsmath}");
			sb.AppendLine(@"\usepackage{caption}");
			sb.AppendLine(@"\usepackage[table]{xcolor}");
			sb.AppendLine(@"\usepackage{tikz}");
			sb.AppendLine(@"\usepackage{pgfplots}");
			sb.AppendLine(@"\pgfplotsset{compat=1.18}");
		}

		private static void GenerateScheduleList(StringBuilder sb, PlanResult results, Dictionary<Maquinista,ScheduleMetrics> metrics)
		{
			// Turnos de Maquinistas y métricas
			sb.AppendLine(@"\subsection*{Turnos de Maquinistas}");
			//sb.AppendLine(@"\begin{itemize}");
			var metricsList = new List<ScheduleMetrics>();
			foreach (KeyValuePair<Maquinista, ScheduleMetrics> pareja in metrics)
			{
				GenerateWorkSheet(sb, pareja.Key, pareja.Value);
				//sb.AppendLine(@"  \begin{itemize}");
				//sb.AppendLine($@"    \item Trenes asignados: {string.Join(", ", pareja.Key.TrenesAsignados.Select(t => t.Id))}");
				//sb.AppendLine($@"    \item Horario: {pareja.Value.ScheduleBegin:hh\:mm} -- {pareja.Value.ScheduleEnd:hh\:mm}");
				//sb.AppendLine($@"    \item Tiempo de conducción: {pareja.Value.DrivingTime:hh\:mm}");
				//sb.AppendLine($@"    \item Descanso total: {pareja.Value.IddleTime:hh\:mm}");
				//sb.AppendLine($@"    \item Eficiencia: {pareja.Value.Efficiency:F1}\%");
				//sb.AppendLine($@"    \item Ratio conducción/descanso: {pareja.Value.DrivingRatio:F2}");
				//sb.AppendLine($@"    \item Pausas: min={pareja.Value.IddleMin:hh\:mm}, max={pareja.Value.IddleMax:hh\:mm}, avg={pareja.Value.IddleAverage:hh\:mm} (n={pareja.Value.GapsCount})");
				//sb.AppendLine($@"    \item Máxima conducción continua: {pareja.Value.MaxDrivingTime:hh\:mm}");
				//sb.AppendLine(@"  \end{itemize}");
			}
			//sb.AppendLine(@"\end{itemize}");
		}

		private static void GenerateTrainsList(IEnumerable<Train> allTrains, StringBuilder sb)
		{
			// Lista de trenes
			sb.AppendLine(@"\subsection*{Lista de Trenes}");
			sb.AppendLine(@"\begin{itemize}");
		
			foreach (var tren in allTrains)
			{
				sb.AppendLine($@"  \item \textbf{{ID:}} {tren.Id} \hspace{{1cm}} \textbf{{Salida:}} {tren.HoraSalida:hh\:mm} \hspace{{1cm}} \textbf{{Llegada:}} {tren.HoraLlegada:hh\:mm}");
			}
			sb.AppendLine(@"\end{itemize}");
		}

		private static void GenerateWorkSheet(StringBuilder sb, Maquinista maq, ScheduleMetrics metrics)
		{
			sb.AppendLine(@"\begin{table}[ht]");
			sb.AppendLine(@"\centering");
			sb.AppendLine(@"\small");
			sb.AppendLine(@"\begin{tabular}{|l|c|c|c|c|c|c|}");
			sb.AppendLine(@"\hline");
			sb.AppendLine(@"\multicolumn{7}{|c|}{\textbf{Maquinistas}} \\");
			sb.Append(@"\multicolumn{7}{|c|}{\textbf{");
			sb.AppendFormat("Turno nº {0}", maq.Id);
			sb.Append(@"} ");
			sb.Append(maq.JourneyType);
			sb.AppendLine(@"}\\");
			sb.AppendLine(@"\multicolumn{7}{|c|}{\textbf{Lunes a viernes no festivos}} \\");
			sb.AppendLine(@"\hline");
			sb.Append(@"\multicolumn{2}{|l|}{\textbf{ Toma en ");
			sb.Append("Palma"); //A cambiar.
			sb.Append(@"}} & \multicolumn{2}{c|}{\textbf{Hora toma:");
			sb.Append($@"{metrics.ScheduleBegin:hh\:mm}");
			sb.AppendLine(@"}} & \multicolumn{3}{c|}{\textbf{Tiempos}} \\");			
			sb.AppendLine(@"\hline");
			sb.AppendLine(@"\textbf{Estación} & \textbf{Hora} & \textbf{Tren} & \textbf{Hora} & \textbf{Estación} & \textbf{Cond.} &  \\");
			Train? anterior = null;
			TimeSpan cumulConduccion = new TimeSpan(0);
			TimeSpan cumulParada = new TimeSpan(0);
			foreach(Train tren in maq.TrenesAsignados)
			{				
				if (null != anterior)
				{
					TimeSpan tiempoParada = tren.HoraSalida - anterior.HoraLlegada;
					bool isAtt = tiempoParada > TimeSpan.FromMinutes(39);
					sb.AppendLine(@"\hline");
					sb.Append($@"xx"); //Estación de origen
					sb.Append(@$"& {anterior.HoraSalida:hh\:mm} ");
					sb.Append($@"& {anterior.Id} ");
					sb.Append($@"& {anterior.HoraLlegada:hh\:mm} ");
					sb.Append($@"& xx"); //Estación de destino
					sb.Append($@"& {anterior.Asimilacion.Duracion:hh\:mm} "); //Tiempo de conducción
					if(isAtt)
						sb.Append($@"& 00:00"); //Tiempo de no conducción.
					else
						sb.Append($@"& {tiempoParada:hh\:mm}"); //Tiempo de no conducción.
					
					cumulParada += tiempoParada;

					sb.AppendLine(@" \\");
					if (isAtt)
					{
						//Depósito o Att
						sb.AppendLine(@"\hline");
						sb.AppendLine(@"\rowcolor{blue!20}");
						sb.Append($@""); //Estación de origen
						sb.Append(@$"& {anterior.HoraLlegada:hh\:mm} ");
						sb.Append($@"& Att ");
						sb.Append($@"& {tren.HoraSalida:hh\:mm} ");
						sb.Append($@"& "); //Estación de destino
						sb.Append($@"& 00:00 "); //Tiempo de conducción
						sb.Append($@"& {tiempoParada:hh\:mm}"); //Tiempo de no conducción.
						sb.AppendLine(@" \\");
					}
				}
				anterior = tren;
				cumulConduccion += tren.Asimilacion.Duracion;
			}
			if(null!=anterior)
			{
				sb.AppendLine(@"\hline");
				sb.Append($@"xx"); //Estación de origen
				sb.Append(@$"& {anterior.HoraSalida:hh\:mm} ");
				sb.Append($@"& {anterior.Id} ");
				sb.Append($@"& {anterior.HoraLlegada:hh\:mm} ");
				sb.Append($@"& xx"); //Estación de destino
				sb.Append($@"& {anterior.Asimilacion.Duracion:hh\:mm} "); //Tiempo de conducción
				sb.Append($@"& 00:00"); //Tiempo de no conducción.
				sb.AppendLine(@" \\");
			}
			sb.AppendLine(@"\hline");
			sb.Append(@"\textbf{Deje en:} & Palma & \textbf{Hora deje:} & ");
			if (null != anterior)
				sb.Append($@"{anterior.HoraLlegada:hh\:mm}");
			sb.Append(@" & & ");				
			sb.AppendLine(@$"{cumulConduccion:hh\:mm} & {cumulParada:hh\:mm} \\");
			sb.AppendLine(@"\hline");
			sb.Append(@"\textbf{Jornada:} & & & & & \multicolumn{2}{c|}{\textbf{");
			sb.Append($@"{cumulConduccion+cumulParada:hh\:mm}");
			sb.AppendLine(@"}} \\");
			sb.AppendLine(@"\hline");
			sb.AppendLine(@"\end{tabular}");
			sb.AppendLine(@"\end{table}");
		}


		
	}



}





/*
 * 





\hline
\rowcolor{blue!20}
Palma & 16:13 & \textbf{Depósito} & 16:20 & Palma &  & 00:17 & \\
\hline
Palma & 16:25 & 4929 & 17:37 & Manacor & 01:12 & 00:00 & \\
Manacor & 17:56 & 4934 & 19:10 & Palma & 01:14 & 00:00 & \\
\hline
\rowcolor{blue!20}
Palma & 19:15 & \textbf{Att trenes} & 20:10 & Palma &  & 01:05 & \\
\hline
Palma & 20:15 & 5089 & 20:30 & PBIT & 00:18 & 00:00 & \\
PBIT & 20:33 & 5090 & 20:48 & Palma & 00:15 & 00:00 & \\
\hline
\rowcolor{blue!20}
Palma & 20:53 & \textbf{Att trenes} & 21:30 & Palma &  & 00:42 & \\
\hline




\hline

 * */
