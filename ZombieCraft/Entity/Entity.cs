using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZombieCraft
{
  struct Entity
  {
    public readonly int Index;              // Index reference to position in array
    public InstanceTransformRef Transform;  // Instanced model transform data
    public float HalfSize;                  // Half the size of the square AABB
    public AABB AABB;                       // AABB updated based on Transform and HalfSize
    public Vector3 NextPosition;            // Target position to move to next frame
    public float NextAngle;                 // Target angle to orient to next frame
    public GridCellListNode GridNode;       // Reference to node in grid
    public Vector3 Direction;
    public float StateTime;
    public float StateDuration;
    public Behavior Behavior;

    public static Entity[] Entities;        // Reference to global array for convenience

    public Entity( int index )
    {
      Index = index;

      Transform = new InstanceTransformRef();
      HalfSize = 1f;
      AABB = new AABB();
      NextPosition = Vector3.Zero;
      NextAngle = 0f;
      GridNode = null;
      Direction = Vector3.Zero;
      StateTime = 0;
      StateDuration = 0;
      Behavior = null;
    }
  }
}