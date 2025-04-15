using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using NetworkStreamNS;
using CarreteraClass;
using VehiculoClass;

namespace Client
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                TcpClient cliente = new TcpClient("127.0.0.1", 5000);
                NetworkStream stream = cliente.GetStream();

                // Crear el vehículo
                Vehiculo miVehiculo = new Vehiculo();
                Console.WriteLine($"Vehículo creado. Dirección: {miVehiculo.Direccion} | Velocidad: {miVehiculo.Velocidad}ms");

                // enviar al servidor
                NetworkStreamClass.EscribirDatosVehiculoNS(stream, miVehiculo);
                Console.WriteLine($"Vehículo enviado. ID: {miVehiculo.Id} | Dirección: {miVehiculo.Direccion}");

                // Recibir ID asignado por el servidor
                string idStr = NetworkStreamClass.LeerMensajeNetworkStream(stream);
                miVehiculo.Id = int.Parse(idStr);
                Console.WriteLine($"Vehículo enviado. ID recibido: {miVehiculo.Id}");

                //token para poder cancelar el hilo de escucha
                CancellationTokenSource cts = new CancellationTokenSource();

                // Iniciar hilo para escuchar datos de la carretera
                Thread hiloRecepcion = new Thread(() => EscucharServidor(stream, cts.Token));
                hiloRecepcion.Start();

                // Crear hilo para mover el vehículo
                Thread hiloMovimiento = new Thread(() => MoverVehiculo(miVehiculo, stream));
                hiloMovimiento.Start();
                hiloMovimiento.Join();
                Console.WriteLine("Movimiento terminado. Cerrando conexión.");

                cts.Cancel();
                hiloRecepcion.Join();

                stream.Close();
                cliente.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }
        static void MoverVehiculo(Vehiculo vehiculo, NetworkStream stream)
        {
            for (int i = 1; i <= 100; i++)
            {
                vehiculo.Pos = i;

                // Enviar datos actualizados al servidor
                NetworkStreamClass.EscribirDatosVehiculoNS(stream, vehiculo);
                //Console.WriteLine($"Posición actual: {vehiculo.Pos} km");
                Thread.Sleep(vehiculo.Velocidad);
            }

            // Marcar como terminado y enviar
            vehiculo.Acabado = true;
            NetworkStreamClass.EscribirDatosVehiculoNS(stream, vehiculo);
            Console.WriteLine("Vehículo ha completado su recorrido.");
        }

        static void EscucharServidor(NetworkStream stream, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (stream.DataAvailable)
                    {
                        // Leer la longitud del mensaje (4 bytes)
                        byte[] bufferLongitud = new byte[4];
                        stream.Read(bufferLongitud, 0, 4);
                        int longitud = BitConverter.ToInt32(bufferLongitud, 0);

                        // Leer el contenido XML según longitud
                        byte[] bufferDatos = new byte[longitud];
                        int totalLeido = 0;
                        while (totalLeido < longitud)
                        {
                            int leido = stream.Read(bufferDatos, totalLeido, longitud - totalLeido);
                            if (leido == 0) break;
                            totalLeido += leido;
                        }

                        Carretera carretera = Carretera.BytesACarretera(bufferDatos);

                        Console.WriteLine("\nInformación recibida del servidor:");
                        foreach (Vehiculo v in carretera.VehiculosEnCarretera)
                        {
                            string estado = v.Pos >= 100 ? "Finalizado" : "En trayecto";
                            Console.WriteLine($"→ Vehículo {v.Id} [{v.Direccion}] - {estado} (Km {v.Pos})");
                        }
                        Console.WriteLine();
                    }

                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    Console.WriteLine($"⚠️ Error al recibir datos del servidor: {ex.Message}");
                }
            }
        }

    }
}