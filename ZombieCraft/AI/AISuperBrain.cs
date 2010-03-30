using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Utility;

namespace ZombieCraft
{
  static class AISuperBrain
  {
    static readonly Vector2 durationRange = new Vector2( 1.5f, 6f );
    static readonly Vector2 speedRange = new Vector2( 1f, 3f );

    static AIGrid grid;
    static Vector2 min, max;
    static Matrix rotation;

    static Random random = new Random();

    public static float Elapsed;

    public static void InitializeGrid( float width, float height, int cols, int rows )
    {
      grid = new AIGrid( width, height, cols, rows );
      min = grid.Min;
      max = grid.Max - new Vector2( .0001f, .0001f );
      rotation = Matrix.CreateRotationY( MathHelper.ToRadians( .1f ) );
    }

    public static void PopulateGrid( Entity[] entities )
    {
      for ( int i = 0; i < entities.Length; ++i )
        grid.AddItem( ref entities[i] );
    }

    public static void DrawGrid( Matrix view, Matrix projection )
    {
      grid.Draw( view, projection );
    }

    public static void Update( ref Entity entity )
    {
      // update AI with a huge switch statement
      // just wander for now...
      if ( entity.StateTime < entity.StateDuration )
      {
        entity.NextPosition.X += Elapsed * entity.Direction.X;
        entity.NextPosition.Z += Elapsed * entity.Direction.Z;
        entity.StateTime += Elapsed;
      }
      else
      {
        entity.StateTime = 0;
        entity.StateDuration = MathHelper.Lerp( durationRange.X, durationRange.Y, random.NextFloat() );

        float speed = MathHelper.Lerp( speedRange.X, speedRange.Y, random.NextFloat() );

        Vector3 nextDirection = random.NextVector3();

        if ( nextDirection.X == 0 && nextDirection.Z == 0 )
          nextDirection.X = speed;
        else
          nextDirection.Normalize();

        nextDirection *= speed;

        //if ( Vector3.Dot( entity.Direction, nextDirection ) < 0 )
        //  nextDirection *= -1;

        entity.Direction = nextDirection;
      }


      // update position
      entity.Transform.Position = entity.NextPosition;

      entity.AABB.Min.X = entity.NextPosition.X - entity.HalfSize;
      entity.AABB.Min.Y = entity.NextPosition.Z - entity.HalfSize;
      entity.AABB.Max.X = entity.NextPosition.X + entity.HalfSize;
      entity.AABB.Max.Y = entity.NextPosition.Z + entity.HalfSize;

      // update bounding box
      Vector2.Clamp( ref entity.AABB.Min, ref min, ref max, out entity.AABB.Min );
      Vector2.Clamp( ref entity.AABB.Max, ref min, ref max, out entity.AABB.Max );

      // update place in grid
      grid.UpdateItem( ref entity );
    }

    public static void UpdateFast( ref Entity entity )
    {
      entity.NextPosition.X += Elapsed * entity.Direction.X;
      entity.NextPosition.Z += Elapsed * entity.Direction.Z;
      entity.StateTime += Elapsed;

      entity.Transform.array[entity.Transform.index].Position = entity.NextPosition;
    }
  }
}
