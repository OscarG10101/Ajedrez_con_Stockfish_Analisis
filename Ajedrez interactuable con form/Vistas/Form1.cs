using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.IO;

using static Ajedrez_interactuable_con_form.FormMenu;
using Ajedrez_interactuable_con_form.Modelos;
using Ajedrez_interactuable_con_form.Vistas;
using Ajedrez_interactuable_con_form.Servicios;

namespace Ajedrez_interactuable_con_form
{
    public partial class Form1 : Form //clase principal del formulario
    {
        private StockfishMotor motor;
        private Bitmap tableroBitmap;

        private int casillaSeleccionadaFila = -1; // -1 indica que no hay casilla seleccionada
        private int casillaSeleccionadaColumna = -1; // -1 indica que no hay casilla seleccionada
        private int tamaño => PanelTablero.Width / 8; // tamaño dinámico basado en el tamaño del panel
        private List<string> movimientosPosibles = new List<string>();

        private GloboComic globoTexto = new GloboComic();
        private string comentarioActual = "";
        private int contadorJugadasUsuario = 0;
        private System.Windows.Forms.Timer timerGlobo = new System.Windows.Forms.Timer { Interval = 3000 };

        public static Dictionary<(TipoPieza, bool), Image> imagenesPiezas;
        private int _evaluacionActual = 0;
        private TableroAjedrez juego;

        public Form1(TipoRival rivalSeleccionado) //constructor del formulario
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            typeof(Panel).InvokeMember
                ("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, PanelTablero, new object[]
                { true });

            this.Paint += Form1_Paint;
            PanelEvaluacion.Paint += (s, e) => DibujarBarraEvaluacion(e.Graphics);

            int rivalElo = (int)rivalSeleccionado;

            juego = new TableroAjedrez();
            juego.TableroActualizado += () => PanelTablero.Invalidate();
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
            MessageBox.Show($"Has elegido jugar contra {rivalSeleccionado} (Elo {rivalElo}). ¡Buena suerte!");
            motor = new StockfishMotor();
            motor.BestMove_Encontrado += OnBestMove_Motor; // Eventos se suscriben a métodos
            motor.Evaluacion_Actualizada += OnEvaluacionActualizada_Motor;
            motor.SinJugadasLegales += OnSinJugada_Motor;

            // Leer README
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string carpeta = Path.Combine(baseDir, @"..\..\..\Stockfish");
            carpeta = Path.GetFullPath(carpeta);

            if (!Directory.Exists(carpeta))
                throw new DirectoryNotFoundException($"No se encontró la carpeta: {carpeta}");

            var files = Directory.GetFiles(carpeta, "stockfish*.exe");
            if (files.Length == 0)
                throw new FileNotFoundException($"No se encontró ningún ejecutable de Stockfish en: {carpeta}");

            string exe = files[0];
            motor.Iniciar(exe);
            motor.ConfigurarNivel((int)rivalElo);


