using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.IO;

using static Ajedrez_interactuable_con_form.FormMenu;

namespace Ajedrez_interactuable_con_form
{
    public partial class Form1 : Form //clase principal del formulario
    {
        private StockfishMotor motor;
        private Bitmap tableroBitmap;

        private int casillaSeleccionadaFila = -1; // -1 indica que no hay casilla seleccionada
        private int casillaSeleccionadaColumna = -1; // -1 indica que no hay casilla seleccionada
        private int tamaño => panelTablero.Width / 8; // tamaño dinámico basado en el tamaño del panel
        private List<string> movimientosPosibles = new List<string>();

        private GloboComic globo = new GloboComic();
        private string comentarioActual = "";
        private int contadorJugadasUsuario = 0;
        private System.Windows.Forms.Timer timerGlobo = new System.Windows.Forms.Timer { Interval = 3000 };

        public static Dictionary<(TipoPieza, bool), Image> imagenesPiezas;
        private TipoRival rival;
        private JuegoAjedrez juego;

        public Form1(TipoRival rivalSeleccionado) //constructor del formulario
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            typeof(Panel).InvokeMember
                ("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, panelTablero, new object[]
                { true });

            this.Paint += Form1_Paint;

            rival = rivalSeleccionado;
            juego = new JuegoAjedrez();
            juego.TableroActualizado += () => panelTablero.Invalidate();
            juego.JugadaRealizada += (jugada) => ProcesarJugada_Stockfish(jugada);

            timerGlobo.Tick += (s, e) =>
            {
                comentarioActual = "";
                timerGlobo.Stop();
                this.Invalidate(); // repinta el Form para borrar el globo
            };

            juego.MostrarMenuCoronacionUI = (fila, col, peon, callback) =>
            {
                MostrarMenuCoronacion(fila, col, peon, callback);
            };

            motor = new StockfishMotor();
            motor.BestMove_Encontrado += Motor_BestMove_Encontrado; // Eventos se suscriben a métodos
            motor.Evaluacion_Actualizada += Motor_Evaluacion_Actualizada;

            // Leer README
            string rutaStockfish = @"Stockfish\stockfish-windows-x86-64-avx2.exe";

            motor.Iniciar(rutaStockfish);


