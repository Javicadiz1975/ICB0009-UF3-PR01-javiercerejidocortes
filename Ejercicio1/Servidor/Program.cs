using System.Net;
using System.Net.Sockets;
using NetworkStreamNS;

namespace Servidor
{
    class Program
    {
        static int contadorVehiculos = 0; // Contador global de IDs
        static readonly object lockObj = new object(); // Objeto para proteger el acceso concurrente
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
            int id;

            // Bloque protegido para asignar ID de forma segura
            lock (lockObj)
            {
                contadorVehiculos++;
                id = contadorVehiculos;
            }

            NetworkStream stream = cliente.GetStream();
            Console.WriteLine("Gestionando nuevo vehículo...");

            //Leer el mensaje inicial del cliente
            string mensajeInicio = NetworkStreamClass.LeerMensajeNetworkStream(stream);

            if (mensajeInicio == "INICIO")
            {
                Console.WriteLine($"Handshake iniciado por cliente.");

                //Enviar el ID al cliente
                NetworkStreamClass.EscribirMensajeNetworkStream(stream, id.ToString());
                Console.WriteLine($"ID {id} enviado al cliente.");

                //Esperar confirmación del cliente
                string confirmacion = NetworkStreamClass.LeerMensajeNetworkStream(stream);
                if (confirmacion == id.ToString())
                {
                    Console.WriteLine($"Cliente confirmó ID correctamente. Vehículo {id} listo.");
                }
                else
                {
                    Console.WriteLine("Cliente devolvió ID incorrecto. Finalizando conexión.");
                }
            }
            else
            {
                Console.WriteLine("No se recibió mensaje INICIO. Finalizando conexión.");
            }

            cliente.Close();
            

        }
    }
}

