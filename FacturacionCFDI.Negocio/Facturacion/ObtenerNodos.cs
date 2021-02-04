using FacturacionCFDI.Datos.Facturacion.Factura;
using FacturacionCFDI.Datos.Facturacion.Nodos;
using FacturacionCFDI.Datos.Facturacion.TablasDB;
using FacturacionCFDI.Datos.Response;
using FacturacionCFDI.Negocio.Datos.Facturacion.Factura;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Utilerias;
using Comprobante = FacturacionCFDI.Datos.Facturacion.Factura.Comprobante;

namespace FacturacionCFDI.Negocio.Facturacion
{
    public class ObtenerNodos
    {
        private readonly BaseDatos _baseDatos;

        private const string _queryConfiguracionSistema = "SELECT id ,llave ,valor FROM ConfiguracionSistema WHERE llave = ";
        private const string _queryNodoComprobante = "SELECT c.id AS id ,c.version AS version ,c.serie AS serie ,c.folio AS folio ,c.fecha AS fecha ,cfp.formaPago AS formaPago ,c.subtotal AS subtotal ,c.descuento AS descuento ,cm.moneda AS moneda ,c.tipoCambio AS tipoCambio ,c.total AS total ,ctdc.tipoDeComprobante AS tipoDeComprobante ,cmp.metodoPago AS metodoPago ,c.lugarExpedicion AS lugarExpedicion ,cm.decimales AS decimales FROM facturacion_comprobante c INNER JOIN facturacion_catformapago cfp ON c.formaPagoId = cfp.formapago INNER JOIN facturacion_catmoneda cm ON c.monedaId = cm.id INNER JOIN facturacion_cattipodecomprobante ctdc ON c.tipoDeComprobanteId = ctdc.id INNER JOIN facturacion_catmetodopago cmp ON c.metodoPagoId = cmp.id WHERE c.id = ";
        private const string _queryNodoCfdiRelacionados = "SELECT cp.tipoRelacion AS tipoRelacion ,cr.uuid AS uuid FROM facturacion_cfdirelacionados cr INNER JOIN facturacion_cattiporelacion cp ON cr.tipoRelacionId = cp.id INNER JOIN facturacion_comprobante c ON c.id = cr.comprobanteid WHERE c.id = ";
        private const string _queryNodoEmisor = "SELECT e.rfc AS rfc ,e.nombre AS nombre ,crf.regimenFiscal AS regimenFiscal FROM facturacion_emisor e INNER JOIN facturacion_catregimenfiscal crf ON e.regimenFiscalId = crf.id INNER JOIN facturacion_comprobante c ON c.emisorid = e.id WHERE c.id = ";
        private const string _queryNodoReceptor = "SELECT r.rfc AS rfc ,CASE r.id WHEN 1 THEN c.receptornombregenerico ELSE r.nombre END AS nombre ,cuc.usocfdi AS usoCfdi FROM facturacion_receptor r INNER JOIN facturacion_comprobante c ON c.receptorid = r.id INNER JOIN facturacion_catusocfdi cuc ON c.usocfdiid = cuc.id WHERE c.id = ";
        private const string _queryNodoConcepto = "SELECT c.id AS id ,ccps.claveProdServ AS claveProdServ ,c.noIdentificacion AS noIdentificacion ,c.cantidad AS cantidad ,ccu.claveUnidad AS claveUnidad ,c.descripcion AS descripcion ,c.valorUnitario AS valorUnitario ,c.importe AS importe ,c.descuento AS descuento ,c.aduanaNumeroPedimento AS aduanaNumeroPedimento ,c.cuentaPredialNumero AS cuentaPredialNumero ,cm.decimales AS decimales FROM facturacion_concepto c INNER JOIN facturacion_catclaveprodserv ccps ON c.claveProdServId = ccps.id INNER JOIN facturacion_catclaveunidad ccu ON c.claveUnidadId = ccu.id INNER JOIN facturacion_comprobante co ON co.id = c.comprobanteId INNER JOIN facturacion_catmoneda cm ON cm.id = co.monedaId WHERE co.id = ";
        private const string _queryNodoConceptoImpuestoRetencion = "SELECT i.base AS ibase ,ci.impuesto AS impuesto ,citf.nombre AS tipoFactor ,i.tasaOCuota AS tasaOCuota ,i.importe AS importe ,cm.decimales AS decimales FROM facturacion_impuesto i INNER JOIN facturacion_catimpuesto ci ON i.impuestoId = ci.id INNER JOIN facturacion_catimpuestotipofactor citf ON i.tipoFactorId = citf.id INNER JOIN facturacion_catimpuestotipo cip ON i.impuestoTipoId = cip.id INNER JOIN facturacion_concepto c ON c.id = i.conceptoid INNER JOIN facturacion_comprobante com ON com.id = c.comprobanteid INNER JOIN facturacion_catmoneda cm ON cm.id = com.monedaId WHERE cip.id = 2 AND c.id = ";
        private const string _queryNodoConceptoImpuestoTraslado = "SELECT i.base AS ibase ,ci.impuesto AS impuesto ,citf.nombre AS tipoFactor ,i.tasaOCuota AS tasaOCuota ,i.importe AS importe ,cm.decimales AS decimales FROM facturacion_impuesto i INNER JOIN facturacion_catimpuesto ci ON i.impuestoId = ci.id INNER JOIN facturacion_catimpuestotipofactor citf ON i.tipoFactorId = citf.id INNER JOIN facturacion_catimpuestotipo cip ON i.impuestoTipoId = cip.id INNER JOIN facturacion_concepto c ON c.id = i.conceptoid INNER JOIN facturacion_comprobante com ON com.id = c.comprobanteid INNER JOIN facturacion_catmoneda cm ON cm.id = com.monedaId WHERE cip.id = 1 AND c.id = ";
        private const string _queryNodoConceptoParte = "SELECT cc.claveProdServ AS claveProdServ ,cc.noIdentificacion AS noIdentificacion ,cc.cantidad AS cantidad ,cc.claveUnidad AS claveUnidad ,cc.descripcion AS descripcion ,cc.valorUnitario AS valorUnitario ,cc.importe AS importe ,cc.descuento AS descuento ,cc.aduanaNumeroPedimento AS aduanaNumeroPedimento FROM facturacion_complementoconcepto cc INNER JOIN facturacion_concepto c ON c.id = cc.conceptoId WHERE c.id = ";
        private const string _queryNodoImpuestoRetencion = "SELECT ROUND(SUM(i.importe),2) AS importe ,ci.impuesto AS impuesto ,cm.decimales AS decimales FROM facturacion_impuesto i INNER JOIN facturacion_catimpuesto ci ON i.impuestoId = ci.id INNER JOIN facturacion_catimpuestotipo cip ON i.impuestoTipoId = cip.id INNER JOIN facturacion_concepto con ON con.id = i.conceptoid INNER JOIN facturacion_comprobante com ON com.id = con.comprobanteid INNER JOIN facturacion_catmoneda cm ON cm.id = com.monedaId WHERE cip.id = 2 AND com.id = @idComprobante GROUP BY ci.impuesto ,cm.decimales ";
        private const string _queryNodoImpuestoTraslado = "SELECT ROUND(SUM(i.importe),2) AS importe ,ci.impuesto AS impuesto ,i.tasaOCuota AS tasaOCuota ,citf.nombre AS tipofactor ,cm.decimales As decimales FROM facturacion_impuesto i INNER JOIN facturacion_catimpuesto ci ON i.impuestoId = ci.id INNER JOIN facturacion_catimpuestotipofactor citf ON i.tipoFactorId = citf.id INNER JOIN facturacion_catimpuestotipo cip ON i.impuestoTipoId = cip.id INNER JOIN facturacion_concepto con ON con.id = i.conceptoid INNER JOIN facturacion_comprobante com ON com.id = con.comprobanteid INNER JOIN facturacion_catmoneda cm ON cm.id = com.monedaId WHERE cip.id = 1 AND citf.id IN (1 , 2) AND com.id = @idComprobante GROUP BY ci.impuesto ,i.tasaOCuota ,citf.nombre ,cm.decimales";
        private const string _queryNodoAddenda = "SELECT a.informacion AS informacion FROM facturacion_addenda a INNER JOIN facturacion_comprobante c ON a.comprobanteId = c.id WHERE c.id = ";
        private const string _queryNodoPago = "SELECT p.id AS id ,p.version AS version ,p.fechaPago AS fechaPago ,p.formaDePagoP AS formaDePagoP ,p.monedaP AS monedaP ,p.tipoCambioP AS tipoCambioP ,p.monto AS monto ,p.numOperacion AS numOperacion ,p.rfcEmisorCtaOrd AS rfcEmisorCtaOrd ,p.nomBancoOrdExt AS nomBancoOrdExt ,p.ctaOrdenante AS ctaOrdenante ,p.rfcEmisorCtaBen AS rfcEmisorCtaBen ,p.ctaBeneficiario AS ctaBeneficiario ,cm.decimales AS decimales FROM facturacion_pago p INNER JOIN facturacion_comprobante c ON c.id = p.comprobanteId INNER JOIN facturacion_catmoneda cm ON cm.moneda = p.monedaP WHERE c.id = ";
        private const string _queryNodoPagoDocRelacionado = "SELECT pdr.idDocumento AS idDocumento ,pdr.serie AS serie ,pdr.folio AS folio ,pdr.monedaDr AS monedaDr ,pdr.tipoCambioDr AS tipoCambioDr ,pdr.metodoDePagoDr AS metodoDePagoDr ,pdr.numParcialidad AS numParcialidad ,pdr.impSaldoAnt AS impSaldoAnt ,pdr.impPagado AS impPagado ,pdr.impSaldoInsoluto AS impSaldoInsoluto ,cm.decimales AS decimales FROM facturacion_pagosdoctorelacionado pdr INNER JOIN facturacion_pago p ON p.id = pdr.pagosId INNER JOIN facturacion_catmoneda cm ON cm.moneda = pdr.monedaDr WHERE p.id = ";

