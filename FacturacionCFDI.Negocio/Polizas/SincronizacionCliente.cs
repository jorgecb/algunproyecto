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

//CAMBIAR RUE Y RUP

namespace FacturacionCFDI.Negocio.Polizas
{
    public class SincronizacionCliente
    {
        private readonly BaseDatos _baseDatosCliente;
        private readonly BaseDatos _baseDatosPolizas;

        private string _queryClienteTablaRUE;
        private string _queryClienteTablaRUP;

        private const string IDENTIFICADOR_TABLA_QUERY = "@TABLA";
        private const string QUERY_POLIZAS_LOGMOVIMIENTOS_ID = "SELECT nvl(MAX(ID) + 1, 1) FROM polizas_logmovimientos";
        private const string QUERY_CLIENTE_RUE = "SELECT 'RUE' AS datoorigen, 'I' AS tiposolicitud, CASE WHEN TRIM(causaendoso) = 'REN 401' THEN 'I' WHEN TRIM(causaendoso) = 'MOE 505 / PON 505' THEN 'I' WHEN TRIM(causaendoso) = 'MOE 504 / PON 504' THEN 'I' WHEN TRIM(causaendoso) = 'PON 474' THEN 'I' WHEN TRIM(causaendoso) = 'PON 475' THEN 'I' WHEN TRIM(causaendoso) = 'REN 290' THEN 'I' WHEN TRIM(causaendoso) = 'REN 525' THEN 'I' WHEN TRIM(causaendoso) = 'REN 69' THEN 'I' WHEN TRIM(causaendoso) = 'REN 42' THEN 'I' WHEN TRIM(causaendoso) = 'REN 70' THEN 'I' WHEN TRIM(causaendoso) = 'REN 86' THEN 'I' WHEN TRIM(causaendoso) = 'EMI' THEN 'I' WHEN TRIM(causaendoso) = 'REH 37' THEN 'E' WHEN TRIM(causaendoso) = 'REA 148' THEN 'E' WHEN TRIM(causaendoso) = 'REHABI' THEN 'E' WHEN TRIM(causaendoso) = 'REH' THEN 'E' WHEN TRIM(causaendoso) = 'CFP 54' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'CFP 54' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'CAP 55' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'CAP 55' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'CAP 155' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'CAP 155' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'REC 63' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'REC 63' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'MOE 184' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'MOE 184' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'REI 482' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'REI 482' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'REI 145' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'REI 145' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'MOE 184' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'MOE 184' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'AJU XXX' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'AJU XXX' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'ANULAC' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'ANULAC' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'MOD 57' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) > 0 THEN 'I' WHEN TRIM(causaendoso) = 'MOD 57' AND round((SUM(NVL(primaneta,0)) + SUM(NVL(financiamiento,0)) + SUM(NVL(gasto,0))) * 1.16, 2) < 0 THEN 'E' WHEN TRIM(causaendoso) = 'BAJ 187' THEN 'E' WHEN TRIM(causaendoso) = 'CSC 38' THEN 'E' WHEN TRIM(causaendoso) = 'CNP 38' THEN 'E' WHEN TRIM(causaendoso) = 'DEV 40' THEN 'E' WHEN TRIM(causaendoso) = 'CYR 38' THEN 'E' WHEN TRIM(causaendoso) = 'SUS XXX' THEN 'E' WHEN TRIM(causaendoso) = 'ENDDIS' THEN 'E' WHEN TRIM(causaendoso) = 'BAJ' THEN 'E' WHEN TRIM(causaendoso) = 'CANMAN' THEN 'E' WHEN TRIM(causaendoso) = 'CANAUT' THEN 'E' WHEN TRIM(causaendoso) = 'CANMAN' THEN 'E' WHEN TRIM(causaendoso) = 'CANAUT' THEN 'E' WHEN TRIM(causaendoso) = 'ALT 457' THEN 'I' WHEN TRIM(causaendoso) = 'ALT 48' THEN 'I' WHEN TRIM(causaendoso) = 'ALT 458' THEN 'I' WHEN TRIM(causaendoso) = 'ALT 49' THEN 'I' WHEN TRIM(causaendoso) = 'ALT 480' THEN 'I' WHEN TRIM(causaendoso) = 'ALT 478' THEN 'I' WHEN TRIM(causaendoso) = 'ALT 477' THEN 'I' WHEN TRIM(causaendoso) = 'ALT 476' THEN 'I' WHEN TRIM(causaendoso) = 'ALT 489 / PON 489' THEN 'I' WHEN TRIM(causaendoso) = 'ALT 493 / PON 489' THEN 'I' WHEN TRIM(causaendoso) = 'ENDAJU' THEN 'I' WHEN TRIM(causaendoso) = 'ALT' THEN 'I' WHEN TRIM(causaendoso) = 'ENDOSO' THEN 'R' WHEN TRIM(causaendoso) = 'MOD 527' THEN 'R' WHEN TRIM(causaendoso) = 'MNC 52 / MNA 52' THEN 'R' WHEN TRIM(causaendoso) = 'MFC 53 / MNC 53' THEN 'R' END AS tipomovimiento, TRIM(sistema) AS sistema, TRIM(poliza) AS poliza, fechaemision AS fechaemision, fechainicio AS fechainicio, fechatermino AS fechatermino, TRIM(causaendoso) AS causaendoso, TRIM(estatuspoliza) AS estatuspoliza, TRIM(subramo) AS tipodepoliza, TRIM(rfc_cont) AS rfccontratante, TRIM(contratante) AS nombrecontratante, TRIM(tipopersona_cont) AS tipopersonacontratante, '' AS rfcasegurado, '' AS nombreasegurado, '' AS tipopersonaasegurado, '' AS pagorfc, '' AS pagonombre, '' AS pagotipopersona, '' AS pagofechapago, '' AS pagoformapago, '' AS pagometodopago, '' AS pagoparcialidad, '' AS pagooperacion, '' AS uuidfactura, TRIM(cod_producto) AS codigoproducto, TRIM(cod_formapago) AS formapago, '' AS lugarexpedicion, TRIM(cod_ramocnsf) AS codigoconcepto, round(SUM(NVL(primaneta,0)),2) AS primaneta, round(SUM(NVL(financiamiento,0)),2) AS financiamiento, round(SUM(NVL(gasto,0)),2) AS gasto, round((round(SUM(NVL(primaneta,0)),2) + round(SUM(NVL(financiamiento,0)),2) + round(SUM(NVL(gasto,0)),2)) *.16, 2) AS iva, round((round(SUM(NVL(primaneta,0)),2) + round(SUM(NVL(financiamiento,0)),2) + round(SUM(NVL(gasto,0)),2)) * 1.16, 2) AS total, sysdate AS fechacreacion, sysdate AS fechamodificacion, 1 AS estatusmovimientoid, 'servicio' AS solicitante, TRIM(sistema) || TRIM(poliza) || to_char(fechainicio, 'YYYY') || replace(causaendoso, ' ', '') || CAST(round((round(SUM(NVL(primaneta,0)),2) + round(SUM(NVL(financiamiento,0)),2) + round(SUM(NVL(gasto,0)),2)) * 1.16, 2) AS VARCHAR(30)) AS llavesincronizacion FROM RUE WHERE TRIM(causaendoso) IN( 'REN 401', 'MOE 505 / PON 505', 'MOE 504 / PON 504', 'PON 474', 'PON 475', 'REN 290', 'REN 525', 'REN 69', 'REN 42', 'REN 70', 'REN 86', 'EMI', 'REH 37', 'REA 148', 'REHABI', 'REH', 'CFP 54', 'CAP 55', 'CAP 155', 'REC 63', 'MOE 184', 'REI 482', 'REI 145', 'MOE 184', 'AJU XXX', 'ANULAC', 'BAJ 187', 'CSC 38', 'CNP 38', 'DEV 40', 'CYR 38', 'SUS XXX', 'ENDDIS', 'BAJ', 'MOD 57', 'ALT 457', 'ALT 48', 'ALT 458', 'ALT 49', 'ALT 480', 'ALT 478', 'ALT 477', 'ALT 476', 'ALT 489 / PON 489', 'ALT 493 / PON 489', 'ENDAJU', 'ALT', 'ENDOSO', 'MOD 527', 'MNC 52 / MNA 52', 'MFC 53 / MNC 53', 'CANMAN', 'CANAUT', 'CANMAN', 'CANAUT') AND estatuspoliza = 'VIGENTE' GROUP BY TRIM(causaendoso), 'RUE', causaendoso, 'I', sistema, TRIM(poliza), poliza, fechaemision, fechainicio, fechatermino, TRIM(estatuspoliza), estatuspoliza, TRIM(subramo), subramo, TRIM(rfc_cont), rfc_cont, TRIM(contratante), contratante, TRIM(tipopersona_cont), tipopersona_cont, TRIM(cod_producto), cod_producto, TRIM(cod_formapago), cod_formapago, TRIM(cod_ramocnsf), cod_ramocnsf, TRIM(sistema), '', sysdate, 1, 'servicio'";
        private const string QUERY_CLIENTE_RUP = "SELECT 'RUP' AS datoorigen, 'I' AS tiposolicitud, 'P' AS tipomovimiento, TRIM(sistema) AS sistema, TRIM(poliza) AS poliza, NULL AS fechaemision, fechainicio AS fechainicio, fechatermino AS fechatermino, NULL AS causaendoso, NULL AS estatuspoliza, TRIM(subramo) AS tipodepoliza, '' AS rfccontratante, '' AS nombrecontratante, '' AS tipopersonacontratante, '' AS rfcasegurado, '' AS nombreasegurado, '' AS tipopersonaasegurado, TRIM(rfc) AS pagorfc, TRIM(contratante) AS pagonombre, TRIM(tipo_persona) AS pagotipopersona, fechapago AS pagofechapago, TRIM(cod_formapago) AS pagoformapago, TRIM(cod_metodopago) AS pagometodopago, TRIM(cuota) AS pagoparcialidad, TRIM(id_pago) AS pagooperacion, '' AS uuidfactura, '' AS codigoproducto, '' AS formapago, '' AS lugarexpedicion, TRIM(cod_ramo) AS codigoconcepto, round(SUM(NVL(primaneta,0)),2) AS primaneta, round(SUM(NVL(financiamiento,0)),2) AS financiamiento, round(SUM(NVL(gasto,0)),2) AS gasto, round((round(SUM(NVL(primaneta,0)),2) + round(SUM(NVL(financiamiento,0)),2) + round(SUM(NVL(gasto,0)),2)) *.16, 2) AS iva, round((round(SUM(NVL(primaneta,0)),2) + round(SUM(NVL(financiamiento,0)),2) + round(SUM(NVL(gasto,0)),2)) * 1.16, 2) AS total, sysdate AS fechacreacion, sysdate AS fechamodificacion, 1 AS estatusmovimientoid, 'servicio' AS solicitante, TRIM(sistema) || TRIM(poliza) || to_char(fechainicio, 'YYYY') || 'PAGO' || TRIM(cuota) || CAST(round((round(SUM(NVL(primaneta,0)),2) + round(SUM(NVL(financiamiento,0)),2) + round(SUM(NVL(gasto,0)),2)) * 1.16, 2) AS VARCHAR(30)) AS llavesincronizacion FROM RUP GROUP BY TRIM(sistema), TRIM(poliza), fechainicio, fechatermino, TRIM(subramo), TRIM(rfc), TRIM(contratante), TRIM(tipo_persona), fechapago, TRIM(cod_formapago), TRIM(cod_metodopago), TRIM(cuota), TRIM(id_pago), TRIM(cod_ramo)";
        private const string QUERY_POLIZAS_MOVIMIENTOS = "SELECT llavesincronizacion FROM polizas_movimientos WHERE datoorigen = '@TABLA' GROUP BY llavesincronizacion";


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseDatosCliente">Instancia Base de Datos Cliente</param>
        /// <param name="baseDatosPolizas">Instancia Base de Datos Intermedia</param>
        public SincronizacionCliente(BaseDatos baseDatosCliente, BaseDatos baseDatosPolizas)
        {
            _baseDatosCliente = baseDatosCliente;
            _baseDatosPolizas = baseDatosPolizas;
            QuerySincronizacion();
        }

