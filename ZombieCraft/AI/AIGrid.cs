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

    // debug fields
    VertexPositionColor[] gridLines;
    Effect lineEffect;
    EffectParameter lineEffectViewParam;
    EffectParameter lineEffectProjectionParam;
    VertexDeclaration lineVertexDeclaration;
    SpriteFont debugFont;
    StringBuilder stringBuilder;


    public AIGrid( float width, float height, int cols, int rows )
    {
      float halfWidth  = width  / 2;
      float halfHeight = height / 2;

      Min = new Vector2( -halfWidth, -halfHeight );
      Max = new Vector2(  halfWidth,  halfHeight );

      Cols = cols;
      Rows = rows;

      float xStep = width  / cols;
      float zStep = height / rows;

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
          max = min + new Vector2( xStep, zStep );
          cells[index] = new GridCell( min, max, r, c );

          x += xStep;
        }
        z += zStep;
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
        gridLines[vertex].Position = new Vector3( col, 0, -Min.Y );
        gridLines[vertex].Color = gridColor;
        vertex++;

        gridLines[vertex].Position = new Vector3( col, 0, Max.Y );
        gridLines[vertex].Color = gridColor;
        vertex++;

        col += xStep;
      }

      float row = -Min.Y;
      for ( int r = 0; r <= Rows; ++r )
      {
        gridLines[vertex].Position = new Vector3( -Min.X, 0, row );
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

      // because we're using top-left rule, we need to make sure the maxs don't go out of bounds
      if ( colMax == Cols )
        colMax = colMin;
      if ( rowMax == Rows )
        rowMax = rowMin;

      GridCellListNode node = new GridCellListNode( entity.Index );

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
    }

    public void RemoveItem( ref Entity entity )
    {
      // this is slow, can be made faster
      foreach ( GridCell cell in cells )
        cell.Items.Remove( entity.Index );
    }

    public void Draw( Matrix view, Matrix projection )
    {
#if !XBOX
      GraphicsDevice device = ZombieCraft.Instance.GraphicsDevice;

      device.VertexDeclaration = lineVertexDeclaration;

      lineEffectViewParam.SetValue( view );
      lineEffectProjectionParam.SetValue( projection );

      lineEffect.Begin();
      lineEffect.CurrentTechnique.Passes[0].Begin();

      device.DrawUserPrimitives( PrimitiveType.LineList, gridLines,
                                 0, gridLines.Length / 2 );

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