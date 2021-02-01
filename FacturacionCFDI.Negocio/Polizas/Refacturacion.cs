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
using FacturacionCFDI.Negocio.Facturacion;
using System.Text.RegularExpressions;

namespace FacturacionCFDI.Negocio.Polizas
{
    public class Refacturacion
    {
        private readonly BaseDatos _baseDatos;
        private readonly FacturacionCFDI.Negocio.Facturacion.Procesador procesador;

        private const string QUERY_FACTURACION_EMISORID = "SELECT ID FROM FACTURACION_EMISOR WHERE ID = 1";
        private const string QUERY_FACTURACION_RECEPTORID = "SELECT NVL(MAX(ID) + 1, 1) FROM FACTURACION_RECEPTOR";
        private const string QUERY_FACTURACION_COMPROBANTEID = "SELECT NVL(MAX(ID) + 1, 1) FROM FACTURACION_COMPROBANTE";
        private const string QUERY_FACTURACION_CFDIRELACIONADOSID = "SELECT NVL(MAX(ID) + 1, 1) FROM FACTURACION_CFDIRELACIONADOS";
        private const string QUERY_FACTURACION_CONCEPTOID = "SELECT NVL(MAX(ID) + 1, 1) FROM FACTURACION_CONCEPTO";
        private const string QUERY_FACTURACION_IMPUESTOID = "SELECT NVL(MAX(ID) + 1, 1) FROM FACTURACION_IMPUESTO";
        private const string QUERY_FACTURACION_PAGOID = "SELECT NVL(MAX(ID) + 1, 1) FROM FACTURACION_PAGO";
        private const string QUERY_FACTURACION_PAGOSDOCTORELACIONADOID = "SELECT NVL(MAX(ID) + 1, 1) FROM FACTURACION_PAGOSDOCTORELACIONADO";
        private const string QUERY_FACTURACION_MONEDAID = "SELECT CAST (VALOR AS INTEGER) FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaMonedaId'";
        private const string QUERY_FACTURACION_MONEDAIDGENERICO = "SELECT CAST (VALOR AS INTEGER) FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaMonedaXXXId'";
        private const string QUERY_FACTURACION_METODOPAGOID = "SELECT CAST (VALOR AS INTEGER) FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaMetodoPagoId'";
        private const string QUERY_FACTURACION_USOCFDIIDFISICA = "SELECT CAST (VALOR AS INTEGER) FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaUsoCfdiIdFisica'";
        private const string QUERY_FACTURACION_USOCFDIIDMORAL = "SELECT CAST (VALOR AS INTEGER) FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaUsoCfdiIdMoral'";
        private const string QUERY_FACTURACION_CLAVEPRODSERVID = "SELECT CAST (VALOR AS INTEGER) FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaClaveProdServId'";
        private const string QUERY_FACTURACION_CLAVEUNIDADID = "SELECT CAST (VALOR AS INTEGER) FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaClaveUnidadId'";
        private const string QUERY_FACTURACION_CONCEPTOPRIMA34 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoPrima34'";
        private const string QUERY_FACTURACION_CONCEPTOPRIMA36 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoPrima36'";
        private const string QUERY_FACTURACION_CONCEPTOPRIMA37 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoPrima37'";
        private const string QUERY_FACTURACION_CONCEPTOPRIMA39 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoPrima39'";
        private const string QUERY_FACTURACION_CONCEPTOGASTO34 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoGasto34'";
        private const string QUERY_FACTURACION_CONCEPTOGASTO36 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoGasto36'";
        private const string QUERY_FACTURACION_CONCEPTOGASTO37 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoGasto37'";
        private const string QUERY_FACTURACION_CONCEPTOGASTO39 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoGasto39'";
        private const string QUERY_FACTURACION_CONCEPTOFINANCIAMIENTO34 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoFinanciamiento34'";
        private const string QUERY_FACTURACION_CONCEPTOFINANCIAMIENTO36 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoFinanciamiento36'";
        private const string QUERY_FACTURACION_CONCEPTOFINANCIAMIENTO37 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoFinanciamiento37'";
        private const string QUERY_FACTURACION_CONCEPTOFINANCIAMIENTO39 = "SELECT VALOR FROM  CONFIGURACIONSISTEMA WHERE LLAVE = 'PolizaConceptoFinanciamiento39'";
        private const string QUERY_FACTURACION_RECEPTOR = "SELECT RFCRECEPTOR AS Rfc ,NOMBRERECEPTOR AS Nombre FROM POLIZAS_FACTURACION WHERE ESTATUSFACTURACIONID = 9 AND NOT EXISTS (SELECT RFC FROM FACTURACION_RECEPTOR WHERE RFC = RFCRECEPTOR) GROUP BY RFCRECEPTOR ,NOMBRERECEPTOR ORDER BY RFCRECEPTOR";
        private const string QUERY_FACTURACION_RELACIONFACTURAS = "SELECT pf.id AS polizafacturacionid, pf.serie AS serie, pf.folio AS folio, pf.fechacomprobante AS fecha, fcfp.formapago AS formapagoid, round(pf.primaneta + pf.financiamiento + pf.gasto, 2) AS subtotal, CASE pf.tipocomprobante WHEN 'P' THEN 0 ELSE 1 END AS tipocambio, round((pf.primaneta + pf.financiamiento + pf.gasto) * 1.16, 2) AS total, fctc.id AS tipocomprobanteid, pf.lugarexpedicion AS lugarexpedicion, fr.id AS rfcreceptorid, fr.rfc AS rfcreceptor, CASE fr.id WHEN 1 THEN pfm.nombrereceptor ELSE NULL END AS nombrereceptor, pf.codigoconcepto AS codigoconcepto, pf.codigoproducto AS conceptonoidentificacion, 1 AS conceptocantidad, round(pf.primaneta, 2) AS conceptoprimamonto, round(pf.primaneta * 0.16, 4) AS conceptoprimaiva, round(pf.financiamiento, 2) AS conceptofinanciamientomonto, round(pf.financiamiento * 0.16, 4) AS conceptofinanciamientoiva, round(pf.gasto, 2) AS conceptogastomonto, round(pf.gasto * 0.16, 4) AS conceptogastoiva, CASE pf.polizamadre WHEN 1 THEN NULL ELSE fc.facturauuid END AS cfdirelacionado, pf.fechapago AS pagofechapago, lpad(pf.formapago, 2, '0') AS pagoformapago, pf.totalpagado AS pagomonto, CASE pf.polizamadre WHEN 1 THEN NULL ELSE fc.facturauuid END AS pagoiddocumento, pf.parcialidad AS pagonumparcialidad, CASE pf.parcialidad WHEN 1 THEN round(pc.totalv, 2) ELSE round(pc.totalfacturado - pc.totalpagado, 2) END AS pagosaldoanterior, CASE pf.parcialidad WHEN 1 THEN round(pc.totalfacturado - pf.totalpagado, 2) ELSE round(pc.totalfacturado - pc.totalpagado, 2) END AS pagosaldoinsoluto, pf.polizasid as polizasid FROM polizas_facturacion pf INNER JOIN facturacion_cattipodecomprobante fctc ON pf.tipocomprobante = fctc.tipodecomprobante INNER JOIN facturacion_catformapago fcfp ON lpad(pf.formapago, 2, '0') = fcfp.id INNER JOIN facturacion_receptor fr ON pf.rfcreceptor = fr.rfc INNER JOIN polizas_facturacion pfm ON pf.polizasid = pfm.polizasid AND pfm.polizamadre = 1 LEFT JOIN facturacion_comprobante fc ON pfm.comprobanteid = fc.id INNER JOIN polizas_concentrado pc ON pfm.polizasid = pc.id WHERE pf.estatusfacturacionid = 9 ORDER BY POLIZAFACTURACIONID ASC, pagoiddocumento ASC, PAGONUMPARCIALIDAD ASC";

