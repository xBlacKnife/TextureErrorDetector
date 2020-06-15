using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPersistence 
{
    void Send(TrackerEvent t);
    void Flush();
}
