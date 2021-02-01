using FacturacionCFDI.Datos.Polizas;
using FacturacionCFDI.Datos.Response;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilerias;

namespace FacturacionCFDI.Negocio.Polizas
{
    public class SincronizacionPoliza
    {
        private readonly BaseDatos _baseDatos;

        private const string QUERY_MOVIMIENTOS_FACTURAGLOBAL = "SELECT m.id AS id, m.datoorigen as datoorigen, m.sistema AS sistema, m.poliza AS poliza, m.fechaemision AS fechaemision, m.fechainicio AS fechainicio, m.fechatermino AS fechatermino, m.estatuspoliza AS estatuspoliza, m.tipodepoliza AS tipodepoliza, m.rfccontratante AS rfcreceptor, m.nombrecontratante AS nombrereceptor, m.tipopersonacontratante AS tipopersonareceptor, m.tipomovimiento AS tipocomprobante, m.codigoconcepto AS codigoconcepto, m.codigoproducto AS codigoproducto, m.formapago AS formapago, m.lugarexpedicion AS lugarexpedicion, m.primaneta AS primaneta, m.financiamiento AS financiamiento, m.gasto AS gasto, m.iva AS iva, m.total AS total, nvl(c.id, 0) AS idconcentrado, 0 AS estatusfacturamadre FROM polizas_movimientos m LEFT JOIN polizas_concentrado c ON c.sistema = m.sistema AND c.poliza = m.poliza AND trunc(c.fechainicio) = trunc(m.fechainicio) WHERE m.estatusmovimientoid = 1 AND m.estatuspoliza = 'VIGENTE' AND m.causaendoso IN( 'REN 401', 'MOE 505 / PON 505', 'MOE 504 / PON 504', 'PON 474', 'PON 475', 'REN 290', 'REN 525', 'REN 69', 'REN 42', 'REN 70', 'REN 86', 'EMI') AND m.datoorigen IN ( 'RUE', 'WEB' )";
        private const string QUERY_MOVIMIENTOS_FACTURANOTADEBITO = "SELECT m.id AS id, m.sistema AS sistema, m.poliza AS poliza, m.fechainicio AS fechainicio, m.fechaemision AS fechaemision, m.fechatermino AS fechatermino, m.estatuspoliza AS estatuspoliza, m.tipodepoliza AS tipodepoliza, m.rfccontratante AS rfcreceptor, m.nombrecontratante AS nombrereceptor, m.tipopersonacontratante AS tipopersonareceptor, m.tipomovimiento AS tipocomprobante, m.codigoconcepto AS codigoconcepto, m.codigoproducto AS codigoproducto, m.formapago AS formapago, m.lugarexpedicion AS lugarexpedicion, m.primaneta AS primaneta, m.financiamiento AS financiamiento, m.gasto AS gasto, m.iva AS iva, m.total AS total, nvl(c.id, 0) AS idconcentrado, nvl(f.estatusfacturacionid, 0) AS estatusfacturamadre FROM polizas_movimientos m LEFT JOIN polizas_concentrado c ON c.sistema = m.sistema AND c.poliza = m.poliza AND trunc(c.fechainicio) = trunc(m.fechainicio) LEFT JOIN polizas_facturacion f ON f.polizasid = c.id AND f.polizamadre = 1 WHERE m.estatusmovimientoid = 1 AND m.estatuspoliza = 'VIGENTE' AND m.tipomovimiento = 'I' AND m.causaendoso IN( 'ALT 457', 'ALT 48', 'ALT 458', 'ALT 49', 'ALT 480', 'ALT 478', 'ALT 477', 'ALT 476', 'ALT 489 / PON 489', 'ALT 493 / PON 489', 'ENDAJU', 'ALT', 'CFP 54', 'CAP 55', 'CAP 155', 'REC 63', 'MOE 184', 'REI 482', 'REI 145', 'MOE 184', 'AJU XXX', 'ANULAC', 'MOD 57') AND m.datoorigen IN ( 'RUE', 'WEB' )";
        private const string QUERY_MOVIMIENTOS_FACTURANOTACREDITO = "SELECT m.id AS id, m.sistema AS sistema, m.poliza AS poliza, m.fechainicio AS fechainicio, m.fechaemision AS fechaemision, m.fechatermino AS fechatermino, m.estatuspoliza AS estatuspoliza, m.tipodepoliza AS tipodepoliza, m.rfccontratante AS rfcreceptor, m.nombrecontratante AS nombrereceptor, m.tipopersonacontratante AS tipopersonareceptor, m.tipomovimiento AS tipocomprobante, m.codigoconcepto AS codigoconcepto, m.codigoproducto AS codigoproducto, m.formapago AS formapago, m.lugarexpedicion AS lugarexpedicion, CASE WHEN m.causaendoso = 'CANMAN' OR m.causaendoso = 'CANAUT' OR m.causaendoso = 'REH 37' OR m.causaendoso = 'REA 148' OR m.causaendoso = 'REHABI' OR m.causaendoso = 'REH' THEN c.primanetam - fp.primaneta ELSE abs(m.primaneta) END AS primaneta, CASE WHEN m.causaendoso = 'CANMAN' OR m.causaendoso = 'CANAUT' OR m.causaendoso = 'REH 37' OR m.causaendoso = 'REA 148' OR m.causaendoso = 'REHABI' OR m.causaendoso = 'REH' THEN c.financiamientom - fp.financiamiento ELSE abs(m.financiamiento) END AS financiamiento, CASE WHEN m.causaendoso = 'CANMAN' OR m.causaendoso = 'CANAUT' OR m.causaendoso = 'REH 37' OR m.causaendoso = 'REA 148' OR m.causaendoso = 'REHABI' OR m.causaendoso = 'REH' THEN c.gastom - fp.gasto ELSE abs(m.gasto) END AS gasto, CASE WHEN m.causaendoso = 'CANMAN' OR m.causaendoso = 'CANAUT' OR m.causaendoso = 'REH 37' OR m.causaendoso = 'REA 148' OR m.causaendoso = 'REHABI' OR m.causaendoso = 'REH' THEN c.ivam - fp.iva ELSE abs(m.iva) END AS iva, CASE WHEN m.causaendoso = 'CANMAN' OR m.causaendoso = 'CANAUT' OR m.causaendoso = 'REH 37' OR m.causaendoso = 'REA 148' OR m.causaendoso = 'REHABI' OR m.causaendoso = 'REH' THEN c.totalm - fp.total ELSE abs(m.total) END AS total, nvl(c.id, 0) AS idconcentrado, nvl(f.estatusfacturacionid, 0) AS estatusfacturamadre FROM polizas_movimientos m LEFT JOIN polizas_concentrado c ON c.sistema = m.sistema AND c.poliza = m.poliza AND trunc(c.fechainicio) = trunc(m.fechainicio) LEFT JOIN polizas_facturacion f ON f.polizasid = c.id AND f.polizamadre = 1 LEFT JOIN( SELECT polizasid, nvl(SUM(primaneta), 0) AS primaneta, nvl(SUM(financiamiento), 0) AS financiamiento, nvl(SUM(gasto), 0) AS gasto, nvl(SUM(iva), 0) AS iva, nvl(SUM(total), 0) AS total FROM polizas_facturacion WHERE tipocomprobante = 'P' AND estatusfacturacionid = 2 AND polizasid IN ( SELECT tc.id FROM polizas_movimientos tm LEFT JOIN polizas_concentrado tc ON tc.sistema = tm.sistema AND tc.poliza = tm.poliza AND trunc(tc.fechainicio) = trunc(tm.fechainicio) WHERE tm.estatusmovimientoid = 1 AND tm.estatuspoliza = 'VIGENTE' AND tm.causaendoso IN ( 'CFP 54', 'CAP 55', 'CAP 155', 'REC 63', 'MOE 184', 'REI 482', 'REI 145', 'MOE 184', 'AJU XXX', 'ANULAC', 'MOD 57', 'BAJ 187', 'CSC 38', 'CNP 38', 'DEV 40', 'CYR 38', 'SUS XXX', 'ENDDIS', 'BAJ', 'CANMAN', 'CANAUT') AND tm.datoorigen = 'RUE' ) GROUP BY polizasid ) fp ON fp.polizasid = c.id WHERE m.estatusmovimientoid = 1 AND m.tipomovimiento = 'E' AND m.estatuspoliza = 'VIGENTE' AND m.causaendoso IN ( 'CFP 54', 'CAP 55', 'CAP 155', 'REC 63', 'MOE 184', 'REI 482', 'REI 145', 'MOE 184', 'AJU XXX', 'ANULAC', 'MOD 57', 'BAJ 187', 'CSC 38', 'CNP 38', 'DEV 40', 'CYR 38', 'SUS XXX', 'ENDDIS', 'BAJ', 'CANMAN', 'CANAUT' ) AND m.datoorigen IN ( 'RUE', 'WEB' )";
        private const string QUERY_MOVIMIENTOS_REFACTURACION = "SELECT m.id AS id, m.sistema AS sistema, m.poliza AS poliza, m.fechainicio AS fechainicio, m.fechaemision AS fechaemision, m.fechatermino AS fechatermino, m.estatuspoliza AS estatuspoliza, m.tipodepoliza AS tipodepoliza, m.rfccontratante AS rfcreceptor, m.nombrecontratante AS nombrereceptor, m.tipopersonacontratante AS tipopersonareceptor, m.tipomovimiento AS tipocomprobante, m.codigoconcepto AS codigoconcepto, m.codigoproducto AS codigoproducto, m.formapago AS formapago, m.lugarexpedicion AS lugarexpedicion, m.primaneta AS primaneta, m.financiamiento AS financiamiento, m.gasto AS gasto, m.iva AS iva, m.total AS total, nvl(c.id, 0) AS idconcentrado, nvl(f.estatusfacturacionid, 0) AS estatusfacturamadre FROM polizas_movimientos m LEFT JOIN polizas_concentrado c ON c.sistema = m.sistema AND c.poliza = m.poliza AND trunc(c.fechainicio) = trunc(m.fechainicio) LEFT JOIN polizas_facturacion f ON f.polizasid = c.id AND f.polizamadre = 1 WHERE m.estatusmovimientoid = 1 AND m.estatuspoliza = 'VIGENTE' AND m.tipomovimiento = 'R' AND m.causaendoso IN( 'ENDOSO', 'MOD 527', 'MNC 52 / MNA 52', 'MFC 53 / MNC 53' ) AND m.datoorigen IN ( 'RUE', 'WEB' )";
        private const string QUERY_MOVIMIENTOS_PAGOS = "SELECT m.id AS id, m.sistema AS sistema, m.poliza AS poliza, m.pagofechapago AS fechaemision, m.fechainicio AS fechainicio, m.fechatermino AS fechatermino, m.estatuspoliza AS estatuspoliza, m.tipodepoliza AS tipodepoliza, m.pagorfc AS rfcreceptor, m.pagonombre AS nombrereceptor, m.tipopersonacontratante AS tipopersonareceptor, m.tipomovimiento AS tipocomprobante, m.codigoconcepto AS codigoconcepto, m.codigoproducto AS codigoproducto, m.pagoformapago AS formapago, m.lugarexpedicion AS lugarexpedicion, m.primaneta AS primaneta, m.financiamiento AS financiamiento, m.gasto AS gasto, m.iva AS iva, m.total AS total, nvl(c.id, 0) AS idconcentrado, nvl(f.estatusfacturacionid, 0) AS estatusfacturamadre , m.pagoparcialidad AS parcialidad, m.pagooperacion AS numoperacion FROM polizas_movimientos m LEFT JOIN polizas_concentrado c ON c.sistema = m.sistema AND c.poliza = m.poliza AND trunc(c.fechainicio) = trunc(m.fechainicio) LEFT JOIN polizas_facturacion f ON f.polizasid = c.id AND f.polizamadre = 1 WHERE m.estatusmovimientoid = 1 AND m.tipomovimiento = 'P' AND m.causaendoso = 'PAGO' AND m.datoorigen IN ( 'RUP', 'WEB' )";

