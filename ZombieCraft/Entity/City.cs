using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombieCraft
{
  class City
  {
    StillModel model;

    public Matrix Transform;
    public VertexDeclaration lineVertexDeclaration;

    public City()
    {
      model = ZombieCraft.Instance.Content.Load<StillModel>( "Models/City" );
      Transform = Matrix.CreateScale( .01f );

      // create some temporary buildings for now
      /*/// Big building in the middle
      Building.Buildings = new Building[1];

      Vector2[] verts = new Vector2[]
      {
        new Vector2( -45, 45 ),
        new Vector2( 95, 95 ),
        new Vector2( 95, -95 ),
        new Vector2( -45, -45 ),
      };
      for ( int i = 0; i < verts.Length; ++i )
        verts[i] *= .1f;
      Building.Buildings[0] = new Building( 0, verts );
      /*/// Lots of little buildings
      Random random = new Random( 0 );
      int rows = 0; //~~~
      int cols = 8;
      Building.Buildings = new Building[rows * cols];
      Vector2 center = new Vector2( -240, -240 );
      for ( int r = 0; r < rows; ++r )
      {
        Vector2[] verts;
        center.X = -240;
        for ( int c = 0; c < cols; ++c )
        {
          switch ( random.Next( 3 ) )
          {
            case 0:
              verts = new Vector2[]
              {
                new Vector2( -12, 12 ) + center,
                new Vector2( 12, 12 ) + center,
                new Vector2( 12, -12 ) + center,
                new Vector2( -12, -12 ) + center,
              };
              Building.Buildings[r * cols + c] = new Building( r * cols + c, verts );
              break;
            case 1:
              verts = new Vector2[]
              {
                new Vector2( -12, 15 ) + center,
                new Vector2( 12, 15 ) + center,
                new Vector2( 15, -12 ) + center,
                new Vector2( -12, -12 ) + center,
              };
              Building.Buildings[r * cols + c] = new Building( r * cols + c, verts );
              break;
            case 2:
              verts = new Vector2[]
              {
                new Vector2( -15, 12 ) + center,
                new Vector2( 12, 15 ) + center,
                new Vector2( 12, -15 ) + center,
                new Vector2( -15, -12 ) + center,
              };
              Building.Buildings[r * cols + c] = new Building( r * cols + c, verts );
              break;
          }
          center.X += 67;
        }
        center.Y += 67;
      }
      /**/

      lineVertexDeclaration = new VertexDeclaration( ZombieCraft.Instance.GraphicsDevice, 
                                                     VertexPositionColor.VertexElements );
    }

    public void Draw( Matrix view, Matrix projection )
    {
      //model.Draw( Transform, view, projection );

      //GraphicsDevice device = ZombieCraft.Instance.GraphicsDevice;
      //device.VertexDeclaration = lineVertexDeclaration;
      //for ( int i = 0; i < Building.Buildings.Length; ++i )
      //  Building.Buildings[i].DrawBoundingData( device );
    }
  }
}
