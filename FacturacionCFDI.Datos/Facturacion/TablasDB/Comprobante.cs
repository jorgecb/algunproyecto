using System;

namespace FacturacionCFDI.Datos.Facturacion.TablasDB
{
    public class Comprobante
    {
        public int id { get; set; }
        public string version { get; set; }
        public string serie { get; set; }
        public string folio { get; set; }
        public DateTime fecha { get; set; }
        public int formaPagoId { get; set; }
        public decimal subtotal { get; set; }
        public decimal descuento { get; set; }
        public int monedaId { get; set; }
        public decimal tipoCambio { get; set; }
        public decimal total { get; set; }
        public int tipoDeComprobanteId { get; set; }
        public int metodoPagoId { get; set; }
        public string lugarExpedicion { get; set; }
        public string confirmacion { get; set; }
        public int emisorId { get; set; }
        public int receptorId { get; set; }
        public int usoCfdiId { get; set; }
        public int estatusFacturaId { get; set; }
        public DateTime fechaCreacion { get; set; }
        public DateTime? fechaModificacion { get; set; }
        public string facturaEstructura { get; set; }
        public string facturaXml { get; set; }
        public string facturaPdf { get; set; }
        public string facturaUuid { get; set; }
        public DateTime? facturaFecha { get; set; }
    }
}
