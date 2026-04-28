using Ajedrez_interactuable_con_form.Modelos;
using System;
using System.Diagnostics;
using System.IO;

namespace Ajedrez_interactuable_con_form.Servicios
{
    internal class StockfishMotor
    {
        private enum EstadoEspera
        {
            Ninguno,
            EsperandoJugadasLegales,
            EsperandoBestMove,
            EsperandoEvaluacion
        }

        private EstadoEspera _estadoActual = EstadoEspera.Ninguno;

        private Process stockfish;
        private StreamWriter input;

        private TaskCompletionSource<List<string>> tcsJugadasLegales;
        private List<string> _jugadasTemp = new List<string>(); // se usa para acumular jugadas legales mientras se espera la respuesta completa
        private bool _ultimaEvaluacionFueMate = false;
        public bool UltimaEvaluacionFueMate => _ultimaEvaluacionFueMate; // propiedad pública para que otras clases puedan consultar si la última evaluación fue un mate

        // Evento para notificar jugadas
        public event Action<string> BestMove_Encontrado;
        public event Action<int> Evaluacion_Actualizada; // centipeones
        public event Action<ResultadoPartida> PartidaFin;
        public event Action SinJugadasLegales;

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
                        ProcesarEvaluacion(e);
                        break;
                }
            };

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
            input.WriteLine("setoption name UCI_LimitStrength value true");
            input.WriteLine($"setoption name UCI_Elo value {elo}");
        }

        public async Task<List<string>> PedirJugadasLegalesAsync(List<string> historial)
        {
            _estadoActual = EstadoEspera.EsperandoJugadasLegales;

            // Preparo para recibir jugadas legales
            tcsJugadasLegales = new TaskCompletionSource<List<string>>(TaskCreationOptions.RunContinuationsAsynchronously); // abro un hilo nuevo para evitar bloqueos UI
            _jugadasTemp = new List<string>();

            string movimientos = string.Join(" ", historial);
            input.WriteLine("position startpos moves " + movimientos);
            input.WriteLine("go perft 1");

            return await tcsJugadasLegales.Task;
        }

        private void ProcesarJugadasLegales(DataReceivedEventArgs e)
        {
            if (tcsJugadasLegales != null)
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

        public void PedirBestMove(List<string> historial)
        {
            _estadoActual = EstadoEspera.EsperandoBestMove;

            string movimientos = string.Join(" ", historial);
            input.WriteLine("position startpos moves " + movimientos);
            input.WriteLine("go movetime 500");
        }

        private void ProcesarBestMove(DataReceivedEventArgs e)
        {
            if (e.Data.StartsWith("bestmove"))
            {
                string[] partes = e.Data.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (partes.Length >= 2 && partes[1] == "(none)")
                {
                    SinJugadasLegales?.Invoke();
                    _estadoActual = EstadoEspera.Ninguno;
                    return;
                }

                if (partes.Length >= 2)
                {
                    string bestMove = partes[1];
                    BestMove_Encontrado?.Invoke(bestMove);

                    _estadoActual = EstadoEspera.Ninguno;
                }
            }
        }

        private void ProcesarEvaluacion(DataReceivedEventArgs e)
        {
            if (e.Data.StartsWith("info") && e.Data.Contains("score"))
            {
                string[] partes = e.Data.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int scoreIndex = Array.IndexOf(partes, "score");
                if (scoreIndex < 0 || scoreIndex + 2 >= partes.Length)
                    return;

                string tipo = partes[scoreIndex + 1];  // cp | mate
                string valor = partes[scoreIndex + 2]; // número

                if (tipo == "cp" && int.TryParse(valor, out int eval))
                {
                    Evaluacion_Actualizada?.Invoke(eval);
                    _ultimaEvaluacionFueMate = false;
                }
                else if (tipo == "mate" && int.TryParse(valor, out int mate))
                {
                    int evalMate = mate > 0 ? 100000 : -100000;
                    Evaluacion_Actualizada?.Invoke(evalMate);
                    _ultimaEvaluacionFueMate = true;
                }
            }
        }

        public void IniciarAnalisisContinuo(List<string> historial)
        {
            _estadoActual = EstadoEspera.EsperandoEvaluacion;

            string movimientos = string.Join(" ", historial);
            input.WriteLine("position startpos moves " + movimientos);
            input.WriteLine("go infinite");
        }

        public void DetenerAnalisis()
        {
            input.WriteLine("stop");
            _estadoActual = EstadoEspera.Ninguno;
        }

        public void EnviarComando(string comando)
        {
            input?.WriteLine(comando);
        }

        public void Cerrar()
        {
            if (stockfish != null && !stockfish.HasExited)
            {
                input.WriteLine("quit");
                stockfish.Close();
                stockfish.Dispose();
            }
        }
    }
}

