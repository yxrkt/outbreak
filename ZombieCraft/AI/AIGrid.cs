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
    public readonly Vector2 Min, Max;
    public readonly int Rows, Cols;
    public readonly float CellColStep, CellRowStep;
    private readonly Color boxLineColor = Color.Red;

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

    public void AddItem( ref Entity entity )
    {
      Vector2 min = Vector2.Clamp( entity.AABB.Min, Min, Max );
      Vector2 max = Vector2.Clamp( entity.AABB.Max, Min, Max );

      // add the data to all touching cells
      float cellWidth = ( Max.X - Min.X ) / Cols;
      int colMin = (int)( ( min.X - Min.X ) / cellWidth );
      int colMax = (int)( ( max.X - Min.X ) / cellWidth );

      float cellHeight = ( Max.Y - Min.Y ) / Rows;
      int rowMin = (int)( ( min.Y - Min.Y ) / cellHeight );
      int rowMax = (int)( ( max.Y - Min.Y ) / cellHeight );

      // make sure the max's don't go out of bounds
      if ( colMax == Cols )
        colMax = colMin;
      if ( rowMax == Rows )
        rowMax = rowMin;

      GridCellListNode node = new GridCellListNode( entity.Index );
      entity.GridNode = node;

      if ( colMin == colMax )
      {
        if ( rowMin == rowMax )
        {
          // the GridData fits nicely into one cell
          cells[Cols * rowMin + colMin].Items.Add( node );
        }
        else
        {
          // the GridData touches two vertical neighbors
          cells[Cols * rowMin + colMin].Items.Add( node );
          cells[Cols * rowMax + colMin].Items.Add( node );
        }
      }
      else
      {
        if ( rowMin == rowMax )
        {
          // the GridData touches two horizontal neighbors
          cells[Cols * rowMin + colMin].Items.Add( node );
          cells[Cols * rowMin + colMax].Items.Add( node );
        }
        else
        {
          // the GridData touches a square of four neighbors
          cells[Cols * rowMin + colMin].Items.Add( node );
          cells[Cols * rowMin + colMax].Items.Add( node );
          cells[Cols * rowMax + colMin].Items.Add( node );
          cells[Cols * rowMax + colMax].Items.Add( node );
        }
      }

      // box verts
      int end = boxLineVertCount + 8;
      while ( boxLineVerts.Length < end )
      {
        //VertexPositionColor[] temp = new VertexPositionColor[boxLineVerts.Length * 2];
        //Array.Copy( boxLineVerts, temp, boxLineVertCount );
        //boxLineVerts = temp;
        boxLineVerts = new VertexPositionColor[boxLineVerts.Length * 2];

        for ( int i = 0; i < boxLineVerts.Length; ++i )
          boxLineVerts[i].Color = boxLineColor;
      }

      //boxLineVerts[boxLineVertCount++].Position = new Vector3( entity.AABB.Min.X, 0, entity.AABB.Min.Y );
      //boxLineVerts[boxLineVertCount++].Position = new Vector3( entity.AABB.Min.X, 0, entity.AABB.Max.Y );

      //boxLineVerts[boxLineVertCount++].Position = new Vector3( entity.AABB.Min.X, 0, entity.AABB.Max.Y );
      //boxLineVerts[boxLineVertCount++].Position = new Vector3( entity.AABB.Max.X, 0, entity.AABB.Max.Y );

      //boxLineVerts[boxLineVertCount++].Position = new Vector3( entity.AABB.Max.X, 0, entity.AABB.Max.Y );
      //boxLineVerts[boxLineVertCount++].Position = new Vector3( entity.AABB.Max.X, 0, entity.AABB.Min.Y );

      //boxLineVerts[boxLineVertCount++].Position = new Vector3( entity.AABB.Max.X, 0, entity.AABB.Min.Y );
      //boxLineVerts[boxLineVertCount++].Position = new Vector3( entity.AABB.Min.X, 0, entity.AABB.Min.Y );
    }

    public void RemoveItem( ref Entity entity )
    {
      // this is slow, can be made faster
      foreach ( GridCell cell in cells )
        cell.Items.Remove( entity.Index );
    }

    public void UpdateItem( ref Entity entity )
    {
      int minCol = (int)( ( entity.AABB.Min.X - Min.X ) / CellColStep );
      int maxCol = (int)( ( entity.AABB.Max.X - Min.X ) / CellColStep );
      int minRow = (int)( ( entity.AABB.Min.Y - Min.Y ) / CellRowStep );
      int maxRow = (int)( ( entity.AABB.Max.Y - Min.Y ) / CellRowStep );

      int cellType00, cellType01, cellType10, cellType11; // r, c
      GridCell cell00, cell01, cell10, cell11; // r, c

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
          break;
      }
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
        for ( GridCellListNode node = cell.Items.First(); node != null; node = cell.Items.Next( node ) )
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

      device.DrawUserPrimitives( PrimitiveType.LineList, gridLines,
                                 0, gridLines.Length / 2 );

      device.DrawUserPrimitives( PrimitiveType.LineList, boxLineVerts,
                                 0, vertCount / 2 );

      lineEffect.CurrentTechnique.Passes[0].End();
      lineEffect.End();

      DrawCellCounts( view, projection );
#endif
    }

    private void DrawCellCounts( Matrix view, Matrix projection )
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
        stringBuilder.AppendInt( cell.Items.Count );

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
  }

  struct GridCell
  {
    public readonly Vector2 Min;
    public readonly Vector2 Max;
    public readonly GridCellList Items;

    public GridCell( Vector2 min, Vector2 max, int row, int col )
    {
      Min = min;
      Max = max;

      Items = new GridCellList( row, col );
    }
  }
}