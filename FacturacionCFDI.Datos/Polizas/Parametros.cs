using System;

namespace FacturacionCFDI.Datos.Polizas
{
    public class Parametros
    {
        public int Id { get; set; }
        public string Parametro { get; set; }
        public string Valor { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Updated_At { get; set; }
    }
}
