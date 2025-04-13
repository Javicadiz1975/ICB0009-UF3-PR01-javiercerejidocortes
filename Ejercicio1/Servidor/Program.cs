using System;
using System.Net;
using System.Net.Sockets;

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
            string direccion;

            // Bloque protegido para asignar ID de forma segura
            lock (lockObj)
            {
                contadorVehiculos++;
                id = contadorVehiculos;
            }

             // Dirección aleatoria
            Random rnd = new Random();
            direccion = rnd.Next(2) == 0 ? "Norte" : "Sur";

            Console.WriteLine($"Gestionando nuevo vehículo...");
            Console.WriteLine($"Vehículo ID: {id}, Dirección asignada: {direccion}");

            cliente.Close();
        }
    }
}

