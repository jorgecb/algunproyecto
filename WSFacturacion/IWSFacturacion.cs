using System.Collections.Generic;
using System.ServiceModel;
using WSFacturacion.Modelos;

namespace WSFacturacion
{
    [ServiceContract]
    public interface IWSFacturacion
    {
        [OperationContract]
        GenericResponse<CFDICancelacion> CancelacionCFDI(CancelacionModelo Cfdi);

        [OperationContract]
        GenericResponse<string> EstatusCFDI(CancelacionModelo Cfdi);

        [OperationContract]
        GenericResponse<List<CFDIRelacionados>> RelacionadosCFDI(string uuid);

        [OperationContract]
        GenericResponse<string> TimbradoCFDI(string estructuraXml);
    }
}