            CargarImagenes();
        }
        private void Motor_BestMove_Encontrado(string BestMove) //método para iniciar el proceso de Stockfish en segundo plano
        {
            this.Invoke((MethodInvoker)delegate
            {
                var historialJugadas = juego.Historial;
                historialJugadas.Add(BestMove);
                LbxHistorial.Items.Add("Negras: " + BestMove);
                LblRespuesta.Text = $"Stockfish juega: {BestMove}";
            });

            juego.MoverPieza(BestMove, false);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            motor.Cerrar();
        }
        private void CargarImagenes()
        {
            imagenesPiezas = new Dictionary<(TipoPieza, bool), Image>
            {
                { (TipoPieza.Peon, true), Properties.Resources.PeonBlanco },
                { (TipoPieza.Peon, false), Properties.Resources.PeonNegro },
                { (TipoPieza.Alfil, true), Properties.Resources.AlfilBlanco },
                { (TipoPieza.Alfil, false), Properties.Resources.AlfilNegro },
                { (TipoPieza.Torre, true), Properties.Resources.TorreBlanca },
                { (TipoPieza.Torre, false), Properties.Resources.TorreNegra },
                { (TipoPieza.Caballo, true), Properties.Resources.CaballoBlanco },
                { (TipoPieza.Caballo, false), Properties.Resources.CaballoNegro },
                { (TipoPieza.Dama, true), Properties.Resources.DamaBlanca },
                { (TipoPieza.Dama, false), Properties.Resources.DamaNegra },
                { (TipoPieza.Rey, true), Properties.Resources.ReyBlanco },
                { (TipoPieza.Rey, false), Properties.Resources.ReyNegro }
            };

            // Asignamos al diccionario estático de la clase Piezas
            Piezas.ImagenesPiezas = imagenesPiezas;
        }
        private void CrearTableroBitmap()
        {
            tableroBitmap = new Bitmap(panelTablero.Width, panelTablero.Height);

            using (Graphics g = Graphics.FromImage(tableroBitmap))
            {
                // Dibujar tablero base
                for (int fila = 0; fila < 8; fila++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        Brush brocha = ((fila + col) % 2 == 0) ? Brushes.Beige : Brushes.Brown;
                        g.FillRectangle(brocha, col * tamaño, fila * tamaño, tamaño, tamaño);
                    }
                }
            }
        }
        private void PanelTablero_Paint(object sender, PaintEventArgs e)
        {
            if (tableroBitmap == null) CrearTableroBitmap();

            e.Graphics.DrawImage(tableroBitmap, 0, 0);

            if (casillaSeleccionadaFila >= 0 && casillaSeleccionadaColumna >= 0)
            {
                e.Graphics.DrawRectangle(new Pen(Color.Gray, 3),
                    casillaSeleccionadaColumna * tamaño,
                    casillaSeleccionadaFila * tamaño,
                    tamaño, tamaño);
            }
            // Dibujar movimientos posibles
            foreach (var jugada in movimientosPosibles)
            {
                string destino = jugada.Substring(2, 2); // por ejemplo: "e4"
                int col = destino[0] - 'a';
                int fila = 8 - (destino[1] - '0');

                int diametro = tamaño / 2;
                int offset = (tamaño - diametro) / 2;

                using (Brush highlight = new SolidBrush(Color.FromArgb(100, Color.Green)))
                {
                    e.Graphics.FillEllipse(highlight, col * tamaño + offset, fila * tamaño + offset, diametro, diametro);
                }
            }

            // Dibujar piezas
            for (int fila = 0; fila < 8; fila++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var pieza = juego.Tablero[fila, col];
                    if (pieza != null)
                        pieza.Dibujar(e.Graphics, tamaño);
                }
            }
        }
        private async void PanelTablero_MouseClick(object sender, MouseEventArgs e)
        {
            var historialJugadas = juego.Historial;

            int col = e.X / tamaño; //columna donde se hizo clic de 0 a 49 en tamaño 50
            int fila = e.Y / tamaño; //fila donde se hizo clic

            if (casillaSeleccionadaFila == -1)
            {
                // casilla de origen seleccionada
                casillaSeleccionadaFila = fila;
                casillaSeleccionadaColumna = col;

                var todasJugadas = await motor.ObtenerJugadasLegalesAsync(historialJugadas);

                string origen = CasillaToTexto(fila, col);
                movimientosPosibles = todasJugadas
                                      .Where(j => j.StartsWith(origen))
                                      .ToList();
            }
            else
            {
                // casilla de destino seleccionada
                string jugada = $"{CasillaToTexto(casillaSeleccionadaFila, casillaSeleccionadaColumna)}" +
                                $"{CasillaToTexto(fila, col)}";

                if (await motor.EsJugadaValidaAsync(jugada, historialJugadas))
                {
                    // Obtener la pieza a mover
                    var pieza = juego.Tablero[casillaSeleccionadaFila, casillaSeleccionadaColumna];

                    // Animar movimiento antes de actualizar el tablero
                    await AnimarMovimiento(pieza, fila, col);

                    // Actualizar el tablero
                    juego.MoverPieza(jugada, true);

                    contadorJugadasUsuario++;
                    if (contadorJugadasUsuario % 3 == 0)
                    {
                        this.Invalidate();
                        timerGlobo.Start(); // el timer ocultará el globo después de 3s
                    }

                }

                casillaSeleccionadaFila = -1; // Resetear selecciones
                casillaSeleccionadaColumna = -1;
                movimientosPosibles.Clear();
            }
            panelTablero.Invalidate();
            // Redibujar el tablero para mostrar selección
        }
        private string CasillaToTexto(int fila, int col)
        {
            char letra = (char)('a' + col); // columnas: a-h es solo para las columnas 0-7    // (char) convierte un número entero en el carácter correspondiente en Unicode/ASCII

            int numero = 8 - fila; // filas: 8-1 es para las filas de arriba hacia abajo
            return $"{letra}{numero}"; // (0,0) -> a8, (7,7) -> h1
        }
        private void ProcesarJugada_Stockfish(string jugadaUsuario)
        {
            var historialJugadas = juego.Historial;

            if (jugadaUsuario.Length < 4 || jugadaUsuario.Length > 5)
            {
                MessageBox.Show("Formato inválido. Usa jugadas como e2e4 o e7e8q.");
                return;
            }

            this.Invoke((MethodInvoker)delegate
            {
                LblRespuesta.Text = $"Procesando tu jugada: {jugadaUsuario}";
                historialJugadas.Add(jugadaUsuario);
                LbxHistorial.Items.Add("Blancas: " + jugadaUsuario);
            });
            // Enviamos posición a Stockfish
            string lineaPosition = "position startpos moves " + string.Join(" ", historialJugadas);
            motor.EnviarComando(lineaPosition);

            int profundidad = rival == TipoRival.Andrea ? 6 : 12; // ejemplo: Andrea = más fácil/menos profundidad

            motor.EnviarComando($"go depth {profundidad}");
        }

        private void MostrarMenuCoronacion(int filaDestino, int colDestino, Piezas peon, Action<char> onElegir)
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Dama", Properties.Resources.DamaBlanca, (s, e) =>
            {
                peon.Tipo = TipoPieza.Dama;
                panelTablero.Invalidate();
                onElegir?.Invoke('q');
            });
            menu.Items.Add("Torre", Properties.Resources.TorreBlanca, (s, e) =>
            {
                peon.Tipo = TipoPieza.Torre;
                panelTablero.Invalidate();
                onElegir?.Invoke('r');
            });
            menu.Items.Add("Alfil", Properties.Resources.AlfilBlanco, (s, e) =>
            {
                peon.Tipo = TipoPieza.Alfil;
                panelTablero.Invalidate();
                onElegir?.Invoke('b');
            });
            menu.Items.Add("Caballo", Properties.Resources.CaballoBlanco, (s, e) =>
            {
                peon.Tipo = TipoPieza.Caballo;
                panelTablero.Invalidate();
                onElegir?.Invoke('n');
            });

            int size = panelTablero.Width / 8;
            int x = colDestino * size;
            int y = filaDestino * size;
            menu.Show(panelTablero, new Point(x, y));
        }
        private async Task AnimarMovimiento(Piezas pieza, int filaDestino, int colDestino)
        {
            float destinoX = colDestino * tamaño;
            float destinoY = filaDestino * tamaño;
            float paso = 5f;

            // Inicializar PosX/PosY si es la primera vez
            if (pieza.PosX < 0) pieza.PosX = pieza.Columna * tamaño;
            if (pieza.PosY < 0) pieza.PosY = pieza.Fila * tamaño;

            while (Math.Abs(pieza.PosX - destinoX) > 0.1 || Math.Abs(pieza.PosY - destinoY) > 0.1)
            {
                pieza.PosX += Math.Sign(destinoX - pieza.PosX) * paso;
                pieza.PosY += Math.Sign(destinoY - pieza.PosY) * paso;

                panelTablero.Invalidate();
                await Task.Delay(15); // espera breve para animación
            }

            // Resetear PosX/PosY para que la pieza se dibuje en la cuadrícula
            pieza.PosX = -1;
            pieza.PosY = -1;
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (!string.IsNullOrEmpty(comentarioActual))
            {
                Rectangle areaGlobo = new Rectangle(
                    PbxRival.Left - 20,  // Ajustes manuales
                    PbxRival.Top - 120,  
                    PbxRival.Width + 40,
                    100
                );

                globo.Dibujar(e.Graphics, areaGlobo, comentarioActual);
            }
        }
        private void Motor_Evaluacion_Actualizada(int eval)
        {
            // quedó pendiente cambiar la imagen del globo y el texto según la evaluación
            this.Invoke((MethodInvoker)delegate
            {
                string comentario;
                Image nuevaImagen;

                if (eval == 10000)
                {
                    comentario = "¡Me ganaste!";
                    nuevaImagen = Properties.Resources.AndreaPierde;
                }
                else if (eval > 50) // ventaja IA
                {
                    comentario = "¡Vas a perder!";
                    nuevaImagen = Properties.Resources.AndreaBuenaJugadaCallando;
                }
                else if (eval < -50) // ventaja jugador
                {
                    comentario = "¡Bien hecho!";
                    nuevaImagen = Properties.Resources.AndreaMenu;
                }
                else
                {
                    comentario = "Esto está parejo...";
                    nuevaImagen = Properties.Resources.AndreaMenu;
                }

                comentarioActual = comentario; // Actualizar comentario y imagen
                PbxRival.Image = nuevaImagen; 
                this.Invalidate(); // repinta el Form para mostrar el globo con el nuevo comentario
            });
        }
    }
}
