using FacturacionCFDI.Datos.Cancelacion;
using FacturacionCFDI.Datos.Facturacion.TablasDB;
using FacturacionCFDI.Datos.Polizas;
using FacturacionCFDI.Datos.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilerias;
using WSFacturacion.Servicios;

namespace FacturacionCFDI.Negocio.Polizas
{
    public class Cancelacion
    {
        private readonly BaseDatos _baseDatos;
        WSCFDICancelacion webService = new WSCFDICancelacion();
        string alias = "97FF91F6-660B-4D5F-84B2-4F138285FE88";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseDatos">BaseDatos</param>
        public Cancelacion(BaseDatos baseDatos)
        {
            _baseDatos = baseDatos;
        }

        public async Task<GenericResponse> ProcesoCancelacion()
        {
            try
            {
                int count = 0;
                bool repeat = true;
                do
                {
                    DboFacturacion padre = _baseDatos.SelectFirst<DboFacturacion>("SELECT * FROM POLIZAS_FACTURACION WHERE POLIZAMADRE = 1 AND TIPOCOMPROBANTE = 'I' AND ESTATUSFACTURACIONID = 3");
                    if (padre != null){
                        string usuario = "SUCPSS1119";
                        string password = "SUCPSS1119*19";
                        string rfcEmisor = "PSS970203FI6";
                        Console.WriteLine("Obteniendo Token");
                        string tokenS = generartokenSuc(usuario, password, rfcEmisor, alias);
                        int contador = CancelacionHijos(padre.ComprobanteId, 0, tokenS);
                        count += contador;
                        Comprobante comprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE ID = {padre.ComprobanteId}");
                        Emisor emisor = _baseDatos.SelectFirst<Emisor>($"SELECT * FROM FACTURACION_EMISOR WHERE ID = {comprobante.emisorId}");
                        Receptor receptor = _baseDatos.SelectFirst<Receptor>($"SELECT * FROM FACTURACION_RECEPTOR WHERE ID = {comprobante.receptorId}");

                        RespuestaDTOOfListOfRespuestaDTOOfCancelacionDTO lisCancel = new RespuestaDTOOfListOfRespuestaDTOOfCancelacionDTO();
                        List<SolicitudCancelacionDTO> cancelacion = new List<SolicitudCancelacionDTO>();

                        cancelacion.Add(new SolicitudCancelacionDTO()
                        {
                            CFDI_UUID = comprobante.facturaUuid,
                            MontoTotal = Convert.ToDecimal(comprobante.total),
                            RFCEmisor = emisor.rfc,
                            RFCReceptor = receptor.rfc
                        });

                        lisCancel = webService.SolicitarCancelacionSUC(cancelacion.ToArray(), tokenS);
                        if (lisCancel.Datos != null)
                        {
                            foreach (var can in cancelacion)
                            {
                                Comprobante rowComprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID = '{can.CFDI_UUID}'");
                                _baseDatos.Update($"UPDATE POLIZAS_FACTURACION SET ESTATUSFACTURACIONID = 9 WHERE COMPROBANTEID = {rowComprobante.id}");
                                count++;
                            }
                        }

                    } else {
                        repeat = false;
                    }
                } while (repeat == true);
                
                return new GenericResponse()
                {
                    Codigo = 1,
                    Mensaje = $"Todo ha salido bien, se han cancelado {count} facturas"
                };
            }
            catch (Exception ex)
            {
                return new GenericResponse()
                {
                    Codigo = 0,
                    Mensaje = $"Excepción; Método: {this.GetType().FullName}; Mensaje: {ex.Message}"
                };
            }
        }

