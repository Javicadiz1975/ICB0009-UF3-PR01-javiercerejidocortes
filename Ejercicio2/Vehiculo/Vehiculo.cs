using System.Xml.Serialization;

namespace VehiculoClass;

[Serializable]
public class Vehiculo
{
    public int Id {get; set;}
    public int Pos {get;set;}
    public int Velocidad {get; set;}
    public string Direccion {get; set;} // "Norte" o "Sur" 
    public bool Acabado {get;set;}
    public bool Parado {get; set;}
    
    public Vehiculo()
    {
        var randVelocidad = new Random();

        
        this.Pos = 0;
        this.Velocidad = randVelocidad.Next(100,500);
        this.Direccion = randVelocidad.Next(0, 2) == 0 ? "Norte" : "Sur";
        this.Acabado = false;
        this.Parado = false;
       
    }

    //Permite serializar Vehiculo a array de bytes mediant formato XML
    public byte[] VehiculoaBytes()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Vehiculo));
            
        MemoryStream MS = new MemoryStream();
  
        serializer.Serialize(MS, this);
       
        return MS.ToArray();
    }

    //Permite desserializar una cadena de bytes a un objeto de tipo Vehiculo
    public static Vehiculo BytesAVehiculo(byte[] bytesVehiculo)
    {
        Vehiculo tmpVehiculo; 
        
        XmlSerializer serializer = new XmlSerializer(typeof(Vehiculo));

        MemoryStream MS = new MemoryStream(bytesVehiculo);

        tmpVehiculo = (Vehiculo)serializer.Deserialize(MS);

        return tmpVehiculo;
    }
}
