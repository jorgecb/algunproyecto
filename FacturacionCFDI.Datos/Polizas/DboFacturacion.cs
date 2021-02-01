using System;

namespace FacturacionCFDI.Datos.Polizas
{
    public class DboFacturacion
    {
        public int Id { get; set; }
        public int PolizasId { get; set; }
        public string TipoComprobante { get; set; }
        public DateTime FechaComprobante { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string RfcReceptor { get; set; }
        public string NombreReceptor { get; set; }
        public string CodigoConcepto { get; set; }
        public string CodigoProducto { get; set; }
        public string FormaPago { get; set; }
        public string MetodoPago { get; set; }
        public string LugarExpedicion { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal PrimaNeta { get; set; }
        public decimal Financiamiento { get; set; }
        public decimal Gasto { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal Parcialidad { get; set; }
        public string NumOperacion { get; set; }
        public int ComprobanteId { get; set; }
        public string Uuid { get; set; }
        public DateTime FechaTimbrado { get; set; }
        public int PolizaMadre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int EstatusFacturacionId { get; set; }
    }
}
