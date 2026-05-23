using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ajedrez_interactuable_con_form.Modelos;
using Ajedrez_interactuable_con_form.Vistas;
using Microsoft.VisualBasic.Logging;

namespace Ajedrez_interactuable_con_form.Servicios
{
    public class PresentadorPartida
    {
        private readonly IVistaPartida _vista;
        private readonly TableroAjedrez _juego;
        private readonly StockfishMotor _motor;
        private readonly StockfishAnalista _analista;

        private List<string> _movimientosPosibles = new List<string>();
        private bool _turnoUsuario = true;
        private bool _partidaTerminada = true;

        public PresentadorPartida(IVistaPartida vista, TableroAjedrez juego,
            StockfishMotor motor, StockfishAnalista analista, TipoRival rival)
        {
            _vista = vista;
            _juego = juego;
            _motor = motor;
            _analista = analista;

            _vista.CasillaSeleccionada += OnCasillaSeleccionada;
            _vista.UndoSolicitado += OnUndoSolicitado;
            _vista.VistaCerrada += () => { _motor.Cerrar(); _analista.Cerrar(); };

            _motor.SinJugadasLegales += () => OnSinJugadasLegales(jugabanBlancas : true);

            _juego.TableroActualizado += () => _vista.ActualizarTablero();
            _vista.SincronizarTablero(_juego.Tablero);
            _juego.MostrarMenuCoronacionUI = _vista.MostrarMenuCoronacion;

            IniciarMotor(rival);

            ConfigurarRival(rival);
        }
        
        public void ConfigurarRival(TipoRival rival)
        {
            if (_vista is FormPartida form)
            {
                form.NombreRival = rival.ToString();
                form.EloRival = rival switch
                {
                    TipoRival.Andrea => "800 ELO",
                    TipoRival.Natasha => "1500 ELO",
                    TipoRival.Alejandra => "2200 ELO",
                    _ => ""
                };
                form.FotoRival = rival switch
                {
                    TipoRival.Andrea => Properties.Resources.Andrea,
                    TipoRival.Natasha => Properties.Resources.Natasha,
                    TipoRival.Alejandra => Properties.Resources.Alejandra,
                    _ => null
                };
                form.PbxRival.Image = form.FotoRival;
            }
        }

        public void IniciarMotor(TipoRival rival)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string carpeta = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Stockfish"));

            if (!Directory.Exists(carpeta))
                throw new DirectoryNotFoundException($"No se encontró la carpeta de Stockfish en: {carpeta}");

            var files = Directory.GetFiles(carpeta, "stockfish*.exe");
            if (files.Length == 0)
                throw new FileNotFoundException($"No se encontró el ejecutable de Stockfish en: {carpeta}");

            _motor.Iniciar(files[0]);
            _motor.ConfigurarNivel((int)rival);
            _analista.Iniciar(files[0]);
        }

        private async void OnCasillaSeleccionada(int fila, int columna)
        {
            if (!_turnoUsuario)
               return;

            _partidaTerminada = false;

            if (_movimientosPosibles.Count == 0) // primer clic
            {
                var legales = await _motor.PedirJugadasLegalesAsync(_juego.Historial);

                if (legales.Count == 0)
                {
                    OnSinJugadasLegales(jugabanBlancas : false);
                    return;
                }

                string origen = CasillaToTexto(fila, columna);
                _movimientosPosibles = legales.Where(j => j.StartsWith(origen)).ToList(); // se quedan en lista solo las jugadas de pieza select
                _vista.MostrarMovimientosPosibles(_movimientosPosibles);
                return;
            }

            string primeraSeleccion = _movimientosPosibles[0].Substring(0, 2);
            string jugada = primeraSeleccion + CasillaToTexto(fila, columna);

            var destinos = _movimientosPosibles.Where(j => j.StartsWith(jugada)).ToList();

            if (destinos.Count > 0) // segundo clic con lista mov posibles
            {
                _turnoUsuario = false;

                _movimientosPosibles.Clear();
                _vista.LimpiarSeleccion();

                var pieza = _juego.ObtenerPieza(8 - (primeraSeleccion[1] - '0'), 
                    primeraSeleccion[0] - 'a');

                await _vista.AnimarMovimientoAsync(pieza!, fila, columna);

                string jugadaFinal = (destinos.Count > 1 || destinos[0].Length == 5)
                    ? jugada : destinos[0];

                JuegaUsuario(jugadaFinal);

                bool esCoronacion = pieza?.Tipo == TipoPieza.Peon && jugadaFinal.Length == 4 &&
                        ((fila == 0 && pieza?.EsBlanca == true) ||
                         (fila == 7 && pieza?.EsBlanca == false));

                if (esCoronacion)
                {
                    char eleccion = await _juego.EsperarCoronacion();
                    jugadaFinal = jugadaFinal + eleccion; // completar jugada con la pieza elegida
                }

                _juego.RegistrarJugada(jugadaFinal);

                _ = EscucharAnalisisContinuoAsync(turnoBlancasAlAnalizar : false);

                _vista.AgregarJugadaHistorial("Blancas", jugadaFinal);
                _vista.MostrarMensajeEstado($"Tu jugada: {jugadaFinal}");

                var jugadasLegales = await _motor.PedirJugadasLegalesAsync(_juego.Historial);
                if (jugadasLegales.Count == 0)
                {
                    OnSinJugadasLegales(jugabanBlancas: false);
                    return;
                }

                OnBestMoveEncontrado(await _motor.PedirBestMove(_juego.Historial));
            }
            else
            {
                _movimientosPosibles.Clear();
                _vista.LimpiarSeleccion();

                var nuevaPieza = _juego.ObtenerPieza(fila, columna);
                if (nuevaPieza != null && nuevaPieza.EsBlanca)
                {
                    // Llamamos recursivamente al mismo método. 
                    // Como acabamos de limpiar '_movimientosPosibles', este llamado actuará automáticamente como un "Primer Clic"
                    OnCasillaSeleccionada(fila, columna);
                }
            }
        }

