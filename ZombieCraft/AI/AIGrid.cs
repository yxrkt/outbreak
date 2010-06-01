using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utility;

namespace ZombieCraft
{
  class AIGrid
  {
    GridCell[] cells;
    PathGrid pathGrid;
    public readonly Vector2 Min, Max;
    public readonly int Rows, Cols;
    public readonly float CellColStep, CellRowStep;
    private readonly Color boxLineColor = Color.Red;

    public GridCell this[int row, int col]
    {
      get { return cells[row * Cols + col]; }
    }
    public GridCell this[int index]
    {
      get { return cells[index]; }
    }

    Vector2[] tempAABBVerts = new Vector2[4];
    List<int> tempWaypoints = new List<int>( 100 );

    // debug fields
    VertexPositionColor[] gridLines;
    Effect lineEffect;
    EffectParameter lineEffectViewParam;
    EffectParameter lineEffectProjectionParam;
    VertexDeclaration lineVertexDeclaration;
    SpriteFont debugFont;
    StringBuilder stringBuilder;

    VertexPositionColor[] boxLineVerts;
    int boxLineVertCount;

#if !XBOX
    int frame;
#endif

    public AIGrid( float width, float height, int cols, int rows )
    {
      float halfWidth  = width  / 2;
      float halfHeight = height / 2;

      Min = new Vector2( -halfWidth, -halfHeight );
      Max = new Vector2(  halfWidth,  halfHeight );

      Cols = cols;
      Rows = rows;

      CellColStep = width  / cols;
      CellRowStep = height / rows;

      int cellCount = cols * rows;
      cells = new GridCell[cellCount];

      Vector2 min, max;

      float z = -halfHeight;
      for ( int r = 0; r < rows; ++r )
      {
        float x = -halfWidth;
        for ( int c = 0; c < cols; ++c )
        {
          int index = cols * r + c;

          min = new Vector2( x, z );
          max = min + new Vector2( CellColStep, CellRowStep );
          cells[index] = new GridCell( min, max, r, c );

          x += CellColStep;
        }
        z += CellRowStep;
      }

      InitializeDebug();
    }

    private void InitializeDebug()
    {
      gridLines = new VertexPositionColor[2 * ( ( Cols + 1 ) + ( Rows + 1 ) )];

      int vertex = 0;
      Color gridColor = Color.Yellow;

      float xStep = ( Max.X - Min.X ) / Cols;
      float zStep = ( Max.Y - Min.Y ) / Rows;

      float col = Min.X;
      for ( int c = 0; c <= Cols; ++c )
      {
        gridLines[vertex].Position = new Vector3( col, 0, Min.Y );
        gridLines[vertex].Color = gridColor;
        vertex++;

        gridLines[vertex].Position = new Vector3( col, 0, Max.Y );
        gridLines[vertex].Color = gridColor;
        vertex++;

        col += xStep;
      }

      float row = Min.Y;
      for ( int r = 0; r <= Rows; ++r )
      {
        gridLines[vertex].Position = new Vector3( Min.X, 0, row );
        gridLines[vertex].Color = gridColor;
        vertex++;

        gridLines[vertex].Position = new Vector3( Max.X, 0, row );
        gridLines[vertex].Color = gridColor;
        vertex++;

        row += zStep;
      }

      vertex = 0;

      lineEffect = ZombieCraft.Instance.Content.Load<Effect>( "Effects/Primitive" );
      lineEffect.CurrentTechnique = lineEffect.Techniques["Color"];
      lineEffectViewParam = lineEffect.Parameters["View"];
      lineEffectProjectionParam = lineEffect.Parameters["Projection"];

      lineVertexDeclaration = new VertexDeclaration( ZombieCraft.Instance.GraphicsDevice,
                                                     VertexPositionColor.VertexElements );

      int initialSize = 100;
      boxLineVerts = new VertexPositionColor[initialSize];
      for ( int i = 0; i < initialSize; ++i )
        boxLineVerts[i].Color = boxLineColor;
      boxLineVertCount = 0;

      // debug font
      debugFont = ZombieCraft.Instance.Content.Load<SpriteFont>( "Fonts/debugFont" );
      stringBuilder = new StringBuilder( 10 );
    }

