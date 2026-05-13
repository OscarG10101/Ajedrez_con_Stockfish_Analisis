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

        private List<string> _movimientosPosibles = new List<string>();
        private bool _turnoUsuario = true;

        public PresentadorPartida(IVistaPartida vista, TableroAjedrez juego, 
            StockfishMotor motor, TipoRival rival)
        {
            _vista = vista;
            _juego = juego;
            _motor = motor;

            _vista.CasillaSeleccionada += OnCasillaSeleccionada;
            _vista.UndoSolicitado += OnUndoSolicitado;
            _vista.VistaCerrada += () => _motor.Cerrar();

            _motor.BestMove_Encontrado += OnBestMoveEncontrado;
            _motor.Evaluacion_Actualizada += OnEvaluacionActualizada;
            _motor.SinJugadasLegales += OnSinJugadasLegales;

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
        }

        private async void OnCasillaSeleccionada(int fila, int columna)
        {
            if (!_turnoUsuario)
                return;

            if (_movimientosPosibles.Count == 0)
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

            if (destinos.Count > 0)
            {
                _turnoUsuario = false;

                var pieza = _juego.ObtenerPieza(8 - (primeraSeleccion[1] - '0'), 
                    primeraSeleccion[0] - 'a');

                await _vista.AnimarMovimientoAsync(pieza!, fila, columna);

                string jugadaFinal = (destinos.Count > 1 || destinos[0].Length == 5)
                    ? jugada : destinos[0];

                _juego.RegistrarJugada(jugadaFinal, true);

                _vista.AgregarJugadaHistorial("Blancas", jugadaFinal);
                _vista.MostrarMensajeEstado($"Tu jugada: {jugadaFinal}");

                _motor.PedirBestMove(_juego.Historial);
            }

            _movimientosPosibles.Clear();
            _vista.LimpiarSeleccion();
        }

        private void OnUndoSolicitado()
        {
            if (_juego.Historial.Count < 2)
                return;

            _juego.DeshacerJugadas();
            _turnoUsuario = true;

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
        private void OnBestMoveEncontrado(string bestMove) // maquina juega
        {
            _juego.RegistrarJugada(bestMove, false);
            _turnoUsuario = true;

            _vista.MostrarJugadaStockfish(bestMove);
            _vista.AgregarJugadaHistorial("Negras", bestMove);
        }

        private void OnEvaluacionActualizada(int eval)
        {
            bool turnoNegras = _juego.Historial.Count % 2 != 0;
            int evalDesdeBlancas = turnoNegras ? -eval : eval;
            _vista.MostrarEvaluacionStockfish(evalDesdeBlancas);
        }

        private void OnSinJugadasLegales()
        {
            ResultadoPartida resultado;

            if (_motor.UltimaEvaluacionFueMate)
            {
                bool turnoBlancas = _juego.Historial.Count % 2 == 0;
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
    }
}
