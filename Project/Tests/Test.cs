﻿namespace Lerp2API.Tests
{
    public class Test
    {
        private static string _hola = "";
        public static string Hola
        {
            get
            {
                return _hola;
            }
            set
            {
                _hola = value + "bbbb";
            }
        }

        //public static string Hola = "asdj";

        public int Suma(int a, int b)
        {
            return a + b;
        }

        public static int Resta(int a, int b)
        {
            return a - b;
        }
    }
}
