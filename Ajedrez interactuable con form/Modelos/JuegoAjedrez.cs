using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajedrez_interactuable_con_form.Modelos
{
    internal class JuegoAjedrez
    {
        public Pieza[,] Tablero { get; private set; } = new Pieza[8, 8];
        public List<string> Historial { get; private set; } = new List<string>();

        // Evento que notifica que se movió una pieza
        public event Action<string> JugadaRealizada;

        // Evento para pedir redibujar el tablero
        public event Action TableroActualizado;

        public Action<int, int, Pieza, Action<char>> MostrarMenuCoronacionUI;


        public JuegoAjedrez()
        {
            InicializarTablero();
        }

        private void InicializarTablero()
        {
            // Inicializar peones
            for (int col = 0; col < 8; col++)
            {
                Tablero[6, col] = new Pieza(6, col, true, TipoPieza.Peon);
                Tablero[1, col] = new Pieza(1, col, false, TipoPieza.Peon);
            }

            // Piezas mayores
            ColocarFila(new[] { TipoPieza.Torre, TipoPieza.Caballo, TipoPieza.Alfil, TipoPieza.Dama,
                            TipoPieza.Rey, TipoPieza.Alfil, TipoPieza.Caballo, TipoPieza.Torre }, 7, true);
            ColocarFila(new[] { TipoPieza.Torre, TipoPieza.Caballo, TipoPieza.Alfil, TipoPieza.Dama,
                            TipoPieza.Rey, TipoPieza.Alfil, TipoPieza.Caballo, TipoPieza.Torre }, 0, false);
        }

        private void ColocarFila(TipoPieza[] piezas, int fila, bool esBlanco)
        {
            for (int col = 0; col < piezas.Length; col++)
                Tablero[fila, col] = new Pieza(fila, col, esBlanco, piezas[col]);
        }

        public void MoverPieza(string jugada, bool EsHumano, Action<char> coronacionCallback = null)
        {
            if (jugada.Length < 4) return;

            // Origen
            int colOrigen = jugada[0] - 'a';
            int filaOrigen = 8 - (jugada[1] - '0'); // aqui se resta '0' para convertir automaticamente a char a int

            // Destino
            int colDestino = jugada[2] - 'a';
            int filaDestino = 8 - int.Parse(jugada[3].ToString());

            var pieza = Tablero[filaOrigen, colOrigen]; // var se usa para que el compilador deduzca el tipo de dato automáticamente de la variable ya asignada en este caso "Piezas[,]"
                                                              // el tipo deducido es Piezas y no puede cambiar después
            if (pieza == null) return;

            if (pieza.Tipo == TipoPieza.Rey && Math.Abs(colDestino - colOrigen) == 2)
            {
                // Enroque corto
                if (colDestino > colOrigen)
                {
                    var torre = Tablero[filaOrigen, 7];
                    Tablero[filaDestino, 5] = torre;
                    Tablero[filaOrigen, 7] = null;
                    torre.Columna = 5;
                }
                // Enroque largo
                else
                {
                    var torre = Tablero[filaOrigen, 0];
                    Tablero[filaDestino, 3] = torre;
                    Tablero[filaOrigen, 0] = null;
                    torre.Columna = 3;
                }
            }

            // Movimiento de la pieza
            Tablero[filaDestino, colDestino] = pieza; // le da los parametros de la pieza que se va a mover
            Tablero[filaOrigen, colOrigen] = null; // deja el lugar de origen vacío en donde estaba la pieza para la siguiente jugada
            pieza.Fila = filaDestino; // actualiza los atributos de la pieza
            pieza.Columna = colDestino;

            if (pieza.Tipo == TipoPieza.Peon && (filaDestino == 0 || filaDestino == 7))
            {
                if (EsHumano)
                {
                    MostrarMenuCoronacionUI(filaDestino, colDestino, pieza, (piezaElegida) =>
                    {
                        string jugadaConCoronacion = jugada + piezaElegida;
                        JugadaRealizada?.Invoke(jugadaConCoronacion); 
                    });
                }
                else
                {
                    char promocion = jugada.Length == 5 ? jugada[4] : 'q'; // stockfish manda e7e8q
                    switch (promocion)
                    {
                        case 'q': pieza.Tipo = TipoPieza.Dama; break;
                        case 'r': pieza.Tipo = TipoPieza.Torre; break;
                        case 'b': pieza.Tipo = TipoPieza.Alfil; break;
                        case 'n': pieza.Tipo = TipoPieza.Caballo; break;
                        default: pieza.Tipo = TipoPieza.Dama; break;
                    }
                }
                return; // salir para que no llame a ProcesarJugada_Stockfish de nuevo
            }

            TableroActualizado?.Invoke(); // notifica al Form1 que redibuje

            if (EsHumano)
                JugadaRealizada?.Invoke(jugada);
        }
    }
}
