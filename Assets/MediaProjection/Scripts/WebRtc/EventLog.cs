#nullable enable

using System;
using System.Collections.Generic;

namespace MediaProjection.WebRtc
{
    [Serializable]
    public struct EventLog
    {
        public List<EventLogEntry> dataList;
    }

    [Serializable]
    public struct EventLogEntry
    {
        public string key;
        public string value;
    }
}