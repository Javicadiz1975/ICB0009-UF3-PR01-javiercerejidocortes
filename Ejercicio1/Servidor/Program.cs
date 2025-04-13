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

            // Acepta un solo cliente
            TcpClient cliente = servidor.AcceptTcpClient();
            Console.WriteLine("Cliente conectado.");

            // Cierra la conexión
            cliente.Close();
            servidor.Stop();

            Console.WriteLine("Conexión cerrada. Fin del servidor.");
        }
    }
}

