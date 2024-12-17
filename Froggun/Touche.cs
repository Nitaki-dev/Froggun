using System;
using System.Windows.Input;

namespace Froggun
{
    internal static class Touche // Ajouter le mot-clé 'static'
    {
        public static Key ToucheHaut { get; set; }
        public static Key ToucheBas { get; set; }
        public static Key ToucheGauche { get; set; }
        public static Key ToucheDroite { get; set; }
        public static Key ToucheRoulade { get; set; }
        public static Key TouchePause { get; set; }

        static Touche()
        {
            // Initialisation des touches par défaut
            ToucheHaut = Key.Z;
            ToucheBas = Key.S;
            ToucheDroite = Key.Q;
            ToucheGauche = Key.D;
            ToucheRoulade = Key.LeftShift;
            TouchePause = Key.Space;
        }
    }
}
