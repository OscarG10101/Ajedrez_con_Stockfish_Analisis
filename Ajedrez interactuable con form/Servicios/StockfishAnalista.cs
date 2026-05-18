using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Ajedrez_interactuable_con_form.Servicios
{
    public class StockfishAnalista
    {
        private Process? analisis;
        private StreamWriter? input;

        private Channel<int>? _canalEvaluacion;
        private CancellationTokenSource? _ctsActual;

        private bool _ultimaEvaluacionFueMate = false;
        public bool UltimaEvaluacionFueMate => _ultimaEvaluacionFueMate; 

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

        public IAsyncEnumerable<int> Analizarposicion(List<string> historial)
        {
            if (input == null) throw new InvalidOperationException("El motor no inició correctamente");

            _ctsActual?.Cancel(); // si !null cancelar anterior
            _canalEvaluacion?.Writer.TryComplete(); // si !null completar int anterior

            _ctsActual = new CancellationTokenSource(); // reiniciar
            _canalEvaluacion = Channel.CreateUnbounded<int>();

            string movimientos = string.Join(" ", historial);
            input.WriteLine("stop");
            input.WriteLine("position startpos moves " + movimientos);
            input.WriteLine("go infinite");

            return _canalEvaluacion.Reader.ReadAllAsync(_ctsActual.Token);
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
                _canalEvaluacion?.Writer.TryWrite(eval);
            }
            else if (tipo == "mate" && int.TryParse(valor, out int mate))
            {   
                _ultimaEvaluacionFueMate = true;
                _canalEvaluacion?.Writer.TryWrite(mate > 0 ? 100000 : -100000);
            }
        }

        public void DetenerAnalisis()
        {
            _ctsActual?.Cancel();
            _canalEvaluacion?.Writer.TryComplete();
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
                    _ctsActual?.Dispose();
                    _canalEvaluacion = null;
                    analisis = null!;
                    input = null;
                }
            }
        }
    }
}
