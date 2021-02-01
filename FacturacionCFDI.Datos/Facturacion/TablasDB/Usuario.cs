
using System;

namespace FacturacionCFDI.Datos.Facturacion.TablasDB
{
    public class Usuario
    {
        public int id { get; set; }
        public string usuario { get; set; }
        public string contraseña { get; set; }
        public string correo { get; set; }
        public string nombre { get; set; }
        public DateTime fechaCreacion { get; set; }
        public DateTime? fechaModificacion { get; set; }
        public DateTime? ultimoInicioSesion { get; set; }
        public int intentos { get; set; }
        public int estatusUsuarioId { get; set; }
        public int rolUsuarioId { get; set; }
    }
}
