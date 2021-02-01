using System;
using System.Text;

namespace Utilerias
{
    public class Seguridad
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Seguridad() { }

        /// <summary>
        /// Método para codificar texto en Base 64
        /// </summary>
        /// <param name="texto">Cadena de texto para codificar</param>
        /// <returns>Cadena de texto en Base 64</returns>
        public string Base64Codificar(string texto)
        {
            string respuesta = "";
            try
            {
                byte[] encbuff = Encoding.UTF8.GetBytes(texto);
                respuesta = Convert.ToBase64String(encbuff);
            }
            catch
            {
                respuesta = "";
            }

            return respuesta;
        }

        /// <summary>
        /// Método para decodificar texto en Base 64
        /// </summary>
        /// <param name="texto">Cadeba de texto para decodificar</param>
        /// <returns>Cadena de texto decodificado</returns>
        public string Base64Decodificar(string texto)
        {
            string respuesta = "";

            try
            {
                byte[] decbuff = Convert.FromBase64String(texto);
                respuesta = Encoding.UTF8.GetString(decbuff);
            }
            catch
            {
                respuesta = "";
            }

            return respuesta;
        }
    }
}
