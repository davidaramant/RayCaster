using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayCasterGame
{
    [Flags]
    enum MovementInputs
    {
        None = 0,
        Forward = 1 << 1,
        Backward = 1 << 2,
        TurnLeft = 1 << 3,
        TurnRight = 1 << 4,
        StrafeLeft = 1 << 5,
        StrafeRight = 1 << 6,
    }
}
