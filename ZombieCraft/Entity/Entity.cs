using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ContentTypes;

namespace ZombieCraft
{
  enum EntityType
	{
    Civilian,
    Militia,
    Zombie,
	}

  struct Entity
  {
    public readonly int Index;              // Index reference to position in array
    public InstanceTransformRef Transform;  // Instanced model transform data
    public EntityType Type;                 // The type of the entity
    public float HalfSize;                  // Half the size of the square AABB
    public AABB AABB;                       // AABB updated based on Transform and HalfSize
    public Vector3 NextPosition;            // Target position to move to next frame
    public float NextAngle;                 // Target angle to orient to next frame
    public EntityListNode ListNode;         // Reference to node in grid
    public Vector3 Direction;               // Added to NextPosition every frame
    public float StateTime;                 // Accumulated time for current behavior
    public float StateDuration;             // Timeout for current behavior
    public Behavior Behavior;               // Update behavior
    public int Collision;                   // Collision priority. Zero if no collision.
    public List<Vector2> Path;              // Current or previous path being followed
    public int Waypoint;                    // Current waypoint in the path
    public float MoveSpeed;                 // Default move speed
    public Beacon Beacon;                   // Current beacon of influence
    public int BeaconVersion;               // Version of the beacon (increments when beacon moves)

    public static Entity[] Entities;            // Reference to global array for convenience
    public static InstancedModel CivilianModel; // Civilian model used for retrieving transform refs
    public static InstancedModel ZombieModel;   // Zombie model used for retrieving transform refs

    public Entity( int index )
    {
      Index = index;

      Transform = new InstanceTransformRef();
      Type = EntityType.Civilian;
      HalfSize = 1f;
      AABB = new AABB();
      NextPosition = Vector3.Zero;
      NextAngle = 0f;
      ListNode = null;
      Direction = Vector3.Zero;
      StateTime = 0;
      StateDuration = 0;
      Behavior = null;
      Collision = 0;
      Path = new List<Vector2>( 60 );
      Waypoint = -1;
      MoveSpeed = 1f;
      Beacon = null;
      BeaconVersion = 0;
    }
  }
}