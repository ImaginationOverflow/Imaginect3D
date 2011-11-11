using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace UI.Kinect.Movement.Gestures
{
    public interface IGesture
    {
        void Register();
        void Unregister();
        Matrix GetTransformedMatrix();
    }
}