        private const string QUERY_POLIZAS_LOGCONCENTRADO_ID = "SELECT nvl(MAX(ID) + 1, 1) FROM polizas_logconcentrado";
        private const string QUERY_POLIZAS_LOGFACTURACION_ID = "SELECT nvl(MAX(ID) + 1, 1) FROM polizas_logfacturacion";
        private const string QUERY_ESTATUSFACTURACION = "SELECT MAX(logfacturaid) AS id, facturacionid, estatusfacturacionid FROM( SELECT l.id AS logfacturaid, f.id AS facturacionid, CASE WHEN l.mensaje LIKE '%CFDI33132%' THEN 993 ELSE c.estatusfacturaid END AS estatusfacturacionid FROM facturacion_comprobante c INNER JOIN polizas_facturacion f ON f.comprobanteid = c.id INNER JOIN facturacion_logfactura l ON l.comprobanteid = c.id WHERE c.estatusfacturaid IN ( 2, 99) AND f.estatusfacturacionid = 7 ) GROUP BY facturacionid, estatusfacturacionid";

        private int _comprobanteId;
        private int _cfdiRelacionadosId;
        private int _conceptoId;
        private int _impuestoId;
        private int _pagoId;
        private int _pagosDoctoRelacionadoId;
        private int _monedaId;
        private int _monedaIdGenerico;
        private int _metodoPagoId;
        private int _usoCfdiIdFisica;
        private int _usoCfdiIdMoral;
        private int _claveProdServId;
        private int _claveUnidadId;
        private int _emisorId;
        private string _descripcionConceptoPrima34;
        private string _descripcionConceptoPrima36;
        private string _descripcionConceptoPrima37;
        private string _descripcionConceptoPrima39;
        private string _descripcionConceptoGasto34;
        private string _descripcionConceptoGasto36;
        private string _descripcionConceptoGasto37;
        private string _descripcionConceptoGasto39;
        private string _descripcionConceptoFinanciamiento34;
        private string _descripcionConceptoFinanciamiento36;
        private string _descripcionConceptoFinanciamiento37;
        private string _descripcionConceptoFinanciamiento39;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseDatos">BaseDatos</param>
        public Refacturacion(BaseDatos baseDatos)
        {
            _baseDatos = baseDatos;
            procesador = new FacturacionCFDI.Negocio.Facturacion.Procesador();
        }

