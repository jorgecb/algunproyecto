
namespace FacturacionCFDI.Datos.Facturacion.TablasDB
{
    public class PagosDoctoRelacionado
    {
        public int id { get; set; }
        public string idDocumento { get; set; }
        public string serie { get; set; }
        public string folio { get; set; }
        public string monedaDr { get; set; }
        public decimal tipoCambioDr { get; set; }
        public string metodoDePagoDr { get; set; }
        public int numParcialidad { get; set; }
        public decimal impSaldoAnt { get; set; }
        public decimal impPagado { get; set; }
        public decimal impSaldoInsoluto { get; set; }
        public int pagosId { get; set; }
    }
}
