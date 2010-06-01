using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Utility
{
  public class DebugGrid
  {
    public readonly Vector2 Min, Max;
    public readonly int Rows, Cols;
    public readonly Color Color;

    VertexDeclaration vertexDeclaration;
    public GraphicsDevice Device { get; private set; }

    VertexPositionColor[] vertices;

    public DebugGrid( Vector2 min, Vector2 max, int rows, int cols, Color color, GraphicsDevice device )
    {
      Min = min;
      Max = max;
      Rows = rows;
      Cols = cols;
      Color = color;

      Device = device;
      vertexDeclaration = new VertexDeclaration( Device, VertexPositionColor.VertexElements );

      vertices = new VertexPositionColor[( 2 * ( cols + 1 ) + 2 * ( rows + 1 ) )];
      int v = 0;

      float y = Min.Y;
      float yStep = ( Max.Y - Min.Y ) / Rows;
      for ( int r = 0; r <= Rows; ++r )
      {
        vertices[v++].Position = new Vector3( Min.X, 0, y );
        vertices[v++].Position = new Vector3( Max.X, 0, y );
        y += yStep;
      }

      float x = Min.X;
      float xStep = ( Max.X - Min.X ) / Cols;
      for ( int c = 0; c <= Cols; ++c )
      {
        vertices[v++].Position = new Vector3( x, 0, Min.Y );
        vertices[v++].Position = new Vector3( x, 0, Max.Y );
        x += xStep;
      }

      for ( int i = 0; i < vertices.Length; ++i )
        vertices[i].Color = Color;
    }

    public void Draw()
    {
      //Device.VertexDeclaration = vertexDeclaration;
      Device.DrawUserPrimitives( PrimitiveType.LineList, vertices, 0, vertices.Length / 2 );
    }
  }
}