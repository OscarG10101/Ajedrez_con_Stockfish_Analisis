using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ajedrez_interactuable_con_form.Modelos;
using Ajedrez_interactuable_con_form.Vistas;

namespace Ajedrez_interactuable_con_form
{
    public partial class FormMenu : Form, IVistaMenu
    {
        public event Action<TipoRival>? RivalSeleccionado;

        // Variables para controlar qué tarjeta tiene el mouse encima (para el borde iluminado)
        private Panel? panelConHover = null;

        // Paleta de colores profesionales (Estilo Chess.com / Lichess)
        private readonly Color colorFondoGlobal = Color.FromArgb(22, 21, 18);   // Gris casi negro
        private readonly Color colorTarjetaBase = Color.FromArgb(38, 37, 34);   // Gris carbón
        private readonly Color colorTarjetaHover = Color.FromArgb(49, 47, 43);  // Gris más claro al pasar el mouse
        private readonly Color colorTextoClaro = Color.White;
        private readonly Color colorTextoGris = Color.FromArgb(153, 153, 153);  // Gris sutil para descripciones
        private readonly Color colorBotonVerde = Color.FromArgb(129, 182, 76);  // Verde Chess para jugar
        private readonly Color colorBordeIluminado = Color.FromArgb(154, 207, 101); // Verde brillante
        
        public FormMenu()
        {
            InitializeComponent(); 
            
            this.BackColor = colorFondoGlobal;
            this.Text = "Selección de Rival - Ajedrez Engine";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;

            rival.ForeColor = colorTextoClaro;
            rival.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            rival.TextAlign = ContentAlignment.TopCenter;
            rival.Location = new Point((ClientSize.Width / 2) - (rival.Width / 2), 20);

            Label lblSubtitulo = new Label
            {
                Text = "Elige tu rival y demuestra tu nivel",
                ForeColor = colorTextoGris,
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(rival.Left + 30, rival.Bottom + 4)
            };
            this.Controls.Add(lblSubtitulo);

            PbxAndrea.Image = Properties.Resources.Andrea;
            PbxNatasha.Image = Properties.Resources.Natasha;
            PbxAlejandra.Image = Properties.Resources.Alejandra;

            ConfigurarTarjetaRival(PbxAndrea, BtnAndrea, TipoRival.Andrea, "800 ELO", 
                "Le gusta atacar rápido, pero suele cometer errores tácticos bajo presión.", 1);
            ConfigurarTarjetaRival(PbxNatasha, BtnNatasha, TipoRival.Natasha, "1500 ELO", 
                "Estilo posicional y equilibrado. Esperará pacientemente a que cometas un error.", 3);
            ConfigurarTarjetaRival(PbxAlejandra, BtnAlejandra, TipoRival.Alejandra, "2200 ELO", 
                "Nivel Maestro. Cálculo perfecto y contraataques fulminantes. No perdona.", 5);
        }

