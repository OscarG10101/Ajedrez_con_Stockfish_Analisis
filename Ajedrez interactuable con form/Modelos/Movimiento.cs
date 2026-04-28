using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajedrez_interactuable_con_form.Modelos
{
    public class Movimiento
    {
        public int FilaOrigen { get; }
        public int ColumnaOrigen { get; }
        public int FilaDestino { get; }
        public int ColumnaDestino { get; }
        public TipoPieza? PiezaCoronada { get; }

        public Movimiento(int filaOrigen, int columnaOrigen, int filaDestino, int columnaDestino, TipoPieza? piezaCoronada = null)
        {
            FilaDestino = filaDestino;
            ColumnaDestino = columnaDestino;
            FilaOrigen = filaOrigen;
            ColumnaOrigen = columnaOrigen;
            PiezaCoronada = piezaCoronada;
        }
    }
}
