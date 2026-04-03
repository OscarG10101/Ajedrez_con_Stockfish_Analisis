using System;
using System.Diagnostics;
using System.IO;

namespace Ajedrez_interactuable_con_form
{
    internal class StockfishMotor
    {
        private Process stockfish;
        private StreamWriter input;

        private TaskCompletionSource<List<string>> tcsJugadasLegales;

        // Evento para notificar jugadas
        public event Action<string> BestMove_Encontrado;
        public event Action<int> Evaluacion_Actualizada; // centipeones

        public void Iniciar(string rutaExe)
        {
            stockfish = new Process();
            stockfish.StartInfo.FileName = rutaExe;
            stockfish.StartInfo.UseShellExecute = false;
            stockfish.StartInfo.RedirectStandardInput = true;
            stockfish.StartInfo.RedirectStandardOutput = true;
            stockfish.StartInfo.CreateNoWindow = true;

            stockfish.OutputDataReceived += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data))
                    return;

                if (e.Data.StartsWith("bestmove")) 
                {
                    string[] partes = e.Data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Length >= 2)
                    {
                        string bestMove = partes[1];
                        BestMove_Encontrado?.Invoke(bestMove);
                    }
                }
                else if (tcsJugadasLegales != null) // estamos esperando respuesta de perft
                {
                    if (e.Data.StartsWith("Nodes searched"))
                    {
                        // terminamos de recibir jugadas legales
                        if (!tcsJugadasLegales.Task.IsCompleted)
                            tcsJugadasLegales.TrySetResult(_jugadasTemp);
                    }
                    else if (e.Data.Contains(":"))
                    {
                        // línea tipo "e2e4: 1"
                        string mov = e.Data.Split(':')[0].Trim();
                        _jugadasTemp.Add(mov);
                    }
                }
                else if (e.Data.StartsWith("info") && e.Data.Contains("score"))
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
                    }
                    else if (tipo == "mate" && int.TryParse(valor, out int mate))
                    {
                        int evalMate = mate > 0 ? 100000 : -100000;
                        Evaluacion_Actualizada?.Invoke(evalMate);
                    }
                }
            };

            stockfish.Start();
            stockfish.BeginOutputReadLine();

            input = stockfish.StandardInput;
            input.WriteLine("uci");
            input.WriteLine("isready");
            input.WriteLine("ucinewgame");
        }

        private List<string> _jugadasTemp = new List<string>();

        public async Task<bool> EsJugadaValidaAsync(string jugada, List<string> historial)
        {
            var jugadasLegales = await ObtenerJugadasLegalesAsync(historial); // llamas al otro método
            return jugadasLegales.Contains(jugada);
        }

        public async Task<List<string>> ObtenerJugadasLegalesAsync(List<string> historial)
        {
            // Preparamos para recibir jugadas legales
            tcsJugadasLegales = new TaskCompletionSource<List<string>>();
            _jugadasTemp = new List<string>();

            string movimientos = string.Join(" ", historial);
            input.WriteLine("position startpos moves " + movimientos);
            input.WriteLine("go perft 1");

            var jugadasLegales = await tcsJugadasLegales.Task;
            return jugadasLegales;
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

