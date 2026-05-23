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
using System.Drawing.Imaging;

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

        public Image? FotoRival {  get; set; }
        public string NombreRival { get; set; } = "";
        public string EloRival { get; set; } = "";

        private readonly Color colorFondoGlobal = Color.FromArgb(22, 21, 18);
        private readonly Color colorPanel = Color.FromArgb(38, 37, 34);
        private readonly Color colorTextoClaro = Color.White;
        private readonly Color colorTextoGris = Color.FromArgb(153, 153, 153);
        private readonly Color colorVerde = Color.FromArgb(129, 182, 76);
        private readonly Color colorBotonRojo = Color.FromArgb(180, 60, 60);

        // Globo de texto
        private GloboComic globoTexto = new GloboComic();
        private System.Windows.Forms.Timer timerGlobo = new System.Windows.Forms.Timer { Interval = 3000 };

        public static Dictionary<(TipoPieza, bool), Image>? imagenesPiezas;

        public Pieza?[,] ObtenerTablero() => _tablero;
        private Pieza?[,] _tablero = new Pieza?[8, 8];
        private int tamaño => PanelTablero.Width / 8; // tamaño de cada casilla

        public FormPartida()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
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
            this.Load += (s, e) => EstilizarUI();
        }
        private void EstilizarUI()
        {
            this.BackColor = colorFondoGlobal;
            this.Text = "Ajedrez Engine";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            int margen = 20;
            int anchoBarra = 14;
            int anchoFoto = 130;
            int altoFoto = 160;
            int anchoPanel = 300;
            int pad = 12;
            int altoForm = this.ClientSize.Height - margen * 2;
            int altoTablero = altoForm;
            int anchoTablero = altoTablero;

            // ── Calcular ancho total del layout para centrar ─────────
            int anchoTotal = anchoFoto + 6 + anchoBarra + 8 + anchoTablero + margen + anchoPanel;
            int xInicio = (this.ClientSize.Width - anchoTotal) / 2;

            // ── Foto del rival ───────────────────────────────────────
            // Foto — posición completamente independiente
            PbxRival.Location = new Point(margen + xInicio - 100, margen + (altoForm / 2) - altoFoto + 40);
            PbxRival.Size = new Size(anchoFoto + 30, altoFoto);
            PbxRival.SizeMode = PictureBoxSizeMode.StretchImage;
            PbxRival.BackColor = colorFondoGlobal;

            // ── Barra de evaluación ──────────────────────────────────
            int xBarra = xInicio + anchoFoto + 6; // mantiene su lugar original en el layout
            PanelEvaluacion.Location = new Point(xBarra, margen);
            PanelEvaluacion.Size = new Size(anchoBarra, altoForm);
            PanelEvaluacion.BackColor = Color.FromArgb(50, 50, 50);

            LblEvaluacionNumero.AutoSize = true;
            LblEvaluacionNumero.ForeColor = colorTextoGris;
            LblEvaluacionNumero.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            LblEvaluacionNumero.BackColor = Color.Transparent;
            LblEvaluacionNumero.Location = new Point(
                xBarra - 45,
                PanelEvaluacion.Top + (PanelEvaluacion.Height / 2) - 12);

            // ── Tablero ──────────────────────────────────────────────
            PanelTablero.Location = new Point(PanelEvaluacion.Right + 8, margen);
            PanelTablero.Size = new Size(anchoTablero, altoTablero);
            PanelTablero.BackColor = colorFondoGlobal;
            tableroBitmap = null;

            // ── Panel lateral derecho ────────────────────────────────
            int xPanel = PanelTablero.Right + margen;
            int altoPanel = altoForm;

            Panel panelLateral = new SmoothPanel
            {
                Location = new Point(xPanel, margen),
                Size = new Size(anchoPanel, altoPanel),
                BackColor = colorPanel,
            };
            this.Controls.Add(panelLateral);

            // Nombre
            Label lblNombre = new Label
            {
                Text = NombreRival,
                ForeColor = colorTextoClaro,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(pad, pad),
                Size = new Size(anchoPanel - pad * 2, 24),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };
            panelLateral.Controls.Add(lblNombre);

            // ELO — más alto para tener aire abajo
            Label lblElo = new Label
            {
                Text = EloRival,
                ForeColor = colorVerde,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(pad, lblNombre.Bottom + 6), // +6 en lugar de +2
                Size = new Size(anchoPanel - pad * 2, 24),   // 24 en lugar de 20
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };

            // Separador con más aire después del ELO
            Panel sep = new Panel
            {
                Location = new Point(pad, lblElo.Bottom + 10), // +10 en lugar de +6
                Size = new Size(anchoPanel - pad * 2, 1),
                BackColor = Color.FromArgb(60, 60, 55),
            };

            // Estado
            LblRespuesta.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            LblRespuesta.ForeColor = colorTextoGris;
            LblRespuesta.BackColor = Color.Transparent;
            LblRespuesta.Location = new Point(pad, sep.Bottom + 4);
            LblRespuesta.Size = new Size(anchoPanel - pad * 2, 32);
            LblRespuesta.TextAlign = ContentAlignment.MiddleCenter;
            panelLateral.Controls.Add(lblElo);   // ← faltaba
            panelLateral.Controls.Add(sep);      // ← faltaba
            panelLateral.Controls.Add(LblRespuesta);
            panelLateral.Controls.Add(LblRespuesta);

            // Historial — todo el espacio disponible entre estado y botón
            int yHistorial = LblRespuesta.Bottom + 6;
            LbxHistorial.Location = new Point(pad, yHistorial);
            LbxHistorial.Size = new Size(anchoPanel - pad * 2, altoPanel - yHistorial - 56);
            LbxHistorial.BackColor = Color.FromArgb(28, 27, 24);
            LbxHistorial.ForeColor = colorTextoClaro;
            LbxHistorial.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            LbxHistorial.BorderStyle = BorderStyle.None;
            panelLateral.Controls.Add(LbxHistorial);

            // Botón deshacer
            LblUndo.Text = "⟵  Deshacer";
            LblUndo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            LblUndo.ForeColor = colorTextoClaro;
            LblUndo.BackColor = colorBotonRojo;
            LblUndo.TextAlign = ContentAlignment.MiddleCenter;
            LblUndo.Cursor = Cursors.Hand;
            LblUndo.Location = new Point(pad, altoPanel - 44);
            LblUndo.Size = new Size(anchoPanel - pad * 2, 34);
            panelLateral.Controls.Add(LblUndo);

            panelLateral.BringToFront();
           // PbxRival.BringToFront();
        }

        private void PanelTablero_MouseClick(object sender, MouseEventArgs e)
        {
            int columna = e.X / tamaño;
            int fila = e.Y / tamaño;
            CasillaSeleccionada?.Invoke(fila, columna);
        }

        private void LblUndo_Click(object sender, EventArgs e)
        {
            if (LbxHistorial.Items.Count >= 2)
                UndoSolicitado?.Invoke();
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
            LbxHistorial.BeginUpdate();
            LbxHistorial.Items.Add($"{turno}: {jugada}");
            LbxHistorial.TopIndex = LbxHistorial.Items.Count - 1;
            LbxHistorial.EndUpdate();
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
                LblRespuesta.Text = $"Stockfish juega: {jugada}";
        }

        public void MostrarEvaluacionStockfish(int centipeones)
        {
            _evaluacionActual = centipeones;

            string texto = centipeones >= 0
            ? $"+{centipeones / 100.0:F1}"
            : $"{centipeones / 100.0:F1}";
            LblEvaluacionNumero.Text = texto;

            PanelEvaluacion.Invalidate();

            comentarioActual = _evaluacionActual > 50 ? "¡Vas a Ganar!" :
                               _evaluacionActual < -50 ? "¡Vas a Perder!" :
                               "¡Partida equilibrada!";
            timerGlobo.Start();
            var areaGlobo = new Rectangle(
                PbxRival.Left - 20,
                PbxRival.Top -110,
                PbxRival.Width + 40,
                100
                );
            this.Invalidate();
        }

        public void MostrarFinPartida(ResultadoPartida resultado)
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

            while (Math.Abs(pieza.PosX - destinoX) > paso ||
                   Math.Abs(pieza.PosY - destinoY) > paso)
            {
                // Mover solo lo que falta si es menor que el paso
                float dx = destinoX - pieza.PosX;
                float dy = destinoY - pieza.PosY;

                pieza.PosX += Math.Abs(dx) > paso ? Math.Sign(dx) * paso : dx;
                pieza.PosY += Math.Abs(dy) > paso ? Math.Sign(dy) * paso : dy;

                PanelTablero.Invalidate();
                await Task.Delay(15);
            }

            // Snap final exacto
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

            // Convertir centipeones a peones
            float peones = Math.Max(-15f, Math.Min(15f, _evaluacionActual / 100f));

            // Escala logarítmica: misma fórmula que Lichess
            // tanh comprime el rango infinito a (-1, 1) suavemente
            // Multiplicador controla qué tan rápido se satura (4 = ~3 peones ya es ventaja clara)
            float normalizado = (float)Math.Tanh(peones / 4.0);

            // Convertir de (-1,1) a (0,1) desde perspectiva de blancas
            float porcentajeBlancas = (normalizado + 1f) / 2f;

            int alturaBlancas = (int)(alto * porcentajeBlancas);
            int alturaNegras = alto - alturaBlancas;

            g.FillRectangle(Brushes.Black, 0, 0, ancho, alturaNegras);
            g.FillRectangle(Brushes.White, 0, alturaNegras, ancho, alturaBlancas);
        }

        private void FormPartida_FormClosing(object sender, FormClosingEventArgs e)
        {
            VistaCerrada?.Invoke();
            Task.Delay(1000);
            Application.Exit();
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
                    PbxRival.Left - 20, PbxRival.Top - 100,
                    PbxRival.Width + 40, 80);
                globoTexto.Dibujar(e.Graphics, area, comentarioActual);
            }
        }
    }
}