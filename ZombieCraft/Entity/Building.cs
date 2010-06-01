using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombieCraft
{
  struct Building
  {
    public readonly int Index;
    public readonly Vector2[] Boundary;
    public readonly Vector2[] Normals;
    public readonly float[] SATProjections;
    public readonly Vector2 Center;
    public readonly AABB AABB;

    public static Building[] Buildings;

    // debug
    readonly Color lineColor;
    readonly VertexPositionColor[] debugLines;

    public Building( int index, Vector2[] boundary )
    {
      if ( boundary.Length < 3 )
        throw new InvalidOperationException( "Bounding polygon must have at least 3 vertices" );

      Index = index;

      lineColor = Color.Red;

      Boundary = new Vector2[boundary.Length];
      Array.Copy( boundary, Boundary, boundary.Length );

      // set edge normals - requires polygon to be defined ccw
      Normals = new Vector2[boundary.Length];
      for ( int i = 0; i < Boundary.Length; ++i )
      {
        Vector2 edge = Boundary[( i + 1 ) % Boundary.Length] - Boundary[i];
        Normals[i] = Vector2.Normalize( new Vector2( -edge.Y, edge.X ) );
      }

      // pre-compute SAT projections
      SATProjections = new float[Boundary.Length];
      for ( int i = 0; i < Boundary.Length; ++i )
      {
        for ( int j = 2; j < Boundary.Length; ++j )
        {
          Vector2 disp = Boundary[i] - Boundary[( i + j ) % Boundary.Length];
          float proj = Vector2.Dot( disp, Normals[i] );
          if ( SATProjections[i] < proj )
            SATProjections[i] = proj;
        }
      }

      Center = Vector2.Zero;
      foreach ( Vector2 v in Boundary )
        Center += v;
      Center /= Boundary.Length;

      AABB.Min = new Vector2( float.MaxValue, float.MaxValue );
      AABB.Max = new Vector2( float.MinValue, float.MinValue );

      foreach ( Vector2 v in Boundary )
      {
        if ( v.X < AABB.Min.X )
          AABB.Min.X = v.X;
        if ( v.X > AABB.Max.X )
          AABB.Max.X = v.X;

        if ( v.Y < AABB.Min.Y )
          AABB.Min.Y = v.Y;
        if ( v.Y > AABB.Max.Y )
          AABB.Max.Y = v.Y;
      }

      debugLines = new VertexPositionColor[Boundary.Length * 2];
      InitializeDebug();
    }

    private void InitializeDebug()
    {
      for ( int i = 0; i < debugLines.Length; ++i )
        debugLines[i].Color = lineColor;

      for ( int i = 0; i < Boundary.Length; ++i )
      {
        int start = i * 2;
        debugLines[start].Position = new Vector3( Boundary[i].X, 0, Boundary[i].Y );

        int next = ( i + 1 ) % Boundary.Length;
        debugLines[start + 1].Position = new Vector3( Boundary[next].X, 0, Boundary[next].Y );
      }
    }

    public void DrawBoundingData( GraphicsDevice device )
    {
      device.DrawUserPrimitives( PrimitiveType.LineList, debugLines, 0, Boundary.Length );
    }
  }
}