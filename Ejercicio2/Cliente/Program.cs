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

                // Crear hilo para mover el vehículo
                Thread hiloMovimiento = new Thread(() => MoverVehiculo(miVehiculo, stream));
                hiloMovimiento.Start();
                hiloMovimiento.Join();
                Console.WriteLine("Movimiento terminado. Cerrando conexión.");

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
                Console.WriteLine($"Posición actual: {vehiculo.Pos} km");
                Thread.Sleep(vehiculo.Velocidad);
            }

            // Marcar como terminado y enviar
            vehiculo.Acabado = true;
            NetworkStreamClass.EscribirDatosVehiculoNS(stream, vehiculo);
            Console.WriteLine("Vehículo ha completado su recorrido.");
        }
    }
}