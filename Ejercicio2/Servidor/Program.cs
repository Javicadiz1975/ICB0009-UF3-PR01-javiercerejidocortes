using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using NetworkStreamNS;
using CarreteraClass;
using VehiculoClass;


namespace Servidor
{

    class Program
    {   
        static Carretera carretera = new Carretera();
        static int contadorVehiculos = 0;
        static object lockObj = new object();

        static void Main(string[] args)
        { 
            
            try
            {
                TcpListener servidor = new TcpListener(IPAddress.Any, 5000);
                servidor.Start();
                Console.WriteLine("Servidor iniciado. Esperando conexiones...");

                while (true)
                {
                    TcpClient cliente = servidor.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado.");

                    Thread hilo = new Thread(() => GestionarVehiculo(cliente));
                    hilo.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }           


        }

        static void GestionarVehiculo(TcpClient cliente)
        {
            try
            {
                NetworkStream stream = cliente.GetStream();

                // Leer el vehículo recibido
                Vehiculo vehiculo = NetworkStreamClass.LeerDatosVehiculoNS(stream);

                // Asignar un ID único
                lock (lockObj)
                {
                    contadorVehiculos++;
                    vehiculo.Id = contadorVehiculos;
                }

                // Enviar ID de vuelta al cliente
                NetworkStreamClass.EscribirMensajeNetworkStream(stream, vehiculo.Id.ToString());

                // Añadir el vehículo a la carretera
                carretera.AñadirVehiculo(vehiculo);

                // Mostrar todos los vehículos actuales
                Console.WriteLine("\n Vehículos en carretera:");

                while (!vehiculo.Acabado)
                {
                    if (stream.DataAvailable)
                    {
                        Vehiculo datosRecibidos = NetworkStreamClass.LeerDatosVehiculoNS(stream);

                        carretera.ActualizarVehiculo(datosRecibidos);
                        
                        // Refrescar referencia local
                        vehiculo = carretera.VehiculosEnCarretera.FirstOrDefault(v => v.Id == datosRecibidos.Id);

                        MostrarCarretera();
                    }

                    Thread.Sleep(50);
                }
                Console.WriteLine("No hay vehiculos en la carretera.");       
                cliente.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al gestionar vehículo: " + ex.Message);
            }
        }

        static void MostrarCarretera()
        {
            Console.Clear();
            Console.WriteLine("Estado actual de la carretera:");
            foreach (Vehiculo v in carretera.VehiculosEnCarretera)
            {
                string estado = v.Acabado ? "Finalizado" : $"Km {v.Pos}";
                Console.WriteLine($"Vehiculo con ID {v.Id} [{v.Direccion}] - {estado}");
            }
        }
    
    }
}

