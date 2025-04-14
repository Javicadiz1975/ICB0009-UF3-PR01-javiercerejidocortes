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

                // nviar al servidor
                NetworkStreamClass.EscribirDatosVehiculoNS(stream, miVehiculo);
                Console.WriteLine($"Vehículo enviado. ID: {miVehiculo.Id} | Dirección: {miVehiculo.Direccion}");

                cliente.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }

    }
}