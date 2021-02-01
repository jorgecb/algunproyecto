using System;

namespace FacturacionCFDI.Datos.Polizas.TablasDB
{
    public class PolizaFacturacion
    {
        public int Id { get; set; }
        public int PolizasId { get; set; }
        public int PolizasRfcId { get; set; }
        public string Endoso { get; set; }
        public string TipoComprobante { get; set; }
        public int CodigoConcepto { get; set; }
        public decimal PrimaNeta { get; set; }
        public decimal Financiamiento { get; set; }
        public decimal Gasto { get; set; }
        public decimal Iva { get; set; }
        public int Total { get; set; }
        public int TotalPagado { get; set; }
        public int Parcialidad { get; set; }
        public int ComprobanteId { get; set; }
        public string Uuid { get; set; }        
        public DateTime FechaTimbrado { get; set; }
        public int PolizaMadre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int EstatusFacturacionId { get; set; }
    }
}
