using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionCFDI.Datos.Polizas.General
{
    public class ConcetradoFactura
    {
        public string Sistema { get; set; }
        public string Poliza { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }
        public string EstatusPoliza { get; set; }
        public string TipoDePoliza { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string RfcReceptor { get; set; }
        public string NombreReceptor { get; set; }
        public string TipoComprobante { get; set; }
        public string CodigoConcepto { get; set; }
        public string CodigoProducto { get; set; }
        public string FormaPago { get; set; }
        public string LugarExpedicion { get; set; }
        public decimal PrimaNeta { get; set; }
        public decimal Financiamiento { get; set; }
        public decimal Gasto { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
    }
}
