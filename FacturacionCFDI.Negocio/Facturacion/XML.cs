using FacturacionCFDI.Datos.Facturacion.TablasDB;
using FacturacionCFDI.Datos.Response;
using FacturacionCFDI.Negocio.Datos.Facturacion.Factura;
using FacturacionCFDI.Negocio.WebService;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Utilerias;
using Comprobante = FacturacionCFDI.Datos.Facturacion.Factura.Comprobante;

namespace FacturacionCFDI.Negocio.Facturacion
{
    public class XML
    {
        private readonly BaseDatos _baseDatos;
        private readonly SelloDigital _selloDigital;
        private readonly WSCFDBuilderPlus _ws;
        private readonly XslCompiledTransform _xslCompiledTransform;

        private const string _queryConfiguracionSistema = "SELECT id ,llave ,valor FROM ConfiguracionSistema WHERE llave = ";

        private string _facturaCertificado;
        private string _facturaRutaCer;
        private string _facturaRutaKey;
        private string _facturaFiel;
        private string _facturaRutaXslt;
        private string _facturaRutaTimbrado;
        private string _wsUrlTimbrado;
        private string _wsUsuarioTimbrado;
        private string _wsContraseñaTimbrado;

        private byte[] _facturaArchivoKey;

        public XML(BaseDatos baseDatos)
        {
            _baseDatos = baseDatos;
            _selloDigital = new SelloDigital();
            _ws = new WSCFDBuilderPlus();
            _xslCompiledTransform = new XslCompiledTransform();

            _facturaRutaCer = ObtenerConfiguracionSistema("FacturaRutaCer");
            if (string.IsNullOrWhiteSpace(_facturaRutaCer))
                throw new ArgumentNullException("FacturaRutaCer, no está definido en Tabla ConfiguracionSistema.");

            _facturaRutaKey = ObtenerConfiguracionSistema("FacturaRutaKey");
            if (string.IsNullOrWhiteSpace(_facturaRutaKey))
                throw new ArgumentNullException("FacturaRutaKey, no está definido en Tabla ConfiguracionSistema.");

            _facturaFiel = ObtenerConfiguracionSistema("FacturaRutaFiel");
            if (string.IsNullOrWhiteSpace(_facturaFiel))
                throw new ArgumentNullException("FacturaRutaFiel, no está definido en Tabla ConfiguracionSistema.");

            _facturaRutaXslt = ObtenerConfiguracionSistema("FacturaRutaXslt");
            if (string.IsNullOrWhiteSpace(_facturaRutaXslt))
                throw new ArgumentNullException("FacturaRutaXslt, no está definido en Tabla ConfiguracionSistema.");

            _facturaRutaTimbrado = ObtenerConfiguracionSistema("FacturaRutaTimbrado");
            if (string.IsNullOrWhiteSpace(_facturaRutaTimbrado))
                throw new ArgumentNullException("FacturaRutaTimbrado, no está definido en Tabla ConfiguracionSistema.");

            _wsUrlTimbrado = ObtenerConfiguracionSistema("WsUrlTimbrado");
            if (string.IsNullOrWhiteSpace(_wsUrlTimbrado))
                throw new ArgumentNullException("WsUrlTimbrado, no está definido en Tabla ConfiguracionSistema.");

            _wsUsuarioTimbrado = ObtenerConfiguracionSistema("WsUsuarioTimbrado");
            if (string.IsNullOrWhiteSpace(_wsUsuarioTimbrado))
                throw new ArgumentNullException("WsUsuarioTimbrado, no está definido en Tabla ConfiguracionSistema.");

            _wsContraseñaTimbrado = ObtenerConfiguracionSistema("WsContraseñaTimbrado");
            if (string.IsNullOrWhiteSpace(_wsContraseñaTimbrado))
                throw new ArgumentNullException("WsContraseñaTimbrado, no está definido en Tabla ConfiguracionSistema.");

            _facturaCertificado = ObtenerCertificado();
            if (string.IsNullOrWhiteSpace(_facturaCertificado))
                throw new ArgumentNullException("FacturaCertificado, no se encontró archivo .cer en la ruta " + _facturaRutaCer);

            _facturaArchivoKey = ObtenerFacturaArchivoKey();
            if (_facturaArchivoKey == null)
                throw new ArgumentNullException("FacturaArchivoKey,  no se encontró archivo .key en la ruta " + _facturaRutaKey);

            if (!File.Exists(_facturaRutaXslt))
                throw new ArgumentNullException("FacturaRutaXslt,  no se encontró archivo .xslt en la ruta " + _facturaRutaXslt);

            _xslCompiledTransform.Load(_facturaRutaXslt);
        }

