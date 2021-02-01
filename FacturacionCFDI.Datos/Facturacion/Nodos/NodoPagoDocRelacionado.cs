
namespace FacturacionCFDI.Datos.Facturacion.Nodos
{
    /// <summary>
    /// Clase NodoPagoDocRelacionado
    /// </summary>
    public class NodoPagoDocRelacionado
    {
        public string idDocumento { get; set; }
        public string serie { get; set; }
        public string folio { get; set; }
        public string monedaDr { get; set; }
        public decimal tipoCambioDr { get; set; }
        public string metodoDePagoDr { get; set; }
        public string numParcialidad { get; set; }
        public decimal impSaldoAnt { get; set; }
        public decimal impPagado { get; set; }
        public decimal impSaldoInsoluto { get; set; }
        public int decimales { get; set; }
    }
}
