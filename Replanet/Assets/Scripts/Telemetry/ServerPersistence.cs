using System.Collections;
using System.Collections.Generic;


public class ServerPersistence : IPersistence
{
    //Usaremos los serializadores que estén activado spara generar en los formatos activados
    List<ISerializer> serializer;
    List<TrackerEvent> events;

    public ServerPersistence(bool CSVSerializer, bool JsonSerializer){

		serializer = new List<ISerializer>();
		events = new List<TrackerEvent>();

		if (CSVSerializer) serializer.Add(new CSVSerializer());
        if (JsonSerializer) serializer.Add(new JsonSerializer());
    }

    //Debe hacer el guardado a archivo de cada "event" y su correspondiente borrado de la lista.
    public void Flush()
    {
        throw new System.NotImplementedException();
    }

    public void Send(TrackerEvent t)
    {
        events.Add(t);
    }
}
