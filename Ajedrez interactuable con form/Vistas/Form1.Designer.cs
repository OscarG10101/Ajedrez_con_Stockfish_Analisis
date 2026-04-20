namespace Ajedrez_interactuable_con_form
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            LbxHistorial = new ListBox();
            LblRespuesta = new Label();
            PanelTablero = new Panel();
            PbxRival = new PictureBox();
            PanelEvaluacion = new Panel();
            LblEvaluacionNumero = new Label();
            LblUndo = new Label();
            ((System.ComponentModel.ISupportInitialize)PbxRival).BeginInit();
            SuspendLayout();
            // 
            // LbxHistorial
            // 
            LbxHistorial.FormattingEnabled = true;
            LbxHistorial.ItemHeight = 25;
            LbxHistorial.Location = new Point(136, 173);
            LbxHistorial.Name = "LbxHistorial";
            LbxHistorial.Size = new Size(180, 129);
            LbxHistorial.TabIndex = 3;
            // 
            // LblRespuesta
            // 
            LblRespuesta.AutoSize = true;
            LblRespuesta.Location = new Point(333, 173);
            LblRespuesta.Name = "LblRespuesta";
            LblRespuesta.Size = new Size(97, 25);
            LblRespuesta.TabIndex = 4;
            LblRespuesta.Text = "Respuesta ";
            // 
            // PanelTablero
            // 
            PanelTablero.Location = new Point(587, 120);
            PanelTablero.Name = "PanelTablero";
            PanelTablero.Size = new Size(400, 400);
            PanelTablero.TabIndex = 5;
            PanelTablero.Paint += PanelTablero_Paint;
            PanelTablero.MouseClick += PanelTablero_MouseClick;
            // 
            // PbxRival
            // 
            PbxRival.Location = new Point(1016, 120);
            PbxRival.Name = "PbxRival";
            PbxRival.Size = new Size(522, 400);
            PbxRival.SizeMode = PictureBoxSizeMode.StretchImage;
            PbxRival.TabIndex = 6;
            PbxRival.TabStop = false;
            // 
            // PanelEvaluacion
            // 
            PanelEvaluacion.Location = new Point(561, 120);
            PanelEvaluacion.Name = "PanelEvaluacion";
            PanelEvaluacion.Size = new Size(20, 400);
            PanelEvaluacion.TabIndex = 7;
            // 
            // LblEvaluacionNumero
            // 
            LblEvaluacionNumero.AutoSize = true;
            LblEvaluacionNumero.Location = new Point(497, 300);
            LblEvaluacionNumero.Name = "LblEvaluacionNumero";
            LblEvaluacionNumero.Size = new Size(36, 25);
            LblEvaluacionNumero.TabIndex = 8;
            LblEvaluacionNumero.Text = "0.0";
            // 
            // LblUndo
            // 
            LblUndo.AutoSize = true;
            LblUndo.BorderStyle = BorderStyle.FixedSingle;
            LblUndo.Location = new Point(333, 213);
            LblUndo.Name = "LblUndo";
            LblUndo.Size = new Size(147, 27);
            LblUndo.TabIndex = 9;
            LblUndo.Text = "Deshacer Jugada";
            LblUndo.Click += LblUndo_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1627, 564);
            Controls.Add(LblUndo);
            Controls.Add(LblEvaluacionNumero);
            Controls.Add(PanelEvaluacion);
            Controls.Add(PbxRival);
            Controls.Add(PanelTablero);
            Controls.Add(LblRespuesta);
            Controls.Add(LbxHistorial);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)PbxRival).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ListBox LbxHistorial;
        private Label LblRespuesta;
        private Panel PanelTablero;
        private PictureBox PbxRival;
        private Panel PanelEvaluacion;
        private Label LblEvaluacionNumero;
        private Label LblUndo;
    }
}