            CargarImagenesPiezas();
        }
        private void OnBestMove_Motor(string BestMove) //método para iniciar el proceso de Stockfish en segundo plano
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
        private void CargarImagenesPiezas()
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
            Pieza.ImagenesPiezas = imagenesPiezas;
        }
        private void CrearTableroBitmap()
        {
            tableroBitmap = new Bitmap(PanelTablero.Width, PanelTablero.Height);

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

        private void DibujarBarraEvaluacion(Graphics g)
        {
            int alto = PanelEvaluacion.Height;
            int ancho = PanelEvaluacion.Width;

            int eval = Math.Max(-10000, Math.Min(10000, _evaluacionActual)); // Limitar entre -10000 y 10000

            float porcentajeBlancas = (eval + 10000f) / 20000f; // Convertir a porcentaje (0 a 1)
            int alturaBlancas = (int)(alto * porcentajeBlancas);
            int alturaNegras = alto - alturaBlancas;

            g.FillRectangle(Brushes.Black, 0, 0, ancho, alturaNegras); // Parte negra

            g.FillRectangle(Brushes.White, 0, alturaNegras, ancho, alturaBlancas); // Parte blanca

            g.DrawLine(Pens.Gray, 0, alturaNegras, ancho, alturaNegras); // Línea divisoria
        }
        private void OnEvaluacionActualizada_Motor(int eval)
        {
            this.Invoke((MethodInvoker)delegate
            {
                // Si el historial tiene número impar de jugadas, Stockfish evaluó
                // desde la perspectiva de negras y se invierte signo
                bool turnoNegras = juego.Historial.Count % 2 != 0;
                int evalDesdeBlancas = turnoNegras ? -eval : eval;

                _evaluacionActual = evalDesdeBlancas;

                string texto = evalDesdeBlancas >= 0
                    ? $"+{evalDesdeBlancas / 100.0:F1}"
                    : $"{evalDesdeBlancas / 100.0:F1}";
                LblEvaluacionNumero.Text = texto;

                PanelEvaluacion.Invalidate();

                string comentario;
                if (evalDesdeBlancas > 50) comentario = "¡Vas a perder!";
                else if (evalDesdeBlancas < -50) comentario = "¡Bien hecho!";
                else comentario = "Esto está parejo...";

                comentarioActual = comentario;
                timerGlobo.Start();
                this.Invalidate();
            });
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

                var todasLasJugadas = await motor.PedirJugadasLegalesAsync(historialJugadas);
                if (todasLasJugadas.Count == 0)
                {
                    OnSinJugada_Motor();
                    return;
                }

                string casillaOrigen = CasillaToTexto(fila, col);
                movimientosPosibles = (await motor.PedirJugadasLegalesAsync(historialJugadas))
                                      .Where(j => j.StartsWith(casillaOrigen))
                                      .ToList();
            }
            else
            {
                // coordenadas completas "e2e4"
                string jugada = $"{CasillaToTexto(casillaSeleccionadaFila, casillaSeleccionadaColumna)}" +
                                $"{CasillaToTexto(fila, col)}";

                if (movimientosPosibles.Contains(jugada))
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
            PanelTablero.Invalidate();
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

            motor.PedirBestMove(historialJugadas);
        }

        private void MostrarMenuCoronacion(int filaDestino, int colDestino, Pieza peon, Action<char> onElegir)
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Dama", Properties.Resources.DamaBlanca, (s, e) =>
            {
                peon.Tipo = TipoPieza.Dama;
                PanelTablero.Invalidate();
                onElegir?.Invoke('q');
            });
            menu.Items.Add("Torre", Properties.Resources.TorreBlanca, (s, e) =>
            {
                peon.Tipo = TipoPieza.Torre;
                PanelTablero.Invalidate();
                onElegir?.Invoke('r');
            });
            menu.Items.Add("Alfil", Properties.Resources.AlfilBlanco, (s, e) =>
            {
                peon.Tipo = TipoPieza.Alfil;
                PanelTablero.Invalidate();
                onElegir?.Invoke('b');
            });
            menu.Items.Add("Caballo", Properties.Resources.CaballoBlanco, (s, e) =>
            {
                peon.Tipo = TipoPieza.Caballo;
                PanelTablero.Invalidate();
                onElegir?.Invoke('n');
            });

            int size = PanelTablero.Width / 8;
            int x = colDestino * size;
            int y = filaDestino * size;
            menu.Show(PanelTablero, new Point(x, y));
        }
        private async Task AnimarMovimiento(Pieza pieza, int filaDestino, int colDestino)
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

                PanelTablero.Invalidate();
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

                globoTexto.Dibujar(e.Graphics, areaGlobo, comentarioActual);
            }
        }

        private void LblUndo_Click(object sender, EventArgs e)
        {
            if (juego.Historial.Count < 2) return;

            juego.DeshacerJugadas();
            var historial = juego.Historial;

            // Actualizar historial label
            LbxHistorial.Items.Clear();
            for (int i = 0; i < historial.Count; i++)
            {
                string prefijo = (i % 2 == 0) ? "Blancas: " : "Negras: ";
                LbxHistorial.Items.Add(prefijo + historial[i]);
            }

            movimientosPosibles.Clear();
            casillaSeleccionadaColumna = -1;
            casillaSeleccionadaFila = -1;
            LblRespuesta.Text = "Jugada deshecha.";
            PanelTablero.Invalidate();
        }

        private void OnSinJugada_Motor()
        {
            this.Invoke((MethodInvoker)delegate
            {
                ResultadoPartida resultado;

                if (motor.UltimaEvaluacionFueMate)
                {
                    bool turnoBlancas = juego.Historial.Count % 2 == 0; // si blancas no puede mover ganan negras
                    resultado = turnoBlancas ?
                    ResultadoPartida.GanaNegras
                    : ResultadoPartida.GanaBlancas;
                }
                else if (juego.EsTripleRepeticion())
                    resultado = ResultadoPartida.TripleRepeticion;
                else if (juego.EsCincuentaMovimientos())
                    resultado = ResultadoPartida.CincuentaMovimientos;
                else
                    resultado = ResultadoPartida.Ahogado;

                MostrarFinPartida(resultado);
            });
        }

        private void MostrarFinPartida(ResultadoPartida resultado)
        {
            string mensaje = resultado switch
            {
                ResultadoPartida.GanaBlancas => "¡Jaque mate! Ganaste.",
                ResultadoPartida.GanaNegras => "¡Jaque mate! Perdiste.",
                ResultadoPartida.Ahogado => "¡Ahogado! Tablas.",
                ResultadoPartida.TripleRepeticion => "Triple repetición. Tablas.",
                ResultadoPartida.CincuentaMovimientos => "Regla de 50 movimientos. Tablas.",
                _ => "Partida terminada."
            };

            MessageBox.Show(mensaje);
            // Aquí después agregaré: guardar en DB y mostrar botón de volver al menú
        }
    }
}