// SERVIDOR CORREGIDO Y OPTIMIZADO
using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkStreamNS;
using CarreteraClass;
using VehiculoClass;
using System.Collections.Generic;
using System.Linq;

namespace Servidor
{
    class Program
    {
        static Carretera carretera = new Carretera();
        static int contadorVehiculos = 0;
        static object lockObj = new object();

        static List<NetworkStream> listaStreams = new List<NetworkStream>();
        static SemaphoreSlim semaforoPuente = new SemaphoreSlim(1, 1);
        static Queue<Vehiculo> colaEspera = new Queue<Vehiculo>();
        static int? vehiculoEnPuenteId = null;

        static void Main(string[] args)
        {
            TcpListener servidor = new TcpListener(IPAddress.Any, 5000);
            servidor.Start();
            Console.WriteLine("Servidor iniciado. Esperando conexiones...");

            while (true)
            {
                TcpClient cliente = servidor.AcceptTcpClient();
                Thread hilo = new Thread(() => GestionarVehiculo(cliente));
                hilo.Start();
            }
        }

        static void GestionarVehiculo(TcpClient cliente)
        {
            NetworkStream stream = cliente.GetStream();
            Vehiculo vehiculo = NetworkStreamClass.LeerDatosVehiculoNS(stream);

            lock (lockObj)
            {
                contadorVehiculos++;
                vehiculo.Id = contadorVehiculos;
                carretera.AñadirVehiculo(vehiculo);
                listaStreams.Add(stream);
            }

            NetworkStreamClass.EscribirMensajeNetworkStream(stream, vehiculo.Id.ToString());

            try
            {
                while (!vehiculo.Acabado)
                {
                    if (stream.DataAvailable)
                    {
                        Vehiculo datosRecibidos = NetworkStreamClass.LeerDatosVehiculoNS(stream);

                        lock (lockObj)
                        {
                            carretera.ActualizarVehiculo(datosRecibidos);
                            vehiculo = carretera.VehiculosEnCarretera.First(v => v.Id == datosRecibidos.Id);

                            // Lógica de entrada al puente
                            if (vehiculo.Pos == 10 && !vehiculo.Parado)
                            {
                                if (vehiculoEnPuenteId == null && semaforoPuente.Wait(0))
                                {
                                    vehiculoEnPuenteId = vehiculo.Id;
                                    vehiculo.Parado = false;
                                    Console.WriteLine($"🚗 Vehículo {vehiculo.Id} comienza a cruzar el puente");
                                }
                                else
                                {
                                    vehiculo.Parado = true;
                                    if (!colaEspera.Any(v => v.Id == vehiculo.Id))
                                        colaEspera.Enqueue(vehiculo);
                                    Console.WriteLine($"⛔ Vehículo {vehiculo.Id} esperando para cruzar el puente");
                                }
                            }


                            // Lógica de salida del puente
                            if (vehiculo.Pos >= 50 && vehiculoEnPuenteId == vehiculo.Id)
                            {
                                Console.WriteLine($"✅ Vehículo {vehiculo.Id} ha salido del puente");
                                vehiculoEnPuenteId = null;
                                semaforoPuente.Release();

                                if (colaEspera.Count > 0)
                                {
                                    Vehiculo siguiente = colaEspera.Dequeue();
                                    siguiente.Parado = false;
                                    vehiculoEnPuenteId = siguiente.Id;
                                    semaforoPuente.Wait();
                                    Console.WriteLine($"🚦 Vehículo {siguiente.Id} puede cruzar el puente");

                                    // Para que el cambio se refleje en la carretera
                                    var enLista = carretera.VehiculosEnCarretera.FirstOrDefault(v => v.Id == siguiente.Id);
                                    if (enLista != null) enLista.Parado = false;
                                    Console.WriteLine($"🚗 Vehículo {siguiente.Id} comienza a cruzar el puente");
                                }

                               
                            }

                            EnviarCarreteraATodos();
                        }
                    }
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error con el vehículo {vehiculo.Id}: {ex.Message}");
            }
            finally
            {
                stream.Close();
                cliente.Close();
                lock (lockObj)
                {
                    listaStreams.Remove(stream);
                }
                Console.WriteLine($"🔌 Vehículo {vehiculo.Id} desconectado");
            }
        }

        static void EnviarCarreteraATodos()
        {
            byte[] datos = carretera.CarreteraABytes();
            byte[] longitud = BitConverter.GetBytes(datos.Length);
            List<NetworkStream> desconectados = new List<NetworkStream>();

            foreach (var stream in listaStreams.ToList())
            {
                try
                {
                    stream.Write(longitud, 0, 4);
                    stream.Write(datos, 0, datos.Length);
                }
                catch
                {
                    desconectados.Add(stream);
                }
            }

            lock (lockObj)
            {
                foreach (var roto in desconectados)
                {
                    listaStreams.Remove(roto);
                }
            }
        }
    }
}
