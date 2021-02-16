using FacturacionCFDI.Negocio.Polizas;
using System;
using System.Threading.Tasks;

namespace ProyectoConsumidorPolizas
{
    class Program
    {

        static void Main(string[] args)
        {
            Procesador procesador = new Procesador();

            _ = Menu(procesador);
        }


        private static async Task Menu(Procesador procesador)
        {
            int opcion;

            string captura, continuar = string.Empty;
            do
            {
                Console.Clear();
                Console.WriteLine("*****Proceso batch simulación - Servicio base Intermedia*****");
                Console.WriteLine("Menú de opciones:");
                Console.WriteLine("[1] Sincronizar información tablas Cliente (RUE,RUP) a tablas polizas");
                Console.WriteLine("[2] Sincronizar tablas polizas - Factura Global");
                Console.WriteLine("[3] Sincronizar tablas polizas - Factura Nota de Débito");
                Console.WriteLine("[4] Sincronizar tablas polizas - Factura Nota de Crédito");
                Console.WriteLine("[5] Sincronizar tablas polizas - Factura de Pagos");
                Console.WriteLine("[6] Sincronizar tablas polizas - Refacturacion");
                Console.WriteLine("[7] Sincronizar información polizas_facturación a facturación");
                Console.WriteLine("[8] Sincronizar tablas polizas - Estatus timbrado de facturas");
                Console.WriteLine("[9] Actualizar estatus facturas - Programados a Por procesar");
                Console.WriteLine("[10] Actualizar estatus facturas - Factura global por timbrar a Por procesar");
                Console.WriteLine("[11] Salir");

                Console.WriteLine("Ingresa opción:");
                captura = Console.ReadLine();

                try
                {
                    opcion = Convert.ToInt32(captura);
                }
                catch
                {
                    Console.WriteLine("Opción ingresada no válida.");
                    return;
                }

                switch (opcion)
                {
                    case 1:
                        await procesador.ProcesoSincronizarInformacionCliente();
                        break;
                    case 2:
                        {
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarFacturaGlobal();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarTablasFacturacion("G");
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoFacturacion();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarEstatusFacturacion();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoActualizarEstatusFacturacionXTimbrarFacturaGlobal();
                        }
                        break;
                    case 3:
                        {
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarFacturaNotaDebito();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarTablasFacturacion("I");
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoFacturacion();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarEstatusFacturacion();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            //await procesador.ProcesoActualizarEstatusFacturacionXTimbrarFacturaGlobal();
                        }
                        break;
                    case 4:
                        {
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarFacturaNotaCredito();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarTablasFacturacion("E");
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoFacturacion();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarEstatusFacturacion();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            //await procesador.ProcesoActualizarEstatusFacturacionXTimbrarFacturaGlobal();
                        }
                        break;
                    case 5:
                        {
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarFacturaPagos();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarTablasFacturacion("P");
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoFacturacion();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            await procesador.ProcesoSincronizarEstatusFacturacion();
                            Console.WriteLine(" ");
                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine(" ");
                            Console.ReadLine();
                            //await procesador.ProcesoActualizarEstatusFacturacionXTimbrarFacturaGlobal();
                        }
                        break;
                    case 6:
                        await procesador.ProcesoSincronizarRefacturacion();
                        break;
                    case 7:
                        await procesador.ProcesoSincronizarTablasFacturacion("G");
                        break;
                    case 8:
                        await procesador.ProcesoSincronizarEstatusFacturacion();
                        break;
                    case 9:
                        await procesador.ProcesoActualizarEstatusFacturacionProgramados();
                        break;
                    case 10:
                        await procesador.ProcesoActualizarEstatusFacturacionXTimbrarFacturaGlobal();
                        break;
                    case 12:
                        await procesador.ProcesoCancelacion();
                        break;
                    case 13:
                        await procesador.ProcesoRefacturacion();
                        break;
                    case 11:
                        continuar = "N";
                        break;
                    default:
                        Console.WriteLine("Opción ingresada no válida.");
                        break;
                }

                if (opcion != 11)
                {
                    Console.WriteLine();
                    Console.WriteLine("Presiona S, para realizar otra operación.");
                    continuar = Console.ReadLine();
                }

            } while (continuar.ToUpper().Equals("S"));
        }
    }
}
