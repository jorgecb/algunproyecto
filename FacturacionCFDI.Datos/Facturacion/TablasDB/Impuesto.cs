
namespace FacturacionCFDI.Datos.Facturacion.TablasDB
{
    public class Impuesto
    {
        public int id { get; set; }
        //public decimal base { get; set; }
        public int impuestoId { get; set; }
        public int tipoFactorId { get; set; }
        public decimal tasaOCuota { get; set; }
        public decimal importe { get; set; }
        public int impuestoTipoId { get; set; }
        public int conceptoId { get; set; }
    }
}