        public async Task<GenericResponse> ProcesoRefacturacion()
        {
            try
            {
                int count = 0;
                await SincronizarFacturasReceptor();
                await ObtenerVariablesFactura();
                var relacionFacturas = await _baseDatos.SelectAsync<FacturaPoliza>(QUERY_FACTURACION_RELACIONFACTURAS);
                if (!(relacionFacturas?.Any() ?? false))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Sin registros para guardar en las tablas de FACTURACION",
                        Data = false
                    };

                decimal? sum_totales = 0;
                decimal? pago_anterior = 0;
                int polizasId = 0;
                String uuid = "";
                int cont = 0;
                foreach (FacturaPoliza x in relacionFacturas) {
                    DateTime fechaActual = DateTime.Now;
                    bool proceso = false;
                    #region Query INSERT,DELETE FACTURACION_COMPROBANTE
                        var insComprobante = new StringBuilder();
                        string delComprobante;

                        insComprobante.Append($"INSERT INTO FACTURACION_COMPROBANTE VALUES({_comprobanteId},'3.3','{x.Serie}','{x.Folio}',TO_DATE('{fechaActual.ToString("dd/MM/yyyy HH:mm:ss")}', 'dd/mm/yyyy hh24:mi:ss')");
                        if (x.TipoComprobanteId == 5)
                            insComprobante.Append($",{x.FormaPagoId},0,0,{_monedaIdGenerico},0,0,{x.TipoComprobanteId},3,'{x.LugarExpedicion}',NULL,{_emisorId}, {x.RfcReceptorId}");
                        else
                            insComprobante.Append($",{x.FormaPagoId},{x.SubTotal},0,{_monedaId},1,{x.Total},{x.TipoComprobanteId},{_metodoPagoId},'{x.LugarExpedicion}',NULL,{_emisorId}, {x.RfcReceptorId}");
                        if (string.IsNullOrWhiteSpace(x.NombreReceptor))
                            insComprobante.Append($",NULL");
                        else
                            insComprobante.Append($",'{x.NombreReceptor}'");
                        if (x.TipoComprobanteId == 5)
                            insComprobante.Append($",22");
                        else
                        {
                            if (x.RfcReceptor.Length == 12)
                                insComprobante.Append($",{_usoCfdiIdMoral}");
                            else
                                insComprobante.Append($",{_usoCfdiIdFisica}");
                        }
                        insComprobante.Append($",1");
                        insComprobante.Append($",SYSDATE");
                        insComprobante.Append($",SYSDATE");
                        insComprobante.Append($",NULL,NULL,NULL,NULL,NULL)");

                        delComprobante = $"DELETE FROM FACTURACION_COMPROBANTE WHERE id = {_comprobanteId}";
                    #endregion

                    #region Query INSERT FACTURACION_CFDIRELACIONADO
                        DboFacturacion rowFacturacion = _baseDatos.SelectFirst<DboFacturacion>($"SELECT * FROM POLIZAS_FACTURACION WHERE ID = {x.PolizaFacturacionId}");
                        Comprobante rowComprobante = _baseDatos.SelectFirst<Comprobante>($"SELECT * FROM FACTURACION_COMPROBANTE WHERE ID = {rowFacturacion.ComprobanteId}");
                        string insCfdiRelacionado = string.Empty;
                        string delCfdiRelacionado = string.Empty;
                        insCfdiRelacionado = $"INSERT INTO FACTURACION_CFDIRELACIONADOS VALUES ({_cfdiRelacionadosId}, '{rowComprobante.facturaUuid}', 4, {_comprobanteId})";
                        delCfdiRelacionado = $"DELETE FROM FACTURACION_CFDIRELACIONADOS WHERE comprobanteId = {_comprobanteId}";
                        _cfdiRelacionadosId++;
                    #endregion

                    #region Query INSERT FACTURACION_CONCEPTO, FACTURACION_IMPUESTO
                        List<string> insConcepto = new List<string>();
                        string delConcepto;
                        List<string> insImpuesto = new List<string>();
                        string delImpuesto;

                        if (x.TipoComprobanteId == 5)
                        {
                            insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, 51498, NULL, 1, 241, 'Pago', 0, 0, 0, NULL, NULL, {_comprobanteId})");
                            _conceptoId++;
                        }
                        else
                        {
                            if (x.ConceptoPrimaMonto > 0)
                            {
                                switch (x.CodigoConcepto)
                                {
                                    case "34":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoPrima34}', {x.ConceptoPrimaMonto}, {x.ConceptoPrimaMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoPrimaMonto}, 2, 1, 0.16, {x.ConceptoPrimaIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    case "36":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoPrima36}', {x.ConceptoPrimaMonto}, {x.ConceptoPrimaMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoPrimaMonto}, 2, 1, 0.16, {x.ConceptoPrimaIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    case "37":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoPrima37}', {x.ConceptoPrimaMonto}, {x.ConceptoPrimaMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoPrimaMonto}, 2, 1, 0.16, {x.ConceptoPrimaIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    case "39":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoPrima39}', {x.ConceptoPrimaMonto}, {x.ConceptoPrimaMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoPrimaMonto}, 2, 1, 0.16, {x.ConceptoPrimaIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if (x.ConceptoFinanciamientoMonto > 0)
                            {
                                switch (x.CodigoConcepto)
                                {
                                    case "34":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoFinanciamiento34}', {x.ConceptoFinanciamientoMonto}, {x.ConceptoFinanciamientoMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoFinanciamientoMonto}, 2, 1, 0.16, {x.ConceptoFinanciamientoIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    case "36":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoFinanciamiento36}', {x.ConceptoFinanciamientoMonto}, {x.ConceptoFinanciamientoMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoFinanciamientoMonto}, 2, 1, 0.16, {x.ConceptoFinanciamientoIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    case "37":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoFinanciamiento37}', {x.ConceptoFinanciamientoMonto}, {x.ConceptoFinanciamientoMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoFinanciamientoMonto}, 2, 1, 0.16, {x.ConceptoFinanciamientoIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    case "39":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoFinanciamiento39}', {x.ConceptoFinanciamientoMonto}, {x.ConceptoFinanciamientoMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoFinanciamientoMonto}, 2, 1, 0.16, {x.ConceptoFinanciamientoIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if (x.ConceptoGastoMonto > 0)
                            {
                                switch (x.CodigoConcepto)
                                {
                                    case "34":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoGasto34}', {x.ConceptoGastoMonto}, {x.ConceptoGastoMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoGastoMonto}, 2, 1, 0.16, {x.ConceptoGastoIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    case "36":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoGasto36}', {x.ConceptoGastoMonto}, {x.ConceptoGastoMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoGastoMonto}, 2, 1, 0.16, {x.ConceptoGastoIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    case "37":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoGasto37}', {x.ConceptoGastoMonto}, {x.ConceptoGastoMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoGastoMonto}, 2, 1, 0.16, {x.ConceptoGastoIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    case "39":
                                        insConcepto.Add($"INSERT INTO FACTURACION_CONCEPTO VALUES ({_conceptoId}, {_claveProdServId}, '{x.ConceptoNoIdentificacion}', 1, {_claveUnidadId}, '{_descripcionConceptoGasto39}', {x.ConceptoGastoMonto}, {x.ConceptoGastoMonto}, 0, NULL, NULL, {_comprobanteId})");
                                        insImpuesto.Add($"INSERT INTO FACTURACION_IMPUESTO VALUES ({_impuestoId}, {x.ConceptoGastoMonto}, 2, 1, 0.16, {x.ConceptoGastoIva}, 1, {_conceptoId})");
                                        _conceptoId++;
                                        _impuestoId++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        delConcepto = $"DELETE FROM FACTURACION_CONCEPTO WHERE comprobenteId = {_comprobanteId}";
                        delImpuesto = $"DELETE FROM FACTURACION_IMPUESTO WHERE conceptoId IN (SELECT id FROM FACTURACION_CONCEPTO WHERE comprobanteId = {_comprobanteId})";
                    #endregion

                    #region Query INSERT FACTURACION_PAGO, FACTURACION_PAGOSDOCTORELACIONADO
                        string insPago = string.Empty;
                        string delPago = string.Empty;
                        string insPagosDoctoRelacionado = string.Empty;
                        string delPagosDoctoRelacionado = string.Empty;
                        if (x.TipoComprobanteId == 5)
                        {
                            if (string.IsNullOrWhiteSpace(x.PagoNumOperacion))
                            {
                                insPago = $"INSERT INTO FACTURACION_PAGO (ID, VERSION, FECHAPAGO, FORMADEPAGOP, MONEDAP, TIPOCAMBIOP, MONTO, NUMOPERACION, RFCEMISORCTAORD, NOMBANCOORDEXT, CTAORDENANTE, RFCEMISORCTABEN, CTABENEFICIARIO, TIPOCADPAGO, CERTPAGO, CADPAGO, SELLOPAGO, COMPROBANTEID) " +
                                $"VALUES ({_pagoId},'1.0',TO_DATE('{x.PagoFechaPago?.ToString("dd/MM/yyyy")}','dd/mm/yyyy'), '{x.FormaPagoId}','MXN',1, {x.PagoMonto}, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, {_comprobanteId})";
                            }
                            else
                            {
                                insPago = $"INSERT INTO FACTURACION_PAGO (ID, VERSION, FECHAPAGO, FORMADEPAGOP, MONEDAP, TIPOCAMBIOP, MONTO, NUMOPERACION, RFCEMISORCTAORD, NOMBANCOORDEXT, CTAORDENANTE, RFCEMISORCTABEN, CTABENEFICIARIO, TIPOCADPAGO, CERTPAGO, CADPAGO, SELLOPAGO, COMPROBANTEID) " +
                                $"VALUES ({_pagoId},'1.0',TO_DATE('{x.PagoFechaPago?.ToString("dd/MM/yyyy")}','dd/mm/yyyy'),'{x.FormaPagoId}','MXN',1, {x.PagoMonto}, '{x.PagoNumOperacion}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, {_comprobanteId})";
                            }

                            if (uuid == "" || uuid != x.PagoIdDocumento)
                            {
                                polizasId = x.PolizasId;
                                sum_totales = 0;
                                uuid = x.PagoIdDocumento;
                                pago_anterior = x.PagoSaldoAnterior;
                                sum_totales += x.Total;
                                insPagosDoctoRelacionado = $"INSERT INTO FACTURACION_PAGOSDOCTORELACIONADO (ID, IDDOCUMENTO, SERIE, FOLIO, MONEDADR, TIPOCAMBIODR, METODODEPAGODR, NUMPARCIALIDAD, IMPSALDOANT, IMPPAGADO, IMPSALDOINSOLUTO, PAGOSID) " +
                                                       $"VALUES ({_pagosDoctoRelacionadoId}, '{x.PagoIdDocumento}', NULL, NULL, 'MXN', 1, 'PPD', {x.PagoNumParcialidad}, {x.PagoSaldoAnterior}, {x.Total}, {x.PagoSaldoInsoluto}, {_pagoId})";
                            }
                            else
                            {
                                decimal? anterior = pago_anterior - sum_totales;
                                sum_totales += x.Total;
                                decimal? insoluto = pago_anterior - sum_totales;
                                insPagosDoctoRelacionado = $"INSERT INTO FACTURACION_PAGOSDOCTORELACIONADO (ID, IDDOCUMENTO, SERIE, FOLIO, MONEDADR, TIPOCAMBIODR, METODODEPAGODR, NUMPARCIALIDAD, IMPSALDOANT, IMPPAGADO, IMPSALDOINSOLUTO, PAGOSID) " +
                                                       $"VALUES ({_pagosDoctoRelacionadoId}, '{x.PagoIdDocumento}', NULL, NULL, 'MXN', 1, 'PPD', {x.PagoNumParcialidad}, {anterior}, {x.Total}, {insoluto}, {_pagoId})";
                            }

                            decimal? pagado = sum_totales + x.TotalPagado;
                            //updatePolizaFacturacionPagado = $"UPDATE POLIZAS_CONCENTRADO SET TOTALPAGADO = {pagado} WHERE ID = {x.PolizasId}";

                            delPago = $"DELETE FROM FACTURACION_PAGO WHERE comprobanteId = {_comprobanteId}";
                            delPagosDoctoRelacionado = $"DELETE FROM FACTURACION_PAGOSDOCTORELACIONADO WHERE pagosId IN (SELECT id FACTURACION_PAGO WHERE comprobanteId = {_comprobanteId})";
                            _pagoId++;
                            _pagosDoctoRelacionadoId++;
                        }

                    #endregion

                    string updPolizaFacturacion = $"UPDATE POLIZAS_FACTURACION SET FECHAMODIFICACION = SYSDATE, COMPROBANTEID = {_comprobanteId}, ESTATUSFACTURACIONID = 7 WHERE ID = {x.PolizaFacturacionId}";

                    int tipoComprobante = x.TipoComprobanteId;

                    var insertComprobate = await _baseDatos.InsertAsync(insComprobante.ToString());
                    if (!insertComprobate)
                    {
                        Console.WriteLine($"Error query: { insComprobante.ToString() }");
                    }
                    else
                    {
                        int contInsertConcepto = 0;
                        foreach (var concepto in insConcepto)
                        {
                            var insertConcepto = await _baseDatos.InsertAsync(concepto);
                            if (insertConcepto)
                                contInsertConcepto++;
                        }

                        if (contInsertConcepto != insConcepto.Count)
                        {
                            Console.WriteLine($"Error: No se guardaron todos los conceptos");
                            await _baseDatos.DeleteAsync(delConcepto);
                            await _baseDatos.DeleteAsync(delComprobante);
                        }
                        else
                        {
                            if (tipoComprobante != 5)
                            {
                                int contInsertImpuesto = 0;
                                foreach (var impuesto in insImpuesto)
                                {
                                    var insertImpuesto = await _baseDatos.InsertAsync(impuesto);
                                    if (insertImpuesto)
                                        contInsertImpuesto++;
                                }

                                if (contInsertImpuesto != insImpuesto.Count)
                                {
                                    Console.WriteLine($"Error: No se guardaron todos los impuestos");
                                    await _baseDatos.DeleteAsync(delImpuesto);
                                    await _baseDatos.DeleteAsync(delComprobante);
                                }
                                else
                                {
                                    if (string.IsNullOrWhiteSpace(insCfdiRelacionado))
                                    {
                                        var updatePolizasFacturacion = await _baseDatos.UpdateAsync(updPolizaFacturacion);
                                        if (!updatePolizasFacturacion)
                                        {
                                            Console.WriteLine($"Error query: {updPolizaFacturacion}");
                                            await _baseDatos.DeleteAsync(delImpuesto);
                                            await _baseDatos.DeleteAsync(delConcepto);
                                            await _baseDatos.DeleteAsync(delComprobante);
                                        }
                                        else
                                        {
                                            proceso = true;
                                            cont++;
                                        }
                                    }
                                    else
                                    {
                                        var insertCfdiRelacionado = await _baseDatos.InsertAsync(insCfdiRelacionado);
                                        if (!insertCfdiRelacionado)
                                        {
                                            Console.WriteLine($"Error query: {insCfdiRelacionado}");
                                            await _baseDatos.DeleteAsync(delImpuesto);
                                            await _baseDatos.DeleteAsync(delConcepto);
                                            await _baseDatos.DeleteAsync(delComprobante);
                                        }
                                        else
                                        {
                                            var updatePolizasFacturacion = await _baseDatos.UpdateAsync(updPolizaFacturacion);
                                            if (!updatePolizasFacturacion)
                                            {
                                                Console.WriteLine($"Error query: {updPolizaFacturacion}");
                                                await _baseDatos.DeleteAsync(delImpuesto);
                                                await _baseDatos.DeleteAsync(delConcepto);
                                                await _baseDatos.DeleteAsync(delComprobante);
                                            }
                                            else
                                            {
                                                proceso = true;
                                                cont++;
                                            }
                                            proceso = true;
                                            cont++;
                                        }
                                    }
                                }
                            }

                            if (tipoComprobante == 5)
                            {
                                var insertPago = await _baseDatos.InsertAsync(insPago);
                                if (!insertPago)
                                {
                                    Console.WriteLine($"Error query: {insPago}");
                                    await _baseDatos.DeleteAsync(delConcepto);
                                    await _baseDatos.DeleteAsync(delComprobante);
                                }
                                else
                                {
                                    var insertPagosDoctoRel = await _baseDatos.InsertAsync(insPagosDoctoRelacionado);
                                    if (!insertPagosDoctoRel)
                                    {
                                        Console.WriteLine($"Error query: {insPagosDoctoRelacionado}");
                                        await _baseDatos.DeleteAsync(delPago);
                                        await _baseDatos.DeleteAsync(delConcepto);
                                        await _baseDatos.DeleteAsync(delComprobante);
                                    }
                                    else
                                    {
                                        await _baseDatos.InsertAsync(updPolizaFacturacion);
                                        proceso = true;
                                        cont++;
                                    }
                                }
                            }
                        }
                    }

                    if(proceso == true) {
                        Console.WriteLine($"Generando comprobante {_comprobanteId}:");
                        var procesoRefacturacion = await procesador.ProcesoTimbradoRefacturacion(Convert.ToString(_comprobanteId));
                        if(procesoRefacturacion.Codigo == 1)
                        {
                            var estatus = await _baseDatos.SelectAsync<FacturaPolizaEstatus>(QUERY_ESTATUSFACTURACION);
                            if (!(estatus?.Any() ?? false))
                                return new GenericResponse()
                                {
                                    Codigo = 2,
                                    Mensaje = "Sin registros para actualizar en POLIZAS_FACTURACION",
                                    Data = false
                                };

                            Parallel.ForEach(estatus, async e =>
                            {
                                await EstatusFacturacion(e.FacturacionId, e.EstatusFacturacionId);
                            });
                            _comprobanteId++;
                        } else {
                            return new GenericResponse()
                            {
                                Codigo = 0,
                                Mensaje = $"El programa se detuvo en el comprobante con Id {_comprobanteId}"
                            };
                        }
                    }
                }
                return new GenericResponse()
                {
                    Codigo = 1,
                    Mensaje = $"Todo ha salido bien, se han refacturado {count} facturas"
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

        public async Task<GenericResponse> SincronizarFacturasReceptor()
        {
            try
            {
                var rfcReceptor = await _baseDatos.SelectAsync<RfcReceptor>(QUERY_FACTURACION_RECEPTOR);

                if (!(rfcReceptor?.Any() ?? false))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Sin registros para guardar en las tablas de FACTURACION_RECEPTOR",
                        Data = false
                    };

                var idReceptor = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_RECEPTORID);
                if (idReceptor == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla FACTURACION_RECEPTOR.",
                        Data = false
                    };

                var querys = rfcReceptor.Select(x =>
                {
                    var query = $"INSERT INTO FACTURACION_RECEPTOR VALUES ({idReceptor++},'{x.Rfc}','{x.Nombre}', SYSDATE, SYSDATE)";

                    return new { InsReceptor = query };
                }).ToList();

                int cont = 0;

                foreach (var q in querys)
                {
                    //Inserta FACTURACION_RECEPTOR
                    var insert = await _baseDatos.InsertAsync(q.InsReceptor);

                    if (!insert)
                    {
                        Console.WriteLine($"Error query: {q.InsReceptor}");
                    }
                    else
                    {
                        cont++;
                    }
                }

                return new GenericResponse()
                {
                    Codigo = rfcReceptor.Count == cont ? 1 : 2,
                    Mensaje = rfcReceptor.Count == cont ? $"Sincronización completa, registros obtenenidos {rfcReceptor.Count} - registros guardados {cont}." : $"Hubo error con el guardado de los registros, registros obtenenidos {rfcReceptor.Count} - registros guardados {cont}. Para más detalles consultar LOG."
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

        private async Task<GenericResponse> ObtenerVariablesFactura()
        {
            try
            {
                _comprobanteId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_COMPROBANTEID);
                if (_comprobanteId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla FACTURACION_COMPROBANTE.",
                        Data = false
                    };

                _emisorId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_EMISORID);
                if (_emisorId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID EMISOR de la Tabla FACTURACION_EMISOR.",
                        Data = false
                    };

                _cfdiRelacionadosId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_CFDIRELACIONADOSID);
                if (_cfdiRelacionadosId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla FACTURACION_CFDIRELACIONADOS.",
                        Data = false
                    };

                _conceptoId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_CONCEPTOID);
                if (_conceptoId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla FACTURACION_CONCEPTO.",
                        Data = false
                    };

                _impuestoId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_IMPUESTOID);
                if (_impuestoId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla FACTURACION_IMPUESTO.",
                        Data = false
                    };

                _pagoId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_PAGOID);
                if (_pagoId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla FACTURACION_PAGO.",
                        Data = false
                    };

                _pagosDoctoRelacionadoId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_PAGOSDOCTORELACIONADOID);
                if (_pagosDoctoRelacionadoId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla FACTURACION_PAGOSDOCTORELACIONADO.",
                        Data = false
                    };

                _monedaId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_MONEDAID);
                if (_monedaId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID MONEDA de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _monedaIdGenerico = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_MONEDAIDGENERICO);
                if (_monedaIdGenerico == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID MONEDA GENÉRICO de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _metodoPagoId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_METODOPAGOID);
                if (_metodoPagoId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID METODOPAGO de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _usoCfdiIdFisica = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_USOCFDIIDFISICA);
                if (_usoCfdiIdFisica == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID USOCFDI para Persona Física de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _usoCfdiIdMoral = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_USOCFDIIDMORAL);
                if (_usoCfdiIdMoral == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID USOCFDI para Persona Moral de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _claveProdServId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_CLAVEPRODSERVID);
                if (_claveProdServId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID CLAVEPRODSERV de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _claveUnidadId = await _baseDatos.SelectFirstAsync<int>(QUERY_FACTURACION_CLAVEUNIDADID);
                if (_claveUnidadId == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID CLAVEUNIDAD de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoPrima34 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOPRIMA34);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO PRIMA CODIGO 34 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoPrima36 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOPRIMA36);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO PRIMA CODIGO 36 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoPrima37 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOPRIMA37);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO PRIMA CODIGO 37 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoPrima39 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOPRIMA39);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO PRIMA CODIGO 39 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoGasto34 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOGASTO34);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO GASTO CODIGO 34 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoGasto36 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOGASTO36);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO GASTO CODIGO 36 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoGasto37 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOGASTO37);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO GASTO CODIGO 37 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoGasto39 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOGASTO39);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO GASTO CODIGO 39 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoFinanciamiento34 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOFINANCIAMIENTO34);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO FINANCIAMIENTO CODIGO 34 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoFinanciamiento36 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOFINANCIAMIENTO36);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO FINANCIAMIENTO CODIGO 36 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoFinanciamiento37 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOFINANCIAMIENTO37);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO FINANCIAMIENTO CODIGO 37 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                _descripcionConceptoFinanciamiento39 = await _baseDatos.SelectFirstAsync<string>(QUERY_FACTURACION_CONCEPTOFINANCIAMIENTO39);
                if (string.IsNullOrWhiteSpace(_descripcionConceptoPrima34))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener DESCRIPCION CONCEPTO FINANCIAMIENTO CODIGO 39 de la Tabla CONFIGURACIONSISTEMA.",
                        Data = false
                    };

                return new GenericResponse();
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

        #region Métodos privados
        /// <summary>
        /// Actualiza estatus en la Tabla POLIZAS_FACTURACION
        /// </summary>
        /// <param name="facturacionId">ID Tablas POLIZAS_FACTURACION</param>
        /// <param name="estatusId">ID estatus de acuerdo a Tabla POLIZAS_CATESTATUSFACTURACION</param>
        /// <returns></returns>
        private async Task EstatusFacturacion(int facturacionId, int estatusId)
        {
            try
            {
                //DboFacturacion fac = _baseDatos.SelectFirst<DboFacturacion>($"SELECT * FROM POLIZAS_FACTURACION WHERE ID = {facturacionId}");
                //Concentrado con = _baseDatos.SelectFirst<Concentrado>($"SELECT * FROM POLIZAS_CONCENTRADO WHERE ID = {fac.PolizasId}");
                await _baseDatos.UpdateAsync($"UPDATE polizas_facturacion SET fechamodificacion = SYSDATE, estatusfacturacionid = {estatusId} WHERE id = {facturacionId}");
                //await _baseDatos.UpdateAsync($"UPDATE POLIZAS_CONCENTRADO SET TOTALPAGADO = {con.TotalPagado} WHERE ID = {con.Id}");
                await LogFacturacion(facturacionId, $"Se actualiza estatus de la Factura a {estatusId}");
            }
            catch
            {

            }
        }

        /// <summary>
        /// Guarda registro en Tabla POLIZAS_LOGCONCENTRADO
        /// </summary>
        /// <param name="polizaId">ID Tabla POLIZAS_CONCENTRADO</param>
        /// <param name="mensaje">Mensaje</param>
        /// <returns></returns>
        private async Task LogConcentrado(int polizaId, string mensaje)
        {
            try
            {
                var id = await _baseDatos.SelectFirstAsync<int>(QUERY_POLIZAS_LOGCONCENTRADO_ID);

                if (id > 0)
                    await _baseDatos.InsertAsync($"INSERT INTO POLIZAS_LOGCONCENTRADO VALUES ({id}, {polizaId}, SYSDATE, '{mensaje}', 'servicio')");
            }
            catch
            {
                //Guardar LOG TXT
            }
        }

        /// <summary>
        /// Guarda registro en Tabla POLIZAS_LOGFACTURACION
        /// </summary>
        /// <param name="facturaId">ID Tabla POLIZAS_FACTURACION</param>
        /// <param name="mensaje">Mensaje</param>
        /// <returns></returns>
        private async Task LogFacturacion(int facturaId, string mensaje)
        {
            try
            {
                var id = await _baseDatos.SelectFirstAsync<int>(QUERY_POLIZAS_LOGFACTURACION_ID);

                if (id > 0)
                    await _baseDatos.InsertAsync($"INSERT INTO POLIZAS_LOGFACTURACION VALUES ({id}, {facturaId}, SYSDATE, '{mensaje}', 'servicio')");
            }
            catch
            {
                //Guardar LOG TXT
            }
        }

        /// <summary>
        /// Validador estructura RFC
        /// RegEx recuperado: https://www.regexlib.com/REDetails.aspx?regexp_id=3455
        /// </summary>
        /// <param name="rfc">RFC</param>
        /// <returns>Válido/No válido</returns>
        private bool ValidarRFC(string rfc)
        {
            string exp = @"^(([ÑA-Z|ña-z|&]{3}|[A-Z|a-z]{4})\d{2}((0[1-9]|1[012])(0[1-9]|1\d|2[0-8])|(0[13456789]|1[012])(29|30)|(0[13578]|1[02])31)(\w{2})([A|a|0-9]{1}))$|^(([ÑA-Z|ña-z|&]{3}|[A-Z|a-z]{4})([02468][048]|[13579][26])0229)(\w{2})([A|a|0-9]{1})$";
            Regex regex = new Regex(exp);
            return regex.IsMatch(rfc);
        }
        #endregion

    }
}
