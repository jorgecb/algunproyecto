using System;

namespace FacturacionCFDI.Datos.Facturacion.Nodos
{
    /// <summary>
    /// Clase NodoComprobante
    /// </summary>
    public class NodoComprobante
    {
        public int id { get; set; }
        public string version { get; set; }
        public string serie { get; set; }
        public string folio { get; set; }
        public DateTime fecha { get; set; }
        public string formaPago { get; set; }
        public decimal subtotal { get; set; }
        public decimal descuento { get; set; }
        public string moneda { get; set; }
        public decimal tipoCambio { get; set; }
        public decimal total { get; set; }
        public string tipoDeComprobante { get; set; }
        public string metodoPago { get; set; }
        public string lugarExpedicion { get; set; }
        public int decimales { get; set; }
    }
}