        /// <summary>
        /// Timbrado de Comprobante
        /// </summary>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Modelo GenericResponse</returns>
        public GenericResponse TimbrarComprobante(Comprobante comprobante)
        {
            try
            {
                var firmaComprobante = ObtenerFirmaComprobante(comprobante);

                if (!string.IsNullOrWhiteSpace(firmaComprobante))
                {
                    return TimbrarXML(firmaComprobante);
                }
                else
                {
                    return new GenericResponse() { Codigo = 2, Mensaje = "" };
                }

            }
            catch (Exception ex)
            {
                return new GenericResponse() { Codigo = 0, Mensaje = ex.Message };
            }
        }

        /// <summary>
        /// Generar archivo XML
        /// </summary>
        /// <param name="comprobanteTimbrado">Cadena XML Timbrado</param>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Modelo GenericResponse</returns>
        public async Task<GenericResponse<RutaArchivo>> GenerarArchivoXML(string comprobanteTimbrado, Comprobante comprobante, string idComprobante)
        {
            try
            {
                string poliza = ObtenerPoliza(int.Parse(idComprobante));

                var anioMes = DateTime.Now.ToString("yyyyMM");
                var carpeta = $@"{poliza}\{anioMes}\";
                var directorio = $@"{_facturaRutaTimbrado}\{carpeta}";
                var archivo = $@"{_facturaRutaTimbrado}\{carpeta}{comprobante.Emisor.Rfc}_{comprobante.TipoDeComprobante}_{comprobante.Serie}_{comprobante.Folio}.xml";
                var rutaRelativa = $@"{carpeta}{comprobante.Emisor.Rfc}_{comprobante.TipoDeComprobante}_{comprobante.Serie}_{comprobante.Folio}.xml";


                if (!Directory.Exists(directorio))
                {
                    DirectoryInfo di = Directory.CreateDirectory(directorio);
                }

                using (StreamWriter outputFile = new StreamWriter(archivo, false, Encoding.UTF8))
                {
                    outputFile.Write(comprobanteTimbrado);
                }

                if (File.Exists(archivo))
                    return new GenericResponse<RutaArchivo>() { Data = new RutaArchivo() { RutaAbsoluta = archivo, RutaRelativa = rutaRelativa } };
                else
                    return new GenericResponse<RutaArchivo>() { Codigo = 2, Mensaje = "XML no Generado", Descripcion = "XML no Generado" };
            }
            catch (Exception ex)
            {
                return new GenericResponse<RutaArchivo>() { Codigo = 0, Mensaje = ex.Message, Descripcion = ex.HelpLink };
            }
        }

