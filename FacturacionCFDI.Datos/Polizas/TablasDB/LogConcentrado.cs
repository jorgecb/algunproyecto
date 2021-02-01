using System;

namespace FacturacionCFDI.Datos.Polizas.TablasDB
{
    public class LogConcentrado
    {
        public int Id { get; set; }
        public int PolizasId { get; set; }
        public DateTime Fecha { get; set; }
        public string Mensaje { get; set; }
        public string Solicitante { get; set; }
    }
}
