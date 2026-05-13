using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.IO;
using Ajedrez_interactuable_con_form.Modelos;
using Ajedrez_interactuable_con_form.Vistas;
using Ajedrez_interactuable_con_form.Servicios;
using System.Threading.Tasks;

namespace Ajedrez_interactuable_con_form
{
    public partial class FormPartida : Form, IVistaPartida
    {
        // Eventos hacia presentador
        public event Action<int, int>? CasillaSeleccionada;
        public event Action? UndoSolicitado;
        public event Action? VistaCerrada;

        // Para estados de visualización
        private Bitmap? tableroBitmap;
        private List<string> _movimientosPosibles = new List<string>();
        private int _casillaSeleccionadaFila = -1;
        private int _casillaSeleccionadaColumna = -1;
        private int _evaluacionActual = 0;
        private string comentarioActual = "";

        // Globo de texto
        private GloboComic globoTexto = new GloboComic();
        private System.Windows.Forms.Timer timerGlobo = new System.Windows.Forms.Timer { Interval = 3000 };

        public static Dictionary<(TipoPieza, bool), Image>? imagenesPiezas;

        public Pieza?[,] ObtenerTablero() => _tablero;
        private Pieza?[,] _tablero = new Pieza?[8, 8];
        private int tamaño => PanelTablero.Width / 8;

        public FormPartida()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            typeof(Panel).InvokeMember
                ("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, PanelTablero, new object[] { true });

            this.Paint += FormPartida_Paint;
            PanelEvaluacion.Paint += (s, e) => DibujarBarraEvaluacion(e.Graphics);

            timerGlobo.Tick += (s, e) =>
            {
                comentarioActual = "";
                timerGlobo.Stop();
                this.Invalidate(); // repinta el Form para borrar el globo
            };

            CargarImagenesPiezas();
        }

        public void MostrarMovimientosPosibles(List<string> jugadas)
        {
            _movimientosPosibles = jugadas;
            PanelTablero.Invalidate();
        }

        public void LimpiarSeleccion()
        {
            _casillaSeleccionadaFila = -1;
            _casillaSeleccionadaColumna = -1;
            _movimientosPosibles.Clear();
            PanelTablero.Invalidate();
        }

        public void AgregarJugadaHistorial(string turno, string jugada)
        {
            this.Invoke(() => { 
                LbxHistorial.Items.Add($"{turno}: {jugada}");
            });
        }

        public void LimpiarHistorial()
        {
            LbxHistorial.Items.Clear();
        }

        public void MostrarMensajeEstado(string mensaje)
        {
            LblRespuesta.Text = mensaje;
        }

        public void MostrarJugadaStockfish(string jugada)
        {
            this.Invoke(() =>
            {
                LblRespuesta.Text = $"Stockfish juega: {jugada}";
            });
        }

        public void MostrarEvaluacionStockfish(int centipeones)
        {
            this.Invoke(() =>
            {
                _evaluacionActual = centipeones;

                string texto = centipeones >= 0
                ? $"+{centipeones / 100.0:F1}"
                : $"{centipeones / 100.0:F1}";
                LblEvaluacionNumero.Text = texto;

                PanelEvaluacion.Invalidate();

                comentarioActual = centipeones > 50 ? "¡Vas a perder!" :
                                 centipeones < -50 ? "¡Vas a ganar!" :
                                 "¡Partida equilibrada!";
                timerGlobo.Start();
                this.Invalidate();
            });
        }

        public void MostrarFinPartida(ResultadoPartida resultado)
        {
            this.Invoke(() =>
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
            });
        }

        public void ActualizarTablero()
        {
            if (PanelTablero.InvokeRequired)
                PanelTablero.Invoke(() => PanelTablero.Invalidate());
            else
                PanelTablero.Invalidate();
        }

        public void SincronizarTablero(Pieza?[,] tablero)
        {
            _tablero = tablero;
        }

