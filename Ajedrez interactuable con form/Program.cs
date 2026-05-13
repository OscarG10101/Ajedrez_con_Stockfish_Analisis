using Ajedrez_interactuable_con_form.Servicios;

namespace Ajedrez_interactuable_con_form
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var formMenu = new FormMenu();
            var presentadorMenu = new PresentadorMenu(formMenu); // Conecta eventos

            Application.Run(formMenu);
        }
    }
}