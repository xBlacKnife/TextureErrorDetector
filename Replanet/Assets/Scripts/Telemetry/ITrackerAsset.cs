using System.Collections;
using System.Collections.Generic;

public interface ITrackerAsset
{
    bool accept(TrackerEvent t);
}
