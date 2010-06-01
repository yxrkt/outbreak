using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombieCraft
{
  class Quad
  {
    public Vector3 Position;
    public float Scale;

    Vector3[] localPositions;
    VertexPositionTexture[] vertices;
    GraphicsDevice device;
    VertexDeclaration vertexDeclaraion;
    Effect effect;
    EffectParameter viewParam;
    EffectParameter projectionParam;
    EffectParameter textureParam;
    EffectParameter colorParam;

    public static readonly Vector3[] XZPlaneUnitQuad = 
    {
      new Vector3(-.5f, 0,-.5f ),
      new Vector3( .5f, 0,-.5f ),
      new Vector3( .5f, 0, .5f ),
      new Vector3(-.5f, 0, .5f ),
    };

    public Texture2D Texture
    {
      get { return textureParam.GetValueTexture2D(); }
      set { textureParam.SetValue( value ); }
    }

    public Color Color
    {
      get { return new Color( colorParam.GetValueVector4() ); }
      set { colorParam.SetValue( value.ToVector4() ); }
    }

    public Quad( Vector3[] verts, Vector3 position, Texture2D texture )
    {
      if ( verts.Length != 4 )
        throw new InvalidOperationException( "Quad must have four vertices." );
      localPositions = verts;
      vertices = new VertexPositionTexture[4];
      Array.Copy( verts, localPositions, 4 );
      vertices[0].TextureCoordinate = new Vector2( 0, 0 );
      vertices[1].TextureCoordinate = new Vector2( 1, 0 );
      vertices[2].TextureCoordinate = new Vector2( 1, 1 );
      vertices[3].TextureCoordinate = new Vector2( 0, 1 );

      Position = position;
      Scale = 1f;

      device = ZombieCraft.Instance.GraphicsDevice;
      vertexDeclaraion = new VertexDeclaration( device, VertexPositionTexture.VertexElements );
      effect = ZombieCraft.Instance.Content.Load<Effect>( "Effects/Primitive" ).Clone( device );
      effect.CurrentTechnique = effect.Techniques["Texture"];
      viewParam = effect.Parameters["View"];
      projectionParam = effect.Parameters["Projection"];
      textureParam = effect.Parameters["Texture"];
      colorParam = effect.Parameters["Color"];

      Texture = texture;
    }

    public void Draw( Matrix view, Matrix projection )
    {
      for ( int i = 0; i < 4; ++i )
        vertices[i].Position = Position + Scale * localPositions[i];

      device.VertexDeclaration = vertexDeclaraion;
      device.RenderState.AlphaBlendEnable = true;
      device.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
      device.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;

      viewParam.SetValue( view );
      projectionParam.SetValue( projection );

      effect.Begin();
      effect.CurrentTechnique.Passes[0].Begin();
      device.DrawUserPrimitives( PrimitiveType.TriangleFan, vertices, 0, 2 );
      effect.CurrentTechnique.Passes[0].End();
      effect.End();
    }
  }
}