using FacturacionCFDI.Datos.Facturacion.Nodos;
using FacturacionCFDI.Datos.Response;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Razor.Generator;
using Utilerias;
using static Utilerias.Enum.TipoBaseDatos;

namespace FacturacionCFDI.Negocio.Facturacion
{
    public class Procesador
    {
        private readonly BaseDatos _baseDatos;
        private readonly ObtenerNodos _nodos;
        private readonly PDF _pdf;
        private readonly XML _xml;


        private const string QUERY_RELACION_FACTURAS_TIMBRAR = "SELECT c.id as id FROM facturacion_comprobante c WHERE c.estatusFacturaId IN (1) AND ROWNUM <= 1";
        private const string QUERY_LOGFACTURAID = "SELECT NVL(MAX(ID) + 1, 1) FROM FACTURACION_LOGFACTURA";
        private const string QUERY_LOGTIMBRADOID = "SELECT NVL(MAX(ID) + 1, 1) FROM FACTURACION_LOGTIMBRADO";
        private const string QUERY_COMPROBANTE = "SELECT c.estatusFacturaId as id FROM facturacion_comprobante c WHERE c.id = ";

        private bool _generaPdf = true;
        private string _conexiondb = ConfigurationManager.ConnectionStrings["FacturacionCFDI"].ToString();

        /// <summary>
        /// Constructor
        ///</summary>
        public Procesador()
        {
            _baseDatos = new BaseDatos(_conexiondb, TipoBase.Oracle);
            _nodos = new ObtenerNodos(_baseDatos);
            _pdf = new PDF(_baseDatos);
            _xml = new XML(_baseDatos);
        }

