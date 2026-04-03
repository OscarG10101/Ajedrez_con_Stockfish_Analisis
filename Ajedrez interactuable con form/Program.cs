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

            // Crear y mostrar el formulario de menú
            using (FormMenu menu = new FormMenu())
            {
                if (menu.ShowDialog() == DialogResult.OK)
                {
                    // Abrir el formulario principal con la opción seleccionada
                    Application.Run(new Form1(menu.RivalSeleccionado));
                }
            }
        }
    }
}