    public void AddEntity( ref Entity entity )
    {
      Vector2 min = Vector2.Clamp( entity.AABB.Min, Min, Max );
      Vector2 max = Vector2.Clamp( entity.AABB.Max, Min, Max );

      // add the entity to all touching cells
      float cellWidth = ( Max.X - Min.X ) / Cols;
      int colMin = (int)( ( min.X - Min.X ) / cellWidth );
      int colMax = (int)( ( max.X - Min.X ) / cellWidth );

      float cellHeight = ( Max.Y - Min.Y ) / Rows;
      int rowMin = (int)( ( min.Y - Min.Y ) / cellHeight );
      int rowMax = (int)( ( max.Y - Min.Y ) / cellHeight );

      // make sure the maxs don't go out of bounds
      if ( colMax >= Cols )
        colMax = colMin;
      if ( rowMax >= Rows )
        rowMax = rowMin;

      EntityListNode node = new EntityListNode( entity.Index );
      entity.ListNode = node;

      for ( int r = rowMin; r <= rowMax; ++r )
      {
        for ( int c = colMin; c <= colMax; ++c )
        {
          cells[Cols * r + c].Entities.Add( node );
        }
      }

      // box verts
      boxLineVertCount += 8;
      while ( boxLineVerts.Length < boxLineVertCount )
      {
        boxLineVerts = new VertexPositionColor[boxLineVerts.Length * 2];

        for ( int i = 0; i < boxLineVerts.Length; ++i )
          boxLineVerts[i].Color = boxLineColor;
      }
    }

    public void RemoveEntity( ref Entity entity )
    {
      // this is slow, can be made faster
      foreach ( GridCell cell in cells )
        cell.Entities.Remove( entity.Index );
    }

