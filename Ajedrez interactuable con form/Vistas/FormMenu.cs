using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ajedrez_interactuable_con_form.Modelos;
using Ajedrez_interactuable_con_form.Vistas;

namespace Ajedrez_interactuable_con_form
{
    public partial class FormMenu : Form, IVistaMenu
    {
        public event Action<TipoRival>? RivalSeleccionado;

        public FormMenu()
        {
            InitializeComponent();
            PbxAndrea.Image = Properties.Resources.Andrea;
            PbxNatasha.Image = Properties.Resources.Natasha;
            PbxAlejandra.Image = Properties.Resources.Alejandra;
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
