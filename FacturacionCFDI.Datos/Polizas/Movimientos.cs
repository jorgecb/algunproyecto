using System;

namespace FacturacionCFDI.Datos.Polizas
{
    public class Movimientos
    {
        public int Id { get; set; }
        public string DatoOrigen { get; set; }
        public string TipoSolicitud { get; set; }
        public string TipoMovimiento { get; set; }
        public string Sistema { get; set; }
        public string Poliza { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }
        public string CausaEndoso { get; set; }
        public string EstatusPoliza { get; set; }
        public string TipoDePoliza { get; set; }
        public string RfcContratante { get; set; }
        public string NombreContratante { get; set; }
        public string TipoPersonaContratante { get; set; }
        public string RfcAsegurado { get; set; }
        public string NombreAsegurado { get; set; }
        public string TipoPersonaAsegurado { get; set; }
        public string PagoRfc { get; set; }
        public string PagoNombre { get; set; }
        public string PagoTipoPersona { get; set; }
        public DateTime PagoFechaPago { get; set; }
        public string PagoFormaPago { get; set; }
        public string PagoMetodoPago { get; set; }
        public int? PagoParcialidad { get; set; }
        public string PagoOperacion { get; set; }
        public string UuidFactura { get; set; }
        public string CodigoProducto { get; set; }
        public string FormaPago { get; set; }
        public string LugarExpedicion { get; set; }
        public string CodigoConcepto { get; set; }
        public decimal PrimaNeta { get; set; }
        public decimal Financiamiento { get; set; }
        public decimal Gasto { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int EstatusMovimientoId { get; set; }
        public string Solicitante { get; set; }
        public string LlaveSincronizacion { get; set; }
        public string IdSolicitudExterna { get; set; }
    }
}