        private void JuegaUsuario(string jugada)
        {
            _juego.RealizarJugada(jugada, true);
        }

        private async Task JuegaStockfishAsync(string bestMove)
        {
            string origen = bestMove.Substring(0, 2);
            int filaOrigen = 8 - (origen[1] - '0');
            int columnaOrigen = origen[0] - 'a';

            string destino = bestMove.Substring(2, 2);
            int filaDestino = 8 - (destino[1] - '0');
            int columnaDestino = destino[0] - 'a';

            var pieza = _juego.ObtenerPieza(filaOrigen, columnaOrigen);
            if (pieza != null)
                await _vista.AnimarMovimientoAsync(pieza, filaDestino, columnaDestino);

            _juego.RealizarJugada(bestMove, false);
            _juego.RegistrarJugada(bestMove);
        }

        private async void OnBestMoveEncontrado(string bestMove) // maquina juega
        {
            if (bestMove == "") 
            {
                _turnoUsuario = true;
                return;
            }

            await JuegaStockfishAsync(bestMove);

            _turnoUsuario = true;

            _ = EscucharAnalisisContinuoAsync(turnoBlancasAlAnalizar : true);

            await _motor.PedirBestMove(_juego.Historial);

            _vista.MostrarJugadaStockfish(bestMove);
            _vista.AgregarJugadaHistorial("Negras", bestMove);
        }

        private async Task EscucharAnalisisContinuoAsync(bool turnoBlancasAlAnalizar)
        {
            try
            {
                await foreach (int eval in _analista.Analizarposicion(_juego.Historial))
                {
                    int evalDesdeBlancas = turnoBlancasAlAnalizar ? eval : -eval;
                    _vista.MostrarEvaluacionStockfish(evalDesdeBlancas);
                }
            }
            catch (OperationCanceledException)
            {

            }
        }

        private void OnSinJugadasLegales(bool jugabanBlancas)
        {
            if (_partidaTerminada) return;
            _partidaTerminada = true;

            File.AppendAllText("stockfish_log.txt",
        $"[FinPartida] Historial.Count={_juego.Historial.Count} " +
        $"UltimaEvaluacionFueMate={_analista.UltimaEvaluacionFueMate}\n");
            _analista.DetenerAnalisis();
            ResultadoPartida resultado;

            if (_analista.UltimaEvaluacionFueMate || _motor.UltimaEvaluacionFueMate) // doble seguridad
            {
                resultado = jugabanBlancas ? ResultadoPartida.GanaNegras : ResultadoPartida.GanaBlancas;
            }
            else if (_juego.EsTripleRepeticion())
                resultado = ResultadoPartida.TripleRepeticion;
            else if (_juego.EsCincuentaMovimientos())
                resultado = ResultadoPartida.CincuentaMovimientos;
            else
                resultado = ResultadoPartida.Ahogado;

            _vista.MostrarFinPartida(resultado);
        }

        private string CasillaToTexto(int fila, int col)
        {
            char letra = (char)('a' + col);
            int numero = 8 - fila;
            return $"{letra}{numero}";
        }

        private void OnUndoSolicitado()
        {
            if (_juego.Historial.Count < 2)
                return;

            _analista.DetenerAnalisis();
            _juego.DeshacerJugadas();
            
            _turnoUsuario = true;
            _partidaTerminada = false;

            _vista.LimpiarHistorial();

            for (int i = 0; i < _juego.Historial.Count; i++)
            {
                string turno = i % 2 == 0 ? "Blancas" : "Negras";
                _vista.AgregarJugadaHistorial(turno, _juego.Historial[i]);
            }

            _movimientosPosibles.Clear();
            _vista.LimpiarSeleccion();
            _vista.MostrarMensajeEstado("Jugada deshecha. Es tu turno.");
            _vista.ActualizarTablero();
            _vista.SincronizarTablero(_juego.Tablero);

            _ = EscucharAnalisisContinuoAsync(true);
        }
    }
}
