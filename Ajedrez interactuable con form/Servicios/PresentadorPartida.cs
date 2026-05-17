using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ajedrez_interactuable_con_form.Modelos;
using Ajedrez_interactuable_con_form.Vistas;

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

            _motor.BestMove_Encontrado += OnBestMoveEncontrado;
            _motor.SinJugadasLegales += OnSinJugadasLegales;

            _analista.Evaluacion_Actualizada += OnEvaluacionActualizada;
            _analista.MateDetectado += OnSinJugadasLegales;

            _juego.TableroActualizado += () => _vista.ActualizarTablero();
            _vista.SincronizarTablero(_juego.Tablero);
            _juego.MostrarMenuCoronacionUI = _vista.MostrarMenuCoronacion;

            IniciarMotor(rival);
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

            if (_movimientosPosibles.Count == 0) // primer clic
            {
                var legales = await _motor.PedirJugadasLegalesAsync(_juego.Historial);

                if (legales.Count == 0)
                {
                    OnSinJugadasLegales();
                    return;
                }

                string origen = CasillaToTexto(fila, columna);
                _movimientosPosibles = legales.Where(j => j.StartsWith(origen)).ToList();
                _vista.MostrarMovimientosPosibles(_movimientosPosibles);
                return;
            }

            string primeraSeleccion = _movimientosPosibles[0].Substring(0, 2);
            string jugada = primeraSeleccion + CasillaToTexto(fila, columna);

            var destinos = _movimientosPosibles.Where(j => j.StartsWith(jugada)).ToList();

            if (destinos.Count > 0) // segundo clic con lista mov posibles
            {
                _turnoUsuario = false;

                var pieza = _juego.ObtenerPieza(8 - (primeraSeleccion[1] - '0'), 
                    primeraSeleccion[0] - 'a');

                await _vista.AnimarMovimientoAsync(pieza!, fila, columna);

                string jugadaFinal = (destinos.Count > 1 || destinos[0].Length == 5)
                    ? jugada : destinos[0];

                JuegaUsuario(jugadaFinal);
                _analista.Analizarposicion(_juego.Historial);

                _vista.AgregarJugadaHistorial("Blancas", jugadaFinal);
                _vista.MostrarMensajeEstado($"Tu jugada: {jugadaFinal}");

                _motor.PedirBestMove(_juego.Historial);
            }

            _movimientosPosibles.Clear();
            _vista.LimpiarSeleccion();
        }

        private void JuegaUsuario(string jugada)
        {
            _juego.RegistrarJugada(jugada, true);
        }

        private void JuegaStockfish(string bestMove)
        {
            _juego.RegistrarJugada(bestMove, false);
        }

        private async void OnBestMoveEncontrado(string bestMove) // maquina juega
        {
            JuegaStockfish(bestMove);
            _turnoUsuario = true;

            _analista.Analizarposicion(_juego.Historial);

            _vista.MostrarJugadaStockfish(bestMove);
            _vista.AgregarJugadaHistorial("Negras", bestMove);

            var legales = await _motor.PedirJugadasLegalesAsync(_juego.Historial);
            if (legales.Count == 0) OnSinJugadasLegales();
        }

        private void OnEvaluacionActualizada(int eval)
        {
            bool turnoNegras = _juego.Historial.Count % 2 != 0;
            int evalDesdeBlancas = turnoNegras ? -eval : eval;
            _vista.MostrarEvaluacionStockfish(evalDesdeBlancas);
        }

        private void OnSinJugadasLegales()
        {
            if (_partidaTerminada) return;
            _partidaTerminada = true;
            File.AppendAllText("stockfish_log.txt",
        $"[FinPartida] Historial.Count={_juego.Historial.Count} " +
        $"UltimaEvaluacionFueMate={_analista.UltimaEvaluacionFueMate}\n");
            _analista.DetenerAnalisis();
            ResultadoPartida resultado;

            bool turnoBlancas = _juego.Historial.Count % 2 == 0;

            if (_analista.UltimaEvaluacionFueMate)
            {
                resultado = turnoBlancas ? ResultadoPartida.GanaNegras : ResultadoPartida.GanaBlancas;
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
        }
    }
}
