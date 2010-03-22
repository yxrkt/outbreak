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

    public static void Update( ref Entity entity )
    {
      entity.Transform.Position = entity.Transform.Position;
      entity.Transform.Angle += .01f;

      //entity.Transform.array[entity.Transform.index].Position = entity.Transform.array[entity.Transform.index].Position;
    }
  }
}
