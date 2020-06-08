using System.Collections;
using System.Collections.Generic;

public class TimeStampTracker : ITrackerAsset
{
    public bool accept(TrackerEvent t)
    {
        return t.commonContent.eventType_ == EventType.Time_Stamp_Event;
    }
}
