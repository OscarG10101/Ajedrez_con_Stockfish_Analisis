using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ajedrez_interactuable_con_form
{
    public partial class FormMenu : Form
    {
        public TipoRival RivalSeleccionado { get; private set; }

        public FormMenu()
        {
            InitializeComponent();

            PbxAndrea.Image = Properties.Resources.AndreaMenu;
        }
        public enum TipoRival
        {
            Humano,
            Andrea,
            Natasha, 
            Alejandra
        }

        private void BtnAndrea_Click(object sender, EventArgs e)
        {
            RivalSeleccionado = TipoRival.Andrea;
            this.DialogResult = DialogResult.OK;
        }
    }
}
