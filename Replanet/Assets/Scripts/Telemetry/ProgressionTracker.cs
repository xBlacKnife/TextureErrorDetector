using System.Collections;
using System.Collections.Generic;

public class ProgressionTracker : ITrackerAsset
{
    public bool accept(TrackerEvent t)
    {
        return t.commonContent.eventType_ == EventType.Progression;
    }
}
