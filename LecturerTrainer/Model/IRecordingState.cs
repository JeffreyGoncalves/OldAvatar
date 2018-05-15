using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LecturerTrainer.Model
{
    public enum IRecordingState
    {
        Stopped,
        Monitoring,
        Recording,
        RequestedStop,
        Paused
    }
}
