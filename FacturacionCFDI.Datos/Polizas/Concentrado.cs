using System;

namespace FacturacionCFDI.Datos.Polizas
{
    public class Concentrado
    {
        public int Id { get; set; }
        public string Sistema { get; set; }
        public string Poliza { get; set; }
        public int AnioInicio { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }
        public string EstatusPoliza { get; set; }
        public string TipoDePoliza { get; set; }
        public decimal PrimaNetaM { get; set; }
        public decimal FinanciamientoM { get; set; }
        public decimal GastoM { get; set; }
        public decimal IvaM { get; set; }
        public decimal TotalM { get; set; }
        public decimal PrimaNetaV { get; set; }
        public decimal FinanciamientoV { get; set; }
        public decimal GastoV { get; set; }
        public decimal IvaV { get; set; }
        public decimal TotalV { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal TotalPagado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public decimal TmpGlobal { get; set; }
        public decimal TmpPago { get; set; }
    }
}
