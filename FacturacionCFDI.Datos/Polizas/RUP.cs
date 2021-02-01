using System;

namespace FacturacionCFDI.Datos.Polizas
{
    public class RUP
    {
        public string Sistema { get; set; }
        public string Poliza { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }
        public string Certificado { get; set; }
        public string Cod_Ramo { get; set; }
        public string Des_Ramo { get; set; }
        public string SubRamo { get; set; }
        public string Cod_Producto { get; set; }
        public string Producto { get; set; }
        public string AutoAdministrada { get; set; }
        public string Clv_Supervisor { get; set; }
        public string Supervisor { get; set; }
        public string Clv_Agente { get; set; }
        public string Agente { get; set; }
        public string Cod_Contratante { get; set; }
        public string Contratante { get; set; }
        public string Tipo_Persona { get; set; }
        public string Rfc { get; set; }
        public decimal PrimaNeta { get; set; }
        public decimal Financiamiento { get; set; }
        public decimal Gasto { get; set; }
        public decimal Iva { get; set; }
        public decimal Monto { get; set; }
        public int Cod_FormaPago { get; set; }
        public string FormaPago { get; set; }
        public string Id_Pago { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime FechaVenPago { get; set; }
        public string Cuota { get; set; }
        public string CuentaClabe { get; set; }
        public decimal MontoAplicado { get; set; }
        public string Cod_MetodoPago { get; set; }
        public string MetodoPago { get; set; }
        public DateTime FechaAplicacion { get; set; }
        public string MedioAplicacion { get; set; }
        public string Cod_Estado { get; set; }
        public string Estado { get; set; }
        public string Observaciones_Pago { get; set; }
        public string Cajero { get; set; }
        public DateTime FechaRecepcion { get; set; }
    }
}