        private const string QUERY_POLIZAS_CONCENTRADO_ID = "SELECT nvl(MAX(id) + 1, 1) FROM polizas_concentrado";
        private const string QUERY_POLIZAS_FACTURACION_ID = "SELECT nvl(MAX(ID) + 1, 1) FROM polizas_facturacion";
        private const string QUERY_POLIZAS_LOGCONCENTRADO_ID = "SELECT nvl(MAX(ID) + 1, 1) FROM polizas_logconcentrado";
        private const string QUERY_POLIZAS_LOGFACTURACION_ID = "SELECT nvl(MAX(ID) + 1, 1) FROM polizas_logfacturacion";
        private const string QUERY_ESTATUSFACTURACION = "SELECT MAX(logfacturaid) AS id, facturacionid, estatusfacturacionid FROM( SELECT l.id AS logfacturaid, f.id AS facturacionid, CASE WHEN l.mensaje LIKE '%CFDI33132%' THEN 993 ELSE c.estatusfacturaid END AS estatusfacturacionid FROM facturacion_comprobante c INNER JOIN polizas_facturacion f ON f.comprobanteid = c.id INNER JOIN facturacion_logfactura l ON l.comprobanteid = c.id WHERE c.estatusfacturaid IN ( 2, 99) AND f.estatusfacturacionid = 7 ) GROUP BY facturacionid, estatusfacturacionid";
        private const string QUERY_ESTATUSFACTURACIONPROGRAMADOS = "SELECT polizasid AS id, id AS facturacionid, 1 AS estatusfacturacionid FROM polizas_facturacion WHERE estatusfacturacionid = 6 AND trunc(fechacomprobante) >= trunc(sysdate)";
        private const string QUERY_ESTATUSFACTURACIONPORTIMBRARFACTURAGLOBAL = "SELECT fp.polizasid AS id, fp.id AS facturacionid, 1 AS estatusfacturacionid FROM polizas_facturacion fp INNER JOIN polizas_facturacion fm ON fp.polizasid = fm.polizasid AND fm.estatusfacturacionid = 2 AND fm.polizamadre = 1 WHERE fp.estatusfacturacionid = 8";