    public void UpdateEntity( ref Entity entity )
    {
      int minCol = (int)( ( entity.AABB.Min.X - Min.X ) / CellColStep );
      int maxCol = (int)( ( entity.AABB.Max.X - Min.X ) / CellColStep );
      int minRow = (int)( ( entity.AABB.Min.Y - Min.Y ) / CellRowStep );
      int maxRow = (int)( ( entity.AABB.Max.Y - Min.Y ) / CellRowStep );

      //minCol = minCol < 0 ? 0 : minCol;
      //maxCol = maxCol

      int cellType00, cellType01, cellType10, cellType11; // r, c
      GridCell cell00, cell01, cell10, cell11; // r, c

      /*/
      if ( minCol == maxCol )
      {
        if ( minRow == maxRow )
        {
          cellType00 = ( ( ( minRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cell00 = cells[minRow * Cols + minCol];

          for ( int i = 0; i < 4; ++i )
          {
            if ( i == cellType00 )
            {
              if ( entity.GridNode.parent[i] == null )
              {
                cell00.Items.Add( entity.GridNode );
              }
              else if ( entity.GridNode.parent[i] != cell00.Items )
              {
                entity.GridNode.parent[i].Remove( entity.GridNode );
                cell00.Items.Add( entity.GridNode );
              }
            }
            else if ( entity.GridNode.parent[i] != null )
            {
              entity.GridNode.parent[i].Remove( entity.GridNode );
            }
          }
        }
        else
        {
          cellType00 = ( ( ( minRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cellType10 = ( ( ( maxRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cell00 = cells[minRow * Cols + minCol];
          cell10 = cells[maxRow * Cols + minCol];

          for ( int i = 0; i < 4; ++i )
          {
            if ( i == cellType00 )
            {
              if ( entity.GridNode.parent[i] == null )
              {
                cell00.Items.Add( entity.GridNode );
              }
              else if ( entity.GridNode.parent[i] != cell00.Items )
              {
                entity.GridNode.parent[i].Remove( entity.GridNode );
                cell00.Items.Add( entity.GridNode );
              }
            }
            else if ( i == cellType10 )
            {
              if ( entity.GridNode.parent[i] == null )
              {
                cell10.Items.Add( entity.GridNode );
              }
              else if ( entity.GridNode.parent[i] != cell10.Items )
              {
                entity.GridNode.parent[i].Remove( entity.GridNode );
                cell10.Items.Add( entity.GridNode );
              }
            }
            else if ( entity.GridNode.parent[i] != null )
            {
              entity.GridNode.parent[i].Remove( entity.GridNode );
            }
          }
        }
      }
      else
      {
        if ( minRow == maxRow )
        {
          cellType00 = ( ( ( minRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cellType01 = ( ( ( minRow & 1 ) << 1 ) | ( maxCol & 1 ) );
          cell00 = cells[minRow * Cols + minCol];
          cell01 = cells[minRow * Cols + maxCol];

          for ( int i = 0; i < 4; ++i )
          {
            if ( i == cellType00 )
            {
              if ( entity.GridNode.parent[i] == null )
              {
                cell00.Items.Add( entity.GridNode );
              }
              else if ( entity.GridNode.parent[i] != cell00.Items )
              {
                entity.GridNode.parent[i].Remove( entity.GridNode );
                cell00.Items.Add( entity.GridNode );
              }
            }
            else if ( i == cellType01 )
            {
              if ( entity.GridNode.parent[i] == null )
              {
                cell01.Items.Add( entity.GridNode );
              }
              else if ( entity.GridNode.parent[i] != cell01.Items )
              {
                entity.GridNode.parent[i].Remove( entity.GridNode );
                cell01.Items.Add( entity.GridNode );
              }
            }
            else if ( entity.GridNode.parent[i] != null )
            {
              entity.GridNode.parent[i].Remove( entity.GridNode );
            }
          }
        }
        else
        {
          cellType00 = ( ( ( minRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cellType01 = ( ( ( minRow & 1 ) << 1 ) | ( maxCol & 1 ) );
          cellType10 = ( ( ( maxRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cellType11 = ( ( ( maxRow & 1 ) << 1 ) | ( maxCol & 1 ) );
          cell00 = cells[minRow * Cols + minCol];
          cell01 = cells[minRow * Cols + maxCol];
          cell10 = cells[maxRow * Cols + minCol];
          cell11 = cells[maxRow * Cols + maxCol];

          for ( int i = 0; i < 4; ++i )
          {
            if ( i == cellType00 )
            {
              if ( entity.GridNode.parent[i] == null )
              {
                cell00.Items.Add( entity.GridNode );
              }
              else if ( entity.GridNode.parent[i] != cell00.Items )
              {
                entity.GridNode.parent[i].Remove( entity.GridNode );
                cell00.Items.Add( entity.GridNode );
              }
            }
            else if ( i == cellType01 )
            {
              if ( entity.GridNode.parent[i] == null )
              {
                cell01.Items.Add( entity.GridNode );
              }
              else if ( entity.GridNode.parent[i] != cell01.Items )
              {
                entity.GridNode.parent[i].Remove( entity.GridNode );
                cell01.Items.Add( entity.GridNode );
              }
            }
            else if ( i == cellType10 )
            {
              if ( entity.GridNode.parent[i] == null )
              {
                cell10.Items.Add( entity.GridNode );
              }
              else if ( entity.GridNode.parent[i] != cell10.Items )
              {
                entity.GridNode.parent[i].Remove( entity.GridNode );
                cell10.Items.Add( entity.GridNode );
              }
            }
            else if ( i == cellType11 )
            {
              if ( entity.GridNode.parent[i] == null )
              {
                cell11.Items.Add( entity.GridNode );
              }
              else if ( entity.GridNode.parent[i] != cell11.Items )
              {
                entity.GridNode.parent[i].Remove( entity.GridNode );
                cell11.Items.Add( entity.GridNode );
              }
            }
          }
        }
      }
      /*/
      int intersection = maxCol - minCol + 2 * ( maxRow - minRow );
      switch ( intersection )
      {
        case 0: // AABB fits completely into one grid cell
          cellType00 = ( ( ( minRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cell00 = cells[minRow * Cols + minCol];

          for ( int i = 0; i < 4; ++i )
          {
            if ( i == cellType00 )
            {
              if ( entity.ListNode.parent[i] == null )
              {
                cell00.Entities.Add( entity.ListNode );
              }
              else if ( entity.ListNode.parent[i] != cell00.Entities )
              {
                entity.ListNode.parent[i].Remove( entity.ListNode );
                cell00.Entities.Add( entity.ListNode );
              }
            }
            else if ( entity.ListNode.parent[i] != null )
            {
              entity.ListNode.parent[i].Remove( entity.ListNode );
            }
          }
          break;
        case 1: // AABB fits into two horizontally adjacent grid cells
          cellType00 = ( ( ( minRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cellType01 = ( ( ( minRow & 1 ) << 1 ) | ( maxCol & 1 ) );
          cell00 = cells[minRow * Cols + minCol];
          cell01 = cells[minRow * Cols + maxCol];

          for ( int i = 0; i < 4; ++i )
          {
            if ( i == cellType00 )
            {
              if ( entity.ListNode.parent[i] == null )
              {
                cell00.Entities.Add( entity.ListNode );
              }
              else if ( entity.ListNode.parent[i] != cell00.Entities )
              {
                entity.ListNode.parent[i].Remove( entity.ListNode );
                cell00.Entities.Add( entity.ListNode );
              }
            }
            else if ( i == cellType01 )
            {
              if ( entity.ListNode.parent[i] == null )
              {
                cell01.Entities.Add( entity.ListNode );
              }
              else if ( entity.ListNode.parent[i] != cell01.Entities )
              {
                entity.ListNode.parent[i].Remove( entity.ListNode );
                cell01.Entities.Add( entity.ListNode );
              }
            }
            else if ( entity.ListNode.parent[i] != null )
            {
              entity.ListNode.parent[i].Remove( entity.ListNode );
            }
          }
          break;
        case 2: // AABB fits into two vertically adjacent grid cells
          cellType00 = ( ( ( minRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cellType10 = ( ( ( maxRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cell00 = cells[minRow * Cols + minCol];
          cell10 = cells[maxRow * Cols + minCol];

          for ( int i = 0; i < 4; ++i )
          {
            if ( i == cellType00 )
            {
              if ( entity.ListNode.parent[i] == null )
              {
                cell00.Entities.Add( entity.ListNode );
              }
              else if ( entity.ListNode.parent[i] != cell00.Entities )
              {
                entity.ListNode.parent[i].Remove( entity.ListNode );
                cell00.Entities.Add( entity.ListNode );
              }
            }
            else if ( i == cellType10 )
            {
              if ( entity.ListNode.parent[i] == null )
              {
                cell10.Entities.Add( entity.ListNode );
              }
              else if ( entity.ListNode.parent[i] != cell10.Entities )
              {
                entity.ListNode.parent[i].Remove( entity.ListNode );
                cell10.Entities.Add( entity.ListNode );
              }
            }
            else if ( entity.ListNode.parent[i] != null )
            {
              entity.ListNode.parent[i].Remove( entity.ListNode );
            }
          }
          break;
        case 3: // AABB fits into a square of four grid cells
          cellType00 = ( ( ( minRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cellType01 = ( ( ( minRow & 1 ) << 1 ) | ( maxCol & 1 ) );
          cellType10 = ( ( ( maxRow & 1 ) << 1 ) | ( minCol & 1 ) );
          cellType11 = ( ( ( maxRow & 1 ) << 1 ) | ( maxCol & 1 ) );
          cell00 = cells[minRow * Cols + minCol];
          cell01 = cells[minRow * Cols + maxCol];
          cell10 = cells[maxRow * Cols + minCol];
          cell11 = cells[maxRow * Cols + maxCol];

          for ( int i = 0; i < 4; ++i )
          {
            if ( i == cellType00 )
            {
              if ( entity.ListNode.parent[i] == null )
              {
                cell00.Entities.Add( entity.ListNode );
              }
              else if ( entity.ListNode.parent[i] != cell00.Entities )
              {
                entity.ListNode.parent[i].Remove( entity.ListNode );
                cell00.Entities.Add( entity.ListNode );
              }
            }
            else if ( i == cellType01 )
            {
              if ( entity.ListNode.parent[i] == null )
              {
                cell01.Entities.Add( entity.ListNode );
              }
              else if ( entity.ListNode.parent[i] != cell01.Entities )
              {
                entity.ListNode.parent[i].Remove( entity.ListNode );
                cell01.Entities.Add( entity.ListNode );
              }
            }
            else if ( i == cellType10 )
            {
              if ( entity.ListNode.parent[i] == null )
              {
                cell10.Entities.Add( entity.ListNode );
              }
              else if ( entity.ListNode.parent[i] != cell10.Entities )
              {
                entity.ListNode.parent[i].Remove( entity.ListNode );
                cell10.Entities.Add( entity.ListNode );
              }
            }
            else if ( i == cellType11 )
            {
              if ( entity.ListNode.parent[i] == null )
              {
                cell11.Entities.Add( entity.ListNode );
              }
              else if ( entity.ListNode.parent[i] != cell11.Entities )
              {
                entity.ListNode.parent[i].Remove( entity.ListNode );
                cell11.Entities.Add( entity.ListNode );
              }
            }
          }
          break;
      }
      /**/
    }

    public void AddBuilding( ref Building building )
    {
      Vector2 min = Vector2.Clamp( building.AABB.Min, Min, Max );
      Vector2 max = Vector2.Clamp( building.AABB.Max, Min, Max );

      float cellWidth = ( Max.X - Min.X ) / Cols;
      int colMin = (int)( ( min.X - Min.X ) / cellWidth );
      int colMax = (int)( ( max.X - Min.X ) / cellWidth );

      float cellHeight = ( Max.Y - Min.Y ) / Rows;
      int rowMin = (int)( ( min.Y - Min.Y ) / cellHeight );
      int rowMax = (int)( ( max.Y - Min.Y ) / cellHeight );

      // make sure the maxs don't go out of bounds
      if ( colMax == Cols )
        colMax = colMin;
      if ( rowMax == Rows )
        rowMax = rowMin;

      for ( int r = rowMin; r <= rowMax; ++r )
      {
        for ( int c = colMin; c <= colMax; ++c )
        {
          int index = Cols * r + c;
          if ( TestBuildingVsAABB( building, new AABB( cells[index].Min, cells[index].Max ) ) )
            cells[index].Buildings.Add( building.Index );
        }
      }
    }

    public void RemoveBuilding( ref Building building )
    {
    }

    public void InitializePathGrid( int squareSize )
    {
      int xDimension = (int)( Max.X - Min.X ) / squareSize;
      int yDimension = (int)( Max.Y - Min.Y ) / squareSize;
      pathGrid = new PathGrid( Min, Max, xDimension, yDimension, DetermineIfPathCellIsWalkable );

      List<int> path = new List<int>( 10 );
      pathGrid.FindPath( 0, 0, 7, 7, path );
    }

    public void GeneratePath( ref Vector2 start, ref Vector2 finish, List<Vector2> output )
    {
#if DEBUG
      System.Diagnostics.Debug.Assert( output.Count == 0, "output path must start as empty" );
#endif
      if ( !pathGrid.IsWalkableAt( ref start ) )
      {
        output.Add( start );
        output.Add( start );
        return;
      }

      int startRow = (int)( pathGrid.Rows * ( start.Y - pathGrid.Min.Y ) / ( pathGrid.Max.Y - pathGrid.Min.Y ) );
      int startCol = (int)( pathGrid.Cols * ( start.X - pathGrid.Min.X ) / ( pathGrid.Max.X - pathGrid.Min.X ) );
      int finishRow = (int)( pathGrid.Rows * ( finish.Y - pathGrid.Min.Y ) / ( pathGrid.Max.Y - pathGrid.Min.Y ) );
      int finishCol = (int)( pathGrid.Cols * ( finish.X - pathGrid.Min.X ) / ( pathGrid.Max.X - pathGrid.Min.X ) );

      tempWaypoints.Clear();
      pathGrid.FindPath( startRow, startCol, finishRow, finishCol, tempWaypoints );
      pathGrid.ConvertPath( tempWaypoints, output );

      while ( output.Count < 2 )
        output.Add( finish );

      output[0] = start;
      if ( pathGrid.IsWalkableAt( ref finish ) )
        output[output.Count - 1] = finish;
    }

    public void Draw( Matrix view, Matrix projection )
    {
#if !XBOX
      GraphicsDevice device = ZombieCraft.Instance.GraphicsDevice;

      device.VertexDeclaration = lineVertexDeclaration;

      // update aabb lines
      int vertCount = 0;
      foreach ( GridCell cell in cells )
      {
        for ( EntityListNode node = cell.Entities.First(); node != null; node = cell.Entities.Next( node ) )
        {
          if ( node.debugFrame == frame )
          {
            AddAABBLines( ref vertCount, ref Entity.Entities[node.EntityIndex] );
            node.debugFrame++;
          }
        }
      }
      frame++;

      lineEffectViewParam.SetValue( view );
      lineEffectProjectionParam.SetValue( projection );

      lineEffect.Begin();
      lineEffect.CurrentTechnique.Passes[0].Begin();

      pathGrid.Draw();
      
      //device.DrawUserPrimitives( PrimitiveType.LineList, gridLines,
      //                           0, gridLines.Length / 2 );

      //device.DrawUserPrimitives( PrimitiveType.LineList, boxLineVerts,
      //                           0, vertCount / 2 );

      for ( int i = 0; i < Building.Buildings.Length; ++i )
        Building.Buildings[i].DrawBoundingData( device );

      lineEffect.CurrentTechnique.Passes[0].End();
      lineEffect.End();

      //DrawCellCounts( view, projection, 1 );
#endif
    }

    private void DrawCellCounts( Matrix view, Matrix projection, int type )
    {
      Matrix viewProjection = view * projection;

      Viewport viewport = ZombieCraft.Instance.GraphicsDevice.Viewport;

      SpriteBatch spriteBatch = ScreenManager.Instance.SpriteBatch;
      spriteBatch.Begin( SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None );

      foreach ( GridCell cell in cells )
      {
        Vector2 center = ( cell.Min + cell.Max ) / 2;
        Vector3 worldPos = new Vector3( center.X, 0, center.Y );
        Vector4 screenPos = Vector4.Transform( worldPos, viewProjection );
        screenPos /= screenPos.W;

        stringBuilder.Clear();
        if ( type == 0 )
          stringBuilder.AppendInt( cell.Entities.Count );
        else
          stringBuilder.AppendInt( cell.Buildings.Count );

        Vector2 position = new Vector2( ( .5f * screenPos.X + .5f ) * viewport.Width,
                                        ( -.5f * screenPos.Y + .5f ) * viewport.Height );
        Vector2 origin = debugFont.MeasureString( stringBuilder ) / 2;
        spriteBatch.DrawString( debugFont, stringBuilder, position, Color.White,
                                0f, origin, 1f, SpriteEffects.None, 0f );
      }

      spriteBatch.End();
    }

    private void AddAABBLines( ref int vertCount, ref Entity entity )
    {
      boxLineVerts[vertCount++].Position = new Vector3( entity.AABB.Min.X, 0, entity.AABB.Min.Y );
      boxLineVerts[vertCount++].Position = new Vector3( entity.AABB.Min.X, 0, entity.AABB.Max.Y );

      boxLineVerts[vertCount++].Position = new Vector3( entity.AABB.Min.X, 0, entity.AABB.Max.Y );
      boxLineVerts[vertCount++].Position = new Vector3( entity.AABB.Max.X, 0, entity.AABB.Max.Y );

      boxLineVerts[vertCount++].Position = new Vector3( entity.AABB.Max.X, 0, entity.AABB.Max.Y );
      boxLineVerts[vertCount++].Position = new Vector3( entity.AABB.Max.X, 0, entity.AABB.Min.Y );

      boxLineVerts[vertCount++].Position = new Vector3( entity.AABB.Max.X, 0, entity.AABB.Min.Y );
      boxLineVerts[vertCount++].Position = new Vector3( entity.AABB.Min.X, 0, entity.AABB.Min.Y );
    }

    private bool TestBuildingVsAABB( Building building, AABB aabb )
    {
      Vector2 min = Vector2.Clamp( building.AABB.Min, Min, Max );
      Vector2 max = Vector2.Clamp( building.AABB.Max, Min, Max );

      float cellWidth = ( Max.X - Min.X ) / Cols;
      int colMin = (int)( ( min.X - Min.X ) / cellWidth );
      int colMax = (int)( ( max.X - Min.X ) / cellWidth );

      float cellHeight = ( Max.Y - Min.Y ) / Rows;
      int rowMin = (int)( ( min.Y - Min.Y ) / cellHeight );
      int rowMax = (int)( ( max.Y - Min.Y ) / cellHeight );

      // make sure the maxs don't go out of bounds
      if ( colMax == Cols )
        colMax = colMin;
      if ( rowMax == Rows )
        rowMax = rowMin;

      // test polygon's aabb against aabb (effectively aabb version of SAT)
      if ( aabb.Max.X < min.X || aabb.Min.X > max.X ) return false;
      if ( aabb.Max.Y < min.Y || aabb.Min.Y > max.Y ) return false;

      tempAABBVerts[0] = aabb.Min;
      tempAABBVerts[1] = new Vector2( aabb.Min.X, aabb.Max.Y );
      tempAABBVerts[2] = aabb.Max;
      tempAABBVerts[3] = new Vector2( aabb.Max.X, aabb.Min.Y );

      // SAT using building's axes
      for ( int i = 0; i < building.Boundary.Length; ++i )
      {
        float lower = float.MaxValue;
        float upper = float.MinValue;

        for ( int j = 0; j < tempAABBVerts.Length; ++j )
        {
          Vector2 disp = building.Boundary[i] - tempAABBVerts[j];
          float proj = Vector2.Dot( disp, building.Normals[i] );
          if ( proj < lower ) lower = proj;
          if ( proj > upper ) upper = proj;
        }

        if ( upper < 0 || lower > building.SATProjections[i] )
          return false;
      }

      return true;
    }

    private bool DetermineIfPathCellIsWalkable( AABB aabb, Building[] buildings )
    {
      for ( int i = 0; i < buildings.Length; ++i )
      {
        if ( TestBuildingVsAABB( buildings[i], aabb ) )
          return false;
      }
      return true;
    }
  }

  struct GridCell
  {
    public readonly Vector2 Min;
    public readonly Vector2 Max;
    public readonly EntityList Entities;
    public readonly List<int> Buildings;

    public GridCell( Vector2 min, Vector2 max, int row, int col )
    {
      Min = min;
      Max = max;

      Entities = new EntityList( row, col );
      Buildings = new List<int>( 10 );
    }
  }
}