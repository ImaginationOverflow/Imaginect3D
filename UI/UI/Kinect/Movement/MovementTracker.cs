using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UI.Kinect.Movement.EventsArgs;

namespace UI.Kinect.Movement
{
    delegate void MovementEventHandler (object state, MovementEventHandlerArgs args);

    public enum MovementType
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    class MovementTracker
    {
        public MovementTracker() {

        }

        private IDictionary<MovementType, IList<Tuple<float, MovementEventHandler>>> _movementHandlers = 
            new Dictionary<MovementType, IList<Tuple<float, MovementEventHandler>>> {
                { MovementType.UP, new SortedList<Tuple<float, MovementEventHandler>>() },
                { MovementType.DOWN, new SortedList<Tuple<float, MovementEventHandler>>() },
                { MovementType.LEFT, new SortedList<Tuple<float, MovementEventHandler>>() },
                { MovementType.RIGHT, new SortedList<Tuple<float, MovementEventHandler>>() }
            };

        public void AddMovementHandler(MovementType type, float threshold, MovementEventHandler handler) {

        }

    }
}
