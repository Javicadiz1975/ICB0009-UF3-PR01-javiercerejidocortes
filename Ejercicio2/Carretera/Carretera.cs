using System.Xml.Serialization; // Permite convertir objetos a XML (serialización)
using VehiculoClass; // Importa la clase Vehiculo desde su namespace

namespace CarreteraClass; // Define el espacio de nombres de esta clase

[Serializable] // Indica que esta clase se puede serializar (por ejemplo, a XML)
public class Carretera
{
    // Lista que contiene todos los vehículos que circulan por la carretera
    public List<Vehiculo> VehiculosEnCarretera = new List<Vehiculo>();

    // Contador de vehículos añadidos a la carretera
    public int NumVehiculosEnCarrera = 0;

    // Constructor vacío
    public Carretera ()
    {
    }

    // Crea un nuevo vehículo y lo añade a la lista de vehículos
    public void CrearVehiculo ()
    {
        Vehiculo V = new Vehiculo(); // Crea una nueva instancia de Vehiculo
        VehiculosEnCarretera.Add(V); // Añade el vehículo a la lista
    }

    // Añade un vehículo ya creado a la lista de vehículos en carretera
    public void AñadirVehiculo (Vehiculo V)
    {
        VehiculosEnCarretera.Add(V); // Añade el vehículo a la lista
        NumVehiculosEnCarrera++; // Incrementa el contador de vehículos
    }

    // Actualiza la información de un vehículo existente en la lista
    public void ActualizarVehiculo (Vehiculo V)
    {
        // Busca el vehículo en la lista por su Id
        Vehiculo veh = VehiculosEnCarretera.FirstOrDefault(x => x.Id == V.Id);

        if (veh != null) // Si lo encuentra
        {
            veh.Pos = V.Pos; // Actualiza la posición
            veh.Velocidad = V.Velocidad; // Actualiza la velocidad
        }
    }

    // Muestra por consola la posición de todos los vehículos en la carretera
    public void MostrarVehiculo ()
    {
        string strVehs = ""; // Cadena para acumular las posiciones

        foreach (Vehiculo v in VehiculosEnCarretera)
        {
            strVehs = strVehs + "\t" + v.Pos; // Añade cada posición con tabulador
        }

        Console.WriteLine(strVehs); // Imprime todas las posiciones
    }

    // Convierte el objeto Carretera a un array de bytes usando XML
    public byte[] CarreteraABytes()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Carretera)); // Serializador XML para Carretera

        MemoryStream MS = new MemoryStream(); // Crea un flujo de memoria

        serializer.Serialize(MS, this); // Serializa el objeto actual en el flujo

        return MS.ToArray(); // Devuelve el contenido del flujo como array de bytes
    }

    // Convierte un array de bytes en un objeto Carretera (deserializa)
    public static Carretera BytesACarretera(byte[] bytesCarrera)
    {
        Carretera tmpCarretera; // Variable temporal para guardar el resultado
        
        XmlSerializer serializer = new XmlSerializer(typeof(Carretera)); // Serializador XML

        MemoryStream MS = new MemoryStream(bytesCarrera); // Crea un flujo con los bytes recibidos

        tmpCarretera = (Carretera) serializer.Deserialize(MS); // Deserializa el contenido a un objeto Carretera

        return tmpCarretera; // Devuelve el objeto reconstruido
    }    
}
