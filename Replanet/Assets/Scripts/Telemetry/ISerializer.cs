using System.Collections;
using System.Collections.Generic;

public interface ISerializer
{
    void Serialize(TrackerEvent t, string filename);
}
