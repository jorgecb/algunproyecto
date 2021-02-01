using System;

namespace FacturacionCFDI.Datos.Facturacion.TablasDB
{
    public class LogFactura
    {
        public int id { get; set; }
        public DateTime fechaHora { get; set; }
        public string mensaje { get; set; }
        public int comprobanteId { get; set; }
    }
}
