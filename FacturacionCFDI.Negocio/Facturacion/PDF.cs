using System;
using System.IO;
using System.Threading.Tasks;
using FacturacionCFDI.Datos.Facturacion.TablasDB;
using FacturacionCFDI.Datos.Response;
using FacturacionCFDI.Negocio.Datos.Facturacion.Factura;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using SelectPdf;
using Utilerias;
using Comprobante = FacturacionCFDI.Datos.Facturacion.Factura.Comprobante;

namespace FacturacionCFDI.Negocio.Facturacion
{
    public class PDF
    {
        private readonly BaseDatos _baseDatos;
        private readonly HtmlToPdf _htmlToPdf;
        private readonly MonedaLetra _monedaLetra;
        private readonly QR _qr;

        private TemplateServiceConfiguration _templateService;
        private IRazorEngineService _razorEngine;

        private PdfDocument _pdfDocument;

        private const string _queryConfiguracionSistema = "SELECT id ,llave ,valor FROM ConfiguracionSistema WHERE llave = ";

        private string _facturaRutaPdfLogo;
        private string _facturaRutaPdfPlantilla;
        private string _facturaRutaTimbrado;
        private string _logo;
        private string _plantilla;
        private string _url = "";

        /// <summary>
        /// Constructor
        /// </summary>
        public PDF(BaseDatos baseDatos)
        {
            _baseDatos = baseDatos;

            _htmlToPdf = new HtmlToPdf();
            _htmlToPdf.Options.PdfPageSize = PdfPageSize.Letter;
            _htmlToPdf.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            _htmlToPdf.Options.WebPageWidth = 1200;
            _htmlToPdf.Options.WebPageHeight = 0;

            _monedaLetra = new MonedaLetra();
            _qr = new QR();

            _templateService = new TemplateServiceConfiguration();
            _templateService.DisableTempFileLocking = true;
            _templateService.CachingProvider = new DefaultCachingProvider(t => { });

            _facturaRutaPdfLogo = ObtenerConfiguracionSistema("FacturaRutaPdfLogo");
            if (string.IsNullOrWhiteSpace(_facturaRutaPdfLogo))
                throw new ArgumentNullException("FacturaRutaPdfLogo, no está definido en Tabla ConfiguracionSistema.");

            _facturaRutaPdfPlantilla = ObtenerConfiguracionSistema("FacturaRutaPdfPantilla");
            if (string.IsNullOrWhiteSpace(_facturaRutaPdfPlantilla))
                throw new ArgumentNullException("FacturaRutaPdfPantilla, no está definido en Tabla ConfiguracionSistema.");

            _facturaRutaTimbrado = ObtenerConfiguracionSistema("FacturaRutaTimbrado");
            if (string.IsNullOrWhiteSpace(_facturaRutaTimbrado))
                throw new ArgumentNullException("FacturaRutaTimbrado, no está definido en Tabla ConfiguracionSistema.");

            _logo = ObtenerStringLogo();
            if (string.IsNullOrWhiteSpace(_logo))
                throw new ArgumentNullException("Logo, no se encontró archivo en la ruta " + _facturaRutaPdfLogo);

            _plantilla = ObtenerStringPlantilla();
            if (string.IsNullOrWhiteSpace(_plantilla))
                throw new ArgumentNullException("Plantilla, no se encontró archivo en la ruta " + _facturaRutaPdfPlantilla);
        }

