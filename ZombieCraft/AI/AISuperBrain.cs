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

    public static void InitializeGrid( float width, float height, int cols, int rows )
    {
      grid = new AIGrid( width, height, cols, rows );
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

      // update bounding box
      entity.AABB.Min.X = entity.NextPosition.X - entity.HalfSize;
      if ( entity.AABB.Min.X < grid.Min.X )
        entity.AABB.Min.X = grid.Min.X;

      entity.AABB.Min.Y = entity.NextPosition.Z - entity.HalfSize;
      if ( entity.AABB.Min.Y < grid.Min.Y )
        entity.AABB.Min.Y = grid.Min.Y;

      entity.AABB.Max.X = entity.NextPosition.X + entity.HalfSize;
      if ( entity.AABB.Max.X > grid.Max.X )
        entity.AABB.Max.X = grid.Max.X - .0001f;

      entity.AABB.Max.Y = entity.NextPosition.Z + entity.HalfSize;
      if ( entity.AABB.Max.Y > grid.Max.Y )
        entity.AABB.Max.Y = grid.Max.Y - .0001f;

      // update place in grid
      grid.UpdateItem( ref entity );

      // check for collisions
      //...

      // update AI with a huge switch statement
      //...
      Matrix rotation = Matrix.CreateRotationY( MathHelper.ToRadians( .1f ) );
      entity.NextPosition = Vector3.Transform( entity.Transform.Position, rotation );
    }
  }
}
