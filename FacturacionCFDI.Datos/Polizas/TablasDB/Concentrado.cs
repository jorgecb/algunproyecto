using System;

namespace FacturacionCFDI.Datos.Polizas.TablasDB
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
        public int PrimaNetaM { get; set; }
        public int FinanciamientoM { get; set; }
        public int GastoM { get; set; }
        public int IvaM { get; set; }
        public int TotalM { get; set; }
        public int PrimaNetaV { get; set; }
        public int FinanciamientoV { get; set; }
        public int GastoV { get; set; }
        public int IvaV { get; set; }
        public int TotalV { get; set; }
        public int TotalFacturado { get; set; }
        public int TotalPagado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
