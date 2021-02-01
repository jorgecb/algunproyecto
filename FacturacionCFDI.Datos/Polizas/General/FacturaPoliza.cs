using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionCFDI.Datos.Polizas.General
{
    public class FacturaPoliza
    {
        public int PolizaFacturacionId { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public DateTime Fecha { get; set; }
        public int FormaPagoId { get; set; }
        public decimal SubTotal { get; set; }
        public int TipoCambio { get; set; }
        public decimal Total { get; set; }
        public int TipoComprobanteId { get; set; }
        public string LugarExpedicion { get; set; }
        public int RfcReceptorId { get; set; }
        public string NombreReceptor { get; set; }
        public string CodigoConcepto { get; set; }
        public string ConceptoNoIdentificacion { get; set; }
        public int ConceptoCantidad { get; set; }
        public decimal ConceptoPrimaMonto { get; set; }
        public decimal ConceptoPrimaIva { get; set; }
        public decimal ConceptoFinanciamientoMonto { get; set; }
        public decimal ConceptoFinanciamientoIva { get; set; }
        public decimal ConceptoGastoMonto { get; set; }
        public decimal ConceptoGastoIva { get; set; }
        public string CfdiRelacionado { get; set; }
        public DateTime? PagoFechaPago { get; set; }
        public decimal? PagoMonto { get; set; }
        public string PagoNumOperacion { get; set; }
        public string PagoIdDocumento { get; set; }
        public int? PagoNumParcialidad { get; set; }
        public decimal? PagoSaldoAnterior { get; set; }
        public decimal? PagoSaldoInsoluto { get; set; }
    }
}
