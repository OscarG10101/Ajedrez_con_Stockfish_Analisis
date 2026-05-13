using Ajedrez_interactuable_con_form.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ajedrez_interactuable_con_form.Vistas;

namespace Ajedrez_interactuable_con_form.Vistas
{
    public interface IVistaPartida
    {
        event Action<int, int> CasillaSeleccionada;
        event Action UndoSolicitado;
        event Action VistaCerrada;

        void MostrarMovimientosPosibles(List<string> movimientos);
        void LimpiarSeleccion();
        void AgregarJugadaHistorial(string turno, string jugada);
        void LimpiarHistorial();
        void MostrarMensajeEstado(string mensaje);
        void MostrarJugadaStockfish(string jugada);
        void MostrarEvaluacionStockfish(int centipeones);
        void MostrarFinPartida(ResultadoPartida resultado);
        void ActualizarTablero();

        Task AnimarMovimientoAsync(Pieza pieza, int filaDestino, int columnaDestino);
        void MostrarMenuCoronacion(int fila, int columna, Pieza peon, Action<char> callback);

        Pieza?[,] ObtenerTablero();

        void SincronizarTablero(Pieza?[,] tablero);
    }
}
