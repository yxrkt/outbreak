using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZombieCraft
{
  struct Entity
  {
    public readonly int Index;
    public InstanceTransformRef Transform;
    public AABB AABB;
    public Vector3 NextPosition;
    public float NextAngle;

    public static Entity[] Entities;

    public Entity( int index )
    {
      Index = index;

      Transform = new InstanceTransformRef();
      AABB = new AABB();
      NextPosition = Vector3.Zero;
      NextAngle = 0f;
    }
  }
}