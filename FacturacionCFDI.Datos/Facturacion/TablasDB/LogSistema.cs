using System;

namespace FacturacionCFDI.Datos.Facturacion.TablasDB
{
    public class LogSistema
    {
        public int id { get; set; }
        public DateTime fechaHora { get; set; }
        public string nivel { get; set; }
        public string clase { get; set; }
        public string metodo { get; set; }
        public string mensaje { get; set; }
        public int aplicacionId { get; set; }
    }
}
