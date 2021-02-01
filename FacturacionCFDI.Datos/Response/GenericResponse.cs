namespace FacturacionCFDI.Datos.Response
{
    public class GenericResponse
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; }
        public string Descripcion { get; set; }
        public object Data { get; set; }

        public GenericResponse()
        {
            Codigo = 1;
            Mensaje = "OK";
            Descripcion = "OK";
            Data = null;
        }

        public GenericResponse(int codigo, string mensaje, object data = null, string descripcion = null)
        {
            Codigo = codigo;
            Mensaje = mensaje;
            Descripcion = descripcion;
            Data = data;
        }
    }

    public class GenericResponse<T>
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; }
        public string Descripcion { get; set; }
        public T Data { get; set; }

        public GenericResponse()
        {
            Codigo = 1;
            Mensaje = "OK";
            Descripcion = "OK";
            Data = default;
        }

        public GenericResponse(int codigo, string mensaje, T data = default, string descripcion = null)
        {
            Codigo = codigo;
            Mensaje = mensaje;
            Descripcion = descripcion;
            Data = data;
        }
    }
}