        /// <summary>
        /// Obtener modelo Comprobante desde un Archivo XML
        /// </summary>
        /// <param name="rutaArchivoXml">Ubicación del archivo XML</param>
        /// <returns>Modelo Comprobante</returns>
        public Comprobante ObtenerComprobanteArchivoXml(string rutaArchivoXml)
        {
            Comprobante respuesta = new Comprobante();
            XmlSerializer oSerializer = new XmlSerializer(typeof(Comprobante));

            try
            {
                using (StreamReader reader = new StreamReader(rutaArchivoXml))
                {

                    var oComprobante = (Comprobante)oSerializer.Deserialize(reader);

                    //complementos
                    foreach (var oComplemento in oComprobante.Complemento)
                    {
                        foreach (var oComplementoInterior in oComplemento.Any)
                        {
                            if (oComplementoInterior.Name.Contains("TimbreFiscalDigital"))
                            {
                                XmlSerializer oSerializerTimbreFiscal = new XmlSerializer(typeof(TimbreFiscalDigital));
                                using (var readerComplemento = new StringReader(oComplementoInterior.OuterXml))
                                {
                                    oComprobante.TimbreFiscalDigital = (TimbreFiscalDigital)oSerializerTimbreFiscal.Deserialize(readerComplemento);
                                }
                            }

                            if (oComplementoInterior.Name.Contains("Pagos"))
                            {
                                XmlSerializer oSerializerPagos = new XmlSerializer(typeof(Pagos));
                                using (var readerComplemento = new StringReader(oComplementoInterior.OuterXml))
                                {
                                    oComprobante.Pagos = (Pagos)oSerializerPagos.Deserialize(readerComplemento);
                                }
                            }
                        }
                    }

                    respuesta = oComprobante;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        #region Métodos privados
        /// <summary>
        /// Obtener Configuración del Sistema en Base de Datos
        /// </summary>
        /// <param name="llave">Llave de valor a obtener</param>
        /// <returns>Cadena con valor de llave</returns>
        private string ObtenerConfiguracionSistema(string llave)
        {
            string respuesta = string.Empty;

            var resultado = _baseDatos.SelectFirst<ConfiguracionSistema>($"{_queryConfiguracionSistema}'{llave}'");

            if (resultado != null)
                if (!string.IsNullOrWhiteSpace(resultado.valor))
                    respuesta = resultado.valor;

            return respuesta;
        }

        /// <summary>
        /// Obtener cadena del Certificado del archivo .cer
        /// </summary>
        /// <returns>Cadena del certificados</returns>
        private string ObtenerCertificado()
        {
            string respuesta;
            SelloDigital sello = new SelloDigital();

            try
            {
                respuesta = sello.Certificado(_facturaRutaCer);
            }
            catch
            {
                respuesta = string.Empty;
            }
            return respuesta;
        }

        /// <summary>
        /// Obtener arreglo en byte del archivo de Clave Privada
        /// </summary>
        /// <returns>Arreglo de byte</returns>
        private byte[] ObtenerFacturaArchivoKey()
        {
            if (File.Exists(_facturaRutaKey))
                return File.ReadAllBytes(_facturaRutaKey);
            else
                return null;
        }

        /// <summary>
        /// Convertir Modelo Comprobante a XML Reader
        /// </summary>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Objeto XmlReader</returns>
        private async Task<string> GenerarComprobanteXmlTemp(Comprobante comprobante)
        {
            string sXml;
            string rutaXmlTemp;
            try
            {
                var xmlNameSpace = new XmlSerializerNamespaces();
                xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
                xmlNameSpace.Add("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
                xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                if(comprobante.TipoDeComprobante == "P")
                {
                    xmlNameSpace.Add("pago10", "http://www.sat.gob.mx/Pagos");
                }

                var oXmlSerializar = new XmlSerializer(typeof(Comprobante));

                using (var sww = new StringWriterUtf8())
                {
                    using (XmlWriter writter = XmlWriter.Create(sww))
                    {
                        oXmlSerializar.Serialize(writter, comprobante, xmlNameSpace);
                        sXml = sww.ToString();
                    }
                }

                var carpeta = $@"Temp\";
                var directorio = $@"{_facturaRutaTimbrado}\{carpeta}";
                var archivo = $@"{_facturaRutaTimbrado}\{carpeta}{comprobante.Emisor.Rfc}_{comprobante.TipoDeComprobante}_{comprobante.Serie}_{comprobante.Folio}.xml";

                if (!string.IsNullOrEmpty(sXml))
                    rutaXmlTemp = archivo;
                else
                    return string.Empty;

                if (!Directory.Exists(directorio))
                {
                    DirectoryInfo di = Directory.CreateDirectory(directorio);
                }

                using (StreamWriter outputFile = new StreamWriter(archivo, false, Encoding.UTF8))
                {
                    await outputFile.WriteAsync(sXml);
                }

                if (File.Exists(archivo))
                    return archivo;
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Convertir Modelo Comprobante a string XML
        /// </summary>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Cadena XML</returns>
        private string ConvertirComprobanteString(Comprobante comprobante)
        {
            try
            {
                var xmlNameSpace = new XmlSerializerNamespaces();
                xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
                xmlNameSpace.Add("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
                xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                if (comprobante.TipoDeComprobante == "P")
                {
                    xmlNameSpace.Add("pago10", "http://www.sat.gob.mx/Pagos");
                }

                XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));

                string sXml = string.Empty;

                using (var sww = new StringWriterUtf8())
                {
                    using (XmlWriter writter = XmlWriter.Create(sww))
                    {
                        oXmlSerializar.Serialize(writter, comprobante, xmlNameSpace);
                        sXml = sww.ToString();
                    }
                }

                return !string.IsNullOrWhiteSpace(sXml) ? sXml : string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtener Cadena Original a partir de XSLT y XML Reader del Comprobante
        /// </summary>
        /// <param name="xmlReaderComprobante">XML Reader del Comprobante</param>
        /// <returns>Cadena original</returns>
        private string ObtenerCadenaOriginal(string rutaXmlTemp)
        {
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlWriter xwo = XmlWriter.Create(sw, _xslCompiledTransform.OutputSettings))
                    {
                        _xslCompiledTransform.Transform(rutaXmlTemp, xwo);
                        return sw.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtener Sello a partir de archivos Clave Privada y Cadena Original
        /// </summary>
        /// <param name="cadenaOriginal">Cadena Original</param>
        /// <returns>Cadena de Sello</returns>
        private string ObtenerSello(string cadenaOriginal)
        {
            try
            {
                return _selloDigital.Sellar(cadenaOriginal, _facturaArchivoKey, _facturaFiel);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtener cadena XML Firmada de un Comprobante
        /// </summary>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Cadena de XML Firmada</returns>
        private string ObtenerFirmaComprobante(Comprobante comprobante)
        {
            try
            {
                var xmlRutaTemp = Task.Run(() => GenerarComprobanteXmlTemp(comprobante)).Result;
                if (string.IsNullOrWhiteSpace(xmlRutaTemp))
                    throw new ArgumentNullException($"GenerarComprobanteXmlTemp, no se pudo Generar XML Temporal");

                var cadenaXmlXslt = ObtenerCadenaOriginal(xmlRutaTemp);
                if (string.IsNullOrWhiteSpace(cadenaXmlXslt))
                    throw new ArgumentNullException($"CadenaXmlXslt,  no se pudo obtener Cadena Original del Comprobante");

                comprobante.Certificado = _facturaCertificado;

                comprobante.Sello = ObtenerSello(cadenaXmlXslt);
                if (string.IsNullOrWhiteSpace(comprobante.Sello))
                    throw new ArgumentNullException($"Sello,  no se pudo obtener Sello del Comprobante, la contraseña del archivo .key es incorrecta");

                if(comprobante.TipoDeComprobante == "P")
                {
                    comprobante.xsiSchemaLocation = "http://www.sat.gob.mx/cfd/3 http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd http://www.sat.gob.mx/Pagos http://www.sat.gob.mx/sitio_internet/cfd/Pagos/Pagos10.xsd";
                }

                var firmadoXml = ConvertirComprobanteString(comprobante);

                if (File.Exists(xmlRutaTemp))
                    File.Delete(xmlRutaTemp);

                return !string.IsNullOrWhiteSpace(firmadoXml) ? firmadoXml : string.Empty;

            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Timbrado XML con PAC
        /// </summary>
        /// <param name="xmlFirmado">Cadena XML</param>
        /// <returns>Modelo GenericResponse</returns>
        private GenericResponse TimbrarXML(string xmlFirmado)
        {
            try
            {
                //string url = "https://edixcfdisecure.ekomercio.com/WSCFDBuilderPlusTurbo/WSCFDBuilderPlus.asmx";
                //string usuario = "CFDPSS1119";
                //string password = "@CFDPSS1119";
                string url = _wsUrlTimbrado;
                string usuario = _wsUsuarioTimbrado;
                string password = _wsContraseñaTimbrado;
                _ws.Url = url;
                var resultado = _ws.getCFDI(usuario, password, xmlFirmado);

                if (resultado.Contains("<Error>"))
                {
                    var document = XDocument.Parse(resultado);
                    var error = document.Descendants("Error").Select(x => x.Element("ErrorMessage").Value).ToList();
                    return new GenericResponse(){ Codigo = 2, Mensaje = error.FirstOrDefault().Replace("'", "''") };
                } else {
                    return new GenericResponse() { Codigo = 1, Data = resultado };
                }
            }
            catch (Exception ex)
            {
                return new GenericResponse() { Codigo = 0, Data = ex.Message };
            }
        }

        /// <summary>
        /// Obtener cadena de Poliza
        /// </summary>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Cadena de Poliza</returns>
        private string ObtenerPoliza(int idComprobante)
        {
            try
            {
                int polizaId = _baseDatos.SelectFirst<int>("SELECT POLIZASID FROM POLIZAS_FACTURACION WHERE COMPROBANTEID = " + $"{idComprobante}");
                string poliza = _baseDatos.SelectFirst<string>("SELECT POLIZA FROM POLIZAS_CONCENTRADO WHERE ID = " + $"{polizaId}");
                return poliza;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        #endregion
    }
}

