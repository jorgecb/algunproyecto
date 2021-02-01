using System;

namespace WSFacturacion.Modelos
{
    public class CFDICancelacion
    {
        public string Uuid { get; set; }
        public string RfcEmisor { get; set; }
        public string RfcReceptor { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public bool? AutorizoCliente { get; set; }
        public DateTime? FechaEstatus { get; set; }
        public decimal MontoTotal { get; set; }
    }
}