        private const string QUERY_PARAMETRO = "SELECT * FROM PARAMETRO WHERE ID = 17";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseDatos">BaseDatos</param>
        public SincronizacionPoliza(BaseDatos baseDatos)
        {
            _baseDatos = baseDatos;
        }

        /// <summary>
        /// Sincronización Facturas Globales
        /// </summary>
        /// <returns>Modelo GenericResponse</returns>
        public async Task<GenericResponse> SincronizacionFacturaGlobal()
        {
            try
            {
                var modelo = await _baseDatos.SelectAsync<ConcetradoFactura>(QUERY_MOVIMIENTOS_FACTURAGLOBAL);
                if (!(modelo?.Any() ?? false))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Sin registros para guardar en POLIZAS_CONCENTRADO",
                        Data = false
                    };

                var idConcentrado = await _baseDatos.SelectFirstAsync<int>(QUERY_POLIZAS_CONCENTRADO_ID);
                if (idConcentrado == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla POLIZAS_CONCENTRADO.",
                        Data = false
                    };


                var idFacturacion = await _baseDatos.SelectFirstAsync<int>(QUERY_POLIZAS_FACTURACION_ID);
                if (idFacturacion == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla POLIZAS_FACTURACION.",
                        Data = false
                    };

                Parametros parametro = _baseDatos.SelectFirst<Parametros>(QUERY_PARAMETRO);
                if (parametro == null)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudieron obtener los parametros de Polizas (PARAMETRO).",
                        Data = false
                    };

