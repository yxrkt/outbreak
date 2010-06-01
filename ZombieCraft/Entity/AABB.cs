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

    public void GetVerts( ref Vector2[] verts )
    {
      verts[0] = Min;
      verts[1] = new Vector2( Min.X, Max.Y );
      verts[2] = Max;
      verts[3] = new Vector2( Max.X, Min.Y );
    }
  }
}