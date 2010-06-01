using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Utility;
using Microsoft.Xna.Framework.Graphics;


namespace ZombieCraft
{
  struct PathCell
  {
    public readonly bool Walkable;
    public readonly int Index;
    public Vector2 Center; // "A readonly field cannot be passed as ref or out." Thank you C# -_-

    public int Parent;

    public PathCell( bool walkable, int index, Vector2 center )
    {
      Walkable = walkable;
      Index = index;
      Center = center;
      Parent = -1;
    }
  }

  struct PathCellCost
  {
    public float Walk;
    public float Heuristic;
    public float Total;
  }

  enum CellState
  {
    Untouched,
    Open,
    Closed,
  }

  class PathGrid
  {
    readonly float diagonal = (float)Math.Sqrt( 2 );
    readonly float smoothStep;

    readonly PathCell[] cells;
    public readonly Vector2 Min, Max;
    public readonly int Rows, Cols;

    List<int> openList;
    PathCellCost[] costs;
    CellState[] states; // true if open, false if closed

    int curCell = -1;
    int curDest = -1;

    DebugGrid debugGrid;

    public delegate bool WalkablePredicate( AABB aabb, Building[] buildings );

    public PathGrid( Vector2 min, Vector2 max, int rows, int cols, WalkablePredicate walkablePredicate )
    {
      smoothStep = .25f * ( max.X - min.X ) / cols;

      Min = min;
      Max = max;

      Rows = rows;
      Cols = cols;

      openList = new List<int>( 100 );

      int cellCount = Rows * Cols;
      cells = new PathCell[cellCount];
      costs = new PathCellCost[cellCount];
      states = new CellState[cellCount];

      float xStep = ( Max.X - Min.X ) / Cols;
      float yStep = ( Max.Y - Min.Y ) / Rows;

      AABB aabb = new AABB();
      aabb.Min.Y = Min.Y;
      aabb.Max.Y = Min.Y + yStep;
      for ( int r = 0; r < Rows; ++r )
      {
        aabb.Min.X = Min.X;
        aabb.Max.X = Min.X + xStep;
        for ( int c = 0; c < Cols; ++c )
        {
          int index = r * Cols + c;

          bool walkable = walkablePredicate != null ? walkablePredicate( aabb, Building.Buildings ) : true;
          cells[index] = new PathCell( walkable, index, Vector2.Lerp( aabb.Min, aabb.Max, .5f ) );

          aabb.Min.X += xStep;
          aabb.Max.X += xStep;
        }
        aabb.Min.Y += yStep;
        aabb.Max.Y += yStep;
      }

      // do the a* stuffs here
      for ( int r = 0; r < Rows; ++r )
      {
        for ( int c = 0; c < Cols; ++c )
        {
          int index = r * Cols + c;

          if ( cells[index].Walkable )
          {
            for ( int r2 = 0; r2 < Rows; ++r2 )
            {
              for ( int c2 = 0; c2 < Cols; ++c2 )
              {
                //FindPath( r, c, r2, c2 );
              }
            }
          }
        }
      }

      debugGrid = new DebugGrid( Min, Max, Rows, Cols, Color.LightGray, ZombieCraft.Instance.GraphicsDevice );
    }

    public void FindPath( int startRow, int startCol, int finishRow, int finishCol, List<int> waypoints )
    {
      openList.Clear();
      int cellCount = states.Length;
      for ( int i = 0; i < cellCount; ++i )
        states[i] = CellState.Untouched;

      int startIndex = startRow * Cols + startCol;
      openList.Add( startIndex );
      states[startIndex] = CellState.Open;
      cells[startIndex].Parent = -1;

      curDest = finishRow * Cols + finishCol;

      float heuristic = ComputeHeuristicCost( ref cells[startIndex].Center, ref cells[curDest].Center );
      costs[startIndex].Heuristic = heuristic;
      costs[startIndex].Walk = 0f;
      costs[startIndex].Total = heuristic;

      // if destinatation is invalid, find a valid one
      if ( !cells[curDest].Walkable )
        curDest = FindValidDestination( startIndex, curDest );

      // if the destination is the starting cell, add the waypoints and return
      if ( curDest == startIndex )
      {
        waypoints.Add( startIndex );
        waypoints.Add( startIndex );
        return;
      }

      int openListCount = openList.Count;
      while ( openListCount != 0 )
      {
        // find cell with lowest total cost
        float best = costs[openList[0]].Total;
        int bestOpenListIndex = 0;

        for ( int i = 1; i < openListCount; ++i )
        {
          float totalCost = costs[openList[i]].Total;
          if ( totalCost < best )
          {
            bestOpenListIndex = i;
            best = totalCost;
          }
        }

        // if cell is destination, break
        curCell = openList[bestOpenListIndex];
        if ( costs[curCell].Heuristic == 0 ) break;

        // remove cell from open list and close cell
        openList.RemoveAt( bestOpenListIndex );
        states[curCell] = CellState.Closed;

        // check neighboring cells
        int row = curCell / Cols;
        int col = curCell % Cols;

        int index;

        // top
        if ( row != 0 )
        {
          if ( col != 0 )
          {
            index = curCell - Cols - 1;
            CheckCell( ref cells[index], ref costs[index], diagonal );
          }
          index = curCell - Cols;
          CheckCell( ref cells[index], ref costs[index], 1f );
          if ( col + 1 < Cols )
          {
            index = curCell - Cols + 1;
            CheckCell( ref cells[index], ref costs[index], diagonal );
          }
        }

        // middle
        if ( col != 0 )
        {
          index = curCell - 1;
          CheckCell( ref cells[index], ref costs[index], 1f );
        }
        if ( col + 1 < Cols )
        {
          index = curCell + 1;
          CheckCell( ref cells[index], ref costs[index], 1f );
        }

        // bottom
        if ( row != Rows - 1 )
        {
          if ( col != 0 )
          {
            index = curCell + Cols - 1;
            CheckCell( ref cells[index], ref costs[index], diagonal );
          }
          index = curCell + Cols;
          CheckCell( ref cells[index], ref costs[index], 1f );
          if ( col + 1 < Cols )
          {
            index = curCell + Cols + 1;
            CheckCell( ref cells[index], ref costs[index], diagonal );
          }
        }

        openListCount = openList.Count;
      }

      curCell = curDest;
      while ( curCell != -1 )
      {
        waypoints.Add( curCell );
        curCell = cells[curCell].Parent;
      }
      waypoints.Reverse();

      //// --------  CUT  --------
      //foreach ( int i in waypoints )
      //{
      //  int row = i / Cols;
      //  int col = i % Cols;
      //  System.Diagnostics.Debug.Write( "{" + row.ToString() + " " + col.ToString() + "} " );
      //}
      //System.Diagnostics.Debug.Write( "\n" );

      SmoothPath( waypoints );

      //foreach ( int i in waypoints )
      //{
      //  int row = i / Cols;
      //  int col = i % Cols;
      //  System.Diagnostics.Debug.Write( "{" + row.ToString() + " " + col.ToString() + "} " );
      //}
      //System.Diagnostics.Debug.Write( "\n" );
      //// --------  CUT  --------
    }