        /// <summary>
        /// Obtener relación de Comprobantes por Facturar
        /// </summary>
        /// <returns>Lista modelo NodoComprobante</returns>
        public async Task<List<NodoComprobante>> ObtenerComprobantePorFacturar()
        {
            try
            {
                return await _baseDatos.SelectAsync<NodoComprobante>(QUERY_RELACION_FACTURAS_TIMBRAR);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Proceso de Timbrado de un Comprobante
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns></returns>
        public async Task ProcesoTibrado(string idComprobante)
        {
            try
            {
                var comprobante = await _nodos.ObtenerNodoComprobante(idComprobante);

                if (comprobante.Codigo != 1)
                {
                    await LogFactura(idComprobante, comprobante.Mensaje);
                    Console.WriteLine($"Codigo: {comprobante.Codigo}; Mensaje: {comprobante.Mensaje}"); //Se debe remover para Servicio
                }

                var xmlTimbrado = _xml.TimbrarComprobante(comprobante.Data);
                if (xmlTimbrado.Codigo == 1 && xmlTimbrado.Data != null)
                {
                    await EstructuraComprobante(idComprobante);
                    await LogFactura(idComprobante, $"Se guarda estructura XML");

                    var archivoXml = await _xml.GenerarArchivoXML(xmlTimbrado.Data.ToString(), comprobante.Data, idComprobante);

                    if (archivoXml.Codigo == 1 && archivoXml.Data != null)
                    {
                        await LogFactura(idComprobante, $"XML Generado");
                        await EstatusComprobante(idComprobante, 2);
                        await LogFactura(idComprobante, $"Estatus comprobante actualizado");
                        await XMLComprobante(idComprobante, archivoXml.Data.RutaRelativa);
                        await LogFactura(idComprobante, $"Se guarda ruta XML");

                        var comprobanteTimbrado = _xml.ObtenerComprobanteArchivoXml(archivoXml.Data.RutaAbsoluta);
                        if (comprobanteTimbrado != null)
                        {
                            await LogFactura(idComprobante, $"Se pudo leer XML para obtener UUID y generar PDF");
                            await UuidComprobante(idComprobante, comprobanteTimbrado.TimbreFiscalDigital.UUID);
                            await LogFactura(idComprobante, $"Se guarda UUID comprobante");

                            if (_generaPdf)
                            {
                                var pdf = await _pdf.GenerarPDF(comprobanteTimbrado, idComprobante);
                                if (pdf.Codigo == 1 && pdf.Data != null)
                                {
                                    await LogFactura(idComprobante, $"PDF Generado");
                                    await PdfComprobante(idComprobante, pdf.Data.RutaRelativa);
                                    await LogFactura(idComprobante, $"Se guarda ruta PDF");
                                    Console.WriteLine($"XML Generado ({archivoXml.Data.RutaAbsoluta}), PDF Generado ({pdf.Data.RutaAbsoluta})");
                                }
                                else
                                {
                                    await LogFactura(idComprobante, $"PDF no Generado, {pdf.Mensaje}");
                                    Console.WriteLine($"XML Generado ({archivoXml.Data.RutaAbsoluta}), PDF No Generado ({pdf.Mensaje})");
                                }
                            }
                        }
                        else
                        {
                            await LogFactura(idComprobante, $"No se pudo leer XML para obtener UUID y generar PDF");
                            Console.WriteLine($"XML Generado ({archivoXml.Data.RutaAbsoluta}), pero no se pudo leer XML para obtener UUID y generar PDF");
                        }
                    }
                    else
                    {
                        await EstatusComprobante(idComprobante, 5);
                        await LogFactura(idComprobante, $"Estatus comprobante actualizado");
                        await LogFactura(idComprobante, $"XML no Generado, {archivoXml.Mensaje}");
                        Console.WriteLine($"Codigo: {archivoXml.Codigo}; Mensaje: {archivoXml.Mensaje}"); //Se debe remover para Servicio
                    }
                }
                else
                {
                    await EstatusComprobante(idComprobante, 99);
                    await LogFactura(idComprobante, $"Estatus comprobante actualizado");
                    await LogFactura(idComprobante, xmlTimbrado.Mensaje);
                    Console.WriteLine($"Codigo: {xmlTimbrado.Codigo}; Mensaje: {xmlTimbrado.Mensaje}"); //Se debe remover para Servicio
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Codigo: 0; Mensaje: {ex.Message}");
            }
        }

        public async Task ProcesoAutomatico()
        {
            try
            {
                bool repeat = true;
                do
                {
                    int Id = _baseDatos.SelectFirst<int>("SELECT ID FROM FACTURACION_COMPROBANTE WHERE ESTATUSFACTURAID = 1");
                    if (Id != 0)
                    {
                        string idComprobante = Convert.ToString(Id);
                        var comprobante = await _nodos.ObtenerNodoComprobante(idComprobante);

                        if (comprobante.Codigo != 1)
                        {
                            await EstatusComprobante(idComprobante, 99);
                            LogTimbrado(idComprobante, comprobante.Mensaje);
                            Console.WriteLine($"Codigo: {comprobante.Codigo}; Mensaje: {comprobante.Mensaje}"); //Se debe remover para Servicio
                        } else {
                            var xmlTimbrado = _xml.TimbrarComprobante(comprobante.Data);
                            if (xmlTimbrado.Codigo == 1 && xmlTimbrado.Data != null)
                            {
                                await EstructuraComprobante(idComprobante);
                                //await LogFactura(idComprobante, $"Se guarda estructura XML");

                                var archivoXml = await _xml.GenerarArchivoXML(xmlTimbrado.Data.ToString(), comprobante.Data, idComprobante);

                                if (archivoXml.Codigo == 1 && archivoXml.Data != null)
                                {
                                    //await LogFactura(idComprobante, $"XML Generado");
                                    await EstatusComprobante(idComprobante, 2);
                                    //await LogFactura(idComprobante, $"Estatus comprobante actualizado");
                                    await XMLComprobante(idComprobante, archivoXml.Data.RutaRelativa);
                                    //await LogFactura(idComprobante, $"Se guarda ruta XML");

                                    var comprobanteTimbrado = _xml.ObtenerComprobanteArchivoXml(archivoXml.Data.RutaAbsoluta);
                                    if (comprobanteTimbrado != null)
                                    {
                                        //await LogFactura(idComprobante, $"Se pudo leer XML para obtener UUID y generar PDF");
                                        LogTimbradoSolucionado(idComprobante);
                                        await UuidComprobante(idComprobante, comprobanteTimbrado.TimbreFiscalDigital.UUID);
                                        //await LogFactura(idComprobante, $"Se guarda UUID comprobante");
                                        Console.WriteLine($"XML Generado ({archivoXml.Data.RutaAbsoluta})");
                                    }
                                }
                                else
                                {
                                    await EstatusComprobante(idComprobante, 5);
                                    LogTimbrado(idComprobante, $"XML no Generado, {archivoXml.Mensaje}");
                                    //await LogFactura(idComprobante, $"Estatus comprobante actualizado");
                                    //await LogFactura(idComprobante, $"XML no Generado, {archivoXml.Mensaje}");
                                    Console.WriteLine($"Codigo: {archivoXml.Codigo}; Mensaje: {archivoXml.Mensaje}"); //Se debe remover para Servicio
                                }
                            }
                            else
                            {
                                await EstatusComprobante(idComprobante, 99);
                                LogTimbrado(idComprobante, xmlTimbrado.Mensaje);
                                //await LogFactura(idComprobante, $"Estatus comprobante actualizado");
                                //await LogFactura(idComprobante, xmlTimbrado.Mensaje);
                                Console.WriteLine($"Codigo: {xmlTimbrado.Codigo}; Mensaje: {xmlTimbrado.Mensaje}"); //Se debe remover para Servicio
                            }
                        }
                    } else {
                        repeat = false;
                    }
                } while (repeat == true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Codigo: 0; Mensaje: {ex.Message}");
            }
        }

        /// <summary>
        /// Proceso de Timbrado de un Comprobante
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns></returns>
        public async Task<GenericResponse> ProcesoTimbradoRefacturacion(string idComprobante)
        {
            try
            {
                var comprobante = await _nodos.ObtenerNodoComprobante(idComprobante);

                if (comprobante.Codigo != 1)
                {
                    await LogFactura(idComprobante, comprobante.Mensaje);
                    return new GenericResponse()
                    {
                        Codigo = 0,
                        Mensaje = $"Codigo: {comprobante.Codigo}; Mensaje: {comprobante.Mensaje}"
                    };
                }

                var xmlTimbrado = _xml.TimbrarComprobante(comprobante.Data);
                if (xmlTimbrado.Codigo == 1 && xmlTimbrado.Data != null)
                {
                    await EstructuraComprobante(idComprobante);
                    await LogFactura(idComprobante, $"Se guarda estructura XML");

                    var archivoXml = await _xml.GenerarArchivoXML(xmlTimbrado.Data.ToString(), comprobante.Data, idComprobante);

                    if (archivoXml.Codigo == 1 && archivoXml.Data != null)
                    {
                        await LogFactura(idComprobante, $"XML Generado");
                        await EstatusComprobante(idComprobante, 2);
                        await LogFactura(idComprobante, $"Estatus comprobante actualizado");
                        await XMLComprobante(idComprobante, archivoXml.Data.RutaRelativa);
                        await LogFactura(idComprobante, $"Se guarda ruta XML");

                        var comprobanteTimbrado = _xml.ObtenerComprobanteArchivoXml(archivoXml.Data.RutaAbsoluta);
                        if (comprobanteTimbrado != null)
                        {
                            await LogFactura(idComprobante, $"Se pudo leer XML para obtener UUID y generar PDF");
                            await UuidComprobante(idComprobante, comprobanteTimbrado.TimbreFiscalDigital.UUID);
                            await LogFactura(idComprobante, $"Se guarda UUID comprobante");

                            if (_generaPdf)
                            {
                                var pdf = await _pdf.GenerarPDF(comprobanteTimbrado, idComprobante);
                                if (pdf.Codigo == 1 && pdf.Data != null)
                                {
                                    await LogFactura(idComprobante, $"PDF Generado");
                                    await PdfComprobante(idComprobante, pdf.Data.RutaRelativa);
                                    await LogFactura(idComprobante, $"Se guarda ruta PDF");
                                    return new GenericResponse()
                                    {
                                        Codigo = 1,
                                        Mensaje = $"XML Generado ({archivoXml.Data.RutaAbsoluta}), PDF Generado ({pdf.Data.RutaAbsoluta})"
                                    };
                                }
                                else
                                {
                                    await LogFactura(idComprobante, $"PDF no Generado, {pdf.Mensaje}");
                                    return new GenericResponse()
                                    {
                                        Codigo = 0,
                                        Mensaje = $"XML Generado ({archivoXml.Data.RutaAbsoluta}), PDF No Generado ({pdf.Mensaje})"
                                    };
                                }
                            }
                        }
                        else
                        {
                            await LogFactura(idComprobante, $"No se pudo leer XML para obtener UUID y generar PDF");
                            return new GenericResponse()
                            {
                                Codigo = 0,
                                Mensaje = $"XML Generado ({archivoXml.Data.RutaAbsoluta}), pero no se pudo leer XML para obtener UUID y generar PDF"
                            };
                        }
                    }
                    else
                    {
                        await EstatusComprobante(idComprobante, 5);
                        await LogFactura(idComprobante, $"Estatus comprobante actualizado");
                        await LogFactura(idComprobante, $"XML no Generado, {archivoXml.Mensaje}");
                        return new GenericResponse()
                        {
                            Codigo = 0,
                            Mensaje = $"Codigo: {archivoXml.Codigo}; Mensaje: {archivoXml.Mensaje}"
                        };
                    }
                }
                else
                {
                    await EstatusComprobante(idComprobante, 99);
                    await LogFactura(idComprobante, $"Estatus comprobante actualizado");
                    await LogFactura(idComprobante, xmlTimbrado.Mensaje);
                    return new GenericResponse()
                    {
                        Codigo = 0,
                        Mensaje = $"Codigo: {xmlTimbrado.Codigo}; Mensaje: {xmlTimbrado.Mensaje}"
                    };
                }

                return new GenericResponse()
                {
                    Codigo = 0,
                    Mensaje = $"Codigo: 0; Mensaje: No realizo nada"
                };
            }
            catch (Exception ex)
            {
                return new GenericResponse()
                {
                    Codigo = 0,
                    Mensaje = $"Codigo: 0; Mensaje: {ex.Message}"
                };
            }
        }


        public bool ValidarIdComprobante(string idComprobante)
        {
            try
            {
                var comprobante = _baseDatos.SelectFirst<int>(QUERY_COMPROBANTE + idComprobante);

                if (comprobante == 0)
                {
                    Console.WriteLine("Comprobante ingresado no exite.");
                    return false;
                }
                else if (comprobante == 2)
                {
                    Console.WriteLine("Comprobante ingresado, ya fue timbrado.");
                    return false;
                }
                else
                {
                    Console.WriteLine("Comprobante válido para procesar.");
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Guarda registro en la Tabla FACTURACION_LOGFACTURA
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <param name="mensaje">Mensaje</param>
        /// <returns></returns>
        private async Task LogFactura(string idComprobante, string mensaje)
        {
            int logFacturaId;

            try
            {
                logFacturaId = await _baseDatos.SelectFirstAsync<int>(QUERY_LOGFACTURAID);
                if (logFacturaId > 0)
                    await _baseDatos.InsertAsync($"INSERT INTO facturacion_logfactura VALUES ({logFacturaId}, SYSDATE, '{mensaje}', {idComprobante})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Codigo: 0; Mensaje: {ex.Message}");
            }
        }

        /// <summary>
        /// Guarda registro en la Tabla FACTURACION_LOGTIMBRADO
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <param name="mensaje">Mensaje</param>
        /// <returns></returns>
        private async Task LogTimbrado(string idComprobante, string mensaje)
        {
            try
            {
                int exist = _baseDatos.SelectFirst<int>($"SELECT ID FROM FACTURACION_LOGTIMBRADO WHERE COMPROBANTEID = {idComprobante} AND ESTATUS = 0");
                if (exist == 0)
                {
                    int Id = _baseDatos.SelectFirst<int>(QUERY_LOGTIMBRADOID);
                    if (Id > 0)
                        _baseDatos.Insert($"INSERT INTO FACTURACION_LOGTIMBRADO VALUES ({Id}, {idComprobante}, SYSDATE, '{mensaje}', NULL, 0)");
                } else {
                    _baseDatos.Update($"UPDATE FACTURACION_LOGTIMBRADO SET FECHAHORAERROR = SYSDATE, FECHAHORASOLUCIONADO = SYSDATE, ESTATUS = 0, ERROR = '{mensaje}' WHERE ID = {exist}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Codigo: 0; Mensaje: {ex.Message}");
            }
        }

        /// <summary>
        /// Guarda registro en la Tabla FACTURACION_LOGTIMBRADO
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <param name="mensaje">Mensaje</param>
        /// <returns></returns>
        private async Task LogTimbradoSolucionado(string idComprobante)
        {
            try
            {
                int exist = _baseDatos.SelectFirst<int>($"SELECT ID FROM FACTURACION_LOGTIMBRADO WHERE COMPROBANTEID = {idComprobante}");
                if(exist != 0)
                    _baseDatos.Update($"UPDATE FACTURACION_LOGTIMBRADO SET FECHAHORASOLUCIONADO = SYSDATE, ESTATUS = 1 WHERE ID = {exist}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Codigo: 0; Mensaje: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza en Tabla FACTURACION_COMPROBANTE Estatus del Comprobante
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <param name="idEstatus">ID Estatus de acuerdo a Tabla FACTURACION_CATESTATUSFACTURA</param>
        /// <returns></returns>
        private async Task EstatusComprobante(string idComprobante, int idEstatus)
        {
            try
            {
                await _baseDatos.UpdateAsync($"UPDATE facturacion_comprobante SET fechamodificacion = SYSDATE, estatusfacturaid = {idEstatus} WHERE id = {idComprobante}");
            }
            catch
            {
            }
        }

        /// <summary>
        /// Actualiza en Tabla FACTURACION_COMPROBANTE Estructura de la Factura
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <param name="xml">Estructura XML</param>
        /// <returns></returns>
        private async Task EstructuraComprobante(string idComprobante)
        {
            try
            {
                await _baseDatos.UpdateAsync($"UPDATE facturacion_comprobante SET fechamodificacion = SYSDATE WHERE id = {idComprobante}");
            }
            catch
            {
            }
        }

        /// <summary>
        /// Actualiza en Tabla FACTURACION_COMPROBANTE ruta XML
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <param name="ruta">Ruta archivo XML</param>
        /// <returns></returns>
        private async Task XMLComprobante(string idComprobante, string ruta)
        {
            try
            {
                await _baseDatos.UpdateAsync($"UPDATE facturacion_comprobante SET fechamodificacion = SYSDATE, facturaxml = '{ruta}' WHERE id = {idComprobante}");
            }
            catch
            {
            }
        }

        /// <summary>
        /// Actualiza en Tabla FACTURACION_COMPROBANTE UUID de la Factura
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <param name="uuid">UUID Factura</param>
        /// <returns></returns>
        private async Task UuidComprobante(string idComprobante, string uuid)
        {
            try
            {
                await _baseDatos.UpdateAsync($"UPDATE facturacion_comprobante SET fechamodificacion = SYSDATE, facturauuid = '{uuid}' WHERE id = {idComprobante}");
            }
            catch
            {
            }
        }

        /// <summary>
        /// Actualiza en Tabla FACTURACION_COMPROBANTE ruta PDF
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <param name="ruta">Ruta archivo PDF</param>
        /// <returns></returns>
        private async Task PdfComprobante(string idComprobante, string ruta)
        {
            try
            {
                await _baseDatos.UpdateAsync($"UPDATE facturacion_comprobante SET fechamodificacion = SYSDATE, facturapdf = '{ruta}' WHERE id = {idComprobante}");
            }
            catch
            {
            }
        }
    }
}