                bool polizaParametrizada = false;
                string[] polizas = parametro.Valor.Split(',');
                int c=0;
                var querys = modelo.Select(x =>
                {
                    c++;
                    if (x.Folio==null)
                    {
                        x.Folio = "";
                        
                    }
                    if (x.Serie == null)
                    {
                        x.Serie = "";
                    }
                    if (x.RfcReceptor == null)
                    {
                        x.RfcReceptor = "";
                    }
                    
                        x.LugarExpedicion = "01900";
                    




                    if (c > 987)
                    {
                        var i = 0;
                    }
                    string pol = x.Poliza + x.Sistema + x.FechaInicio.ToString("yyyy");
                    foreach (var poliza in polizas){
                        if (poliza == pol){
                            polizaParametrizada = true;
                            break;
                        }
                    }
                    var sbInsertConcentrado = new StringBuilder();
                    var sbInsertFacturacion = new StringBuilder();
                    var queryUpdMovimientos = "";
                    var queryDelConcentrado = "";
                    var queryDelFacturacion = "";
                    var ConcentradoId = idConcentrado;
                    var FacturacionId = idFacturacion;

                    if (x.DatoOrigen == "RUE") {
                        if (polizaParametrizada == true){
                            #region Query INSERT POLIZAS_CONCENTRADO
                            sbInsertConcentrado = new StringBuilder();
                            sbInsertConcentrado.Append("INSERT INTO POLIZAS_CONCENTRADO ");
                            sbInsertConcentrado.Append("VALUES(");
                            sbInsertConcentrado.Append($"{idConcentrado}");
                            sbInsertConcentrado.Append($",'{x.Sistema}'");
                            sbInsertConcentrado.Append($",'{x.Poliza}'");
                            sbInsertConcentrado.Append($",{x.FechaInicio.ToString("yyyy")}");
                            sbInsertConcentrado.Append($",TO_DATE('{x.FechaEmision.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                            sbInsertConcentrado.Append($",TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                            sbInsertConcentrado.Append($",TO_DATE('{x.FechaTermino.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                            if (string.IsNullOrWhiteSpace(x.EstatusPoliza)) sbInsertConcentrado.Append($",NULL"); else sbInsertConcentrado.Append($",'{x.EstatusPoliza}'");
                            sbInsertConcentrado.Append($",'{x.TipoDePoliza}'");
                            sbInsertConcentrado.Append($",{x.PrimaNeta}");
                            sbInsertConcentrado.Append($",{x.Financiamiento}");
                            sbInsertConcentrado.Append($",{x.Gasto}");
                            sbInsertConcentrado.Append($",{x.Iva}");
                            sbInsertConcentrado.Append($",{x.Total}");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",SYSDATE");
                            sbInsertConcentrado.Append($",SYSDATE");
                            sbInsertConcentrado.Append($",{x.Total}");
                            sbInsertConcentrado.Append($",NULL)");
                            #endregion

                            idConcentrado++;
                            queryUpdMovimientos = $@"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 2 WHERE id = {x.Id}";
                            queryDelConcentrado = $@"DELETE POLIZAS_CONCENTRADO WHERE ID = {idConcentrado}";
                        } else if (polizaParametrizada == false) {
                            #region Query INSERT POLIZAS_CONCENTRADO
                            sbInsertConcentrado = new StringBuilder();
                            sbInsertConcentrado.Append("INSERT INTO POLIZAS_CONCENTRADO ");
                            sbInsertConcentrado.Append("VALUES(");
                            sbInsertConcentrado.Append($"{idConcentrado}");
                            sbInsertConcentrado.Append($",'{x.Sistema}'");
                            sbInsertConcentrado.Append($",'{x.Poliza}'");
                            sbInsertConcentrado.Append($",{x.FechaInicio.ToString("yyyy")}");
                            sbInsertConcentrado.Append($",TO_DATE('{x.FechaEmision.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                            sbInsertConcentrado.Append($",TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                            sbInsertConcentrado.Append($",TO_DATE('{x.FechaTermino.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                            if (string.IsNullOrWhiteSpace(x.EstatusPoliza)) sbInsertConcentrado.Append($",NULL"); else sbInsertConcentrado.Append($",'{x.EstatusPoliza}'");
                            sbInsertConcentrado.Append($",'{x.TipoDePoliza}'");
                            sbInsertConcentrado.Append($",{x.PrimaNeta}");
                            sbInsertConcentrado.Append($",{x.Financiamiento}");
                            sbInsertConcentrado.Append($",{x.Gasto}");
                            sbInsertConcentrado.Append($",{x.Iva}");
                            sbInsertConcentrado.Append($",{x.Total}");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",0");
                            sbInsertConcentrado.Append($",SYSDATE");
                            sbInsertConcentrado.Append($",SYSDATE");
                            sbInsertConcentrado.Append($",NULL");
                            sbInsertConcentrado.Append($",NULL)");
                            #endregion

                            #region Query INSERT POLIZAS_FACTURACION
                            string nombreReceptor = x.NombreReceptor.Replace("‘", "''");
                            sbInsertFacturacion = new StringBuilder();
                            sbInsertFacturacion.Append("INSERT INTO POLIZAS_FACTURACION ");
                            sbInsertFacturacion.Append("VALUES(");
                            sbInsertFacturacion.Append($"{idFacturacion}");
                            sbInsertFacturacion.Append($",{idConcentrado}");
                            sbInsertFacturacion.Append($",'{x.TipoComprobante}'");
                            if (x.FechaInicio > DateTime.Now)
                                sbInsertFacturacion.Append($",TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")} {DateTime.Now.ToString("HH:mm:ss")}', 'dd/mm/yyyy hh24:mi:ss')");
                            else
                                sbInsertFacturacion.Append($",SYSDATE");
                            sbInsertFacturacion.Append($",'PSFACT{x.TipoComprobante}'");
                            sbInsertFacturacion.Append($",'{idFacturacion.ToString("D10")}'");
                            sbInsertFacturacion.Append($",'{x.RfcReceptor}'");
                            sbInsertFacturacion.Append($",'{nombreReceptor}'");
                            sbInsertFacturacion.Append($",'{x.CodigoConcepto}'");
                            sbInsertFacturacion.Append($",'{x.CodigoProducto}'");
                            sbInsertFacturacion.Append($",'{x.FormaPago}'");
                            sbInsertFacturacion.Append($",'PPD'");
                            sbInsertFacturacion.Append($",'14120'");
                            sbInsertFacturacion.Append($",NULL");
                            sbInsertFacturacion.Append($",{x.PrimaNeta}");
                            sbInsertFacturacion.Append($",{x.Financiamiento}");
                            sbInsertFacturacion.Append($",{x.Gasto}");
                            sbInsertFacturacion.Append($",{x.Iva}");
                            sbInsertFacturacion.Append($",{x.Total}");
                            sbInsertFacturacion.Append($",NULL");
                            sbInsertFacturacion.Append($",NULL");
                            sbInsertFacturacion.Append($",NULL");
                            sbInsertFacturacion.Append($",NULL");
                            sbInsertFacturacion.Append($",NULL");
                            sbInsertFacturacion.Append($",NULL");
                            sbInsertFacturacion.Append($",1");
                            sbInsertFacturacion.Append($",SYSDATE");
                            sbInsertFacturacion.Append($",SYSDATE");
                            if (ValidarRFC(x.RfcReceptor))
                            {
                                if (x.FechaInicio > DateTime.Now)
                                    sbInsertFacturacion.Append($",6)");
                                else
                                    sbInsertFacturacion.Append($",1)");
                            }
                            else
                            {
                                sbInsertFacturacion.Append($",992)");
                            }
                            #endregion

                            queryUpdMovimientos = $@"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 2 WHERE id = {x.Id}";
                            queryDelConcentrado = $@"DELETE POLIZAS_CONCENTRADO WHERE ID = {idConcentrado}";
                            queryDelFacturacion = $@"DELETE POLIZAS_FACTURACION WHERE ID = {idFacturacion}";
                            idConcentrado++;
                            idFacturacion++;
                        }
                    } else if (x.DatoOrigen == "WEB") {
                        int idCon = _baseDatos.SelectFirst<int>($"SELECT ID FROM POLIZAS_CONCENTRADO WHERE POLIZA = '{x.Poliza}' AND SISTEMA = '{x.Sistema}' AND ANIOINICIO = '{x.FechaInicio.ToString("yyyy")}'");
                        #region Query INSERT POLIZAS_FACTURACION
                        string nombreReceptor = x.NombreReceptor.Replace("‘", "''");
                        sbInsertFacturacion = new StringBuilder();
                        sbInsertFacturacion.Append("INSERT INTO POLIZAS_FACTURACION VALUES(");
                        sbInsertFacturacion.Append($"{idFacturacion}");
                        sbInsertFacturacion.Append($",{idCon}");
                        sbInsertFacturacion.Append($",'{x.TipoComprobante}'");
                        if (x.FechaInicio > DateTime.Now)
                            sbInsertFacturacion.Append($",TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")} {DateTime.Now.ToString("HH:mm:ss")}', 'dd/mm/yyyy hh24:mi:ss')");
                        else
                            sbInsertFacturacion.Append($",SYSDATE");
                        sbInsertFacturacion.Append($",'PSFACT{x.TipoComprobante}'");
                        sbInsertFacturacion.Append($",'{idFacturacion.ToString("D10")}'");
                        sbInsertFacturacion.Append($",'{x.RfcReceptor}'");
                        sbInsertFacturacion.Append($",'{nombreReceptor}'");
                        sbInsertFacturacion.Append($",'{x.CodigoConcepto}'");
                        sbInsertFacturacion.Append($",'{x.CodigoProducto}'");
                        sbInsertFacturacion.Append($",'{x.FormaPago}'");
                        sbInsertFacturacion.Append($",'PPD'");
                        sbInsertFacturacion.Append($",'14120'");
                        sbInsertFacturacion.Append($",NULL");
                        sbInsertFacturacion.Append($",{x.PrimaNeta}");
                        sbInsertFacturacion.Append($",{x.Financiamiento}");
                        sbInsertFacturacion.Append($",{x.Gasto}");
                        sbInsertFacturacion.Append($",{x.Iva}");
                        sbInsertFacturacion.Append($",{x.Total}");
                        sbInsertFacturacion.Append($",NULL");
                        sbInsertFacturacion.Append($",NULL");
                        sbInsertFacturacion.Append($",NULL");
                        sbInsertFacturacion.Append($",NULL");
                        sbInsertFacturacion.Append($",NULL");
                        sbInsertFacturacion.Append($",NULL");
                        sbInsertFacturacion.Append($",1");
                        sbInsertFacturacion.Append($",SYSDATE");
                        sbInsertFacturacion.Append($",SYSDATE");
                        if (ValidarRFC(x.RfcReceptor))
                        {
                            if (x.FechaInicio > DateTime.Now)
                                sbInsertFacturacion.Append($",6)");
                            else
                                sbInsertFacturacion.Append($",1)");
                        }
                        else
                        {
                            sbInsertFacturacion.Append($",992)");
                        }
                        #endregion

                        queryUpdMovimientos = $@"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 2 WHERE id = {x.Id}";
                        queryDelFacturacion = $@"DELETE POLIZAS_FACTURACION WHERE ID = {idFacturacion}";
                        idFacturacion++;
                    } return new {
                        ConcentradoId,
                        FacturacionId,
                        QueryInsConcentrado = sbInsertConcentrado,
                        QueryInsFacturacion = sbInsertFacturacion,
                        QueryUpdMovimientos = queryUpdMovimientos,
                        QueryDelConcentrado = queryDelConcentrado,
                        QueryDelFacturacion = queryDelFacturacion
                    };
                }).ToList();

