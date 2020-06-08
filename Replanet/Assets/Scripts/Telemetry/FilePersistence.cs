using System.Collections;
using System.Collections.Generic;


public class FilePersistence : IPersistence
{
    //Usaremos los serializadores que estén activado spara generar en los formatos activados
    List<ISerializer> serializer;
    List<TrackerEvent> events;

    public FilePersistence(bool CSVSerializer, bool JsonSerializer) {

		serializer = new List<ISerializer>();
		events = new List<TrackerEvent>();

        if (CSVSerializer) serializer.Add(new CSVSerializer());
        if (JsonSerializer) serializer.Add(new JsonSerializer());
    }

    //Debe hacer el guardado a archivo de cada "event" y su correspondiente borrado de la lista.
    public void Flush()
    {
		while (events.Count > 0) //Mientras haya eventos en la lista...
		{
			TrackerEvent t = events[0];
			events.RemoveAt(0);

			foreach(ISerializer IS in serializer)
			{
				IS.Serialize(t, "TrackedEvents");
			}

		}
    }

    public void Send(TrackerEvent t)
    {
        events.Add(t);
    }
}
