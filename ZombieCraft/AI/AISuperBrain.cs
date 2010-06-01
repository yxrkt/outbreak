using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Utility;

namespace ZombieCraft
{
  delegate void Behavior( ref Entity entity );

  static class AISuperBrain
  {
    static readonly Vector2 durationRange = new Vector2( 1.5f, 6f );
    static readonly Vector2 speedRange = new Vector2( 1f, 3f );

    static AIGrid grid;
    static Vector2 min, max;
    static Vector3 min3D, max3D;
    static Matrix rotation;

    static Random random = new Random();

    public static float Elapsed;
    public static int PathsFound = 0;
    public const int MaxPathsPerFrame = 10;

    public static Vector2 GridMin { get { return min; } }
    public static Vector2 GridMax { get { return max; } }

    public static void InitializeGrid( float width, float height, int cols, int rows )
    {
      grid = new AIGrid( width, height, cols, rows );
      min = grid.Min;
      max = grid.Max - new Vector2( .0001f, .0001f );
      min3D = new Vector3( min.X, 0, min.Y );
      max3D = new Vector3( max.X, 0, max.Y );
      rotation = Matrix.CreateRotationY( MathHelper.ToRadians( .1f ) );

      CollisionManager.Initialize( grid, 16 );
    }

    public static void PopulateGrid( Entity[] entities, Building[] buildings )
    {
      for ( int i = 0; i < entities.Length; ++i )
        grid.AddEntity( ref entities[i] );

      for ( int i = 0; i < buildings.Length; ++i )
        grid.AddBuilding( ref buildings[i] );

      grid.InitializePathGrid( 6 );
    }

    public static void DrawGrid( Matrix view, Matrix projection )
    {
      grid.Draw( view, projection );
    }

    public static void Update( ref Entity entity )
    {
      // update behavior
      if ( entity.Behavior != null )
      {
        entity.StateTime += Elapsed;
        entity.Behavior( ref entity );
      }

      // update position and angle
      entity.NextPosition.X += Elapsed * entity.Direction.X;
      entity.NextPosition.Z += Elapsed * entity.Direction.Z;
      entity.Transform.Position = entity.NextPosition;
      entity.Transform.Angle = entity.NextAngle;

      Vector3.Clamp( ref entity.NextPosition, ref min3D, ref max3D, out entity.NextPosition );

      entity.AABB.Min.X = entity.NextPosition.X - entity.HalfSize;
      entity.AABB.Min.Y = entity.NextPosition.Z - entity.HalfSize;
      entity.AABB.Max.X = entity.NextPosition.X + entity.HalfSize;
      entity.AABB.Max.Y = entity.NextPosition.Z + entity.HalfSize;

      // update bounding box
      Vector2.Clamp( ref entity.AABB.Min, ref min, ref max, out entity.AABB.Min );
      Vector2.Clamp( ref entity.AABB.Max, ref min, ref max, out entity.AABB.Max );

      // update place in grid
      grid.UpdateEntity( ref entity );
    }

    public static void UpdateFast( ref Entity entity )
    {
      entity.NextPosition.X += Elapsed * entity.Direction.X;
      entity.NextPosition.Z += Elapsed * entity.Direction.Z;
      entity.StateTime += Elapsed;

      Vector3.Clamp( ref entity.NextPosition, ref min3D, ref max3D, out entity.NextPosition );

      entity.Transform.array[entity.Transform.index].Position = entity.NextPosition;
    }

    public static void PathTo( ref Entity entity, ref Vector2 destination )
    {
      Vector2 position = Vector2.Zero;
      position.X = entity.NextPosition.X;
      position.Y = entity.NextPosition.Z;

      entity.Path.Clear();
      grid.GeneratePath( ref position, ref destination, entity.Path );
      entity.Waypoint = 1;
      entity.Behavior = TracePath;

      PathsFound++;
    }

    private static void PollBeacons( ref Entity entity )
    {
      // have the beacons only summon zombies
      if ( entity.Type != EntityType.Zombie || entity.Behavior == MorphToZombie )
        return;

      float bestDist = float.MaxValue;
      Beacon bestBeacon = null;
      foreach ( Beacon beacon in Beacon.Beacons )
      {
        if ( !beacon.Active ) continue;

        float distSquared;
        Vector3.DistanceSquared( ref beacon.Position, ref entity.NextPosition, out distSquared );
        if ( distSquared < bestDist && distSquared < beacon.RadiusSquared )
        {
          bestDist = distSquared;
          bestBeacon = beacon;
        }
      }

      if ( bestBeacon == null )
      {
        //if ( entity.Behavior != Wander )
          entity.Behavior = Wander;
        return;
      }

      // generating paths is slow; we may only make so many per frame
      if ( PathsFound < MaxPathsPerFrame )
      {
        if ( bestBeacon != entity.Beacon || bestBeacon.Version != entity.BeaconVersion )
        {
          Vector2 target = Vector2.Zero;
          target.X = bestBeacon.Position.X;
          target.Y = bestBeacon.Position.Z;
          PathTo( ref entity, ref target );
          entity.Beacon = bestBeacon;
          entity.BeaconVersion = bestBeacon.Version;
        }
      }
    }