        /// <summary>
        /// Generar archivo PDF a partir de Modelo Comprobante
        /// </summary>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Modelo GenericResponse</returns>
        public async Task<GenericResponse<RutaArchivo>> GenerarPDF(Comprobante comprobante, string idComprobante)
        {
            try
            {
                if (comprobante != null)
                {
                    var qr = ObtenerQr(comprobante);
                    if (string.IsNullOrWhiteSpace(qr))
                        return new GenericResponse<RutaArchivo>() { Codigo = 2, Mensaje = "No se pudo obtener código QR del comprobante", Descripcion = "No se pudo obtener código QR del comprobante" };

                    var monedaLetra = ObtenerMonedaLetra(comprobante);
                    if (string.IsNullOrWhiteSpace(monedaLetra))
                        return new GenericResponse<RutaArchivo>() { Codigo = 2, Mensaje = "No se pudo obtener moneda en letra", Descripcion = "No se pudo obtener moneda en letra" };

                    //Se complementa información al modelo de Comprobante
                    comprobante.logo = _logo;
                    comprobante.qr = qr;
                    comprobante.monedaLetra = monedaLetra;

                    string poliza = ObtenerPoliza(int.Parse(idComprobante));
                    if (string.IsNullOrWhiteSpace(poliza))
                        return new GenericResponse<RutaArchivo>() { Codigo = 2, Mensaje = "No se pudo obtener la poliza", Descripcion = "No se pudo obtener la poliza" };
                    comprobante.poliza = poliza;

                    if (!string.IsNullOrWhiteSpace(comprobante.FormaPago)){
                        string formaPagoLetra = ObtenerFormaPagoLetra(comprobante.FormaPago);
                        if (string.IsNullOrWhiteSpace(formaPagoLetra))
                            return new GenericResponse<RutaArchivo>() { Codigo = 2, Mensaje = "No se pudo obtener la forma de pago letra", Descripcion = "No se pudo obtener la forma de pago letra" };
                        comprobante.formaPagoLetra = formaPagoLetra;
                    }

                    int vrow = 0;
                    if (comprobante.Pagos != null)
                    {
                        foreach (PagosPago row in comprobante.Pagos.Pago)
                        {
                            string formaPagoLetra = ObtenerFormaPagoLetra(row.FormaDePagoP);
                            if (string.IsNullOrWhiteSpace(formaPagoLetra))
                                return new GenericResponse<RutaArchivo>() { Codigo = 2, Mensaje = "No se pudo obtener la forma de pago letra del comprobante de pago", Descripcion = "No se pudo obtener la forma de pago letra del comprobante de pago" };
                            comprobante.Pagos.Pago[vrow].formaDePagoPLetra = formaPagoLetra;
                            vrow++;
                        }
                    }

                    string pdf = string.Empty;

                    using (_razorEngine = RazorEngineService.Create(_templateService))
                    {
                        pdf = _razorEngine.RunCompile(_plantilla, "plantilla", null, comprobante);
                    }

                    if (!string.IsNullOrEmpty(pdf))
                    {
                        var anioMes = DateTime.Now.ToString("yyyyMM");
                        var carpeta = $@"{poliza}\{anioMes}\";
                        var directorio = $@"{_facturaRutaTimbrado}\{carpeta}";
                        var archivo = $@"{_facturaRutaTimbrado}\{carpeta}{comprobante.Emisor.Rfc}_{comprobante.TipoDeComprobante}_{comprobante.Serie}_{comprobante.Folio}.pdf";
                        var rutaRelativa = $@"{carpeta}{comprobante.Emisor.Rfc}_{comprobante.TipoDeComprobante}_{comprobante.Serie}_{comprobante.Folio}.pdf";

                        if (!Directory.Exists(directorio))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(directorio);
                        }

                        _pdfDocument = _htmlToPdf.ConvertHtmlString(pdf);
                        _pdfDocument.Save(archivo);
                        _pdfDocument.Close();

                        if (File.Exists(archivo))
                            return new GenericResponse<RutaArchivo>() { Data = new RutaArchivo { RutaAbsoluta = archivo, RutaRelativa = rutaRelativa } };
                        else
                            return new GenericResponse<RutaArchivo>() { Codigo = 2, Mensaje = "PDF no Generado", Descripcion = "PDF no Generado" };
                    }
                    else
                        return new GenericResponse<RutaArchivo>() { Codigo = 2, Mensaje = "No se pudo obtener HTML del Comprobante", Descripcion = "No se pudo obtener HTML del Comprobante" };
                }
                else
                {
                    return new GenericResponse<RutaArchivo>() { Codigo = 2, Mensaje = "Módelo Comprobante NULL", Descripcion = "Módelo Comprobante NULL" };
                }
            }
            catch (Exception ex)
            {
                return new GenericResponse<RutaArchivo>() { Codigo = 0, Mensaje = ex.Message, Descripcion = ex.Message };
            }
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
        /// Obtener cadena de Plantilla para PDF
        /// </summary>
        /// <returns>Cadena de plantilla</returns>
        private string ObtenerStringPlantilla()
        {
            return File.Exists(_facturaRutaPdfPlantilla) ? File.ReadAllText(_facturaRutaPdfPlantilla) : string.Empty;
        }

        /// <summary>
        /// Obtener cadena imagen en Base64
        /// </summary>
        /// <returns>Cadena imagen</returns>
        private string ObtenerStringLogo()
        {
            if (!File.Exists(_facturaRutaPdfLogo))
                return string.Empty;

            byte[] imageBytes = File.ReadAllBytes(_facturaRutaPdfLogo);
            return "data:image/png;base64," + Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Obtener cadena Código QR
        /// </summary>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Cadena código QR</returns>
        private string ObtenerQr(Comprobante comprobante)
        {
            try
            {
                var url = "https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?" +
                          "id=" + comprobante.TimbreFiscalDigital.UUID +
                          "&re=" + comprobante.Emisor.Rfc +
                          "&rr=" + comprobante.Receptor.Rfc +
                          "&tt=" + comprobante.Total.ToString() +
                          "&fe=" + comprobante.Sello.Substring(comprobante.Sello.Length - 9, 8);

                return _qr.GenerarQR(url);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtener cadena de Moneda en Letra
        /// </summary>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Cadena de Moneda en Letra</returns>
        private string ObtenerMonedaLetra(Comprobante comprobante)
        {
            try
            {
                if (comprobante.TipoDeComprobante == "P")
                    return _monedaLetra.Convertir(comprobante.Pagos.Pago[0].Monto.ToString("#.00"), true);
                else
                    return _monedaLetra.Convertir(comprobante.Total.ToString("#.00"), true);
            }
            catch (Exception ex)
            {
                return string.Empty;
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

        /// <summary>
        /// Obtener cadena de Forma de Pago
        /// </summary>
        /// <param name="comprobante">Modelo Comprobante</param>
        /// <returns>Cadena de Forma de Pago</returns>
        private string ObtenerFormaPagoLetra(string idFormaPago)
        {
            try
            {
                string formaPagoLetra = _baseDatos.SelectFirst<string>("SELECT DESCRIPCION FROM FACTURACION_CATFORMAPAGO WHERE FORMAPAGO = " + $"'{idFormaPago}'");
                return formaPagoLetra;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