        public async Task AnimarMovimientoAsync(Pieza pieza, int filaDestino, int columnaDestino)
        {
            float destinoX = columnaDestino * tamaño;
            float destinoY = filaDestino * tamaño;
            float paso = 5f;

            if (pieza.PosX < 0) pieza.PosX = pieza.Columna * tamaño;
            if (pieza.PosY < 0) pieza.PosY = pieza.Fila * tamaño;

            while (Math.Abs(pieza.PosX - destinoX) > 0.1 ||
                   Math.Abs(pieza.PosY - destinoY) > 0.1)
            {
                pieza.PosX += Math.Sign(destinoX - pieza.PosX) * paso;
                pieza.PosY += Math.Sign(destinoY - pieza.PosY) * paso;
                PanelTablero.Invalidate();
                await Task.Delay(15);
            }

            pieza.PosX = -1;
            pieza.PosY = -1;
        }

        public void MostrarMenuCoronacion(int fila, int columna, Pieza peon, Action<char> callback)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => MostrarMenuCoronacion(fila, columna, peon, callback));
                return;
            }

            var menu = new ContextMenuStrip();
            menu.Items.Add("Dama", Properties.Resources.DamaBlanca, (s, e) => { peon.Tipo = TipoPieza.Dama; PanelTablero.Invalidate(); callback('q'); });
            menu.Items.Add("Torre", Properties.Resources.TorreBlanca, (s, e) => { peon.Tipo = TipoPieza.Torre; PanelTablero.Invalidate(); callback('r'); });
            menu.Items.Add("Alfil", Properties.Resources.AlfilBlanco, (s, e) => { peon.Tipo = TipoPieza.Alfil; PanelTablero.Invalidate(); callback('b'); });
            menu.Items.Add("Caballo", Properties.Resources.CaballoBlanco, (s, e) => { peon.Tipo = TipoPieza.Caballo; PanelTablero.Invalidate(); callback('n'); });

            menu.Show(PanelTablero, new Point(columna * tamaño, fila * tamaño));
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

        }

        private void PanelTablero_MouseClick(object sender, MouseEventArgs e)
        {
            int columna = e.X / tamaño;
            int fila = e.Y / tamaño;
            CasillaSeleccionada?.Invoke(fila, columna);
        }

        private void LblUndo_Click(object sender, EventArgs e)
        {
            UndoSolicitado?.Invoke();
        }

        private void FormPartida_FormClosing(object sender, FormClosingEventArgs e)
        {
            VistaCerrada?.Invoke();
            this.Close();
        }

        private void PanelTablero_Paint(object sender, PaintEventArgs e)
        {
            if (tableroBitmap == null) CrearTableroBitmap();
            if (tableroBitmap == null) return;
            
            e.Graphics.DrawImage(tableroBitmap, 0, 0);

            // selección
            if (_casillaSeleccionadaFila >= 0 && _casillaSeleccionadaColumna >= 0)
                e.Graphics.DrawRectangle(new Pen(Color.Gray, 3),
                    _casillaSeleccionadaColumna * tamaño, _casillaSeleccionadaFila * tamaño, tamaño, tamaño);

            // movimientos posibles
            foreach (var jugada in _movimientosPosibles)
            {
                string destino = jugada.Substring(2, 2);
                int col = destino[0] - 'a';
                int fila = 8 - (destino[1] - '0');
                int diam = tamaño / 2;
                int off = (tamaño - diam) / 2;
                using Brush h = new SolidBrush(Color.FromArgb(100, Color.Green));
                e.Graphics.FillEllipse(h, col * tamaño + off, fila * tamaño + off, diam, diam);
            }

            // piezas — lee del tablero que le pasó el presentador
            var tablero = ObtenerTablero();
            for (int f = 0; f < 8; f++)
                for (int c = 0; c < 8; c++)
                    tablero[f, c]?.Dibujar(e.Graphics, tamaño);
        }

        private void FormPartida_Paint(object? sender, PaintEventArgs e)
        {
            if (!string.IsNullOrEmpty(comentarioActual))
            {
                var area = new Rectangle(
                    PbxRival.Left - 20, PbxRival.Top - 120,
                    PbxRival.Width + 40, 100);
                globoTexto.Dibujar(e.Graphics, area, comentarioActual);
            }
        }
    }
}