        private string _facturaPagoVersion;
        private string _facturaImpuestoDecimales;
        private string _facturaRutaCer;
        private string _facturaNumeroCertificado;
        private string _monedaP;

        public ObtenerNodos(BaseDatos baseDatos)
        {
            _baseDatos = baseDatos;

            _facturaPagoVersion = ObtenerConfiguracionSistema("FacturaPagoVersion");
            if (string.IsNullOrWhiteSpace(_facturaPagoVersion))
                throw new ArgumentNullException("FacturaPagoVersion, no está definido en Tabla ConfiguracionSistema.");

            _facturaImpuestoDecimales = ObtenerConfiguracionSistema("FacturaImpuestoDecimales");
            if (string.IsNullOrWhiteSpace(_facturaImpuestoDecimales))
                throw new ArgumentNullException("FacturaImpuestoDecimales, no está definido en Tabla ConfiguracionSistema.");

            _facturaRutaCer = ObtenerConfiguracionSistema("FacturaRutaCer");
            if (string.IsNullOrWhiteSpace(_facturaRutaCer))
                throw new ArgumentNullException("FacturaRutaCer, no está definido en Tabla ConfiguracionSistema.");

            _facturaNumeroCertificado = ObtenerNumeroCertificado();
            if (string.IsNullOrWhiteSpace(_facturaNumeroCertificado))
                throw new ArgumentNullException("FacturaNumeroCertificado, no está definido.");
        }


