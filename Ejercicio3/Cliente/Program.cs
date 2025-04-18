using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NetworkStreamNS;
using CarreteraClass;
using VehiculoClass;
using System.Linq;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Conectando al servidor...");
                using TcpClient cliente = new TcpClient("127.0.0.1", 5000);
                using NetworkStream stream = cliente.GetStream();
                using CancellationTokenSource cts = new CancellationTokenSource();

                Vehiculo miVehiculo = new Vehiculo();
                Console.WriteLine($"🚗 Vehículo creado. Dirección: {miVehiculo.Direccion} | Velocidad: {miVehiculo.Velocidad}ms");

                NetworkStreamClass.EscribirDatosVehiculoNS(stream, miVehiculo);
                string idStr = NetworkStreamClass.LeerMensajeNetworkStream(stream);
                miVehiculo.Id = int.Parse(idStr);
                Console.WriteLine($"📥 ID asignado: {miVehiculo.Id}");

                Thread hiloRecepcion = new Thread(() => EscucharServidor(stream, cts.Token, miVehiculo));
                hiloRecepcion.Start();

                MoverVehiculo(miVehiculo, stream, cts);

                hiloRecepcion.Join();
                Console.WriteLine("✅ Conexión finalizada correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error crítico: {ex.Message}");
            }
        }

        static void MoverVehiculo(Vehiculo vehiculo, NetworkStream stream, CancellationTokenSource cts)
        {
            try
            {
                while (true)
                {
                    bool estaParado;
                    int posicion;

                    // 🔒 Leemos estado actual de forma segura
                    lock (vehiculo)
                    {
                        if (vehiculo.Pos >= 100 || cts.Token.IsCancellationRequested)
                            break;

                        estaParado = vehiculo.Parado;
                        posicion = vehiculo.Pos;
                    }

                    if (estaParado)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[CLIENTE {vehiculo.Id}] ⛔ En espera (Km {posicion})");
                        Thread.Sleep(300);
                        continue;
                    }

                    // Actualizar posición solo si no está parado
                    lock (vehiculo)
                    {
                        vehiculo.Pos++;

                        try
                        {
                            NetworkStreamClass.EscribirDatosVehiculoNS(stream, vehiculo);
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[CLIENTE {vehiculo.Id}] ⚠️ Error enviando datos al servidor");
                            Console.ResetColor();
                            break;
                        }

                        string estado = vehiculo.Pos == 10 ? $"🚦 Intentando cruzar el puente"
                                        : vehiculo.Pos > 10 && vehiculo.Pos < 50 ? $"🌉 Cruzando puente"
                                        : $"🛣️ Circulando";

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[CLIENTE {vehiculo.Id}] {estado} (Km {vehiculo.Pos})");
                        Console.ResetColor();
                    }

                    Thread.Sleep(vehiculo.Velocidad);
                }

                // Finalización
                lock (vehiculo)
                {
                    vehiculo.Acabado = true;
                    NetworkStreamClass.EscribirDatosVehiculoNS(stream, vehiculo);
        
                    Console.WriteLine($"[CLIENTE {vehiculo.Id}] 🎉 ¡Recorrido completado!");
            
                }

                cts.Cancel(); // Cancela el hilo receptor
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[CLIENTE {vehiculo.Id}] ⚠️ Error en movimiento: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void EscucharServidor(NetworkStream stream, CancellationToken token, Vehiculo vehiculo)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (stream.DataAvailable)
                    {
                        byte[] bufferLongitud = new byte[4];
                        stream.Read(bufferLongitud, 0, 4);
                        int longitud = BitConverter.ToInt32(bufferLongitud, 0);

                        byte[] bufferDatos = new byte[longitud];
                        int totalLeido = 0;
                        while (totalLeido < longitud)
                        {
                            int leido = stream.Read(bufferDatos, totalLeido, longitud - totalLeido);
                            if (leido == 0) break;
                            totalLeido += leido;
                        }

                        Carretera carretera = Carretera.BytesACarretera(bufferDatos);
                        if (carretera == null) continue;

                        lock (vehiculo)
                        {
                            var actualizado = carretera.VehiculosEnCarretera.FirstOrDefault(v => v.Id == vehiculo.Id);
                            if (actualizado != null)
                            {
                                bool estabaParadoAntes = vehiculo.Parado;
                                vehiculo.Parado = actualizado.Parado;

                                // Entrando al puente por primera vez tras esperar
                                if (estabaParadoAntes && !vehiculo.Parado && vehiculo.Pos == 10)
                                {
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                    Console.WriteLine($"[CLIENTE {vehiculo.Id}] 🚦 Entrando al puente desde Km 10");
                                    Console.ResetColor();
                                }
                            }
                        }

                        // Log del estado general de la carretera
                        Console.WriteLine("\n📡 Estado actualizado del servidor:");
                        foreach (var v in carretera.VehiculosEnCarretera.OrderBy(v => v.Id))
                        {
                            string estado = v.Acabado ? $"✅ Finalizado (Km {v.Pos})"
                                        : v.Parado ? $"⛔ Esperando (Km {v.Pos})"
                                        : v.Pos == 10 ? $"🚦 Intentando cruzar (Km {v.Pos})"
                                        : v.Pos > 10 && v.Pos < 50 ? $"🌉 Cruzando puente (Km {v.Pos})"
                                        : $"🛣️ En trayecto (Km {v.Pos})";

                            Console.WriteLine($"→ Vehículo {v.Id} [{v.Direccion}] - {estado}");
                        }
                        Console.WriteLine();
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error en recepción: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}

