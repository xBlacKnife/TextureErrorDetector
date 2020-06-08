using System.Collections;
using System.Collections.Generic;

public class ResourceTracker : ITrackerAsset
{
    public bool accept(TrackerEvent t)
    {
        return t.commonContent.eventType_ == EventType.Resource_Event;
    }
}
