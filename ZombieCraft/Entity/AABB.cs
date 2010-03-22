using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZombieCraft
{
  struct AABB
  {
    public Vector2 Min;
    public Vector2 Max;

    public AABB( Vector2 min, Vector2 max )
    {
      Min = min;
      Max = max;
    }
  }
}