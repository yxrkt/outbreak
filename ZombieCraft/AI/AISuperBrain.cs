using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZombieCraft
{
  static class AISuperBrain
  {
    static AIGrid grid;
    static Vector2 min, max;
    static Matrix rotation;

    public static void InitializeGrid( float width, float height, int cols, int rows )
    {
      grid = new AIGrid( width, height, cols, rows );
      min = grid.Min;
      max = grid.Max;
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
      // update position
      entity.Transform.Position = entity.NextPosition;

      entity.AABB.Min.X = entity.NextPosition.X - entity.HalfSize;
      entity.AABB.Min.Y = entity.NextPosition.Z - entity.HalfSize;
      entity.AABB.Max.X = entity.NextPosition.X + entity.HalfSize;
      entity.AABB.Max.Y = entity.NextPosition.Z + entity.HalfSize;

      // update bounding box
      /**/
      //Vector2.Clamp( ref entity.AABB.Min, ref min, ref max, out entity.AABB.Min );
      //Vector2.Clamp( ref entity.AABB.Max, ref min, ref max, out entity.AABB.Max );

      //Vector2.Max( ref entity.AABB.Min, ref min, out entity.AABB.Min );
      //Vector2.Min( ref entity.AABB.Max, ref max, out entity.AABB.Max );

      //entity.AABB.Min.X = ( entity.AABB.Min.X > max.X ) ? max.X : entity.AABB.Min.X;
      //entity.AABB.Min.X = ( entity.AABB.Min.X < min.X ) ? min.X : entity.AABB.Min.X;
      //entity.AABB.Min.Y = ( entity.AABB.Min.Y > max.Y ) ? max.Y : entity.AABB.Min.Y;
      //entity.AABB.Min.Y = ( entity.AABB.Min.Y < min.Y ) ? min.Y : entity.AABB.Min.Y;

      //entity.AABB.Max.X = ( entity.AABB.Max.X > max.X ) ? max.X : entity.AABB.Max.X;
      //entity.AABB.Max.X = ( entity.AABB.Max.X < min.X ) ? min.X : entity.AABB.Max.X;
      //entity.AABB.Max.Y = ( entity.AABB.Max.Y > max.Y ) ? max.Y : entity.AABB.Max.Y;
      //entity.AABB.Max.Y = ( entity.AABB.Max.Y < min.Y ) ? min.Y : entity.AABB.Max.Y;
      /*/
      if ( entity.AABB.Min.X < grid.Min.X )
        entity.AABB.Min.X = grid.Min.X;

      if ( entity.AABB.Min.Y < grid.Min.Y )
        entity.AABB.Min.Y = grid.Min.Y;

      if ( entity.AABB.Max.X > grid.Max.X )
        entity.AABB.Max.X = grid.Max.X - .0001f;

      if ( entity.AABB.Max.Y > grid.Max.Y )
        entity.AABB.Max.Y = grid.Max.Y - .0001f;
      /**/

      // update place in grid
      grid.UpdateItem( ref entity );

      // check for collisions
      //...

      // update AI with a huge switch statement
      //...
      //entity.NextPosition = Vector3.Transform( entity.Transform.Position, rotation );
      Vector3.Transform( ref entity.NextPosition, ref rotation, out entity.NextPosition );
    }
  }
}
