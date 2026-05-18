using Ajedrez_interactuable_con_form.Modelos;
using System;
using System.Diagnostics;
using System.IO;

namespace Ajedrez_interactuable_con_form.Servicios
{
    public class StockfishMotor
    {
        private enum EstadoEspera
        {
            Ninguno,
            EsperandoJugadasLegales,
            EsperandoBestMove,
        }

        private EstadoEspera _estadoActual = EstadoEspera.Ninguno;

        private Process stockfish = null!;
        private StreamWriter? input = null;

        private TaskCompletionSource<List<string>>? tcsJugadasLegales;
        private TaskCompletionSource<string>? tcsbestMove;
        private List<string> _jugadasTemp = new List<string>(); // se usa para acumular jugadas legales mientras se espera la respuesta completa
        private bool _ultimaEvaluacionFueMate = false;
        public bool UltimaEvaluacionFueMate => _ultimaEvaluacionFueMate; // otras clases puedan consultar si la última evaluación fue un mate

        // Evento para notificar jugadas
        public event Action? SinJugadasLegales;

        public void Iniciar(string rutaExe)
        {
            // parametros
            stockfish = new Process();
            stockfish.StartInfo.FileName = rutaExe;
            stockfish.StartInfo.UseShellExecute = false;
            stockfish.StartInfo.RedirectStandardInput = true;
            stockfish.StartInfo.RedirectStandardOutput = true;
            stockfish.StartInfo.CreateNoWindow = true;

            // suscribirse a la salida del proceso con filtro de data
            stockfish.OutputDataReceived += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data))
                    return;

                File.AppendAllText("stockfish_log.txt",
        $"[Estado:{_estadoActual}] {e.Data}\n");

                switch (_estadoActual)
                {
                    case EstadoEspera.EsperandoJugadasLegales:
                        ProcesarJugadasLegales(e);
                        break;

                    case EstadoEspera.EsperandoBestMove:
                        ProcesarBestMove(e);
                        break;
                }
            };

            if (stockfish == null)
                return;

            // iniciar proceso
            stockfish.Start();
            stockfish.BeginOutputReadLine();

            input = stockfish.StandardInput;
            input.WriteLine("uci");
            input.WriteLine("isready");
            input.WriteLine("ucinewgame");
        }

        public void ConfigurarNivel(int elo)
        {
            if (input == null) return;

            input.WriteLine("setoption name UCI_LimitStrength value true");
            input.WriteLine($"setoption name UCI_Elo value {elo}");
        }

        public async Task<List<string>> PedirJugadasLegalesAsync(List<string> historial)
        {
            if (input == null) return new List<string>();

            _estadoActual = EstadoEspera.EsperandoJugadasLegales;

            // Preparo para recibir jugadas legales
            tcsJugadasLegales = new TaskCompletionSource<List<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
            _jugadasTemp = new List<string>();

            string movimientos = string.Join(" ", historial);
            input.WriteLine("position startpos moves " + movimientos);
            input.WriteLine("go perft 1");

            return await tcsJugadasLegales.Task;
        }

        private void ProcesarJugadasLegales(DataReceivedEventArgs? e)
        {
            if (tcsJugadasLegales != null && e != null && e.Data != null)
            {
                if (e.Data.StartsWith("Nodes searched"))
                {
                    // terminamos de recibir jugadas legales
                    if (!tcsJugadasLegales.Task.IsCompleted)
                        tcsJugadasLegales.TrySetResult(_jugadasTemp);

                    _estadoActual = EstadoEspera.Ninguno;
                }
                else if (e.Data.Contains(":"))
                {
                    // línea tipo "e2e4: 1"
                    string mov = e.Data.Split(':')[0].Trim();
                    if (mov.Length >= 4)
                        _jugadasTemp.Add(mov);
                }
            }
        }

        public async Task<string> PedirBestMove(List<string> historial)
        {
            if (input == null) return "";

            _estadoActual = EstadoEspera.EsperandoBestMove;

            tcsbestMove = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            string movimientos = string.Join(" ", historial);
            input.WriteLine("position startpos moves " + movimientos);
            input.WriteLine("go movetime 500");

            return await tcsbestMove.Task;
        }

        private void ProcesarBestMove(DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data.StartsWith("bestmove"))
            {
                string[] partes = e.Data.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (partes.Length >= 2 && partes[1] == "(none)")
                {
                    SinJugadasLegales?.Invoke();
                    tcsbestMove!.TrySetResult("");
                    _estadoActual = EstadoEspera.Ninguno;
                    return;
                }

                if (partes.Length >= 2)
                {
                    string bestMove = partes[1];
                    tcsbestMove!.TrySetResult(bestMove);

                    _estadoActual = EstadoEspera.Ninguno;
                }
            }
        }

        public void DetenerAnalisis()
        {
            input?.WriteLine("stop");
            _estadoActual = EstadoEspera.Ninguno;
        }

        public void EnviarComando(string comando)
        {
            input?.WriteLine(comando);
        }

        public void Cerrar()
        {
            if (stockfish != null && input != null)
            {
                try
                {
                    if (!stockfish.HasExited)
                    {
                        input.WriteLine("quit");

                        if (!stockfish.WaitForExit(1000))
                        {
                            stockfish.Kill();
                        }
                    }
                }
                catch (Exception) { } // luego agregar log de error

                finally
                {
                    stockfish.Dispose();
                    stockfish = null!;
                    input = null;
                }
                
            }
        }
    }
}