                int cont = 0;

                foreach (var q in querys)
                {
                    var insertConcentrado = await _baseDatos.InsertAsync(q.QueryInsConcentrado.ToString());

                    if (!insertConcentrado && q.QueryInsConcentrado.ToString() != "")
                    {
                        Console.WriteLine($"Error query: {q.QueryInsConcentrado}");
                    } else {
                        if(q.QueryInsConcentrado.ToString() != "")
                            await LogConcentrado(q.ConcentradoId, "Se genera Concentrado de la Póliza");

                        var insertFacturacion = await _baseDatos.InsertAsync(q.QueryInsFacturacion.ToString());

                        if (!insertFacturacion && q.QueryInsFacturacion.ToString() != "")
                        {
                            Console.WriteLine($"Error query: {q.QueryInsFacturacion}");
                            if (q.QueryInsConcentrado.ToString() != "")
                                 await _baseDatos.DeleteAsync(q.QueryDelConcentrado);
                        }
                        else
                        {
                            if (q.QueryInsFacturacion.ToString() != "")
                                await LogFacturacion(q.FacturacionId, "Se genera Factura de la Póliza");

                            var updateMovimientos = await _baseDatos.UpdateAsync(q.QueryUpdMovimientos);
                            if (!updateMovimientos)
                            {
                                Console.WriteLine($"Error query: {q.QueryUpdMovimientos}");
                                var deleteFacturacion = await _baseDatos.DeleteAsync(q.QueryDelFacturacion);
                                var deleteConcentrado = await _baseDatos.DeleteAsync(q.QueryDelConcentrado);
                            }
                            else
                            {
                                await LogConcentrado(q.ConcentradoId, "Se actualiza estatus Movimientos que genera Concentrado");
                                await LogFacturacion(q.FacturacionId, "Se actualiza estatus Movimientos que genera Factura de la Póliza");
                                cont++;
                            }
                        }
                    }
                }

                return new GenericResponse()
                {
                    Codigo = modelo.Count == cont ? 1 : 2,
                    Mensaje = modelo.Count == cont ? $"Sincronización completa, registros obtenenidos {modelo.Count} - registros guardados {cont}." : $"Hubo error con el guardado de los registros, registros obtenenidos {modelo.Count} - registros guardados {cont}. Para más detalles consultar LOG."
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

        /// <summary>
        /// Sincronización Facturas Nota de Debito
        /// </summary>
        /// <returns></returns>
        public async Task<GenericResponse> SincronizacionFacturaNotaDebito()
        {
            try
            {
                var modelo = await _baseDatos.SelectAsync<ConcetradoFactura>(QUERY_MOVIMIENTOS_FACTURANOTADEBITO);
                if (!(modelo?.Any() ?? false))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Sin registros para guardar en POLIZAS_FACTURACION",
                        Data = false
                    };

                var idFacturacion = await _baseDatos.SelectFirstAsync<int>(QUERY_POLIZAS_FACTURACION_ID);
                if (idFacturacion == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla POLIZAS_FACTURACION.",
                        Data = false
                    };

                var querys = modelo.Select(x =>
                {
                    #region Query INSERT POLIZAS_FACTURACION
                    //AGREGAR CONDICIONES PARA EVITAR ERROR AL INSERTAR 31/01/2021
                    if (x.Folio == null)
                    {
                        x.Folio = "";

                    }
                    if (x.Serie == null)
                    {
                        x.Serie = "";
                    }
                    if (x.RfcReceptor == null)
                    {
                        x.RfcReceptor = "";
                    }
                   
                        x.LugarExpedicion = "01900";
                    


                    string nombreReceptor = x.NombreReceptor.Replace("‘", "''");
                    var sbInsertFacturacion = new StringBuilder();
                    sbInsertFacturacion = new StringBuilder();
                    sbInsertFacturacion.Append("INSERT INTO POLIZAS_FACTURACION VALUES(");
                    sbInsertFacturacion.Append($"{idFacturacion}");
                    sbInsertFacturacion.Append($",{x.IdConcentrado}");
                    sbInsertFacturacion.Append($",'{x.TipoComprobante}'");
                    if (x.FechaInicio > DateTime.Now)
                        sbInsertFacturacion.Append($",TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")} {DateTime.Now.ToString("HH:mm:ss")}', 'dd/mm/yyyy hh24:mi:ss')");
                    else
                        sbInsertFacturacion.Append($",SYSDATE");
                    sbInsertFacturacion.Append($",'PSFACT{x.TipoComprobante}'");
                    sbInsertFacturacion.Append($",'{idFacturacion.ToString("D10")}'");
                    sbInsertFacturacion.Append($",'{x.RfcReceptor}'");
                    sbInsertFacturacion.Append($",'{nombreReceptor}'");
                    sbInsertFacturacion.Append($",'{x.CodigoConcepto}'");
                    sbInsertFacturacion.Append($",'{x.CodigoProducto}'");
                    sbInsertFacturacion.Append($",'{x.FormaPago}'");
                    sbInsertFacturacion.Append($",'PPD'");
                    sbInsertFacturacion.Append($",'14120'");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",{x.PrimaNeta}");
                    sbInsertFacturacion.Append($",{x.Financiamiento}");
                    sbInsertFacturacion.Append($",{x.Gasto}");
                    sbInsertFacturacion.Append($",{x.Iva}");
                    sbInsertFacturacion.Append($",{x.Total}");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",0");
                    sbInsertFacturacion.Append($",SYSDATE");
                    sbInsertFacturacion.Append($",SYSDATE");
                    if (ValidarRFC(x.RfcReceptor))
                    {
                        if (x.EstatusFacturaMadre != 2)
                            sbInsertFacturacion.Append($",8)");
                        else if (x.FechaInicio > DateTime.Now)
                            sbInsertFacturacion.Append($",6)");
                        else
                            sbInsertFacturacion.Append($",1)");
                    }
                    else
                    {
                        sbInsertFacturacion.Append($",992)");
                    }

                    var queryUpdMovimientosProcesado = $@"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 2 WHERE id = {x.Id}";

                    //var queryUpdMovimientosNoValido = $@"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 4 WHERE id = {x.Id}";

                    var queryDelFacturacion = $@"DELETE POLIZAS_FACTURACION WHERE ID = {idFacturacion}";

                    var FacturacionId = idFacturacion;

                    //var estatusConcentrado = x.EstatusFacturaMadre;

                    idFacturacion++;

                    return new
                    {
                        FacturacionId,
                        //estatusConcentrado,
                        QueryInsFacturacion = sbInsertFacturacion.ToString(),
                        QueryUpdMovimientosProcesado = queryUpdMovimientosProcesado,
                        //QueryUpdMovimientosNoValido = queryUpdMovimientosNoValido,
                        QueryDelFacturacion = queryDelFacturacion
                    };
                    #endregion
                }).ToList();

