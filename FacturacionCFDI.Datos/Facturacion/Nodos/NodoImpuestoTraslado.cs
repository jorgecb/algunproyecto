
namespace FacturacionCFDI.Datos.Facturacion.Nodos
{
    /// <summary>
    /// Clase NodoImpuestoTraslado
    /// </summary>
    public class NodoImpuestoTraslado
    {
        public decimal importe { get; set; }
        public string impuesto { get; set; }
        public decimal tasaOCuota { get; set; }
        public string tipofactor { get; set; }
        public int decimales { get; set; }
    }
}
