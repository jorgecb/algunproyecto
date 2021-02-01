namespace WSFacturacion.Modelos
{
    public class CancelacionModelo
    {
        public string Uuid { get; set; }
        public string RfcEmisor { get; set; }
        public string RfcReceptor { get; set; }
        public decimal MontoTotal { get; set; }
    }
}