using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ajedrez_interactuable_con_form.Modelos;
using Ajedrez_interactuable_con_form.Vistas;

namespace Ajedrez_interactuable_con_form.Servicios
{
    public class PresentadorMenu
    {
        private readonly IVistaMenu _vista;
        
        public PresentadorMenu(IVistaMenu vista)
        {
            _vista = vista;
            _vista.RivalSeleccionado += OnRivalSeleccionado;
        }

        private void OnRivalSeleccionado(TipoRival rival)
        {
            var juego = new TableroAjedrez();
            var motor = new StockfishMotor();
            var analista = new StockfishAnalista();
            var formPartida = new FormPartida();
            var presentador = new PresentadorPartida(formPartida, juego, motor, analista, rival);

            _vista.CerrarMenu();
            formPartida.Show();
        }
    }
}
