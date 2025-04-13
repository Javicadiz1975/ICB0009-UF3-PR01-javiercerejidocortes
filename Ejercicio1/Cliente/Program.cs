using System;
using System.Net.Sockets;

namespace Cliente
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Iniciando cliente...");
                TcpClient cliente = new TcpClient("127.0.0.1", 5000); // Conexión al servidor
                Console.WriteLine("Conectado al servidor.");

                cliente.Close(); // Cierra la conexión
                Console.WriteLine("Conexión cerrada.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error de conexión: " + ex.Message);
            }
        }
    }
}

