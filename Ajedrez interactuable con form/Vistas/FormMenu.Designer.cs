namespace Ajedrez_interactuable_con_form
{
    partial class FormMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            rival = new Label();
            BtnAndrea = new Button();
            BtnNatasha = new Button();
            BtnAlejandra = new Button();
            PbxAndrea = new SmoothPictureBox();
            PbxNatasha = new SmoothPictureBox();
            PbxAlejandra = new SmoothPictureBox();
            ((System.ComponentModel.ISupportInitialize)PbxAndrea).BeginInit();
            ((System.ComponentModel.ISupportInitialize)PbxNatasha).BeginInit();
            ((System.ComponentModel.ISupportInitialize)PbxAlejandra).BeginInit();
            SuspendLayout();
            // 
            // rival
            // 
            rival.AutoSize = true;
            rival.Font = new Font("Rockwell", 20F, FontStyle.Italic, GraphicsUnit.Point, 0);
            rival.Location = new Point(533, 26);
            rival.Name = "rival";
            rival.Size = new Size(345, 46);
            rival.TabIndex = 0;
            rival.Text = "Seleccione el rival";
            // 
            // BtnAndrea
            // 
            BtnAndrea.Location = new Point(170, 514);
            BtnAndrea.Name = "BtnAndrea";
            BtnAndrea.Size = new Size(112, 34);
            BtnAndrea.TabIndex = 1;
            BtnAndrea.Text = "Andrea";
            BtnAndrea.UseVisualStyleBackColor = true;
            BtnAndrea.Click += BtnAndrea_Click;
            // 
            // BtnNatasha
            // 
            BtnNatasha.Location = new Point(646, 514);
            BtnNatasha.Name = "BtnNatasha";
            BtnNatasha.Size = new Size(112, 34);
            BtnNatasha.TabIndex = 2;
            BtnNatasha.Text = "Natasha";
            BtnNatasha.UseVisualStyleBackColor = true;
            BtnNatasha.Click += BtnNatasha_Click;
            // 
            // BtnAlejandra
            // 
            BtnAlejandra.Location = new Point(1110, 514);
            BtnAlejandra.Name = "BtnAlejandra";
            BtnAlejandra.Size = new Size(112, 34);
            BtnAlejandra.TabIndex = 3;
            BtnAlejandra.Text = "Alejandra";
            BtnAlejandra.UseVisualStyleBackColor = true;
            BtnAlejandra.Click += BtnAlejandra_Click;
            // 
            // PbxAndrea
            // 
            PbxAndrea.Location = new Point(33, 108);
            PbxAndrea.Name = "PbxAndrea";
            PbxAndrea.Size = new Size(400, 400);
            PbxAndrea.SizeMode = PictureBoxSizeMode.StretchImage;
            PbxAndrea.TabIndex = 5;
            PbxAndrea.TabStop = false;
            // 
            // PbxNatasha
            // 
            PbxNatasha.Location = new Point(505, 108);
            PbxNatasha.Name = "PbxNatasha";
            PbxNatasha.Size = new Size(400, 400);
            PbxNatasha.SizeMode = PictureBoxSizeMode.StretchImage;
            PbxNatasha.TabIndex = 6;
            PbxNatasha.TabStop = false;
            // 
            // PbxAlejandra
            // 
            PbxAlejandra.Location = new Point(969, 108);
            PbxAlejandra.Name = "PbxAlejandra";
            PbxAlejandra.Size = new Size(400, 400);
            PbxAlejandra.SizeMode = PictureBoxSizeMode.StretchImage;
            PbxAlejandra.TabIndex = 7;
            PbxAlejandra.TabStop = false;
            // 
            // FormMenu
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlDark;
            ClientSize = new Size(1406, 743);
            Controls.Add(PbxAlejandra);
            Controls.Add(PbxNatasha);
            Controls.Add(PbxAndrea);
            Controls.Add(BtnAlejandra);
            Controls.Add(BtnNatasha);
            Controls.Add(BtnAndrea);
            Controls.Add(rival);
            Name = "FormMenu";
            Text = "FormMenu";
            FormClosing += FormMenu_FormClosing;
            ((System.ComponentModel.ISupportInitialize)PbxAndrea).EndInit();
            ((System.ComponentModel.ISupportInitialize)PbxNatasha).EndInit();
            ((System.ComponentModel.ISupportInitialize)PbxAlejandra).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label rival;
        private Button BtnAndrea;
        private Button BtnNatasha;
        private Button BtnAlejandra;
        private SmoothPictureBox PbxAndrea;
        private SmoothPictureBox PbxNatasha;
        private SmoothPictureBox PbxAlejandra;
    }
}