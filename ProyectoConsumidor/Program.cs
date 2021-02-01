using FacturacionCFDI.Negocio.Facturacion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoConsumidor
{
    class Program
    {
        static void Main(string[] args)
        {
            Procesador procesador = new Procesador();
            _ = Proceso(procesador);
        }


        private static async Task Proceso(Procesador procesador)
        {
            string comprobante, continuar;

            try
            {                
                do
                {
                    Console.Clear();
                    Console.WriteLine("*****Proceso batch simulación - Servicio Timbrado*****");
                    Console.WriteLine("Ingresa Id Comprobante:");
                    comprobante = Console.ReadLine();

                    comprobante = comprobante.Length <= 0 ? "0" : comprobante;

                    //var timbrar = procesador.ValidarIdComprobante(comprobante);
                    var timbrar = true;

                    if (timbrar)
                    {
                        //Console.WriteLine($"Generando comprobante {comprobante}:");
                        Console.WriteLine($"Generando comprobantes:");
                        //await procesador.ProcesoTibrado(comprobante);
                        await procesador.ProcesoAutomatico();
                    }

                    Console.WriteLine();
                    Console.WriteLine("Presiona S, para realizar otra operación.");
                    continuar = Console.ReadLine();
                } while (continuar.ToUpper().Equals("S"));

                //var comprobantes = await procesador.ObtenerComprobantePorFacturar();

                //if ((comprobantes?.Any() ?? false))
                //{
                //    Parallel.ForEach(comprobantes, async c =>
                //    {
                //        Console.WriteLine($"Generando comprobante {c.id.ToString()}:");
                //        await procesador.ProcesoTibrado(c.id.ToString());
                //    });
                //}
                //else
                //{
                //    Console.WriteLine("No hay facturas por procesar.");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