        #region Métodos para Nodo Comprobante
        /// <summary>
        /// Obtener información para Nodo CFDI Relacionados
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns>Modelo ComprobanteCfdiRelacionados</returns>
        public async Task<ComprobanteCfdiRelacionados> ObtenerCfdiRelacionados(string idComprobante)
        {
            ComprobanteCfdiRelacionados respuesta = new ComprobanteCfdiRelacionados();

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoCfdiRelacionados>(_queryNodoCfdiRelacionados + idComprobante);

                if ((resultado?.Any() ?? false))
                {
                    respuesta.TipoRelacion = resultado.FirstOrDefault().tipoRelacion;

                    var uuids = resultado.Select(x => new ComprobanteCfdiRelacionadosCfdiRelacionado() { UUID = x.uuid }).ToList();

                    respuesta.CfdiRelacionado = uuids.ToArray();
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información para Nodo Emisor
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns>Modelo ComprobanteEmisor</returns>
        public async Task<ComprobanteEmisor> ObtenerNodoEmisor(string idComprobante)
        {
            ComprobanteEmisor respuesta = new ComprobanteEmisor();

            try
            {
                var resultado = await _baseDatos.SelectFirstAsync<NodoEmisor>(_queryNodoEmisor + idComprobante);

                if (resultado != null && !string.IsNullOrWhiteSpace(resultado.rfc))
                {
                    respuesta.Nombre = resultado.nombre;
                    respuesta.RegimenFiscal = resultado.regimenFiscal;
                    respuesta.Rfc = resultado.rfc;
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }
            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Receptor
        /// </summary>
        /// <param name="idComprobante">ID Comporbante</param>
        /// <returns>Modelo ComprobanteReceptor</returns>
        public async Task<ComprobanteReceptor> ObtenerNodoReceptor(string idComprobante)
        {
            ComprobanteReceptor respuesta = new ComprobanteReceptor();

            try
            {
                var resultado = await _baseDatos.SelectFirstAsync<NodoReceptor>(_queryNodoReceptor + idComprobante);

                if (resultado != null && resultado.rfc != null)
                {
                    respuesta.Rfc = resultado.rfc;
                    respuesta.Nombre = resultado.nombre;
                    respuesta.UsoCFDI = resultado.usoCfdi;
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }
            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Concepto
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns>Lista de modelo ComprobanteConcepto</returns>
        public async Task<List<ComprobanteConcepto>> ObtenerNodoConcepto(string idComprobante)
        {
            List<ComprobanteConcepto> respuesta = new List<ComprobanteConcepto>();

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoConcepto>(_queryNodoConcepto + idComprobante);

                if ((resultado?.Any() ?? false))
                {
                    var parte = new List<ComprobanteConceptoParte>();

                    foreach (var dato in resultado)
                    {
                        var concepto = new ComprobanteConcepto();
                        concepto.ClaveProdServ = dato.claveProdServ;
                        concepto.NoIdentificacion = !string.IsNullOrEmpty(dato.noIdentificacion) ? dato.noIdentificacion : null;
                        concepto.Cantidad = TruncarDecimal(dato.cantidad, dato.decimales, false);
                        concepto.ClaveUnidad = dato.claveUnidad;
                        concepto.Descripcion = dato.descripcion;
                        concepto.ValorUnitario = TruncarDecimal(dato.valorUnitario, dato.decimales, true);
                        concepto.Importe = TruncarDecimal(dato.importe, dato.decimales, true);
                        concepto.Descuento = TruncarDecimal(Convert.ToDecimal(dato.descuento), dato.decimales, true);
                        concepto.DescuentoSpecified = Convert.ToDecimal(dato.descuento) > 0 ? true : false;
                        concepto.Impuestos = await ObtenerNodoConceptoImpuesto(dato.id.ToString());
                        concepto.InformacionAduanera = !string.IsNullOrEmpty(dato.aduanaNumeroPedimento) ? ObtenerNodoConceptoInformacionAduanera(dato.aduanaNumeroPedimento).ToArray() : null;
                        concepto.Parte = (parte = await ObtenerNodoConceptoParte(dato.id.ToString())) != null ? parte.ToArray() : null;

                        respuesta.Add(concepto);
                    }
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Impuestos por Concepto
        /// </summary>
        /// <param name="idConcepto">ID Concepto</param>
        /// <returns>Modelo ComprobanteConceptoImpuestos</returns>
        public async Task<ComprobanteConceptoImpuestos> ObtenerNodoConceptoImpuesto(string idConcepto)
        {
            ComprobanteConceptoImpuestos respuesta = new ComprobanteConceptoImpuestos();

            try
            {
                var impuestoTraslado = await ObtenerNodoConceptoImpuestoTraslado(idConcepto);
                var impuestoRetencion = await ObtenerNodoConceptoImpuestoRetencion(idConcepto);

                if ((impuestoTraslado?.Any() ?? false) || (impuestoRetencion?.Any() ?? false))
                {
                    if ((impuestoTraslado?.Any() ?? false))
                        respuesta.Traslados = impuestoTraslado.ToArray();

                    if ((impuestoRetencion?.Any() ?? false))
                        respuesta.Retenciones = impuestoRetencion.ToArray();
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Impuesto Retención por Concepto
        /// </summary>
        /// <param name="idConcepto">ID Concepto</param>
        /// <returns>Lista modelo ComprobanteConceptoImpuestosRetencion</returns>
        public async Task<List<ComprobanteConceptoImpuestosRetencion>> ObtenerNodoConceptoImpuestoRetencion(string idConcepto)
        {
            List<ComprobanteConceptoImpuestosRetencion> respuesta = new List<ComprobanteConceptoImpuestosRetencion>();

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoImpuestoConcepto>(_queryNodoConceptoImpuestoRetencion + idConcepto);

                if ((resultado?.Any() ?? false))
                {
                    respuesta = resultado.Select(x => new ComprobanteConceptoImpuestosRetencion()
                    {
                        Base = TruncarDecimal(x.ibase, x.decimales, true),
                        Importe = x.importe,
                        Impuesto = x.impuesto,
                        TasaOCuota = x.tasaOCuota,
                        TipoFactor = x.tipoFactor
                    }).ToList();
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Impuesto Traslado por Concepto
        /// </summary>
        /// <param name="idConcepto">ID Concepto</param>
        /// <returns>Lista modelo ComprobanteConceptoImpuestosTraslado</returns>
        public async Task<List<ComprobanteConceptoImpuestosTraslado>> ObtenerNodoConceptoImpuestoTraslado(string idConcepto)
        {
            List<ComprobanteConceptoImpuestosTraslado> respuesta = new List<ComprobanteConceptoImpuestosTraslado>();

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoImpuestoConcepto>(_queryNodoConceptoImpuestoTraslado + idConcepto);

                if ((resultado?.Any() ?? false))
                {
                    respuesta = resultado.Select(x => new ComprobanteConceptoImpuestosTraslado()
                    {
                        Base = TruncarDecimal(x.ibase, x.decimales, true),
                        Importe = TruncarDecimal(x.importe, Convert.ToInt32(_facturaImpuestoDecimales), true),
                        ImporteSpecified = x.importe >= 0 ? true : false,
                        Impuesto = x.impuesto,
                        TasaOCuota = TruncarDecimal(x.tasaOCuota, 6, true),
                        TasaOCuotaSpecified = x.tasaOCuota > 0 ? true : false,
                        TipoFactor = x.tipoFactor,
                    }).ToList();
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener informacion Nodo Información Aduanera por Concepto
        /// </summary>
        /// <param name="numeroPedimento">Número de Pedimento</param>
        /// <returns>Lista modelo ComprobanteConceptoInformacionAduanera</returns>
        public List<ComprobanteConceptoInformacionAduanera> ObtenerNodoConceptoInformacionAduanera(string numeroPedimento)
        {
            List<ComprobanteConceptoInformacionAduanera> respuesta = new List<ComprobanteConceptoInformacionAduanera>();

            try
            {
                if (!string.IsNullOrEmpty(numeroPedimento))
                {
                    ComprobanteConceptoInformacionAduanera pedimento = new ComprobanteConceptoInformacionAduanera();
                    pedimento.NumeroPedimento = numeroPedimento;
                    respuesta.Add(pedimento);
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Parte por Concepto
        /// </summary>
        /// <param name="idConcepto">ID Concepto</param>
        /// <returns>Lista modelo ComprobanteConceptoParte</returns>
        public async Task<List<ComprobanteConceptoParte>> ObtenerNodoConceptoParte(string idConcepto)
        {
            List<ComprobanteConceptoParte> respuesta = new List<ComprobanteConceptoParte>();

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoConceptoParte>(_queryNodoConceptoParte + idConcepto);

                if ((resultado?.Any() ?? false))
                {
                    var informacionAduanera = new List<ComprobanteConceptoParteInformacionAduanera>();

                    respuesta = resultado.Select(x => new ComprobanteConceptoParte()
                    {
                        ClaveProdServ = x.claveProdServ,
                        NoIdentificacion = x.noIdentificacion,
                        Cantidad = x.cantidad,
                        Descripcion = x.descripcion,
                        ValorUnitario = x.valorUnitario,
                        Importe = x.importe,
                        InformacionAduanera = (informacionAduanera = ObtenerNodoConceptoParteInformacionAduanera(x.aduanaNumeroPedimento)) != null ? informacionAduanera.ToArray() : null
                    }).ToList();
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Información Aduanera por Parte
        /// </summary>
        /// <param name="numeroPedimento">Número de Pedimento</param>
        /// <returns>Lista modelo ComprobanteConceptoParteInformacionAduanera</returns>
        public List<ComprobanteConceptoParteInformacionAduanera> ObtenerNodoConceptoParteInformacionAduanera(string numeroPedimento)
        {
            List<ComprobanteConceptoParteInformacionAduanera> respuesta = new List<ComprobanteConceptoParteInformacionAduanera>();

            try
            {
                if (!string.IsNullOrEmpty(numeroPedimento))
                {
                    ComprobanteConceptoParteInformacionAduanera pedimento = new ComprobanteConceptoParteInformacionAduanera();
                    pedimento.NumeroPedimento = numeroPedimento;
                    respuesta.Add(pedimento);
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Impuestos
        /// </summary>
        /// <param name="idComprobante"></param>
        /// <returns></returns>
        public async Task<ComprobanteImpuestos> ObtenerNodoImpuestos(string idComprobante)
        {
            ComprobanteImpuestos respuesta = new ComprobanteImpuestos();

            try
            {
                var impuestoR = await ObtenerNodoImpuestoRetencion(idComprobante);
                var impuestoT = await ObtenerNodoImpuestoTraslado(idComprobante);
                if ((impuestoR?.Any() ?? false) || (impuestoT?.Any() ?? false))
                {
                    if ((impuestoR?.Any() ?? false))
                    {
                        respuesta.TotalImpuestosRetenidos = impuestoR.Sum(x => x.Importe);
                        respuesta.TotalImpuestosRetenidosSpecified = true;
                        respuesta.Retenciones = impuestoR.ToArray();
                    }


                    if ((impuestoT?.Any() ?? false))
                    {
                        respuesta.TotalImpuestosTrasladados = impuestoT.Sum(x => x.Importe);
                        respuesta.TotalImpuestosTrasladadosSpecified = true;
                        respuesta.Traslados = impuestoT.ToArray();
                    }
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Impuestos de Retención
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns>Lista modelo ComprobanteImpuestosRetencion</returns>
        public async Task<List<ComprobanteImpuestosRetencion>> ObtenerNodoImpuestoRetencion(string idComprobante)
        {
            List<ComprobanteImpuestosRetencion> respuesta = new List<ComprobanteImpuestosRetencion>();

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoImpuestoRetencion>(_queryNodoImpuestoRetencion.Replace("@idComprobante", idComprobante));

                if ((resultado?.Any() ?? false))
                {
                    respuesta = resultado.Select(x => new ComprobanteImpuestosRetencion()
                    {
                        Importe = TruncarDecimal(x.importe, x.decimales, true),
                        Impuesto = x.impuesto
                    }).ToList();
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Impuestos de Traslado
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns>Lista modelo ComprobanteImpuestosTraslado</returns>
        public async Task<List<ComprobanteImpuestosTraslado>> ObtenerNodoImpuestoTraslado(string idComprobante)
        {
            List<ComprobanteImpuestosTraslado> respuesta = new List<ComprobanteImpuestosTraslado>();

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoImpuestoTraslado>(_queryNodoImpuestoTraslado.Replace("@idComprobante", idComprobante));

                if ((resultado?.Any() ?? false))
                {
                    respuesta = resultado.Select(x => new ComprobanteImpuestosTraslado()
                    {
                        Importe = TruncarDecimal(x.importe, x.decimales, true),
                        Impuesto = x.impuesto,
                        TasaOCuota = TruncarDecimal(x.tasaOCuota, 6, true),
                        TipoFactor = x.tipofactor
                    }).ToList();
                }
                else
                {
                    respuesta = null;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Addenda
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns>Modelo ComprobanteAddenda</returns>
        public async Task<ComprobanteAddenda> ObtenerNodoAddenda(string idComprobante)
        {
            ComprobanteAddenda respuesta = new ComprobanteAddenda();

            string datoAddenda = "";

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoAddenda>(_queryNodoAddenda + idComprobante);

                if ((resultado?.Any() ?? false))
                {
                    XmlDocument addenda = new XmlDocument();
                    XmlSerializerNamespaces nameSpaceAddenda = new XmlSerializerNamespaces();
                    nameSpaceAddenda.Add("cfdi:Addenda", "http://www.sat.gob.mx/cfd/3");
                    XmlElement infoAddenda = addenda.CreateElement("cfdi:InfoAdicional", "http://www.sat.gob.mx/cfd/3");

                    foreach (var dato in resultado)
                    {
                        datoAddenda += dato.informacion + "|";
                    }

                    infoAddenda.SetAttribute("InfoAdenda", datoAddenda);

                    using (XmlWriter write = addenda.CreateNavigator().AppendChild())
                    {
                        new XmlSerializer(infoAddenda.GetType()).Serialize(write, infoAddenda, nameSpaceAddenda);
                    }

                    respuesta.Any = new XmlElement[1];
                    respuesta.Any[0] = addenda.DocumentElement;
                }
                else
                {
                    respuesta = null;
                }
            }
            catch
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Pagos
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns>Modelo Pagos</returns>
        public async Task<Pagos> ObtenerNodoPagos(string idComprobante)
        {
            Pagos respuesta = new Pagos();

            try
            {
                var resultado = await ObtenerNodoPagosPago(idComprobante);

                if ((resultado?.Any() ?? false))
                {
                    respuesta.Version = _facturaPagoVersion;
                    respuesta.Pago = resultado.ToArray();
                }
                else
                {
                    respuesta = null;
                }
            }
            catch
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Pagos Pago
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns>Lista modelo PagosPago</returns>
        public async Task<List<PagosPago>> ObtenerNodoPagosPago(string idComprobante)
        {
            List<PagosPago> respuesta = new List<PagosPago>();

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoPago>(_queryNodoPago + idComprobante);

                if ((resultado?.Any() ?? false))
                {
                    foreach (var dato in resultado)
                    {
                        var pago = new PagosPago();

                        pago.FechaPago = dato.fechaPago.ToString("yyyy-MM-ddTHH:mm:ss");
                        pago.FormaDePagoP = String.Format("{0:00}", Int32.Parse(dato.formaDePagoP));
                        pago.MonedaP = dato.monedaP;
                        if (dato.monedaP != "MXN")
                        {
                            pago.TipoCambioP = Convert.ToDecimal(dato.tipoCambioP);
                            pago.TipoCambioPSpecified = dato.tipoCambioP > 0 ? true : false;
                        }
                        pago.Monto = TruncarDecimal(dato.monto, dato.decimales, true);
                        pago.NumOperacion = !string.IsNullOrEmpty(dato.numOperacion) ? dato.numOperacion : null;
                        pago.RfcEmisorCtaOrd = !string.IsNullOrEmpty(dato.rfcEmisorCtaOrd) ? dato.rfcEmisorCtaOrd : null;
                        pago.NomBancoOrdExt = !string.IsNullOrEmpty(dato.nomBancoOrdExt) ? dato.nomBancoOrdExt : null;
                        pago.CtaOrdenante = !string.IsNullOrEmpty(dato.ctaOrdenante) ? dato.ctaOrdenante : null;
                        pago.RfcEmisorCtaBen = !string.IsNullOrEmpty(dato.rfcEmisorCtaBen) ? dato.rfcEmisorCtaBen : null;
                        pago.CtaBeneficiario = !string.IsNullOrEmpty(dato.ctaBeneficiario) ? dato.ctaBeneficiario : null;

                        var pagoRelacionado = await ObtenerNodoPagosPagoDocRelacionado(dato.id.ToString(), dato.monedaP);
                        pago.DoctoRelacionado = pagoRelacionado != null ? pagoRelacionado.ToArray() : null;

                        respuesta.Add(pago);
                    }
                }
                else
                {
                    respuesta = null;
                }
            }
            catch
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Pagos Pago DoctoRelacionado
        /// </summary>
        /// <param name="idPago">ID Pago</param>
        /// <returns>Lista modelo PagosPagoDoctoRelacionado</returns>
        public async Task<List<PagosPagoDoctoRelacionado>> ObtenerNodoPagosPagoDocRelacionado(string idPago, string MonedaP)
        {
            List<PagosPagoDoctoRelacionado> respuesta = new List<PagosPagoDoctoRelacionado>();

            try
            {
                var resultado = await _baseDatos.SelectAsync<NodoPagoDocRelacionado>(_queryNodoPagoDocRelacionado + idPago);

                if ((resultado?.Any() ?? false))
                {
                    respuesta = resultado.Select(x => new PagosPagoDoctoRelacionado()
                    {
                        IdDocumento = x.idDocumento,
                        Serie = !string.IsNullOrWhiteSpace(x.serie) ? x.serie : null,
                        Folio = !string.IsNullOrWhiteSpace(x.folio) ? x.folio : null,
                        MonedaDR = x.monedaDr,
                        TipoCambioDR = (MonedaP != x.monedaDr) ? TruncarDecimal(x.tipoCambioDr, x.decimales, false) : 0,
                        TipoCambioDRSpecified = (MonedaP != x.monedaDr) ? x.tipoCambioDr > 0 ? true : false : false,
                        MetodoDePagoDR = x.metodoDePagoDr,
                        NumParcialidad = x.numParcialidad,
                        ImpSaldoAnt = TruncarDecimal(x.impSaldoAnt, x.decimales, true),
                        ImpPagadoSpecified = x.impSaldoAnt > 0 ? true : false,
                        ImpPagado = TruncarDecimal(x.impPagado, x.decimales, true),
                        ImpSaldoAntSpecified = x.impPagado > 0 ? true : false,
                        ImpSaldoInsoluto = TruncarDecimal(x.impSaldoInsoluto, x.decimales, true),
                        ImpSaldoInsolutoSpecified = x.impSaldoInsoluto >= 0 ? true : false
                    }).ToList();
                }
                else
                {
                    respuesta = null;
                }
            }
            catch
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// Obtener información Nodo Comporbante
        /// </summary>
        /// <param name="idComprobante">ID Comprobante</param>
        /// <returns></returns>
        public async Task<GenericResponse<Comprobante>> ObtenerNodoComprobante(string idComprobante)
        {
            var comprobante = new Comprobante();

            try
            {
                var resultado = await _baseDatos.SelectFirstAsync<NodoComprobante>(_queryNodoComprobante + idComprobante);

                DateTime datelimit = new DateTime(2021, 01, 31, 23, 59, 00);
                if (resultado != null)
                {
                    if (resultado.tipoDeComprobante != "P")
                    {
                        comprobante.Version = resultado.version;
                        comprobante.Serie = resultado.serie;
                        comprobante.Folio = resultado.folio;
                        comprobante.Fecha = datelimit.ToString("yyyy-MM-ddTHH:mm:ss");
                        comprobante.NoCertificado = _facturaNumeroCertificado;
                        comprobante.FormaPago = resultado.formaPago;
                        comprobante.FormaPagoSpecified = !string.IsNullOrEmpty(resultado.formaPago) ? true : false;
                        comprobante.SubTotal = TruncarDecimal(resultado.subtotal, resultado.decimales, true);
                        comprobante.Descuento = TruncarDecimal(resultado.descuento, resultado.decimales, true);
                        comprobante.DescuentoSpecified = resultado.descuento > 0 ? true : false;
                        comprobante.Moneda = resultado.moneda;
                        comprobante.TipoCambio = TruncarDecimal(resultado.tipoCambio, resultado.decimales, false);
                        comprobante.TipoCambioSpecified = resultado.tipoCambio > 0 ? true : false;
                        comprobante.Total = TruncarDecimal(resultado.total, resultado.decimales, true);
                        comprobante.TipoDeComprobante = resultado.tipoDeComprobante;
                        comprobante.MetodoPago = resultado.metodoPago;
                        comprobante.MetodoPagoSpecified = !string.IsNullOrEmpty(resultado.metodoPago) ? true : false;
                        comprobante.LugarExpedicion = resultado.lugarExpedicion;

                        //Cfdi Relacionados
                        var cfdiRelacionado = await ObtenerCfdiRelacionados(resultado.id.ToString());
                        comprobante.CfdiRelacionados = cfdiRelacionado != null ? cfdiRelacionado : null;

                        //Emisor
                        var emisor = await ObtenerNodoEmisor(resultado.id.ToString());
                        if (emisor != null)
                            comprobante.Emisor = emisor;
                        else
                            return new GenericResponse<Comprobante>() { Codigo = 2, Mensaje = $"NodoEmisor, no se encontró información del Comprobante {resultado.id}" };

                        //Receptor
                        var receptor = await ObtenerNodoReceptor(resultado.id.ToString());
                        if (receptor != null)
                            comprobante.Receptor = receptor;
                        else
                            return new GenericResponse<Comprobante>() { Codigo = 2, Mensaje = $"NodoReceptor, no se encontró información del Comprobante {resultado.id}" };

                        //Concepto
                        var conceptos = (await ObtenerNodoConcepto(resultado.id.ToString())).ToArray();
                        if (conceptos != null)
                            comprobante.Conceptos = conceptos;
                        else
                            return new GenericResponse<Comprobante>() { Codigo = 2, Mensaje = $"NodoConceptos, no se encontró información del Comprobante {resultado.id}" };

                        //Impuestos
                        var impuestos = await ObtenerNodoImpuestos(resultado.id.ToString());
                        if (impuestos != null)
                            comprobante.Impuestos = impuestos;
                        else
                            return new GenericResponse<Comprobante>() { Codigo = 2, Mensaje = $"NodoImpuestos, no se encontró información del Comprobante {resultado.id}" };

                        //Addenda
                        var addenda = await ObtenerNodoAddenda(resultado.id.ToString());
                        comprobante.Addenda = addenda != null ? addenda : null;
                    }
                    else if (resultado.tipoDeComprobante == "P")
                    {
                        comprobante.Version = resultado.version;
                        comprobante.Serie = resultado.serie;
                        comprobante.Folio = resultado.folio;
                        comprobante.Fecha = datelimit.ToString("yyyy-MM-ddTHH:mm:ss");
                        comprobante.NoCertificado = _facturaNumeroCertificado;
                        comprobante.SubTotal = TruncarDecimal(resultado.subtotal, resultado.decimales, true);
                        comprobante.Descuento = TruncarDecimal(resultado.descuento, resultado.decimales, true);
                        comprobante.DescuentoSpecified = resultado.descuento > 0 ? true : false;
                        comprobante.Moneda = resultado.moneda;
                        comprobante.TipoCambio = TruncarDecimal(resultado.tipoCambio, resultado.decimales, false);
                        comprobante.TipoCambioSpecified = resultado.tipoCambio > 0 ? true : false;
                        comprobante.Total = TruncarDecimal(resultado.total, resultado.decimales, true);
                        comprobante.TipoDeComprobante = resultado.tipoDeComprobante;
                        comprobante.LugarExpedicion = resultado.lugarExpedicion;

                        //Cfdi Relacionados
                        var cfdiRelacionado = await ObtenerCfdiRelacionados(resultado.id.ToString());
                        comprobante.CfdiRelacionados = cfdiRelacionado != null ? cfdiRelacionado : null;

                        //Emisor
                        var emisor = await ObtenerNodoEmisor(resultado.id.ToString());
                        if (emisor != null)
                            comprobante.Emisor = emisor;
                        else
                            return new GenericResponse<Comprobante>() { Codigo = 2, Mensaje = $"NodoEmisor, no se encontró información del Comprobante {resultado.id}" };

                        //Receptor
                        var receptor = await ObtenerNodoReceptor(resultado.id.ToString());
                        if (receptor != null)
                            comprobante.Receptor = receptor;
                        else
                            return new GenericResponse<Comprobante>() { Codigo = 2, Mensaje = $"NodoReceptor, no se encontró información del Comprobante {resultado.id}" };

                        //Concepto
                        var conceptos = (await ObtenerNodoConcepto(resultado.id.ToString())).ToArray();
                        if (conceptos != null)
                            comprobante.Conceptos = conceptos;
                        else
                            return new GenericResponse<Comprobante>() { Codigo = 2, Mensaje = $"NodoConceptos, no se encontró información del Comprobante {resultado.id}" };

                        //Addenda
                        var addenda = await ObtenerNodoAddenda(resultado.id.ToString());
                        comprobante.Addenda = addenda != null ? addenda : null;

                        comprobante.Complemento = new ComprobanteComplemento[1];
                        comprobante.Complemento[0] = new ComprobanteComplemento();

                        var pago = await ObtenerNodoPagos(resultado.id.ToString());

                        if (pago != null)
                        {
                            XmlDocument nodoPago = new XmlDocument();
                            XmlSerializerNamespaces xmlNameSpacePago = new XmlSerializerNamespaces();
                            xmlNameSpacePago.Add("pago10", "http://www.sat.gob.mx/Pagos");
                            using (XmlWriter write = nodoPago.CreateNavigator().AppendChild())
                            {
                                new XmlSerializer(pago.GetType()).Serialize(write, pago, xmlNameSpacePago);
                            }

                            comprobante.Complemento[0].Any = new XmlElement[1];
                            comprobante.Complemento[0].Any[0] = nodoPago.DocumentElement;
                        }
                        else
                        {
                            return new GenericResponse<Comprobante>() { Codigo = 2, Mensaje = $"NodoPagos, no se encontró información del Comprobante {resultado.id}" };
                        }
                    }


                    return new GenericResponse<Comprobante>() { Data = comprobante };
                }
                else
                {
                    return new GenericResponse<Comprobante>() { Codigo = 2, Mensaje = $"NodoComprobante, no se encontró información del Comprobante {resultado.id}" };
                }
            }
            catch (Exception ex)
            {
                return new GenericResponse<Comprobante>() { Codigo = 0, Mensaje = $"NodoComprobante, ObtenerNodoComprobante({idComprobante}). Excepción: {ex.Message}" };
            }
        }
        #endregion

        #region Métodos Privados
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
        /// Truncado de Decimales
        /// </summary>
        /// <param name="value">Valor</param>
        /// <param name="precision">Número de decimales</param>
        /// <param name="preciso">Recortar cantidad de decimales del valor inicial</param>
        /// <returns>Decimal truncado</returns>
        private decimal TruncarDecimal(decimal value, int precision, bool preciso = false)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            decimal truncado = tmp / step;
            if (preciso)
            {
                string struncado = truncado.ToString("N" + precision.ToString());
                truncado = Convert.ToDecimal(struncado);
            }
            return truncado;
        }

        /// <summary>
        /// Obtener número de certificado del Certificado Emisor
        /// </summary>
        /// <returns>Cadena con número de certificado</returns>
        private string ObtenerNumeroCertificado()
        {
            string respuesta, aa, b, c;

            try
            {
                if (!SelloDigital.LeerCER(_facturaRutaCer, out aa, out b, out c, out respuesta))
                {
                    respuesta = string.Empty;
                }
            }
            catch
            {
                respuesta = string.Empty;
            }

            return respuesta;
        }
        #endregion
    }
}
