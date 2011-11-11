using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.Kinect.Movement.EventsArgs
{
    public enum SkeletonOnViewType
    {
        Entered,
        Exited
    }

    public class SkeletonOnViewEventArgs
    {
        public SkeletonOnViewType State { get; internal set; }
    }
}
