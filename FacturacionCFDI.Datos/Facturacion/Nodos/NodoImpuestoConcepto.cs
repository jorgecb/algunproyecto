
namespace FacturacionCFDI.Datos.Facturacion.Nodos
{
    /// <summary>
    /// Clase NodoImpuestoConcepto
    /// </summary>
    public class NodoImpuestoConcepto
    {
        public decimal ibase { get; set; }
        public string impuesto { get; set; }
        public string tipoFactor { get; set; }
        public decimal tasaOCuota { get; set; }
        public decimal importe { get; set; }
        public int decimales { get; set; }
    }
}
