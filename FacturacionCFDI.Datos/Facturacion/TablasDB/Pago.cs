
using System;

namespace FacturacionCFDI.Datos.Facturacion.TablasDB
{
    public class Pago
    {
        public int id { get; set; }
        public string version { get; set; }
        public DateTime fechaPago { get; set; }
        public string formaDePagoP { get; set; }
        public string monedaP { get; set; }
        public decimal tipoCambioP { get; set; }
        public decimal monto { get; set; }
        public string numOperacion { get; set; }
        public string rfcEmisorCtaOrd { get; set; }
        public string nomBancoOrdExt { get; set; }
        public string ctaOrdenante { get; set; }
        public string rfcEmisorCtaBen { get; set; }
        public string ctaBeneficiario { get; set; }
        public string tipoCadPago { get; set; }
        public string certPago { get; set; }
        public string cadPago { get; set; }
        public string selloPago { get; set; }
        public int comprobanteId { get; set; }
    }
}
