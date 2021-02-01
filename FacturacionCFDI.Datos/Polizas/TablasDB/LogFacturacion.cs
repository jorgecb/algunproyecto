using System;

namespace FacturacionCFDI.Datos.Polizas.TablasDB
{
    public class LogFacturacion
    {
        public int Id { get; set; }
        public int PolizasFacturacionId { get; set; }
        public DateTime Fecha { get; set; }
        public string Mensaje { get; set; }
        public string Solicitante { get; set; }
    }
}
