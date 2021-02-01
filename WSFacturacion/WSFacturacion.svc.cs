using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using WSFacturacion.Modelos;
using WSFacturacion.Servicios;

namespace WSFacturacion
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class WSFacturacion : IWSFacturacion
    {
        /// <summary>
        /// Variables privadas
        /// </summary>
        private readonly WSCFDBuilderPlus _wsTimbrado;
        private readonly WSCFDICancelacion _wsCancelacion;

        private string WS_FACTURACION_URL = ConfigurationManager.AppSettings["WSFacturacionUrl"].ToString();
        private string WS_FACTURACION_USUARIO = ConfigurationManager.AppSettings["WSFacturacionUsuario"].ToString();
        private string WS_FACTURACION_CONTRASEÑA = ConfigurationManager.AppSettings["WSFacturacionContraseña"].ToString();
        private string WS_CANCELACION_URL = ConfigurationManager.AppSettings["WSCancelacionUrl"].ToString();
        private string WS_CANCELACION_TOKEN = ConfigurationManager.AppSettings["WSCancelacionToken"].ToString();

        /// <summary>
        /// Constructor
        /// </summary>
        public WSFacturacion()
        {
            _wsTimbrado = new WSCFDBuilderPlus();
            _wsCancelacion = new WSCFDICancelacion();
        }

        /// <summary>
        /// Cancelación CFDI
        /// </summary>
        /// <param name="modelo">Modelo CancelacionModelo</param>
        /// <returns>Modelo GenericResponse</returns>
        public GenericResponse<CFDICancelacion> CancelacionCFDI(CancelacionModelo Cfdi)
        {
            CFDICancelacion respuesta = new CFDICancelacion();
            List<SolicitudCancelacionDTO> solicitud = new List<SolicitudCancelacionDTO>();
            Guid guid;

            try
            {
                if (string.IsNullOrWhiteSpace(WS_CANCELACION_URL))
                    throw new ArgumentNullException("WSCancelacionUrl, no está definido en el archivo de configuración.");

                if (string.IsNullOrWhiteSpace(WS_CANCELACION_TOKEN))
                    throw new ArgumentNullException("WSCancelacionToken, no está definido en el archivo de configuración.");

                if (string.IsNullOrWhiteSpace(Cfdi.Uuid))
                    return new GenericResponse<CFDICancelacion>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "UUID, no puede ir nulo o vacío."
                    };

                if (!Guid.TryParse(Cfdi.Uuid, out guid))
                    return new GenericResponse<CFDICancelacion>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "UUID, no es válido."
                    };

                if (string.IsNullOrWhiteSpace(Cfdi.RfcEmisor))
                    return new GenericResponse<CFDICancelacion>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "RFC Emisor, no puede ir nulo o vacío."
                    };

                if (string.IsNullOrWhiteSpace(Cfdi.RfcReceptor))
                    return new GenericResponse<CFDICancelacion>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "RFC Receptor, no puede ir nulo o vacío."
                    };

                if (Cfdi.MontoTotal < 0)
                    return new GenericResponse<CFDICancelacion>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "Monto Total, no puede ser menor a 0."
                    };


                solicitud.Add(new SolicitudCancelacionDTO()
                {
                    CFDI_UUID = Cfdi.Uuid,
                    RFCEmisor = Cfdi.RfcEmisor,
                    RFCReceptor = Cfdi.RfcReceptor,
                    MontoTotal = Cfdi.MontoTotal
                });

                _wsCancelacion.Url = WS_CANCELACION_URL;
                var resultado = _wsCancelacion.SolicitarCancelacionSUC(solicitud.ToArray(), WS_CANCELACION_TOKEN);

                if (resultado == null)
                {
                    throw new ArgumentNullException($"Servicio SolicitarCancelacionSUC no disponible, URL: {WS_CANCELACION_URL}");
                }
                else
                {
                    if (!resultado.Estado)
                    {
                        return new GenericResponse<CFDICancelacion>()
                        {
                            Codigo = (int)Codigo.Logico,
                            Mensaje = resultado.Mensaje
                        };
                    }
                    else
                    {
                        return new GenericResponse<CFDICancelacion>()
                        {
                            Codigo = resultado.Datos.FirstOrDefault().Estado ? (int)Codigo.Exito : (int)Codigo.Logico,
                            Mensaje = resultado.Datos.FirstOrDefault().Mensaje,
                            Resultado = !resultado.Datos.FirstOrDefault().Estado ? respuesta
                                        : resultado.Datos.Select(x => new CFDICancelacion()
                                        {
                                            Uuid = x.Datos.CFDI_UUID,
                                            RfcEmisor = x.Datos.RFCEmisor,
                                            RfcReceptor = x.Datos.RFCReceptor,
                                            FechaSolicitud = x.Datos.FechaSolicitud,
                                            AutorizoCliente = x.Datos.AutorizoCliente,
                                            FechaEstatus = x.Datos.FechaEstatus,
                                            MontoTotal = x.Datos.MontoTotal
                                        }).FirstOrDefault()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new GenericResponse<CFDICancelacion>()
                {
                    Codigo = (int)Codigo.Error,
                    Mensaje = $"Cancelación CFDI, falló: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Estatus CFDI
        /// </summary>
        /// <param name="modelo">Modelo CancelacionModelo</param>
        /// <returns>Modelo GenericResponse</returns>
        public GenericResponse<string> EstatusCFDI(CancelacionModelo Cfdi)
        {
            List<ValidarUUIDS> solicitud = new List<ValidarUUIDS>();
            Guid guid;

            try
            {
                if (string.IsNullOrWhiteSpace(WS_CANCELACION_URL))
                    throw new ArgumentNullException("WSCancelacionUrl, no está definido en el archivo de configuración.");

                if (string.IsNullOrWhiteSpace(WS_CANCELACION_TOKEN))
                    throw new ArgumentNullException("WSCancelacionToken, no está definido en el archivo de configuración.");

                if (string.IsNullOrWhiteSpace(Cfdi.Uuid))
                    return new GenericResponse<string>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "UUID, no puede ir nulo o vacío."
                    };

                if (!Guid.TryParse(Cfdi.Uuid, out guid))
                    return new GenericResponse<string>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "UUID, no es válido."
                    };

                if (string.IsNullOrWhiteSpace(Cfdi.RfcEmisor))
                    return new GenericResponse<string>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "RFC Emisor, no puede ir nulo o vacío."
                    };

                if (string.IsNullOrWhiteSpace(Cfdi.RfcReceptor))
                    return new GenericResponse<string>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "RFC Receptor, no puede ir nulo o vacío."
                    };

                if (Cfdi.MontoTotal < 0)
                    return new GenericResponse<string>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "Monto Total, no puede ser menor a 0."
                    };

                solicitud.Add(new ValidarUUIDS()
                {
                    CFDI_UUID = Cfdi.Uuid,
                    RFCEmisor = Cfdi.RfcEmisor,
                    RFCReceptor = Cfdi.RfcReceptor,
                    MontoTotal = Cfdi.MontoTotal
                });

                _wsCancelacion.Url = WS_CANCELACION_URL;

                var resultado = _wsCancelacion.ValidarUUIDSUC(solicitud.ToArray(), WS_CANCELACION_TOKEN);

                if (resultado == null)
                {
                    throw new ArgumentNullException($"Servicio ValidarUUIDSUC no disponible, URL: {WS_CANCELACION_URL}");
                }
                else
                {
                    if (!resultado.Estado)
                    {
                        return new GenericResponse<string>()
                        {
                            Codigo = (int)Codigo.Logico,
                            Mensaje = resultado.Mensaje
                        };
                    }
                    else
                    {
                        return new GenericResponse<string>()
                        {
                            Codigo = resultado.Datos.FirstOrDefault().Estado ? (int)Codigo.Exito : (int)Codigo.Logico,
                            Mensaje = resultado.Datos.FirstOrDefault().Mensaje,
                            Resultado = resultado.Datos.FirstOrDefault().Datos
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new GenericResponse<string>()
                {
                    Codigo = (int)Codigo.Error,
                    Mensaje = $"Cancelación CFDI, falló: {ex.Message}",
                    Resultado = string.Empty
                };
            }
        }

        /// <summary>
        /// Relacionados CFDI
        /// </summary>
        /// <param name="Uuid">UUID CFDI</param>
        /// <returns>Modelo GenericResponse</returns>
        public GenericResponse<List<CFDIRelacionados>> RelacionadosCFDI(string Uuid)
        {
            List<CFDIRelacionados> respuesta = new List<CFDIRelacionados>();
            Guid guid;

            try
            {
                if (string.IsNullOrWhiteSpace(WS_CANCELACION_URL))
                    throw new ArgumentNullException("WSCancelacionUrl, no está definido en el archivo de configuración.");

                if (string.IsNullOrWhiteSpace(WS_CANCELACION_TOKEN))
                    throw new ArgumentNullException("WSCancelacionToken, no está definido en el archivo de configuración.");

                if (string.IsNullOrWhiteSpace(Uuid))
                    return new GenericResponse<List<CFDIRelacionados>>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "UUID, no puede ir nulo o vacío."
                    };

                if (!Guid.TryParse(Uuid, out guid))
                    return new GenericResponse<List<CFDIRelacionados>>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "UUID, no es válido."
                    };

                _wsCancelacion.Url = WS_CANCELACION_URL;
                var resultado = _wsCancelacion.ConsultarRelacionadosSUC(Uuid, WS_CANCELACION_TOKEN);

                if (resultado == null)
                {
                    throw new ArgumentNullException($"Servicio ConsultarRelacionadosSUC no disponible, URL: {WS_CANCELACION_URL}");
                }
                else
                {
                    return new GenericResponse<List<CFDIRelacionados>>()
                    {
                        Codigo = resultado.Estado ? (int)Codigo.Exito : (int)Codigo.Logico,
                        Mensaje = !resultado.Estado ? resultado.Mensaje : resultado.Datos.Length > 0 ? "Se encontraron CFDI Relacionado para UUID" : "No se encontraron CFDI Relacionado para UUID",
                        Resultado = !resultado.Estado ? respuesta
                                    : resultado.Datos.Length > 0 ? resultado.Datos.Select(x => new CFDIRelacionados() { Estado = x.Estado, TipoRelacion = x.TipoRelacion, Uuid = x.CFDI_UUID, RfcEmisor = x.RFCEmisor, RfcReceptor = x.RFCReceptor }).ToList()
                                    : respuesta
                    };
                }
            }
            catch (Exception ex)
            {
                return new GenericResponse<List<CFDIRelacionados>>()
                {
                    Codigo = (int)Codigo.Error,
                    Mensaje = $"Cancelación CFDI, falló: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Timbrado XML
        /// </summary>
        /// <param name="estructuraXml">Cadena XML para Timbrar</param>
        /// <returns>Modelo GenericResponse</returns>
        public GenericResponse<string> TimbradoCFDI(string EstructuraXml)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(WS_FACTURACION_URL))
                    throw new ArgumentNullException("WSFacturacionUrl, no está definido en el archivo de configuración.");

                if (string.IsNullOrWhiteSpace(WS_FACTURACION_USUARIO))
                    throw new ArgumentNullException("WSFacturacionUrl, no está definido en el archivo de configuración.");

                if (string.IsNullOrWhiteSpace(WS_FACTURACION_CONTRASEÑA))
                    throw new ArgumentNullException("WSFacturacionContraseña, no está definido en el archivo de configuración.");

                if (string.IsNullOrWhiteSpace(EstructuraXml))
                    return new GenericResponse<string>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = "La estructura del XML, no puede ir nula o vacía.",
                        Resultado = string.Empty
                    };

                _wsTimbrado.Url = WS_FACTURACION_URL;
                var resultado = _wsTimbrado.getCFDI(WS_FACTURACION_USUARIO, WS_FACTURACION_CONTRASEÑA, EstructuraXml);

                if (resultado.Contains("<Error>"))
                {
                    var document = XDocument.Parse(resultado);
                    var error = document.Descendants("Error").Select(x => x.Element("ErrorMessage").Value).ToList();

                    return new GenericResponse<string>()
                    {
                        Codigo = (int)Codigo.Logico,
                        Mensaje = $"Timbrado CFDI, falló: {error.FirstOrDefault().Replace("'", "''")}",
                        Resultado = string.Empty
                    };
                }
                else
                {
                    return new GenericResponse<string>()
                    {
                        Codigo = (int)Codigo.Exito,
                        Mensaje = "Timbrado CFDI, realizado con éxito.",
                        Resultado = resultado
                    };
                }
            }
            catch (Exception ex)
            {
                return new GenericResponse<string>()
                {
                    Codigo = (int)Codigo.Error,
                    Mensaje = $"Timbrado CFDI, falló: {ex.Message}",
                    Resultado = string.Empty
                };
            }
        }


    }
}
