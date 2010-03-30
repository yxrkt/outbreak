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
      Vector2.Clamp( ref entity.AABB.Min, ref min, ref max, out entity.AABB.Min );
      Vector2.Clamp( ref entity.AABB.Max, ref min, ref max, out entity.AABB.Max );

      // update place in grid
      grid.UpdateItem( ref entity );

      // check for collisions
      foreach ( GridCellList list in entity.GridNode.parent )
      {
        if ( list != null )
        {
          for ( GridCellListNode node = list.First(); node != null; node = node.next[list.cellType] )
          {
          }
        }
      }

      // update AI with a huge switch statement
      //...
      //Vector3.Transform( ref entity.NextPosition, ref rotation, out entity.NextPosition );
    }
  }
}