        /// <summary>
        /// Guarda Información de los Movimientos en las Tablas Intermedia
        /// </summary>
        /// <returns>Modelo GenericResponse</returns>
        public async Task<GenericResponse> SincronizarInformacionCliente()
        {
            int idMovimiento;
            string query;
            try
            {
                var rue = SincronizacionTablaRue();
                if (rue == false) {
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Hubo un error en el RUE.",
                        Data = false
                    };
                }
                var rup = SincronizacionTablaRup();
                if (rup == false)
                {
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Hubo un error en el RUP.",
                        Data = false
                    };
                }
                var modelo = await ObtenerMovimientosCliente();
                //var modelo = await _baseDatosCliente.SelectAsync<Movimientos>(QUERY_CLIENTE_RUE);

                if (!(modelo?.Any() ?? false))
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "Sin registros para guardar en POLIZAS_MOVIMIENTOS",
                        Data = false
                    };

                query = "SELECT NVL(MAX(ID) + 1, 1) FROM POLIZAS_MOVIMIENTOS";
                idMovimiento = await _baseDatosPolizas.SelectFirstAsync<int>(query);

                if (idMovimiento == 0)
                    return new GenericResponse()
                    {
                        Codigo = 2,
                        Mensaje = "No se pudo obtener ID de la Tabla POLIZAS_MOVIMIENTOS.",
                        Data = false
                    };

