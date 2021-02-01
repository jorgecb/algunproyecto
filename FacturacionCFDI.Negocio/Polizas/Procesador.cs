using System;
using System.Configuration;
using System.Threading.Tasks;
using Utilerias;
using static Utilerias.Enum.TipoBaseDatos;

namespace FacturacionCFDI.Negocio.Polizas
{
    public class Procesador
    {
        private readonly BaseDatos _baseDatosCliente;
        private readonly BaseDatos _baseDatosPolizas;
        private readonly SincronizacionCliente _cliente;
        private readonly SincronizacionPoliza _poliza;
        private readonly SincronizacionFacturacion _facturacion;
        private readonly Cancelacion _cancelacion;
        private readonly Refacturacion _refacturacion;

        private string _conexionCliente = ConfigurationManager.ConnectionStrings["InformacionPS"].ToString();
        private string _conexionPolizas = ConfigurationManager.ConnectionStrings["AdmonPolizaPS"].ToString();

        public Procesador()
        {
            _baseDatosCliente = new BaseDatos(_conexionCliente, TipoBase.Oracle);
            _baseDatosPolizas = new BaseDatos(_conexionPolizas, TipoBase.Oracle);
            _cliente = new SincronizacionCliente(_baseDatosCliente, _baseDatosPolizas);
            _poliza = new SincronizacionPoliza(_baseDatosPolizas);
            _facturacion = new SincronizacionFacturacion(_baseDatosPolizas);
            _cancelacion = new Cancelacion(_baseDatosPolizas);
            _refacturacion = new Refacturacion(_baseDatosPolizas);
        }

        /// <summary>
        /// Proceso para Sincronización de la Información del Cliente
        /// </summary>
        /// <returns></returns>
        public async Task ProcesoSincronizarInformacionCliente()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Sincronizar Informacion Cliente {DateTime.Now}");
                var proceso = await _cliente.SincronizarInformacionCliente();

                Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Fin Proceso Sincronizar Informacion Cliente {DateTime.Now}");
            }
        }

        /// <summary>
        /// Proceso para Sincronizar POLIZAS_FACTURACION a Tablas de FACTURACION
        /// </summary>
        /// <returns></returns>
        public async Task ProcesoSincronizarTablasFacturacion()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Sincronizar Tablas Facturacion {DateTime.Now}");
                var procesoRfc = await _facturacion.SincronizarFacturasReceptor();
                if (procesoRfc.Codigo != 0)
                {
                    var procesoFact = await _facturacion.SincronizarFacturas();
                    if (procesoFact != null)
                    {
                        Console.WriteLine($"Código: {procesoFact.Codigo}; Mensaje: {procesoFact.Mensaje}");
                        Console.WriteLine($"Fin Proceso Sincronizar Tablas Facturacion {DateTime.Now}");
                    }
                }
                else
                {
                    Console.WriteLine($"Código: {procesoRfc.Codigo}; Mensaje: {procesoRfc.Mensaje}");
                    Console.WriteLine($"Fin Proceso Sincronizar Tablas Facturacion {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }

        /// <summary>
        /// Proceso para llenado de Tablas polizas_concentrado, polizas_facturación (Factura Global)
        /// </summary>
        /// <returns></returns>
        public async Task ProcesoSincronizarFacturaGlobal()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Sincronizar Factura Global {DateTime.Now}");
                var proceso = await _poliza.SincronizacionFacturaGlobal();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso Sincronizar Factura Global {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }

        public async Task ProcesoSincronizarFacturaNotaDebito()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Sincronizar Factura Nota de Débito {DateTime.Now}");
                var proceso = await _poliza.SincronizacionFacturaNotaDebito();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso Sincronizar Factura Nota de Débito {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }

        public async Task ProcesoSincronizarFacturaNotaCredito()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Sincronizar Factura Nota de Crédito {DateTime.Now}");
                var proceso = await _poliza.SincronizacionFacturaNotaCredito();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso Sincronizar Factura Nota de Crédito {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }

        public async Task ProcesoSincronizarEstatusFacturacion()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Sincronizar Estatus de Facturación {DateTime.Now}");
                var proceso = await _poliza.SincronizarEstatusFacturacion();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso Sincronizar Estatus de Facturación  {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }

        public async Task ProcesoActualizarEstatusFacturacionProgramados()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Actualización Estatus Programados a Por procesar {DateTime.Now}");
                var proceso = await _poliza.ActualizarEstatusFacturacionProgramados();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso Actualización Estatus Programados a Por procesar  {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }

        public async Task ProcesoActualizarEstatusFacturacionXTimbrarFacturaGlobal()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Actualización Estatus Factura Global por timbrar a Por procesar {DateTime.Now}");
                var proceso = await _poliza.ActualizarEstatusFacturacionXTimbrarFacturaGlobal();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso Actualización Estatus Factura Global por timbrar a Por procesar  {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }

        public async Task ProcesoSincronizarRefacturacion()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Sincronizar Refacturación {DateTime.Now}");
                var proceso = await _poliza.SincronizacionRefacturacion();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso Sincronizar Refacturacion {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }


        public async Task ProcesoSincronizarFacturaPagos()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso Sincronizar Factura Pagos {DateTime.Now}");
                var proceso = await _poliza.SincronizacionFacturaPagos();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso Sincronizar Factura Pagos {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }

        public async Task ProcesoCancelacion()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso de Cancelación {DateTime.Now}");
                var proceso = await _cancelacion.ProcesoCancelacion();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso de Cancelación  {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }

        public async Task ProcesoRefacturacion()
        {
            try
            {
                Console.WriteLine($"Inicia Proceso de Refacturación {DateTime.Now}");
                var proceso = await _refacturacion.ProcesoRefacturacion();

                if (proceso != null)
                {
                    Console.WriteLine($"Código: {proceso.Codigo}; Mensaje: {proceso.Mensaje}");
                    Console.WriteLine($"Fin Proceso de Refacturación  {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
            }
        }
    }
}