                int cont = 0;

                foreach (var q in querys)
                {
                    /*if (q.estatusConcentrado != 2)
                    {
                        await _baseDatos.UpdateAsync(q.QueryUpdMovimientosNoValido);
                    }
                    else
                    {
                        
                    }*/
                    var insertFacturacion = await _baseDatos.InsertAsync(q.QueryInsFacturacion.ToString());

                    if (!insertFacturacion)
                    {
                        Console.WriteLine($"Error query: {q.QueryInsFacturacion}");
                    }
                    else
                    {
                        await LogFacturacion(q.FacturacionId, "Se genera adiciona factura a la Póliza");

                        var updateMovimientos = await _baseDatos.UpdateAsync(q.QueryUpdMovimientosProcesado);
                        if (!updateMovimientos)
                        {
                            Console.WriteLine($"Error query: {q.QueryUpdMovimientosProcesado}");
                            var deleteFacturacion = await _baseDatos.DeleteAsync(q.QueryDelFacturacion);
                        }
                        else
                        {
                            await LogFacturacion(q.FacturacionId, "Se actualiza estatus Movimientos que genera Factura de la Póliza");
                            cont++;
                        }
                    }
                }

                return new GenericResponse()
                {
                    Codigo = modelo.Count == cont ? 1 : 2,
                    Mensaje = modelo.Count == cont ? $"Sincronización completa, registros obtenenidos {modelo.Count} - registros guardados {cont}." : $"Hubo error con el guardado de los registros, registros obtenenidos {modelo.Count} - registros guardados {cont}. Para más detalles consultar LOG."
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

        /// <summary>
        /// Sincronización Facturas Nota de Crédito
        /// </summary>
        /// <returns></returns>
        public async Task<GenericResponse> SincronizacionFacturaNotaCredito()
        {
            try
            {
                var modelo = await _baseDatos.SelectAsync<ConcetradoFactura>(QUERY_MOVIMIENTOS_FACTURANOTACREDITO);
                if (!(modelo?.Any() ?? false))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Sin registros para guardar en POLIZAS_FACTURACION",
                        Data = false
                    };

                var idFacturacion = await _baseDatos.SelectFirstAsync<int>(QUERY_POLIZAS_FACTURACION_ID);
                if (idFacturacion == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla POLIZAS_FACTURACION.",
                        Data = false
                    };

                var querys = modelo.Select(x =>
                {
                    #region Query INSERT POLIZAS_FACTURACION
                    string nombreReceptor = x.NombreReceptor.Replace("‘", "''");
                    var sbInsertFacturacion = new StringBuilder();
                    sbInsertFacturacion.Append("INSERT INTO POLIZAS_FACTURACION ");
                    sbInsertFacturacion.Append("VALUES(");
                    sbInsertFacturacion.Append($"{idFacturacion}");
                    sbInsertFacturacion.Append($",{x.IdConcentrado}");
                    sbInsertFacturacion.Append($",'{x.TipoComprobante}'");
                    sbInsertFacturacion.Append($",SYSDATE");
                    sbInsertFacturacion.Append($",'PSFACT{x.TipoComprobante}'");
                    sbInsertFacturacion.Append($",'{idFacturacion.ToString("D10")}'");
                    sbInsertFacturacion.Append($",'{x.RfcReceptor}'");
                    sbInsertFacturacion.Append($",'{nombreReceptor}'");
                    sbInsertFacturacion.Append($",'{x.CodigoConcepto}'");
                    sbInsertFacturacion.Append($",'{x.CodigoProducto}'");
                    sbInsertFacturacion.Append($",'{x.FormaPago}'");
                    sbInsertFacturacion.Append($",'PPD'");
                    sbInsertFacturacion.Append($",'14120'");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",{x.PrimaNeta}");
                    sbInsertFacturacion.Append($",{x.Financiamiento}");
                    sbInsertFacturacion.Append($",{x.Gasto}");
                    sbInsertFacturacion.Append($",{x.Iva}");
                    sbInsertFacturacion.Append($",{x.Total}");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",0");
                    sbInsertFacturacion.Append($",SYSDATE");
                    sbInsertFacturacion.Append($",SYSDATE");
                    if (ValidarRFC(x.RfcReceptor))
                    {
                        if (x.EstatusFacturaMadre != 2)
                            sbInsertFacturacion.Append($",8)");
                        else if (x.FechaInicio > DateTime.Now)
                            sbInsertFacturacion.Append($",6)");
                        else
                            sbInsertFacturacion.Append($",1)");
                    }
                    else
                    {
                        sbInsertFacturacion.Append($",992)");
                    }


                    var queryUpdMovimientosProcesado = $@"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 2 WHERE id = {x.Id}";

                    //var queryUpdMovimientosNoValido = $@"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 4 WHERE id = {x.Id}";

                    var queryDelFacturacion = $@"DELETE POLIZAS_FACTURACION WHERE ID = {idFacturacion}";

                    var FacturacionId = idFacturacion;

                    //var estatusConcentrado = x.EstatusFacturaMadre;

                    idFacturacion++;

                    return new
                    {
                        FacturacionId,
                        //estatusConcentrado,
                        QueryInsFacturacion = sbInsertFacturacion.ToString(),
                        QueryUpdMovimientosProcesado = queryUpdMovimientosProcesado,
                        //QueryUpdMovimientosNoValido = queryUpdMovimientosNoValido,
                        QueryDelFacturacion = queryDelFacturacion
                    };
                    #endregion
                }).ToList();