    private static void IdleFunction( ref Entity entity )
    {
      if ( entity.Collision == 0 )
        entity.Direction = Vector3.Zero;
      PollBeacons( ref entity );
    }
    public static Behavior Idle = IdleFunction;

    private static void WanderFunction( ref Entity entity )
    {
      if ( entity.StateTime < entity.StateDuration )
      {
        entity.StateTime += Elapsed;
      }
      else
      {
        entity.StateTime = 0;
        entity.StateDuration = MathHelper.Lerp( durationRange.X, durationRange.Y, random.NextFloat() );

        //if ( entity.Direction.X == 0 || entity.Direction.Z == 0 )
        //{
          float speed = MathHelper.Lerp( speedRange.X, speedRange.Y, random.NextFloat() );
          Vector3 nextDirection = random.NextVector3();
          if ( nextDirection.X == 0 && nextDirection.Z == 0 )
            nextDirection.X = speed;
          else
            nextDirection.Normalize();
          nextDirection *= speed;
          entity.Direction = nextDirection;
        //}
        //else
        //{
        //  entity.Direction = Vector3.Zero;
        //}
      }

      PollBeacons( ref entity );
    }
    public static Behavior Wander = WanderFunction;

    private static void TracePathFunction( ref Entity entity )
    {
      Vector2 position = Vector2.Zero;
      position.X = entity.NextPosition.X;
      position.Y = entity.NextPosition.Z;

      Vector2 lastPoint = entity.Path[entity.Waypoint - 1];
      Vector2 nextPoint = entity.Path[entity.Waypoint];

      Vector2 totalMovement;
      Vector2.Subtract( ref position, ref lastPoint, out totalMovement );

      Vector2 toNextWaypoint;
      Vector2.Subtract( ref nextPoint, ref position, out toNextWaypoint );

      float dot;
      Vector2.Dot( ref totalMovement, ref toNextWaypoint, out dot );
      if ( totalMovement != Vector2.Zero && dot <= 0 )
      {
        // reached end of path
        if ( ++entity.Waypoint == entity.Path.Count )
        {
          entity.Path.Clear();
          entity.Waypoint = -1;
          entity.Behavior = IdleFunction;
        }
        // moving on to next waypoint
        else
        {
          Vector2 newDir = entity.Path[entity.Waypoint] - nextPoint;
          newDir.Normalize();
          Vector2.Multiply( ref newDir, entity.MoveSpeed, out newDir );
          entity.Direction.X = newDir.X;
          entity.Direction.Z = newDir.Y;
        }
      }
      else
      {
        Vector2 dir = nextPoint - lastPoint;
        dir.Normalize();
        Vector2.Multiply( ref dir, entity.MoveSpeed, out dir );
        entity.Direction.X = dir.X;
        entity.Direction.Z = dir.Y;
      }

      PollBeacons( ref entity );
    }
    public static Behavior TracePath = TracePathFunction;

    private static void MorphToZombieFunction( ref Entity entity )
    {
      if ( entity.Type == EntityType.Civilian )
      {
        entity.StateTime = 0f;
        entity.Type = EntityType.Zombie;
      }
      else if ( entity.StateTime < 1f )
      {
        //entity.NextPosition += Vector3.Lerp( new Vector3( -.5f, 0f, -.5f ), new Vector3( .5f, 0f, .5f ), random.NextFloat() );
        entity.NextAngle += Elapsed * MathHelper.TwoPi * 5;
        entity.StateTime += Elapsed;
      }
      else
      {
        InstanceTransformRef oldRef = entity.Transform;
        entity.Transform = InstancedModelDrawer.GetInstanceRef( Entity.ZombieModel );
        entity.Transform.Position = oldRef.Position;
        entity.Transform.Scale = oldRef.Scale;
        entity.Transform.Axis = oldRef.Axis;
        entity.Transform.Angle = oldRef.Angle;
        InstancedModelDrawer.ReleaseInstanceRef( oldRef );
        entity.Behavior = Wander;
      }
    }
    public static Behavior MorphToZombie = MorphToZombieFunction;
  }
}
