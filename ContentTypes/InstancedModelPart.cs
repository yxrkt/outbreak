#region File Description
//-----------------------------------------------------------------------------
// InstancedModelPart.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

#endregion

namespace ContentTypes
{
  public partial class InstancedModelPart
  {
    #region Properties

    public int MaxInstances { get; private set; }

    public Effect Effect { get; private set; }

    #endregion

    #region Fields

    int indexCount;
    int vertexCount;
    int vertexStride;

    VertexDeclaration vertexDeclaration;
    VertexBuffer vertexBuffer;
    IndexBuffer indexBuffer;

    InstancedModel owner;

    VertexElement[] originalVertexDeclaration;

    EffectParameter effectVertexCountParam;
    EffectParameter effectViewParam;
    EffectParameter effectProjectionParam;
    EffectParameter effectEyeParam;

    #endregion

    #region Initialization

    internal InstancedModelPart( InstancedModel owner, ContentReader input,
                                 GraphicsDevice graphicsDevice )
    {
      this.owner = owner;

      indexCount = input.ReadInt32();
      vertexCount = input.ReadInt32();
      vertexStride = input.ReadInt32();

      vertexDeclaration = input.ReadObject<VertexDeclaration>();
      vertexBuffer = input.ReadObject<VertexBuffer>();
      indexBuffer = input.ReadObject<IndexBuffer>();

      input.ReadSharedResource<Effect>( delegate( Effect value )
      {
        Effect = value;
        effectVertexCountParam = Effect.Parameters["VertexCount"];
        effectViewParam = Effect.Parameters["View"];
        effectProjectionParam = Effect.Parameters["Projection"];
        effectEyeParam = Effect.Parameters["EyePosition"];
      } );

      originalVertexDeclaration = vertexDeclaration.GetVertexElements();
    }

    internal void Initialize( InstancingTechnique instancingTechnique,
                              VertexElement[] instanceVertexElements )
    {
      switch ( instancingTechnique )
      {
        case InstancingTechnique.ShaderInstancing:
          InitializeShaderInstancing( false );
          Effect.CurrentTechnique = Effect.Techniques["ShaderInstancing"];
          break;
        case InstancingTechnique.HardwareInstancing:
          InitializeHardwareInstancing( instanceVertexElements );
          Effect.CurrentTechnique = Effect.Techniques["HardwareInstancing"];
          break;
      }
    }


    #endregion

    #region Draw

    public void Begin( Matrix view, Matrix projection, Vector3 eyePosition )
    {
      GraphicsDevice gd = vertexBuffer.GraphicsDevice;

      switch ( owner.InstancingTechnique )
      {
        case InstancingTechnique.ShaderInstancing:

          gd.Vertices[0].SetSource( shaderInstanceVB, 0, shaderInstanceVertexStride );

          gd.Indices = shaderInstanceIB;
          gd.VertexDeclaration = shaderInstanceVDecl;
          break;

        case InstancingTechnique.HardwareInstancing:

          gd.Vertices[0].SetSource( vertexBuffer, 0, vertexStride );
          gd.Indices = indexBuffer;
          gd.VertexDeclaration = vertexDeclaration;
#if XBOX360
          effectVertexCountParam.SetValue( indexCount );
#endif
          break;
      }

      effectViewParam.SetValue( view );
      effectProjectionParam.SetValue( projection );
      effectEyeParam.SetValue( eyePosition );

      Effect.Begin();
      Effect.CurrentTechnique.Passes[0].Begin();
    }

    public void Draw( int instanceCount )
    {
      GraphicsDevice gd = vertexDeclaration.GraphicsDevice;

      switch ( owner.InstancingTechnique )
      {
        case InstancingTechnique.ShaderInstancing:

          gd.DrawIndexedPrimitives( PrimitiveType.TriangleList, 0, 0,
                                    vertexCount * instanceCount, 0,
                                    instanceCount * indexCount / 3 );

          break;

        case InstancingTechnique.HardwareInstancing:
#if XBOX360
          int primitiveCount = instanceCount * indexCount / 3;
#else
          int primitiveCount = indexCount / 3;

          gd.Vertices[0].SetFrequencyOfIndexData( instanceCount );
          gd.Vertices[1].SetFrequencyOfInstanceData( 1 );
#endif
          gd.DrawIndexedPrimitives( PrimitiveType.TriangleList,
                                    0, 0, vertexCount,
                                    0, primitiveCount );
          break;
      }
    }

    public void End()
    {
      GraphicsDevice gd = vertexBuffer.GraphicsDevice;

      gd.Vertices[0].SetSource( null, 0, 0 );
      gd.Vertices[1].SetSource( null, 0, 0 );

      Effect.CurrentTechnique.Passes[0].End();
      Effect.End();
    }

    #endregion
  }
}
