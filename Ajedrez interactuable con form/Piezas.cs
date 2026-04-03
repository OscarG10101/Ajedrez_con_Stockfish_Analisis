using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ajedrez_interactuable_con_form
{
    public enum TipoPieza
    {
        Peon,
        Alfil,
        Torre,
        Caballo,
        Dama,
        Rey
    }
    internal class Piezas
    {
        // Diccionario para mapear tipos de piezas y colores a imágenes
        public static Dictionary<(TipoPieza, bool), Image> ImagenesPiezas;

        // Propiedades: fila, columna y color
        public int Fila { get; set; }
        public int Columna { get; set; }

        [JsonIgnore]
        public float PosX { get; set; }
        [JsonIgnore]
        public float PosY { get; set; }

        public bool EsBlanca { get; set; }
        public TipoPieza Tipo { get; set; } // "peon", "alfil", etc.

        // Constructor: se llama cuando creamos la instancia
        public Piezas(int fila, int columna, bool esBlanca, TipoPieza tipo)
        {
            Fila = fila;
            Columna = columna;
            PosX = -1;
            PosY = -1;
            EsBlanca = esBlanca;
            Tipo = tipo;
        }

        // Método para dibujar la pieza
        public void Dibujar(Graphics g, int tamaño)
        {
            float x = PosX >= 0 ? PosX : Columna * tamaño;
            float y = PosY >= 0 ? PosY : Fila * tamaño;

            Brush color = EsBlanca ? Brushes.White : Brushes.Black;
            RectangleF rect = new RectangleF(x, y, tamaño, tamaño);

            // Buscar la imagen correcta del diccionario estático
            Image img = Form1.imagenesPiezas[(Tipo, EsBlanca)];

            // Dibujar la imagen redimensionada
            g.DrawImage(img, rect);

            if (PosX < 0 && PosY < 0)
                g.DrawRectangle(Pens.Gray, Columna * tamaño, Fila * tamaño, tamaño, tamaño);
        }
    }
}
