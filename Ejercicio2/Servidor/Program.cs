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

        static List<NetworkStream> listaStreams = new List<NetworkStream>();


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

                lock (lockObj)
                {
                    listaStreams.Add(stream);
                }

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

                         lock (lockObj)
                        {
                            carretera.ActualizarVehiculo(datosRecibidos);
                        }
                        
                        // Refrescar referencia local
                        vehiculo = carretera.VehiculosEnCarretera.FirstOrDefault(v => v.Id == datosRecibidos.Id);
                        lock (lockObj) // Proteger envío concurrente
                        {
                            EnviarCarreteraATodos();
                        }

                        MostrarCarretera();
                    }

                    Thread.Sleep(50);
                }
                Console.WriteLine("No hay vehiculos en la carretera.");

                lock (lockObj)
                {
                    listaStreams.Remove(stream);
                }       
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
            foreach (var v in carretera.VehiculosEnCarretera)
            {
                string estado = v.Acabado ? "Finalizado" : $"Km {v.Pos}";
                Console.WriteLine($"ID {v.Id} [{v.Direccion}] - {estado}");
            }
        }

        static void EnviarCarreteraATodos()
        {
            byte[] datos = carretera.CarreteraABytes();
            byte[] longitud = BitConverter.GetBytes(datos.Length);

            List<NetworkStream> streamsInvalidos = new List<NetworkStream>();

            foreach (var stream in listaStreams.ToList())
            {
                try
                {
                    stream.Write(longitud, 0, 4);  // Enviar longitud primero
                    stream.Write(datos, 0, datos.Length);  // Luego los datos
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al enviar a un cliente: " + ex.Message);
                    streamsInvalidos.Add(stream); // Marcar para eliminar
                }
            }

            // Eliminar streams que han fallado
            lock (lockObj)
            {
                foreach (var s in streamsInvalidos)
                {
                    listaStreams.Remove(s);
                    try { s.Close(); } catch {}
                }
            }
        }
    
    }
}

