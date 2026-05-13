using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ajedrez_interactuable_con_form.Modelos;

namespace Ajedrez_interactuable_con_form.Vistas
{
    public interface IVistaMenu
    {
        event Action<TipoRival> RivalSeleccionado;
        void CerrarMenu();
    }
}