    private int FindValidDestination( int source, int destination )
    {
      if ( !cells[source].Walkable )
        throw new InvalidOperationException( "source must be walkable" );

      Vector2 sourcePosition = cells[source].Center;
      Vector2 destPosition = cells[destination].Center;

      Vector2 walkDiretion = Vector2.Zero;
      Vector2.Subtract( ref sourcePosition, ref destPosition, out walkDiretion );
      Vector2.Normalize( ref walkDiretion, out walkDiretion );

      Vector2 walkStep;
      Vector2.Multiply( ref walkDiretion, smoothStep, out walkStep );
      Vector2 curPos = destPosition;

      int current = destination;
      while ( !cells[current].Walkable )
      {
        int last = current;
        while ( current == last )
        {
          Vector2.Add( ref curPos, ref walkStep, out curPos );
          int row = (int)( Rows * ( curPos.Y - Min.Y ) / ( Max.Y - Min.Y ) );
          int col = (int)( Cols * ( curPos.X - Min.X ) / ( Max.X - Min.X ) );
          current = row * Cols + col;
        }
      }

      return current;
    }

    public void ConvertPath( List<int> waypoints, List<Vector2> positions )
    {
      foreach ( int waypoint in waypoints )
        positions.Add( cells[waypoint].Center );
    }

    public bool IsWalkableAt( ref Vector2 position )
    {
      int row = (int)( Rows * ( position.Y - Min.Y ) / ( Max.Y - Min.Y ) );
      int col = (int)( Cols * ( position.X - Min.X ) / ( Max.X - Min.X ) );
      return ( cells[row * Cols + col].Walkable );
    }

    private void SmoothPath( List<int> waypoints )
    {
      for ( int i = 0; i < waypoints.Count - 2; ++i )
      {
        for ( int j = i + 1; j < waypoints.Count - 1; )
        {
          bool sakujo = true;

          int cur = waypoints[i];
          int mid = waypoints[j];
          int next = waypoints[j + 1];

          //// this will leave collinear neighbors in tact
          //if ( mid - cur == next - mid )
          //{
          //  sakujo = false;
          //  break;
          //}

          Vector2 direction;
          Vector2.Subtract( ref cells[next].Center, ref cells[cur].Center, out direction );
          float length = direction.Length();
          Vector2.Multiply( ref direction, smoothStep / length, out direction );

          Vector2 position = cells[cur].Center;
          int steps = (int)( length / smoothStep );
          for ( int k = 1; k < steps; ++k )
          {
            Vector2.Add( ref position, ref direction, out position );

            int row = (int)( Rows * ( position.Y - Min.Y ) / ( Max.Y - Min.Y ) );
            int col = (int)( Cols * ( position.X - Min.X ) / ( Max.X - Min.X ) );

            if ( !cells[Cols * row + col].Walkable )
            {
              sakujo = false;
              break;
            }
          }

          if ( sakujo )
            waypoints.RemoveAt( j );
          else
            ++j;
        }
      }
    }

    private void CheckCell( ref PathCell cell, ref PathCellCost cost, float stepCost )
    {
      if ( cell.Walkable )
      {
        CellState state = states[cell.Index];
        if ( state == CellState.Untouched )
        {
          openList.Add( cell.Index );
          states[cell.Index] = CellState.Open;
          cost.Heuristic = ComputeHeuristicCost( ref cell.Center, ref cells[curDest].Center );
          cost.Walk = costs[curCell].Walk + stepCost;
          cost.Total = cost.Heuristic + cost.Walk;
          cell.Parent = curCell;
        }
        else if ( state == CellState.Open )
        {
          float walkCost = costs[curCell].Walk + stepCost;
          if ( walkCost < cost.Walk )
          {
            cost.Walk = walkCost;
            cost.Total = cost.Heuristic + cost.Walk;
            cell.Parent = curCell;
          }
        }
      }
    }

    private float ComputeHeuristicCost( ref Vector2 p1, ref Vector2 p2 )
    {
      float dx = p2.X - p1.X;
      float dy = p2.Y - p1.Y;
      return (float)Math.Sqrt( dx * dx + dy * dy );
    }

    public void Draw()
    {
      debugGrid.Draw();
    }
  }
}
