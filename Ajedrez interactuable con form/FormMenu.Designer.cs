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
            label1 = new Label();
            BtnAndrea = new Button();
            BtnNatasha = new Button();
            BtnAlejandra = new Button();
            PbxAndrea = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)PbxAndrea).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(133, 80);
            label1.Name = "label1";
            label1.Size = new Size(149, 25);
            label1.TabIndex = 0;
            label1.Text = "Seleccione el rival";
            // 
            // BtnAndrea
            // 
            BtnAndrea.Location = new Point(224, 428);
            BtnAndrea.Name = "BtnAndrea";
            BtnAndrea.Size = new Size(112, 34);
            BtnAndrea.TabIndex = 1;
            BtnAndrea.Text = "Andrea";
            BtnAndrea.UseVisualStyleBackColor = true;
            BtnAndrea.Click += BtnAndrea_Click;
            // 
            // BtnNatasha
            // 
            BtnNatasha.Location = new Point(748, 468);
            BtnNatasha.Name = "BtnNatasha";
            BtnNatasha.Size = new Size(112, 34);
            BtnNatasha.TabIndex = 2;
            BtnNatasha.Text = "Natasha";
            BtnNatasha.UseVisualStyleBackColor = true;
            // 
            // BtnAlejandra
            // 
            BtnAlejandra.Location = new Point(992, 468);
            BtnAlejandra.Name = "BtnAlejandra";
            BtnAlejandra.Size = new Size(112, 34);
            BtnAlejandra.TabIndex = 3;
            BtnAlejandra.Text = "Alejandra";
            BtnAlejandra.UseVisualStyleBackColor = true;
            // 
            // PbxAndrea
            // 
            PbxAndrea.Location = new Point(84, 128);
            PbxAndrea.Name = "PbxAndrea";
            PbxAndrea.Size = new Size(411, 294);
            PbxAndrea.SizeMode = PictureBoxSizeMode.StretchImage;
            PbxAndrea.TabIndex = 5;
            PbxAndrea.TabStop = false;
            // 
            // FormMenu
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1624, 648);
            Controls.Add(PbxAndrea);
            Controls.Add(BtnAlejandra);
            Controls.Add(BtnNatasha);
            Controls.Add(BtnAndrea);
            Controls.Add(label1);
            Name = "FormMenu";
            Text = "FormMenu";
            ((System.ComponentModel.ISupportInitialize)PbxAndrea).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button BtnAndrea;
        private Button BtnNatasha;
        private Button BtnAlejandra;
        private PictureBox PbxAndrea;
    }
}