                DateTime mesActual = DateTime.UtcNow;

                var mov = new List<Movimientos>();

                foreach (var m in modelo)
                {
                    var d = mov.Where(x => x.LlaveSincronizacion.Equals(m.LlaveSincronizacion)).Select(x => x.LlaveSincronizacion).FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(d))
                        mov.Add(m);
                }
                int cont = 0;
                int count2 = 0;
                Console.WriteLine($"Obtuve lista para añadir a movimientos - Count: {mov.Count}");
                Parametros lugarexpedicion = _baseDatosPolizas.SelectFirst<Parametros>("SELECT * FROM PARAMETRO WHERE PARAMETRO = 'LUGAREXPEDICION'");

                foreach (var x in mov){
                    var sbInsert = new StringBuilder();
                    sbInsert.Append("INSERT INTO POLIZAS_MOVIMIENTOS VALUES(");
                    sbInsert.Append($"{idMovimiento}");
                    sbInsert.Append($",'{x.DatoOrigen}'");
                    sbInsert.Append($",'{x.TipoSolicitud}'");
                    sbInsert.Append($",'{x.TipoMovimiento}'");
                    sbInsert.Append($",'{x.Sistema}'");
                    sbInsert.Append($",'{x.Poliza}'");
                    if (string.IsNullOrWhiteSpace(x.FechaEmision.ToString())) sbInsert.Append($",NULL"); else sbInsert.Append($",TO_DATE('{x.FechaEmision.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                    if (string.IsNullOrWhiteSpace(x.FechaInicio.ToString())) sbInsert.Append($",NULL"); else sbInsert.Append($",TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                    if (string.IsNullOrWhiteSpace(x.FechaTermino.ToString())) sbInsert.Append($",NULL"); else sbInsert.Append($",TO_DATE('{x.FechaTermino.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                    if (x.DatoOrigen == "RUP") sbInsert.Append($",'PAGO'"); else sbInsert.Append($",'{x.CausaEndoso}'");
                    if (string.IsNullOrWhiteSpace(x.EstatusPoliza)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.EstatusPoliza}'");
                    if (string.IsNullOrWhiteSpace(x.TipoDePoliza)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.TipoDePoliza}'");
                    if (string.IsNullOrWhiteSpace(x.RfcContratante)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.RfcContratante}'");
                    if (string.IsNullOrWhiteSpace(x.NombreContratante)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.NombreContratante}'");
                    if (string.IsNullOrWhiteSpace(x.TipoPersonaContratante)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.TipoPersonaContratante}'");
                    if (string.IsNullOrWhiteSpace(x.RfcAsegurado)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.RfcAsegurado}'");
                    if (string.IsNullOrWhiteSpace(x.NombreAsegurado)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.NombreAsegurado}'");
                    if (string.IsNullOrWhiteSpace(x.TipoPersonaAsegurado)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.TipoPersonaAsegurado}'");
                    if (string.IsNullOrWhiteSpace(x.PagoRfc)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.PagoRfc}'");
                    if (string.IsNullOrWhiteSpace(x.PagoNombre)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.PagoNombre}'");
                    if (string.IsNullOrWhiteSpace(x.PagoTipoPersona)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.PagoTipoPersona}'");
                    if (string.IsNullOrWhiteSpace(x.PagoFechaPago.ToString())) sbInsert.Append($",NULL"); else sbInsert.Append($",TO_DATE('{x.PagoFechaPago.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                    if (string.IsNullOrWhiteSpace(x.PagoFormaPago)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.PagoFormaPago}'");
                    if (string.IsNullOrWhiteSpace(x.PagoMetodoPago)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.PagoMetodoPago}'");
                    if (x.PagoParcialidad == null) sbInsert.Append($",NULL"); else sbInsert.Append($",{x.PagoParcialidad}");
                    if (string.IsNullOrWhiteSpace(x.PagoOperacion)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.PagoOperacion}'");
                    if (string.IsNullOrWhiteSpace(x.UuidFactura)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.UuidFactura}'");
                    if (string.IsNullOrWhiteSpace(x.CodigoProducto)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.CodigoProducto}'");
                    if (string.IsNullOrWhiteSpace(x.FormaPago)) sbInsert.Append($",NULL"); else sbInsert.Append($",'{x.FormaPago}'");
                    sbInsert.Append($",'{lugarexpedicion.Valor}'");
                    sbInsert.Append($",'{x.CodigoConcepto}'");
                    sbInsert.Append($",{x.PrimaNeta}");
                    sbInsert.Append($",{x.Financiamiento}");
                    sbInsert.Append($",{x.Gasto}");
                    sbInsert.Append($",{x.Iva}");
                    sbInsert.Append($",{x.Total}");
                    sbInsert.Append($",SYSDATE");
                    sbInsert.Append($",SYSDATE");
                    sbInsert.Append($",{x.EstatusMovimientoId}");
                    sbInsert.Append($",'{x.Solicitante}'");
                    sbInsert.Append($",'{x.LlaveSincronizacion}'");
                    if (x.IdSolicitudExterna == null) sbInsert.Append($",NULL)"); else sbInsert.Append($",{x.IdSolicitudExterna})");

                    var insertMov = _baseDatosPolizas.Insert(sbInsert.ToString());
                    if (insertMov == true)
                    {
                        cont++;
                        idMovimiento++;
                    } else {
                        LogMovimientos(x.LlaveSincronizacion, $"Hubo un error para insertar a movimientos");
                    }
                    count2++;
                }

                return new GenericResponse()
                {
                    Codigo = count2 == cont ? 1 : 2,
                    Mensaje = count2 == cont ? $"Sincronización completa, registros obtenenidos {count2} - registros guardados {cont}." : $"Hubo error con el guardado de los registros, registros obtenenidos {count2} - registros guardados {cont}. Para más detalles consultar LOG."
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
        /// Obtiene Información de los Movimientos de las tablas RUE y RUP
        /// </summary>
        /// <returns>Lista modelo Movimientos</returns>
        private async Task<List<Movimientos>> ObtenerMovimientosCliente()
        {
            var consulta = new List<Movimientos>();
            string query;

            try
            {
                var consultaRUE = await _baseDatosPolizas.SelectAsync<Movimientos>(_queryClienteTablaRUE);

                var consultaRUP = await _baseDatosPolizas.SelectAsync<Movimientos>(_queryClienteTablaRUP);

                if ((consultaRUE?.Any() ?? false) || (consultaRUP?.Any() ?? false))
                {
                    query = QUERY_POLIZAS_MOVIMIENTOS.Replace(IDENTIFICADOR_TABLA_QUERY, consultaRUE.FirstOrDefault().DatoOrigen);
                    var llavesRUE = await _baseDatosPolizas.SelectAsync<string>(query);
                    var movimientosRUE = consultaRUE.Where(x => !llavesRUE.Contains(x.LlaveSincronizacion)).ToList();

                    query = QUERY_POLIZAS_MOVIMIENTOS.Replace(IDENTIFICADOR_TABLA_QUERY, consultaRUP.FirstOrDefault().DatoOrigen);
                    var llavesRUP = await _baseDatosPolizas.SelectAsync<string>(query);
                    var movimientosRUP = consultaRUP.Where(x => !llavesRUP.Contains(x.LlaveSincronizacion)).ToList();

                    consulta = movimientosRUE.Concat(movimientosRUP).ToList();

                    return consulta;
                }
                else
                {
                    return default;
                }
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public bool SincronizacionTablaRue()
        {
            string query;
            RUE query2 = null;
            DateTime mesActual = DateTime.UtcNow;
            DateTime mesAnterior = mesActual.AddMonths(-1);
            int dia = Int32.Parse(mesActual.Day.ToString());
            int año = mesActual.Year;
            string mesActualLetra = mesActual.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")).ToUpper();
            string mesAnteriorLetra = mesAnterior.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")).ToUpper();
            string tabla_anterior;

            if (mesActualLetra == "ENERO")
                tabla_anterior = $"RUE_{mesAnteriorLetra}_{(año - 1).ToString()}";
            else
                tabla_anterior = $"RUE_{mesAnteriorLetra}_{año.ToString()}";

            string tabla_actual = $"RUE_{mesActualLetra}_{año.ToString()}";

            try
            {
                List<RUE> rue_list_anterior, rue_list_actual;
                List<RUE> rue_list = null;

                query = $"SELECT * FROM OSIRIS.{tabla_actual}";
                rue_list_actual = _baseDatosCliente.Select<RUE>(query);

                if (dia <= 15)
                {
                    query = $"SELECT * FROM OSIRIS.{tabla_anterior}";
                    rue_list_anterior = _baseDatosCliente.Select<RUE>(query);
                    if (rue_list_actual == null){
                        if(rue_list_anterior != null)
                        {
                            rue_list = rue_list_anterior.ToList();
                            Console.WriteLine($"Obtuvo la lista de {tabla_anterior} --- Count: " + rue_list.Count());
                        }
                            
                    } else {
                        if (rue_list_anterior == null)
                        {
                            rue_list = rue_list_actual.ToList();
                            Console.WriteLine($"Obtuvo la lista de {tabla_actual} --- Count: " + rue_list.Count());
                        } else {
                            rue_list = rue_list_actual.Concat(rue_list_anterior).ToList();
                            Console.WriteLine($"Obtuvo la lista de {tabla_anterior} - {tabla_actual} --- Count: " + rue_list.Count());
                        }
                    }
                } else {
                    rue_list = rue_list_actual.ToList();
                    Console.WriteLine($"Obtuvo la lista de {tabla_actual} --- Count: " + rue_list.Count());
                }

                
                Parametros rowCausas = _baseDatosPolizas.SelectFirst<Parametros>("SELECT * FROM PARAMETRO WHERE ID = 18");
                int cont = 0;

                if (rowCausas == null)
                {
                    Console.WriteLine("Hay un error con el parametro de las causas de endoso disponibles (PARAMETROS ID 18)");
                    return false;
                }
                    

                string causaEndoso = "";
                string[] causasEndoso = rowCausas.Valor.Split(',');
                Console.WriteLine("Se obtuvo la lista de causas de endoso");
                // SISTEMA - POLIZA - AÑO/INICIO - ENDOSO - MONTO
                foreach(var x in rue_list) { 
                    
                    query2 = x;
                    var sbInsert = new StringBuilder();
                    var llave = new StringBuilder();
                    if (string.IsNullOrWhiteSpace(x.Sistema)) llave.Append("-/"); else llave.Append($"{x.Sistema}/");
                    if (string.IsNullOrWhiteSpace(x.Poliza)) llave.Append("-/"); else llave.Append($"{x.Poliza}/");
                    if (string.IsNullOrWhiteSpace(x.FechaInicio.ToString())) llave.Append("-/"); else llave.Append($"{x.FechaInicio}/");
                    if (string.IsNullOrWhiteSpace(x.CausaEndoso)) llave.Append("-/"); else llave.Append($"{x.CausaEndoso}/");
                    llave.Append($"{x.PrimaNeta}/");
                    llave.Append($"{x.Financiamiento}/");
                    llave.Append($"{x.Gasto}");
                    bool errorRow = false;
                    string errorDescrip = "";
                    if (x.Cod_Ramo.Length > 2)
                    {
                        errorRow = true;
                        errorDescrip = "El codigo de ramo no es numerico o es mayor a 2 caracteres";
                    }
                    if (x.Sistema == null || x.Poliza == null || x.FechaInicio == null || x.CausaEndoso == null || errorRow == true)
                    {
                        if (errorRow == false)
                            query = $@"INSERT INTO RUE_NO_CARGADOS VALUES('Los campos de la llave de sincronización no estan completos LLAVE: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'), 1002)";
                        else
                            query = $@"INSERT INTO RUE_NO_CARGADOS VALUES('{errorDescrip} LLAVE: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'), 1002)";
                        
                        var jala = _baseDatosPolizas.Insert(query);
                        if (jala == false) {
                            Console.WriteLine(query);
                        }
                    } else {
                        foreach (var rowCausa in causasEndoso)
                        {
                            if (x.CausaEndoso.Contains(rowCausa))
                            {
                                causaEndoso = rowCausa;
                            }
                        }
                        var llavesincronizacion = new StringBuilder();
                        llavesincronizacion.Append($"{x.Sistema}");
                        llavesincronizacion.Append($"{x.Poliza}");
                        llavesincronizacion.Append($"{x.FechaInicio.ToString("yyyy")}");
                        llavesincronizacion.Append($"{causaEndoso.Replace(" ", "")}");
                        var prima = x.PrimaNeta + x.Financiamiento + x.Gasto;
                        prima = prima * 1.16m;
                        prima = Math.Round(prima, 2);
                        String cadena = prima.ToString();
                        char last_char = cadena[cadena.Length - 1];
                        if (last_char == '0')
                        {
                            cadena = cadena.Substring(0, cadena.Length - 1);
                            last_char = cadena[cadena.Length - 1];
                            if (last_char == '0')
                            {
                                cadena = cadena.Substring(0, cadena.Length - 2);
                            }
                        }
                        // SISTEMA - POLIZA - AÑO/INICIO - ENDOSO - MONTO
                        String txt_prima = cadena;
                        llavesincronizacion.Append($"{txt_prima}");

                        query = $"SELECT * FROM POLIZAS_MOVIMIENTOS WHERE LLAVESINCRONIZACION = '{llavesincronizacion}'";
                        Movimientos row = _baseDatosPolizas.SelectFirst<Movimientos>(query);

                        query = $"SELECT * FROM RUE WHERE SISTEMA = '{x.Sistema}' AND POLIZA = '{x.Poliza}' AND FECHAINICIO = TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy') AND PRIMANETA = {x.PrimaNeta} AND FINANCIAMIENTO = {x.Financiamiento} AND GASTO = {x.Gasto} AND CAUSAENDOSO = '{x.CausaEndoso}'";
                        RUE rue = _baseDatosPolizas.SelectFirst<RUE>(query);

                        var error = 0;
                        if (causaEndoso == "ENDOSO" || causaEndoso == "MOD 527" || causaEndoso == "MNC 52 / MNA 52" || causaEndoso == "MFC 53 / MNC 53")
                        {
                            error = 998;
                            query = $"SELECT * FROM POLIZAS_CONCENTRADO WHERE SISTEMA = '{x.Sistema}' AND POLIZA = '{x.Poliza}' AND ANIOINICIO = '{x.FechaInicio.ToString("yyyy")}'";
                            Concentrado concen = _baseDatosPolizas.SelectFirst<Concentrado>(query);
                            if (concen != null)
                            {
                                query = $"SELECT * FROM POLIZAS_FACTURACION WHERE POLIZASID = {concen.Id}";
                                DboFacturacion fac = _baseDatosPolizas.SelectFirst<DboFacturacion>(query);
                                if (fac != null)
                                {
                                    if (fac.RfcReceptor != x.Rfc_Cont || fac.NombreReceptor != x.Contratante || fac.PrimaNeta != x.PrimaNeta)
                                    {
                                        error = 0;
                                    }
                                }
                            }
                        }
                        if (prima == 0)
                        {
                            error = 999;
                        }
                        if (row != null || rue != null)
                        {
                            error = 1000;
                        }
                        if(causaEndoso == "")
                        {
                            error = 1001;
                        }
                        switch (error)
                        {
                            case 0:
                                {
                                    sbInsert.Append("INSERT INTO RUE VALUES(");
                                    sbInsert.Append($"'{x.Sistema}'");
                                    sbInsert.Append($",TO_DATE('{x.FechaEmision.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",{x.AmarreOrden}");
                                    sbInsert.Append($",'{x.Poliza}'");
                                    sbInsert.Append($",'{x.CT}'");
                                    sbInsert.Append($",TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",TO_DATE('{x.FechaTermino.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{x.Endoso}'");
                                    sbInsert.Append($",TO_DATE('{x.FechaIniEndoso.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",TO_DATE('{x.FechaFinEndoso.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{causaEndoso}'");
                                    sbInsert.Append($",'{x.Cod_Ramo}'");
                                    sbInsert.Append($",'{x.Des_Ramo}'");
                                    sbInsert.Append($",'{x.SubRamo}'");
                                    sbInsert.Append($",'{x.Cod_Ramocnsf}'");
                                    sbInsert.Append($",'{x.Desc_Ramocnsf}'");
                                    sbInsert.Append($",'{x.Cod_Producto}'");
                                    sbInsert.Append($",'{x.Producto}'");
                                    sbInsert.Append($",'{x.Cod_Plan}'");
                                    sbInsert.Append($",'{x.Plan}'");
                                    sbInsert.Append($",{x.PrimaNeta}");
                                    sbInsert.Append($",{x.Financiamiento}");
                                    sbInsert.Append($",{x.Gasto}");
                                    sbInsert.Append($",{x.Iva}");
                                    sbInsert.Append($",TO_DATE('{x.FechaContable.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{x.Cod_Oficina}'");
                                    sbInsert.Append($",'{x.Oficina}'");
                                    sbInsert.Append($",'{x.Zona}'");
                                    sbInsert.Append($",'{x.Cve_Agente}'");
                                    sbInsert.Append($",'{x.Agente}'");
                                    sbInsert.Append($",'{x.Cve_Supervisor}'");
                                    sbInsert.Append($",'{x.Supervisor}'");
                                    sbInsert.Append($",'{x.Cod_Contratante}'");
                                    sbInsert.Append($",'{x.Contratante}'");
                                    sbInsert.Append($",'{x.TipoPersona_Cont}'");
                                    sbInsert.Append($",'{x.Rfc_Cont}'");
                                    sbInsert.Append($",TO_DATE('{x.FechaNacimiento_Cont.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{x.Sexo_Cont}'");
                                    sbInsert.Append($",'{x.PaisNacimiento_Cont}'");
                                    sbInsert.Append($",NULL");
                                    sbInsert.Append($",'{x.Nacionalidad_Cont}'");
                                    sbInsert.Append($",'{x.Residencia_Cont}'");
                                    sbInsert.Append($",'{x.Domicilio_Cont}'");
                                    sbInsert.Append($",'{x.NumInterior_Cont}'");
                                    sbInsert.Append($",'{x.NumExterior_Cont}'");
                                    sbInsert.Append($",'{x.Colonia_Cont}'");
                                    sbInsert.Append($",'{x.Estado_Cont}'");
                                    sbInsert.Append($",'{x.Municipio_Cont}'");
                                    sbInsert.Append($",'{x.CodigoPostal_Cont}'");
                                    sbInsert.Append($",'{x.ComprobanteDomicilio}'");
                                    sbInsert.Append($",'{x.Ocupacion_Cont}'");
                                    sbInsert.Append($",'{x.Telefono_Cont}'");
                                    sbInsert.Append($",'{x.Correo_Cont}'");
                                    sbInsert.Append($",'{x.Giro_Mercantil}'");
                                    sbInsert.Append($",'{x.Fecha_Constitucion}'");
                                    sbInsert.Append($",'{x.Folio_Mercantil}'");
                                    sbInsert.Append($",'{x.Nombre_Apoderado}'");
                                    sbInsert.Append($",'{x.Politicamente_Expuesto}'");
                                    sbInsert.Append($",'{x.Cod_Asegurado}'");
                                    sbInsert.Append($",'{x.Asegurado}'");
                                    sbInsert.Append($",{x.Orden}");
                                    sbInsert.Append($",{x.Inciso}");
                                    sbInsert.Append($",TO_DATE('{x.FechaIniAsegurado.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",TO_DATE('{x.FechaFinAsegurado.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{x.ParentesCoase}'");
                                    sbInsert.Append($",'{x.EstatusAsegurado}'");
                                    sbInsert.Append($",{x.PNeta_Ase}");
                                    sbInsert.Append($",TO_DATE('{x.AntReconocida.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",TO_DATE('{x.AntPlanSeguro.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{x.Tipopersona_Ase}'");
                                    sbInsert.Append($",'{x.Rfc_Ase}'");
                                    sbInsert.Append($",TO_DATE('{x.FechaNacimiento_Ase.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{x.Sexo_Ase}'");
                                    sbInsert.Append($",'{x.PaisNacimiento_Ase}'");
                                    sbInsert.Append($",'{x.Nacionalidad_Ase}'");
                                    sbInsert.Append($",'{x.Residencia_Ase}'");
                                    sbInsert.Append($",'{x.Domicilio_Ase}'");
                                    sbInsert.Append($",'{x.NumInterior_Ase}'");
                                    sbInsert.Append($",'{x.NumExterior_Ase}'");
                                    sbInsert.Append($",'{x.Colonia_Ase}'");
                                    sbInsert.Append($",'{x.Estado_Ase}'");
                                    sbInsert.Append($",'{x.Municipio_Ase}'");
                                    sbInsert.Append($",'{x.Codigopostal_Ase}'");
                                    sbInsert.Append($",'{x.Ocupacion_Ase}'");
                                    sbInsert.Append($",'{x.Telefono_Ase}'");
                                    sbInsert.Append($",'{x.Correo_Ase}'");
                                    sbInsert.Append($",'{x.Politicamente_Expuesto_Ase}'");
                                    sbInsert.Append($",'{x.Cod_Tarifa}'");
                                    sbInsert.Append($",'{x.Tarifa}'");
                                    sbInsert.Append($",'{x.Cod_Tecnico}'");
                                    sbInsert.Append($",'{x.Tecnico}'");
                                    sbInsert.Append($",'{x.Deducible}'");
                                    sbInsert.Append($",'{x.SumAsegurada}'");
                                    sbInsert.Append($",'{x.CoaSeguro}'");
                                    sbInsert.Append($",'{x.Thq}'");
                                    sbInsert.Append($",'{x.Cod_Basehosp}'");
                                    sbInsert.Append($",'{x.BaseHospitalaria}'");
                                    sbInsert.Append($",'{x.ElimDeducible}'");
                                    sbInsert.Append($",'{x.Incre_Saparto}'");
                                    sbInsert.Append($",'{x.TopecoAseguro}'");
                                    sbInsert.Append($",'{x.AsistenciaDental}'");
                                    sbInsert.Append($",'{x.AsitenciaFuneraria}'");
                                    sbInsert.Append($",'{x.EmergenciaExtranjero}'");
                                    sbInsert.Append($",'{x.CoberturaExtranjero}'");
                                    sbInsert.Append($",'{x.FichaTecnica}'");
                                    sbInsert.Append($",'{x.RedCoaNariz}'");
                                    sbInsert.Append($",'{x.CobVisual}'");
                                    sbInsert.Append($",'{x.CobDental}'");
                                    sbInsert.Append($",'{x.Exclusiones}'");
                                    sbInsert.Append($",'{x.PorExtraPrima}'");
                                    sbInsert.Append($",'{x.PreExistencia}'");
                                    sbInsert.Append($",{x.Ind_Idha}");
                                    sbInsert.Append($",{x.Ind_Can}");
                                    sbInsert.Append($",'{x.Idha_Sas}'");
                                    sbInsert.Append($",'{x.Can_Sas}'");
                                    sbInsert.Append($",'{x.MonedaContratada}'");
                                    sbInsert.Append($",{x.Cod_FormaPago}");
                                    sbInsert.Append($",'{x.FormaPago}'");
                                    sbInsert.Append($",'{x.EstatusPoliza}'");
                                    sbInsert.Append($",'{x.FechaCancelacion}'");
                                    sbInsert.Append($",'{x.MotivoCan}'");
                                    sbInsert.Append($",'{x.TipoPoliza}'");
                                    sbInsert.Append($",'{x.AutoAdministrada}'");
                                    sbInsert.Append($",'{x.Pool}'");
                                    sbInsert.Append($",'{x.Ejecutivo}'");
                                    sbInsert.Append($",'{x.Zonatencion}'");
                                    sbInsert.Append($",'{x.Region}'");
                                    sbInsert.Append($",'{x.Registro_Cnsf_Condusef}'");
                                    sbInsert.Append($",'{x.Recargo}'");
                                    sbInsert.Append($",'{x.Siniestros}'");
                                    sbInsert.Append($",{x.Comisiones}");
                                    sbInsert.Append($",{x.Prima_Total})");
                                    var insertado = _baseDatosPolizas.Insert(sbInsert.ToString());
                                    if (insertado == true) {
                                        cont++;
                                    } else {
                                        query = $"INSERT INTO RUE_NO_CARGADOS VALUES('Hubo un error en el registo con: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'), 1003)";
                                        var jala = _baseDatosPolizas.Insert(query);
                                        if (jala == false)
                                        {
                                            Console.WriteLine(query);
                                        }
                                    }
                                }
                                break;
                            case 998:
                                {
                                    query = $"INSERT INTO RUE_NO_CARGADOS VALUES('Hubo un error en el registo con: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'),{error})";
                                    var jala = _baseDatosPolizas.Insert(query);
                                    if (jala == false)
                                    {
                                        Console.WriteLine(query);
                                    }
                                }
                                break;
                            case 999:
                                {
                                    query = $"INSERT INTO RUE_NO_CARGADOS VALUES('Hubo un error en el registo con: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'),{error})";
                                    var jala = _baseDatosPolizas.Insert(query);
                                    if (jala == false)
                                    {
                                        Console.WriteLine(query);
                                    }
                                }
                                break;
                            case 1000:
                                {
                                    query = $"INSERT INTO RUE_NO_CARGADOS VALUES('Hubo un error en el registo con: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'),{error})";
                                    var jala = _baseDatosPolizas.Insert(query);
                                    if (jala == false)
                                    {
                                        Console.WriteLine(query);
                                    }
                                }
                                break;
                            case 1001:
                                {
                                    query = $"INSERT INTO RUE_NO_CARGADOS VALUES('Hubo un error en el registo con: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'),{error})";
                                    var jala = _baseDatosPolizas.Insert(query);
                                    if (jala == false)
                                    {
                                        Console.WriteLine(query);
                                    }
                                }
                                break;
                        }
                    }
                }

                if (rue_list.Count == cont){
                    Console.WriteLine($"Sincronización de la tabla de RUE completa, registros obtenenidos {rue_list.Count} - registros guardados {cont}.");
                } else {
                    Console.WriteLine($"Hubo error con el guardado de los registros, registros obtenenidos {rue_list.Count} - registros guardados {cont}. Para más detalles consultar LOG.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EN: ---> SELECT * FROM RUE_ENERO_2021 WHERE SISTEMA = '{query2.Sistema}' AND POLIZA = '{query2.Poliza}' AND PRIMA_TOTAL = {query2.Prima_Total}");
                Console.WriteLine($"Excepción; Método: {this.GetType().FullName}; Mensaje: {ex.Message}");
                return false;
            }
        }

        public bool SincronizacionTablaRup()
        {
            string query;
            RUP query2 = null;
            DateTime mesActual = DateTime.UtcNow;
            DateTime mesAnterior = mesActual.AddMonths(-1);
            int dia = Int32.Parse(mesActual.Day.ToString());
            int año = mesActual.Year;
            string mesActualLetra = mesActual.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")).ToUpper();
            string mesAnteriorLetra = mesAnterior.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")).ToUpper();
            string tabla_anterior;
            if (mesActualLetra == "ENERO")
                tabla_anterior = $"RUP_{mesAnteriorLetra}_{(año - 1).ToString()}";
            else
                tabla_anterior = $"RUP_{mesAnteriorLetra}_{año.ToString()}";
            string tabla_actual = $"RUP_{mesActualLetra}_{año}";
            try
            {
                List<RUP> rup_list_anterior, rup_list_actual;
                List<RUP> rup_list = null;

                query = $"SELECT * FROM OSIRIS.{tabla_actual}";
                rup_list_actual = _baseDatosCliente.Select<RUP>(query);

                if (dia <= 15)
                {
                    query = $"SELECT * FROM OSIRIS.{tabla_anterior}";
                    rup_list_anterior = _baseDatosCliente.Select<RUP>(query);
                    if (rup_list_actual == null)
                    {
                        if (rup_list_anterior != null)
                        {
                            rup_list = rup_list_anterior.ToList();
                            Console.WriteLine($"Obtuvo la lista de {tabla_anterior} --- Count: " + rup_list.Count());
                        }

                    }
                    else
                    {
                        if (rup_list_anterior == null)
                        {
                            rup_list = rup_list_actual.ToList();
                            Console.WriteLine($"Obtuvo la lista de {tabla_actual} --- Count: " + rup_list.Count());
                        }
                        else
                        {
                            rup_list = rup_list_actual.Concat(rup_list_anterior).ToList();
                            Console.WriteLine($"Obtuvo la lista de {tabla_anterior} - {tabla_actual} --- Count: " + rup_list.Count());
                        }
                    }
                }
                else
                {
                    rup_list = rup_list_actual.ToList();
                    Console.WriteLine($"Obtuvo la lista de {tabla_actual} --- Count: " + rup_list.Count());
                }

                Console.WriteLine("Obtuvo la lista de RUP - Count: " + rup_list.Count());
                // SISTEMA - POLIZA - AÑO/INICIO - ENDOSO - MONTO
                int cont = 0;

                foreach (var x in rup_list)
                {
                    query2 = x;
                    var sbInsert = new StringBuilder();
                    var llave = new StringBuilder();
                    if (string.IsNullOrWhiteSpace(x.Sistema)) llave.Append("-/"); else llave.Append($"{x.Sistema}/");
                    if (string.IsNullOrWhiteSpace(x.Poliza)) llave.Append("-/"); else llave.Append($"{x.Poliza}/");
                    if (string.IsNullOrWhiteSpace(x.FechaInicio.ToString())) llave.Append("-/"); else llave.Append($"{x.FechaInicio}/");
                    if (string.IsNullOrWhiteSpace(x.Cuota)) llave.Append("PAGO/-/"); else llave.Append($"PAGO/{x.Cuota}/");
                    llave.Append($"{x.PrimaNeta}/");
                    llave.Append($"{x.Financiamiento}/");
                    llave.Append($"{x.Gasto}");
                    bool errorRow = false;
                    string errorDescrip = "";
                    if (x.Cod_Ramo.Length > 2)
                    {
                        errorRow = true;
                        errorDescrip = "El codigo de ramo no es numerico o es mayor a 2 caracteres";
                    }
                    if (x.Sistema == null || x.Poliza == null || x.FechaInicio == null || x.Cuota == null || errorRow == true)
                    {
                        if (errorRow == false)
                            query = $"INSERT INTO RUP_NO_CARGADOS VALUES('Los campos de la llave de sincronización no estan completos LLAVE: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'), 1002)";
                        else
                            query = $"INSERT INTO RUP_NO_CARGADOS VALUES('{errorDescrip} LLAVE: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'), 1002)";
                        _baseDatosPolizas.Insert(query);
                    }
                    else
                    {
                        var llavesincronizacion = new StringBuilder();
                        llavesincronizacion.Append($"{x.Sistema}");
                        llavesincronizacion.Append($"{x.Poliza}");
                        llavesincronizacion.Append($"{x.FechaInicio.ToString("yyyy")}");
                        llavesincronizacion.Append($"PAGO");
                        llavesincronizacion.Append($"{x.Cuota}");
                        var prima = x.PrimaNeta + x.Financiamiento + x.Gasto;
                        prima = prima * 1.16m;
                        prima = Math.Round(prima, 2);
                        String cadena = prima.ToString();
                        char last_char = cadena[cadena.Length - 1];
                        if (last_char == '0')
                        {
                            cadena = cadena.Substring(0, cadena.Length - 1);
                        }
                        String txt_prima = cadena;
                        llavesincronizacion.Append($"{txt_prima}");

                        query = $"SELECT * FROM POLIZAS_MOVIMIENTOS WHERE LLAVESINCRONIZACION = '{llavesincronizacion}'";
                        Movimientos row = _baseDatosPolizas.SelectFirst<Movimientos>(query);
                        query = $"SELECT * FROM RUP WHERE SISTEMA = '{x.Sistema}' AND POLIZA = '{x.Poliza}' AND FECHAINICIO = TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy') AND PRIMANETA = {x.PrimaNeta} AND FINANCIAMIENTO = {x.Financiamiento} AND GASTO = {x.Gasto} AND CUOTA = '{x.Cuota}'";
                        RUP rup = _baseDatosPolizas.SelectFirst<RUP>(query);

                        var error = 0;
                        if (prima == 0)
                        {
                            error = 999;
                        }
                        if (row != null || rup != null)
                        {
                            error = 1000;
                        }
                        switch (error)
                        {
                            case 0:
                                {
                                    sbInsert.Append("INSERT INTO RUP VALUES(");
                                    sbInsert.Append($"'{x.Sistema}'");
                                    sbInsert.Append($",'{x.Poliza}'");
                                    sbInsert.Append($",TO_DATE('{x.FechaInicio.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",TO_DATE('{x.FechaTermino.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{x.Certificado}'");
                                    sbInsert.Append($",'{x.Cod_Ramo}'");
                                    sbInsert.Append($",'{x.Des_Ramo}'");
                                    sbInsert.Append($",'{x.SubRamo}'");
                                    sbInsert.Append($",'{x.Cod_Producto}'");
                                    sbInsert.Append($",'{x.Producto}'");
                                    sbInsert.Append($",'{x.AutoAdministrada}'");
                                    sbInsert.Append($",'{x.Clv_Supervisor}'");
                                    sbInsert.Append($",'{x.Supervisor}'");
                                    sbInsert.Append($",'{x.Clv_Agente}'");
                                    sbInsert.Append($",'{x.Agente}'");
                                    sbInsert.Append($",'{x.Cod_Contratante}'");
                                    sbInsert.Append($",'{x.Contratante}'");
                                    sbInsert.Append($",'{x.Tipo_Persona}'");
                                    sbInsert.Append($",'{x.Rfc}'");
                                    sbInsert.Append($",{x.PrimaNeta}");
                                    sbInsert.Append($",{x.Financiamiento}");
                                    sbInsert.Append($",{x.Gasto}");
                                    sbInsert.Append($",{x.Iva}");
                                    sbInsert.Append($",{x.Monto}");
                                    sbInsert.Append($",{x.Cod_FormaPago}");
                                    sbInsert.Append($",'{x.FormaPago}'");
                                    sbInsert.Append($",'{x.Id_Pago}'");
                                    sbInsert.Append($",TO_DATE('{x.FechaPago.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",TO_DATE('{x.FechaVenPago.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{x.Cuota}'");
                                    sbInsert.Append($",'{x.CuentaClabe}'");
                                    sbInsert.Append($",{x.MontoAplicado}");
                                    sbInsert.Append($",'{x.Cod_MetodoPago}'");
                                    sbInsert.Append($",'{x.MetodoPago}'");
                                    sbInsert.Append($",TO_DATE('{x.FechaAplicacion.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy')");
                                    sbInsert.Append($",'{x.MedioAplicacion}'");
                                    sbInsert.Append($",'{x.Cod_Estado}'");
                                    sbInsert.Append($",'{x.Estado}'");
                                    if (string.IsNullOrWhiteSpace(x.Observaciones_Pago)) sbInsert.Append($",'{x.Observaciones_Pago}'"); else sbInsert.Append($",'{x.Observaciones_Pago.Trim()}'");
                                    sbInsert.Append($",'{x.Cajero}'");
                                    sbInsert.Append($",TO_DATE('{x.FechaRecepcion.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'))");
                                    var insertado = _baseDatosPolizas.Insert(sbInsert.ToString());
                                    if (insertado == true) { 
                                        cont++;
                                    } else {
                                        _baseDatosPolizas.Insert($"INSERT INTO RUP_NO_CARGADOS VALUES('Hubo un error en el registo con: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'), 1003)");
                                    }
                                }
                                break;
                            case 999:
                                {
                                    query = $"INSERT INTO RUP_NO_CARGADOS VALUES('Hubo un error en el registo con: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'), {error})";
                                    _baseDatosPolizas.Insert(query);
                                }
                                break;
                            case 1000:
                                {
                                    query = $"INSERT INTO RUP_NO_CARGADOS VALUES('Hubo un error en el registo con: {llave}',TO_DATE('{mesActual.ToString("dd/MM/yyyy")}', 'dd/mm/yyyy'), {error})";
                                    _baseDatosPolizas.Insert(query);
                                }
                                break;
                        }
                    }
                }

                if (rup_list.Count == cont){
                    Console.WriteLine($"Sincronización de la tabla de RUP completa, registros obtenenidos {rup_list.Count} - registros guardados {cont}.");
                } else {
                    Console.WriteLine($"Hubo error con el guardado de los registros, registros obtenenidos {rup_list.Count} - registros guardados {cont}. Para más detalles consultar LOG.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EN: ---> SELECT * FROM RUP_ENERO_2021 WHERE SISTEMA = '{query2.Sistema}' AND POLIZA = '{query2.Poliza}' AND MONTOAPLICADO = {query2.MontoAplicado}");
                Console.WriteLine($"Excepción; Método: {this.GetType().FullName}; Mensaje: {ex.Message}");
                return false;
            }
        }

        private void QuerySincronizacion()
        {
            DateTime mesActual = DateTime.UtcNow;
            DateTime mesAnterior = mesActual.AddMonths(-1);
            string año = mesActual.Year.ToString();
            string mesActualLetra = mesActual.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")).ToUpper();
            string mesAnteriorLetra = mesAnterior.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")).ToUpper();

            string tabla;
            string query;

            //if (mesActual.Day <= 7)
            //{
            //    //Tabla RUE
            //    tabla = $"RUE_{mesAnteriorLetra}_{año}";
            //    query = QUERY_CLIENTE_RUE.Replace(IDENTIFICADOR_TABLA_QUERY, tabla);

            //    query += Environment.NewLine + "UNION";

            //    tabla = $"RUE_{mesActualLetra}_{año}";
            //    query += QUERY_CLIENTE_RUE.Replace(IDENTIFICADOR_TABLA_QUERY, tabla);

            //    _queryClienteTablaRUE = query;

            //    //Tablas RUP
            //    tabla = $"RUP_{mesAnteriorLetra}_{año}";
            //    query = QUERY_CLIENTE_RUP.Replace(IDENTIFICADOR_TABLA_QUERY, tabla);

            //    query += Environment.NewLine + "UNION";

            //    tabla = $"RUP_{mesActualLetra}_{año}";
            //    query += QUERY_CLIENTE_RUP.Replace(IDENTIFICADOR_TABLA_QUERY, tabla);

            //    _queryClienteTablaRUP = query;
            //}
            //else
            //{
            //Tabla RUE
            tabla = $"RUE_{mesActualLetra}_{año}";
            _queryClienteTablaRUE = QUERY_CLIENTE_RUE.Replace(IDENTIFICADOR_TABLA_QUERY, tabla);

            //Tablas RUP
            tabla = $"RUP_{mesActualLetra}_{año}";
            _queryClienteTablaRUP = QUERY_CLIENTE_RUP.Replace(IDENTIFICADOR_TABLA_QUERY, tabla);
            //}
        }

        /// <summary>
        /// Guarda registro en Tabla POLIZAS_LOGMOVIMIENTOS
        /// </summary>
        /// <param name="facturaId">ID Tabla POLIZAS_MOVIMIENTO</param>
        /// <param name="mensaje">Mensaje</param>
        /// <returns></returns>
        private async Task LogMovimientos(string llave, string mensaje)
        {
            try
            {
                var id = _baseDatosPolizas.SelectFirst<int>(QUERY_POLIZAS_LOGMOVIMIENTOS_ID);

                if (id > 0)
                    _baseDatosPolizas.Insert($"INSERT INTO POLIZAS_LOGMOVIMIENTOS VALUES ({id}, '{llave}', SYSDATE, '{mensaje}')");
            }
            catch
            {
                //Guardar LOG TXT
            }
        }
    }
}
