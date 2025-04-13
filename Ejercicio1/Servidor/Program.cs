using System;
using System.Net;
using System.Net.Sockets;

namespace Servidor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Servidor iniciado. Esperando un cliente...");

            // Crear el servidor en el puerto 5000
            TcpListener servidor = new TcpListener(IPAddress.Any, 5000);
            servidor.Start();

            // Bucle infinito para aceptar muchos clientes
            while (true)
            {
                TcpClient cliente = servidor.AcceptTcpClient();
                Console.WriteLine("Nuevo cliente conectado.");

                // Crear un hilo por cliente
                Thread hiloCliente = new Thread(() => GestionarVehiculo(cliente));
                hiloCliente.Start();
            }
        }

        // Método que simula la gestión de un vehículo
        static void GestionarVehiculo(TcpClient cliente)
        {
            Console.WriteLine("Gestionando nuevo vehículo...");
            cliente.Close();
        }
    }
}