        private int CancelacionHijos(int comprobanteId, int count, string token) {
            Comprobante comprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID IS NOT NULL AND ID = {comprobanteId}");
            RespuestaDTOOfListOfRespuestaDTOOfCancelacionDTO lisCancel = new RespuestaDTOOfListOfRespuestaDTOOfCancelacionDTO();
            List<SolicitudCancelacionDTO> cancelacion = new List<SolicitudCancelacionDTO>();

            //Cancelar pagos del padre
            List<PagosDoctoRelacionado> pagosRelacionados = _baseDatos.Select<PagosDoctoRelacionado>($"SELECT * FROM FACTURACION_PAGOSDOCTORELACIONADO WHERE IDDOCUMENTO = '{comprobante.facturaUuid}'");
            if(pagosRelacionados.Count != 0){
                foreach (var row in pagosRelacionados) {
                    Pago pago = _baseDatos.SelectFirst<Pago>($"SELECT * FROM FACTURACION_PAGO WHERE ID = {row.pagosId}");
                    Comprobante rowComprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID IS NOT NULL AND ID = {pago.comprobanteId}");
                    if (rowComprobante != null) {
                        int count2 = CancelacionHijos(pago.comprobanteId, count, token);
                        Emisor emisor = _baseDatos.SelectFirst<Emisor>($"SELECT * FROM FACTURACION_EMISOR WHERE ID = {rowComprobante.emisorId}");
                        Receptor receptor = _baseDatos.SelectFirst<Receptor>($"SELECT * FROM FACTURACION_RECEPTOR WHERE ID = {rowComprobante.receptorId}");
                        cancelacion.Add(new SolicitudCancelacionDTO()
                        {
                            CFDI_UUID = rowComprobante.facturaUuid,
                            MontoTotal = Convert.ToDecimal(rowComprobante.total),
                            RFCEmisor = emisor.rfc,
                            RFCReceptor = receptor.rfc
                        });
                    }
                }
                if (cancelacion.Count != 0) {
                    lisCancel = webService.SolicitarCancelacionSUC(cancelacion.ToArray(), token);
                    if (lisCancel.Datos != null)
                    {
                        foreach (var can in cancelacion)
                        {
                            Comprobante rowComprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID = '{can.CFDI_UUID}'");
                            _baseDatos.Update($"UPDATE POLIZAS_FACTURACION SET ESTATUSFACTURACIONID = 9 WHERE COMPROBANTEID = {rowComprobante.id}");
                            count++;
                        }
                    }
                }
            }

            //Cancelar egresos
            comprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID IS NOT NULL AND ID = {comprobanteId}");
            lisCancel = new RespuestaDTOOfListOfRespuestaDTOOfCancelacionDTO();
            cancelacion = new List<SolicitudCancelacionDTO>();
            List<CfdiRelacionados> egresosRelacionados = _baseDatos.Select<CfdiRelacionados>($"SELECT * FROM FACTURACION_CFDIRELACIONADOS WHERE TIPORELACIONID = 2 AND UUID = '{comprobante.facturaUuid}'");
            if (egresosRelacionados.Count != 0)
            {
                foreach (var row in egresosRelacionados)
                {
                    Comprobante rowComprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID IS NOT NULL AND ID = {row.comprobanteId}");
                    if (rowComprobante != null)
                    {
                        int count2 = CancelacionHijos(row.comprobanteId, count, token);
                        Emisor emisor = _baseDatos.SelectFirst<Emisor>($"SELECT * FROM FACTURACION_EMISOR WHERE ID = {rowComprobante.emisorId}");
                        Receptor receptor = _baseDatos.SelectFirst<Receptor>($"SELECT * FROM FACTURACION_RECEPTOR WHERE ID = {rowComprobante.receptorId}");
                        cancelacion.Add(new SolicitudCancelacionDTO()
                        {
                            CFDI_UUID = rowComprobante.facturaUuid,
                            MontoTotal = Convert.ToDecimal(rowComprobante.total),
                            RFCEmisor = emisor.rfc,
                            RFCReceptor = receptor.rfc
                        });
                    }
                }
                if (cancelacion.Count != 0)
                {
                    lisCancel = webService.SolicitarCancelacionSUC(cancelacion.ToArray(), token);
                    if (lisCancel.Datos != null)
                    {
                        foreach (var can in cancelacion)
                        {
                            Comprobante rowComprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID = '{can.CFDI_UUID}'");
                            _baseDatos.Update($"UPDATE POLIZAS_FACTURACION SET ESTATUSFACTURACIONID = 9 WHERE COMPROBANTEID = {rowComprobante.id}");
                            count++;
                        }
                    }
                }
            }

            //Cancelar ingresos
            comprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID IS NOT NULL AND ID = {comprobanteId}");
            lisCancel = new RespuestaDTOOfListOfRespuestaDTOOfCancelacionDTO();
            cancelacion = new List<SolicitudCancelacionDTO>();
            List<CfdiRelacionados> ingresosRelacionados = _baseDatos.Select<CfdiRelacionados>($"SELECT * FROM FACTURACION_CFDIRELACIONADOS WHERE TIPORELACIONID = 1 AND UUID = '{comprobante.facturaUuid}'");
            if (ingresosRelacionados.Count != 0)
            {
                foreach (var row in ingresosRelacionados)
                {
                    Comprobante rowComprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID IS NOT NULL AND ID = {row.comprobanteId}");
                    if (rowComprobante != null)
                    {
                        int count2 = CancelacionHijos(row.comprobanteId, count, token);
                        Emisor emisor = _baseDatos.SelectFirst<Emisor>($"SELECT * FROM FACTURACION_EMISOR WHERE ID = {rowComprobante.emisorId}");
                        Receptor receptor = _baseDatos.SelectFirst<Receptor>($"SELECT * FROM FACTURACION_RECEPTOR WHERE ID = {rowComprobante.receptorId}");
                        cancelacion.Add(new SolicitudCancelacionDTO()
                        {
                            CFDI_UUID = rowComprobante.facturaUuid,
                            MontoTotal = Convert.ToDecimal(rowComprobante.total),
                            RFCEmisor = emisor.rfc,
                            RFCReceptor = receptor.rfc
                        });
                    }
                }
                if (cancelacion.Count != 0)
                {
                    lisCancel = webService.SolicitarCancelacionSUC(cancelacion.ToArray(), token);
                    if (lisCancel.Datos != null)
                    {
                        foreach (var can in cancelacion)
                        {
                            Comprobante rowComprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE FACTURAUUID = '{can.CFDI_UUID}'");
                            _baseDatos.Update($"UPDATE POLIZAS_FACTURACION SET ESTATUSFACTURACIONID = 9 WHERE COMPROBANTEID = {rowComprobante.id}");
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        public string generartokenSuc(string cUsuario, string cPaswordUser, string rfcEmisor, string alias)
        {

            RespuestaDTOOfString respuesta = new RespuestaDTOOfString();

            respuesta = webService.GenerarTokenSUC(cUsuario, cPaswordUser, rfcEmisor, alias);

            return respuesta.Datos.ToString();
        }
    }
}
