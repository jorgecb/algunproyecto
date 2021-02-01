
namespace FacturacionCFDI.Datos.Facturacion.TablasDB
{
    public class Concepto
    {
        public int id { get; set; }
        public int claveProdServId { get; set; }
        public string noIdentificacion { get; set; }
        public decimal cantidad { get; set; }
        public int claveUnidadId { get; set; }
        public string descripcion { get; set; }
        public decimal valorUnitario { get; set; }
        public decimal importe { get; set; }
        public decimal descuento { get; set; }
        public string aduanaNumeroPedimento { get; set; }
        public string cuentaPredialNumero { get; set; }
        public int comprobanteId { get; set; }
    }
}
