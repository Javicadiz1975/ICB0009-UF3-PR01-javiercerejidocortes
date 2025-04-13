using System;
using System.Net.Sockets;
using NetworkStreamNS;

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

                // Obtener el NetworkStream
                NetworkStream stream = cliente.GetStream();
                //Enviar "INICIO"
                NetworkStreamClass.EscribirMensajeNetworkStream(stream, "INICIO");
                Console.WriteLine("Mensaje 'INICIO' enviado al servidor.");

                //Leer ID enviado por el servidor
                string idRecibido = NetworkStreamClass.LeerMensajeNetworkStream(stream);
                Console.WriteLine($"ID recibido del servidor: {idRecibido}");

                //Enviar confirmación de vuelta
                NetworkStreamClass.EscribirMensajeNetworkStream(stream, idRecibido);
                Console.WriteLine("ID confirmado al servidor.");

                cliente.Close();
                Console.WriteLine("Conexión cerrada.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error de conexión: " + ex.Message);
            }
        }
    }
}

