using System.Runtime.Serialization;

namespace WSFacturacion.Modelos
{
    public enum Codigo
    {
        Error = 0,
        Exito = 1,
        Logico = 2
    }

    [DataContract]
    public class GenericResponse<T>
    {
        [DataMember]
        public int Codigo { get; set; }
        [DataMember]
        public string Mensaje { get; set; }
        [DataMember]
        public T Resultado { get; set; }

        public GenericResponse()
        {
            Codigo = 1;
            Mensaje = "OK";
            Resultado = default;
        }
    }
}
