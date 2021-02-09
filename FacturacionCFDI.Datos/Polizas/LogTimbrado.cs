using System;

namespace FacturacionCFDI.Datos.Polizas
{
    public class LogTimbrado
    {
        public int Id { get; set; }
        public int ComprobanteId  { get; set; }
        public DateTime FechaHoraError  { get; set; }
        public string Error { get; set; }
        public DateTime? FechaHoraSolucionado { get; set; }
        public int Estatus { get; set; }
    }
}