                int cont = 0;

                foreach (var q in querys)
                {
                    /*if (q.estatusConcentrado != 2)
                    {
                        await _baseDatos.UpdateAsync(q.QueryUpdMovimientosNoValido);
                    }
                    else
                    {
                    }*/
                    var insertFacturacion = await _baseDatos.InsertAsync(q.QueryInsFacturacion.ToString());

                    if (!insertFacturacion)
                    {
                        Console.WriteLine($"Error query: {q.QueryInsFacturacion}");
                    }
                    else
                    {
                        await LogFacturacion(q.FacturacionId, "Se genera adiciona factura a la Póliza");

                        var updateMovimientos = await _baseDatos.UpdateAsync(q.QueryUpdMovimientosProcesado);
                        if (!updateMovimientos)
                        {
                            Console.WriteLine($"Error query: {q.QueryUpdMovimientosProcesado}");
                            var deleteFacturacion = await _baseDatos.DeleteAsync(q.QueryDelFacturacion);
                        }
                        else
                        {
                            await LogFacturacion(q.FacturacionId, "Se actualiza estatus Movimientos que genera Factura de la Póliza");
                            cont++;
                        }
                    }
                }

                return new GenericResponse()
                {
                    Codigo = modelo.Count == cont ? 1 : 2,
                    Mensaje = modelo.Count == cont ? $"Sincronización completa, registros obtenenidos {modelo.Count} - registros guardados {cont}." : $"Hubo error con el guardado de los registros, registros obtenenidos {modelo.Count} - registros guardados {cont}. Para más detalles consultar LOG."
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

        /// <summary>
        /// Sincronización Facturas de Pago
        /// </summary>
        /// <returns></returns>
        public async Task<GenericResponse> SincronizacionFacturaPagos()
        {
            try
            {
                var modelo = await _baseDatos.SelectAsync<ConcentradoFacturaPago>(QUERY_MOVIMIENTOS_PAGOS);
                if (!(modelo?.Any() ?? false))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Sin registros para guardar en POLIZAS_FACTURACION",
                        Data = false
                    };

                var idFacturacion = await _baseDatos.SelectFirstAsync<int>(QUERY_POLIZAS_FACTURACION_ID);
                if (idFacturacion == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla POLIZAS_FACTURACION.",
                        Data = false
                    };

                var querys = modelo.Select(x =>
                {
                    #region Query INSERT POLIZAS_FACTURACION
                    string nombreReceptor = x.NombreReceptor.Replace("‘", "''");
                    var sbInsertFacturacion = new StringBuilder();
                    sbInsertFacturacion.Append("INSERT INTO POLIZAS_FACTURACION ");
                    sbInsertFacturacion.Append("VALUES(");
                    sbInsertFacturacion.Append($"{idFacturacion}");
                    sbInsertFacturacion.Append($",{x.IdConcentrado}");
                    sbInsertFacturacion.Append($",'{x.TipoComprobante}'");
                    sbInsertFacturacion.Append($",SYSDATE");
                    sbInsertFacturacion.Append($",'PSFACT{x.TipoComprobante}'");
                    sbInsertFacturacion.Append($",'{idFacturacion.ToString("D10")}'");
                    sbInsertFacturacion.Append($",'{x.RfcReceptor}'");
                    sbInsertFacturacion.Append($",'{nombreReceptor}'");
                    sbInsertFacturacion.Append($",'{x.CodigoConcepto}'");
                    sbInsertFacturacion.Append($",'{x.CodigoProducto}'");
                    sbInsertFacturacion.Append($",'{x.FormaPago}'");
                    sbInsertFacturacion.Append($",'PPD'");
                    sbInsertFacturacion.Append($",'14120'");
                    sbInsertFacturacion.Append($",TO_DATE('{x.FechaEmision.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                    sbInsertFacturacion.Append($",{x.PrimaNeta}");
                    sbInsertFacturacion.Append($",{x.Financiamiento}");
                    sbInsertFacturacion.Append($",{x.Gasto}");
                    sbInsertFacturacion.Append($",{x.Iva}");
                    sbInsertFacturacion.Append($",{x.Total}");
                    sbInsertFacturacion.Append($",{x.Total}");
                    sbInsertFacturacion.Append($",{x.Parcialidad}");
                    sbInsertFacturacion.Append($",'{x.Numoperacion}'");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",NULL");
                    sbInsertFacturacion.Append($",0");
                    sbInsertFacturacion.Append($",SYSDATE");
                    sbInsertFacturacion.Append($",SYSDATE");
                    if (ValidarRFC(x.RfcReceptor))
                    {
                        if (x.EstatusFacturaMadre != 2)
                            sbInsertFacturacion.Append($",8)");
                        else if (x.FechaInicio > DateTime.Now)
                            sbInsertFacturacion.Append($",6)");
                        else
                            sbInsertFacturacion.Append($",1)");
                    }
                    else
                    {
                        sbInsertFacturacion.Append($",992)");
                    }


                    var queryUpdMovimientosProcesado = $@"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 2 WHERE id = {x.Id}";

                    //var queryUpdMovimientosNoValido = $@"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 4 WHERE id = {x.Id}";

                    var queryDelFacturacion = $@"DELETE POLIZAS_FACTURACION WHERE ID = {idFacturacion}";

                    var FacturacionId = idFacturacion;

                    //var estatusConcentrado = x.EstatusFacturaMadre;

                    idFacturacion++;

                    return new
                    {
                        FacturacionId,
                        //estatusConcentrado,
                        QueryInsFacturacion = sbInsertFacturacion.ToString(),
                        QueryUpdMovimientosProcesado = queryUpdMovimientosProcesado,
                        //QueryUpdMovimientosNoValido = queryUpdMovimientosNoValido,
                        QueryDelFacturacion = queryDelFacturacion
                    };
                    #endregion
                }).ToList();

                int cont = 0;

                foreach (var q in querys)
                {
                    var insertFacturacion = await _baseDatos.InsertAsync(q.QueryInsFacturacion.ToString());

                    if (!insertFacturacion)
                    {
                        Console.WriteLine($"Error query: {q.QueryInsFacturacion}");
                    }
                    else
                    {
                        await LogFacturacion(q.FacturacionId, "Se genera adiciona factura a la Póliza");

                        var updateMovimientos = await _baseDatos.UpdateAsync(q.QueryUpdMovimientosProcesado);
                        if (!updateMovimientos)
                        {
                            Console.WriteLine($"Error query: {q.QueryUpdMovimientosProcesado}");
                            var deleteFacturacion = await _baseDatos.DeleteAsync(q.QueryDelFacturacion);
                        }
                        else
                        {
                            await LogFacturacion(q.FacturacionId, "Se actualiza estatus Movimientos que genera Factura de la Póliza");
                            cont++;
                        }
                    }
                }

                return new GenericResponse()
                {
                    Codigo = modelo.Count == cont ? 1 : 2,
                    Mensaje = modelo.Count == cont ? $"Sincronización completa, registros obtenenidos {modelo.Count} - registros guardados {cont}." : $"Hubo error con el guardado de los registros, registros obtenenidos {modelo.Count} - registros guardados {cont}. Para más detalles consultar LOG."
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

        /// <summary>
        /// Sincronización Refacturación
        /// </summary>
        /// <returns></returns>
        public async Task<GenericResponse> SincronizacionRefacturacion()
        {
            try
            {
                var modelo = await _baseDatos.SelectAsync<ConcetradoFactura>(QUERY_MOVIMIENTOS_REFACTURACION);
                if (!(modelo?.Any() ?? false))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Sin registros para REFACTURAR",
                        Data = false
                    };

                var idFacturacion = await _baseDatos.SelectFirstAsync<int>(QUERY_POLIZAS_FACTURACION_ID);
                if (idFacturacion == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla POLIZAS_FACTURACION.",
                        Data = false
                    };

                string query;
                int cont = 0;

                var querys = modelo.Select(x =>
                {
                    query = $"SELECT * FROM POLIZAS_CONCENTRADO WHERE SISTEMA = '{x.Sistema}' AND POLIZA = '{x.Poliza}' AND ANIOINICIO = '{x.FechaInicio.ToString("yyyy")}'";
                    Concentrado concen = _baseDatos.SelectFirst<Concentrado>(query);
                    if (concen != null)
                    {
                        query = $"SELECT * FROM POLIZAS_FACTURACION WHERE POLIZASID = {concen.Id} AND POLIZAMADRE = 1";
                        DboFacturacion fac = _baseDatos.SelectFirst<DboFacturacion>(query);
                        if (fac != null)
                        {
                            query = $"UPDATE POLIZAS_CONCENTRADO SET PRIMANETAM = {x.PrimaNeta}, FINANCIAMIENTOM = {x.Financiamiento}, GASTOM = {x.Gasto}, IVAM = {x.Iva}, TOTALM = {x.Total}, FECHAMODIFICACION = SYSDATE WHERE id = {concen.Id}";
                            _baseDatos.Update(query);

                            if(fac.PrimaNeta == x.PrimaNeta){
                                query = $"UPDATE POLIZAS_FACTURACION SET ESTATUSFACTURACIONID = 3, RFCRECEPTOR = '{x.RfcReceptor}', NOMBRERECEPTOR = '{x.NombreReceptor}', FECHAMODIFICACION = SYSDATE WHERE ID = {fac.Id}";
                            } else {
                                query = $"UPDATE POLIZAS_FACTURACION SET ESTATUSFACTURACIONID = 3, RFCRECEPTOR = '{x.RfcReceptor}', NOMBRERECEPTOR = '{x.NombreReceptor}',  PRIMANETA = {x.PrimaNeta},  FINANCIAMIENTO = {x.Financiamiento},  GASTO = {x.Gasto},  IVA = {x.Iva}, TOTAL = {x.Total}, FECHAMODIFICACION = SYSDATE WHERE ID = {fac.Id}";
                            }
                            _baseDatos.Update(query);

                            query = $"UPDATE POLIZAS_FACTURACION SET ESTATUSFACTURACIONID = 3, RFCRECEPTOR = '{x.RfcReceptor}', NOMBRERECEPTOR = '{x.NombreReceptor}', FECHAMODIFICACION = SYSDATE WHERE POLIZASID = {fac.PolizasId} AND POLIZAMADRE != 1";
                            _baseDatos.Update(query);

                            query = $"UPDATE POLIZAS_MOVIMIENTOS SET ESTATUSMOVIMIENTOID = 2 WHERE id = {x.Id}";
                            _baseDatos.Update(query);

                            cont++;
                        }
                    }
                    return new { };
                }).ToList();

                return new GenericResponse()
                {
                    Codigo = modelo.Count == cont ? 1 : 2,
                    Mensaje = modelo.Count == cont ? $"Sincronización completa, registros obtenenidos {modelo.Count} - registros guardados {cont}." : $"Hubo error con el guardado de los registros, registros obtenenidos {modelo.Count} - registros guardados {cont}. Para más detalles consultar LOG."
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

        /// <summary>
        /// Sincronización de Estatus Facturación de FACTURACION_COMPROBANTE a POLIZAS_FACTURACION
        /// </summary>
        /// <returns>Modelo GenericResponse</returns>
        public async Task<GenericResponse> SincronizarEstatusFacturacion()
        {
            try
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

        /// <summary>
        /// Actualización de Estatus Facturación de las Facturas Programadas 
        /// </summary>
        /// <returns></returns>
        public async Task<GenericResponse> ActualizarEstatusFacturacionProgramados()
        {
            try
            {
                var estatus = await _baseDatos.SelectAsync<FacturaPolizaEstatus>(QUERY_ESTATUSFACTURACIONPROGRAMADOS);
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

        /// <summary>
        /// Actualización de Estatus Facturación de las Facturas donde la Factura Global estaba pendiente de Timbrar 
        /// </summary>
        /// <returns></returns>
        public async Task<GenericResponse> ActualizarEstatusFacturacionXTimbrarFacturaGlobal()
        {
            try
            {
                var estatus = await _baseDatos.SelectAsync<FacturaPolizaEstatus>(QUERY_ESTATUSFACTURACIONPORTIMBRARFACTURAGLOBAL);
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
