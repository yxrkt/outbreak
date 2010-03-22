#region File Description
//-----------------------------------------------------------------------------
// ShaderInstancePart.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// このソースコードはクリエータークラブオンラインのMeshInstancingの
// コードのコメントを翻訳、変更したもの
// http://creators.xna.com/en-US/sample/meshinstancing
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;

using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ContentTypes
{
  public partial class InstancedModelPart
  {
    #region Constant Declarations

    const int MaxShaderInstanceCount = 100;

    #endregion

    #region Fields

    VertexBuffer shaderInstanceVB;
    IndexBuffer shaderInstanceIB;
    int shaderInstanceVertexStride;
    VertexDeclaration shaderInstanceVDecl;

    bool shaderInstanceInitialized;

    #endregion

    #region Initialization

    void InitializeShaderInstancing( bool disposeOriginalResources )
    {
      int indexOverflowLimit = ushort.MaxValue / vertexCount;

      MaxInstances = Math.Min( indexOverflowLimit, MaxShaderInstanceCount );

      if ( !shaderInstanceInitialized )
      {
        ReplicateVertexData();
        ReplicateIndexData();

        int instanceIndexOffset = vertexStride;
        byte usageIndex = 1;
        short stream = 0;

        VertexElement[] extraElements =
        {
            new VertexElement( stream, (short)instanceIndexOffset,
                               VertexElementFormat.Single,
                               VertexElementMethod.Default,
                               VertexElementUsage.TextureCoordinate, usageIndex )
        };

        shaderInstanceVertexStride = vertexStride + sizeof( float );
        shaderInstanceVDecl = RenderHelper.ExtendVertexDeclaration(
                                            vertexDeclaration.GraphicsDevice,
                                            originalVertexDeclaration,
                                            extraElements );

        shaderInstanceInitialized = true;
      }

      if ( disposeOriginalResources )
      {
        if ( vertexBuffer != null )
        {
          vertexBuffer.Dispose();
          vertexBuffer = null;
        }

        if ( indexBuffer != null )
        {
          indexBuffer.Dispose();
          indexBuffer = null;
        }
      }
    }

    void ReplicateVertexData()
    {
      byte[] oldVertexData = new byte[vertexCount * vertexStride];

      vertexBuffer.GetData( oldVertexData );

      shaderInstanceVertexStride = vertexStride + sizeof( float );

      int stride = shaderInstanceVertexStride;

      byte[] newVertexData = new byte[vertexCount * stride * MaxInstances];

      int outputPosition = 0;

      for ( int instanceIndex = 0; instanceIndex < MaxInstances; instanceIndex++ )
      {
        int sourcePosition = 0;

        byte[] instanceIndexBits = BitConverter.GetBytes( (float)instanceIndex );

        for ( int i = 0; i < vertexCount; i++ )
        {
          Array.Copy( oldVertexData, sourcePosition,
                      newVertexData, outputPosition, vertexStride );

          outputPosition += vertexStride;
          sourcePosition += vertexStride;

          instanceIndexBits.CopyTo( newVertexData, outputPosition );

          outputPosition += instanceIndexBits.Length;
        }
      }

      shaderInstanceVB = new VertexBuffer( vertexBuffer.GraphicsDevice,
                                      newVertexData.Length, BufferUsage.None );

      shaderInstanceVB.SetData( newVertexData );
    }


    void ReplicateIndexData()
    {
      ushort[] oldIndices = new ushort[indexCount];

      indexBuffer.GetData( oldIndices );

      ushort[] newIndices = new ushort[indexCount * MaxInstances];

      int outputPosition = 0;

      for ( int instanceIndex = 0; instanceIndex < MaxInstances; instanceIndex++ )
      {
        int instanceOffset = instanceIndex * vertexCount;

        for ( int i = 0; i < indexCount; i++ )
        {
          newIndices[outputPosition] = (ushort)( oldIndices[i] +
                                                 instanceOffset );

          outputPosition++;
        }
      }

      shaderInstanceIB = new IndexBuffer( indexBuffer.GraphicsDevice,
                                          sizeof( ushort ) * newIndices.Length,
                                          BufferUsage.None,
                                          IndexElementSize.SixteenBits );

      shaderInstanceIB.SetData( newIndices );
    }

    #endregion
  }
}