        private void ConfigurarTarjetaRival(SmoothPictureBox pbx, Button btn, TipoRival rival, string eloText, string descripcionText, int estrellas)
        {
            // Medidas fijas para cada sección
            int padding = 12;
            int altoFoto = pbx.Height;
            int altoElo = 25;
            int altoEstrellas = 28;
            int altoDesc = 55;
            int espaciado = 6;   // separación entre cada bloque
            int altoBoton = btn.Height;

            // Calcular posiciones Y de cada elemento
            int yFoto = padding;
            int yElo = yFoto + altoFoto + espaciado;
            int yEstrellas = yElo + altoElo + espaciado;
            int yDesc = yEstrellas + altoEstrellas + espaciado;
            int yBoton = yDesc + altoDesc + espaciado;

            // Alto total del panel calculado exactamente
            int altoPanel = yBoton + altoBoton + padding;

            Panel panelTarjeta = new SmoothPanel
            {
                Size = new Size(pbx.Width + 24, altoPanel),
                Location = new Point(pbx.Left - 12, pbx.Top + 30),
                BackColor = colorTarjetaBase,
                Padding = new Padding(padding)
            };

            this.Controls.Add(panelTarjeta);
            panelTarjeta.Controls.Add(pbx);
            panelTarjeta.Controls.Add(btn);

            pbx.Location = new Point(padding, yFoto);
            pbx.SizeMode = PictureBoxSizeMode.StretchImage;
            pbx.BackColor = colorTarjetaBase;

            Label lblElo = new Label
            {
                Text = eloText,
                ForeColor = colorBotonVerde,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(0, yElo),
                Size = new Size(panelTarjeta.Width, altoElo),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };

            Label lblEstrellas = new Label
            {
                Text = new string('★', estrellas) + new string('☆', 5 - estrellas),
                ForeColor = Color.FromArgb(255, 200, 0),
                Font = new Font("Segoe UI", 13, FontStyle.Regular),
                Location = new Point(0, yEstrellas),
                Size = new Size(panelTarjeta.Width, altoEstrellas),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label lblDescripcion = new Label
            {
                Text = descripcionText,
                ForeColor = colorTextoGris,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Location = new Point(padding, yDesc),
                Size = new Size(panelTarjeta.Width - padding * 2, altoDesc),
                AutoSize = false,
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent
            };

            btn.Location = new Point(padding, yBoton);
            btn.Width = panelTarjeta.Width - padding * 2;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = colorBotonVerde;
            btn.ForeColor = colorTextoClaro;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btn.Text = $"Jugar contra {rival}";

            panelTarjeta.Controls.Add(lblElo);
            panelTarjeta.Controls.Add(lblEstrellas);
            panelTarjeta.Controls.Add(lblDescripcion);
            panelTarjeta.BringToFront();

            VincularEventosMouseAtarjeta(panelTarjeta);
        }

        private void VincularEventosMouseAtarjeta(Panel panel)
        {
            panel.Cursor = Cursors.Hand;

            // MouseEnter activa este panel y automáticamente desactiva el anterior
            panel.MouseEnter += (s, e) => ActivarHoverPanel(panel);

            // MouseLeave solo desactiva si el cursor salió fuera de TODOS los paneles de tarjeta
            panel.MouseLeave += (s, e) =>
            {
                // Verificar si el cursor entró a un control hijo del mismo panel
                // (ej: Label, PictureBox dentro de la tarjeta) — si es así, no desactivar
                Point cursorEnPanel = panel.PointToClient(Cursor.Position);
                if (panel.ClientRectangle.Contains(cursorEnPanel))
                    return;

                // Solo desactivar si el mouse no está ya sobre otro panel
                // (ActivarHoverPanel del nuevo panel ya habrá corrido antes)
                if (panelConHover == panel)
                    DesactivarHoverPanel(panel);
            };

            panel.Click += (s, e) => DispararSeleccionPorTarjeta(panel);

            // Vincular MouseEnter/Leave también a los controles hijos para evitar gaps
            foreach (Control hijo in panel.Controls)
            {
                hijo.MouseEnter += (s, e) => ActivarHoverPanel(panel);
                hijo.MouseLeave += (s, e) =>
                {
                    Point cursorEnPanel = panel.PointToClient(Cursor.Position);
                    if (!panel.ClientRectangle.Contains(cursorEnPanel))
                    {
                        if (panelConHover == panel)
                            DesactivarHoverPanel(panel);
                    }
                };
                hijo.Click += (s, e) => DispararSeleccionPorTarjeta(panel);
            }

            panel.Paint += (sender, e) =>
            {
                if (panelConHover == panel)
                {
                    using (Pen pen = new Pen(colorBordeIluminado, 2))
                    {
                        e.Graphics.DrawRectangle(pen, 1, 1, panel.Width - 2, panel.Height - 2);
                    }
                }
            };
        }

        private void AplicarEfectoZoom(SmoothPictureBox pbx, bool activar)
        {
            pbx.AnimarHacia(activar);
        }

        private void ActivarHoverPanel(Panel nuevoPanel)
        {
            if (panelConHover != null && panelConHover != nuevoPanel)
            {
                panelConHover.BackColor = colorTarjetaBase;
                foreach (Control c in panelConHover.Controls)
                    if (c is SmoothPictureBox pbx)
                    {
                        pbx.BackColor = colorTarjetaBase;
                        AplicarEfectoZoom(pbx, false); // ← usar el método
                    }
                panelConHover.Invalidate();
            }

            panelConHover = nuevoPanel;
            nuevoPanel.BackColor = colorTarjetaHover;
            foreach (Control c in nuevoPanel.Controls)
                if (c is SmoothPictureBox pbx)
                {
                    pbx.BackColor = colorTarjetaHover;
                    AplicarEfectoZoom(pbx, true);
                }
            nuevoPanel.Invalidate();
        }

        private void DesactivarHoverPanel(Panel panel)
        {
            if (panelConHover == panel)
            {
                panel.BackColor = colorTarjetaBase;
                foreach (Control c in panel.Controls)
                    if (c is SmoothPictureBox pbx)
                    {
                        pbx.BackColor = colorTarjetaBase;
                        AplicarEfectoZoom(pbx, false);
                    }
                panelConHover = null;
                panel.Invalidate();
            }
        }

        private void DispararSeleccionPorTarjeta(Panel panel)
        {
            // Identificamos inteligentemente a qué rival corresponde la tarjeta mediante los botones internos
            foreach (Control c in panel.Controls)
            {
                if (c == BtnAndrea) RivalSeleccionado?.Invoke(TipoRival.Andrea);
                if (c == BtnNatasha) RivalSeleccionado?.Invoke(TipoRival.Natasha);
                if (c == BtnAlejandra) RivalSeleccionado?.Invoke(TipoRival.Alejandra);
            }
        }

        public void CerrarMenu()
        {
            PbxAndrea.Image = null;
            PbxNatasha.Image = null;
            PbxAlejandra.Image = null;

            this.Hide();
        }

        private void BtnAndrea_Click(object sender, EventArgs e)
        {
            RivalSeleccionado?.Invoke(TipoRival.Andrea);
        }

        private void BtnNatasha_Click(object sender, EventArgs e)
        {
            RivalSeleccionado?.Invoke(TipoRival.Natasha);
        }

        private void BtnAlejandra_Click(object sender, EventArgs e)
        {
            RivalSeleccionado?.Invoke(TipoRival.Alejandra);
        }

        private void FormMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
