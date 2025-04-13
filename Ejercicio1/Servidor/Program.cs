using System.Net;
using System.Net.Sockets;
using NetworkStreamNS;

namespace Servidor
{
    class Program
    {
        static int contadorVehiculos = 0; // Contador global de IDs
        static readonly object lockObj = new object(); // Objeto para proteger el acceso concurrente
        static List<Cliente> listaClientes = new List<Cliente>(); // Lista para guardar los clientes conectados

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
            NetworkStream stream;

            // Bloque protegido para asignar ID de forma segura
            lock (lockObj)
            {
                contadorVehiculos++;
                id = contadorVehiculos;
            }

            try
            {
                // Obtener el NetworkStream del cliente
                stream = cliente.GetStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el stream del cliente ID {id}: {ex.Message}");
                return; // Si falla, salimos del método
            }
            Console.WriteLine("Gestionando nuevo vehículo...");

            Cliente nuevoCliente = new Cliente(id, stream);

            lock (listaClientes)
            {
                listaClientes.Add(nuevoCliente);
                Console.WriteLine($"Vehículo ID {id} añadido a la lista.");
                Console.WriteLine($"Total de clientes conectados: {listaClientes.Count}");
            }


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

