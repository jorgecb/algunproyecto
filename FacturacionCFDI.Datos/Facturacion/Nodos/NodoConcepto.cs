
namespace FacturacionCFDI.Datos.Facturacion.Nodos
{
    /// <summary>
    /// Clase NodoConcepto
    /// </summary>
    public class NodoConcepto
    {
        public int id { get; set; }
        public string claveProdServ { get; set; }
        public string noIdentificacion { get; set; }
        public decimal cantidad { get; set; }
        public string claveUnidad { get; set; }
        public string descripcion { get; set; }
        public decimal valorUnitario { get; set; }
        public decimal importe { get; set; }
        public decimal? descuento { get; set; }
        public string aduanaNumeroPedimento { get; set; }
        public string cuentaPredialNumero { get; set; }
        public int decimales { get; set; }
    }
}
