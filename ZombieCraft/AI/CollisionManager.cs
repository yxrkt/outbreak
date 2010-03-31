using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZombieCraft
{
  static class CollisionManager
  {
    static AIGrid grid;
    static int interleave;

    public static void Initialize( AIGrid grid, int interleave )
    {
      CollisionManager.grid = grid;
      CollisionManager.interleave = interleave;
    }

    public static void Update()
    {
      for ( int r = 0; r < grid.Rows; ++r )
      {
        for ( int c = 0; c < grid.Cols; ++c )
        {
          GridCellList list = grid[r, c];

          //for ( GridCellListNode nodeA = list.First(); nodeA != null; nodeA = list.Next( nodeA ) )
          //{
          //  for ( GridCellListNode nodeB = list.Next( nodeA ); nodeB != null; nodeB = list.Next( nodeB ) )
          //  {

          //  }
          //}
        }
      }
    }
  }
}