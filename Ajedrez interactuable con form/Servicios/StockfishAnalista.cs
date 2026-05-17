using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajedrez_interactuable_con_form.Servicios
{
    public class StockfishAnalista
    {
        private Process? analisis;
        private StreamWriter? input;

        private bool _ultimaEvaluacionFueMate = false;
        public bool UltimaEvaluacionFueMate => _ultimaEvaluacionFueMate; 
        public event Action<int>? Evaluacion_Actualizada;
        public event Action? MateDetectado;

        public void Iniciar(string rutaExe)
        {
            // parametros
            analisis = new Process();
            analisis.StartInfo.FileName = rutaExe;
            analisis.StartInfo.UseShellExecute = false;
            analisis.StartInfo.RedirectStandardInput = true;
            analisis.StartInfo.RedirectStandardOutput = true;
            analisis.StartInfo.CreateNoWindow = true;

            // suscribirse a la salida del proceso con filtro de data
            analisis.OutputDataReceived += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data))
                    return;

                ProcesarLinea(e);
            };

            analisis.Start();
            analisis.BeginOutputReadLine();
            input = analisis.StandardInput;

            input.WriteLine("uci");
            input.WriteLine("isready");
        }

        public void Analizarposicion(List<string> historial)
        {
            if (input == null) return;

            string movimientos = string.Join(" ", historial);
            input.WriteLine("stop");
            input.WriteLine("position startpos moves " + movimientos);
            input.WriteLine("go infinite");
        }

        public void ProcesarLinea(DataReceivedEventArgs? e)
        {
            if (e == null || e.Data == null) return;
            if (!e.Data.StartsWith("info") || !e.Data.Contains("score")) return;

            string[] partes = e.Data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int scoreIndex = Array.IndexOf(partes, "score");

            if (scoreIndex < 0 || scoreIndex + 2 >= partes.Length) return;

            string tipo = partes[scoreIndex + 1];
            string valor = partes[scoreIndex + 2];

            if (tipo == "cp" && int.TryParse(valor, out int eval))
            {
                _ultimaEvaluacionFueMate = false;
                Evaluacion_Actualizada?.Invoke(eval);
            }
            else if (tipo == "mate" && int.TryParse(valor, out int mate))
            {
                _ultimaEvaluacionFueMate = true;
                Evaluacion_Actualizada?.Invoke(mate > 0 ? 100000 : -100000);

                if (mate == 1 || mate == -1) MateDetectado?.Invoke();
            }
        }

        public void DetenerAnalisis()
        {
            input?.WriteLine("stop");
        }

        public void Cerrar()
        {
            if (analisis != null && input != null)
            {
                try
                {
                    if (!analisis.HasExited)
                    {
                        input.WriteLine("quit");

                        if (!analisis.WaitForExit(1000))
                        {
                            analisis.Kill();
                        }
                    }
                }
                catch (Exception) { } // luego agregar log de error

                finally
                {
                    analisis.Dispose();
                    analisis = null!;
                    input = null;
                }
            }
        }
    }
}
