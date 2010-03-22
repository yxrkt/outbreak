#region Using Statements

using System;

using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ZombieCraft
{
  class WritableVertexBuffer
  {
    #region Properties

    public DynamicVertexBuffer VertexBuffer
    {
      get { return vertexBuffers[bufferIndex]; }
    }

    #endregion

    #region Fields

    DynamicVertexBuffer[] vertexBuffers;

    int[] writeOffsets;

    int bufferIndex;

    #endregion

    public WritableVertexBuffer( GraphicsDevice graphicsDevice,
                                    int sizeInBytes, int bufferCount )
    {
      writeOffsets = new int[bufferCount];
      vertexBuffers = new DynamicVertexBuffer[bufferCount];
      for ( int i = 0; i < bufferCount; ++i )
      {
        vertexBuffers[i] = new DynamicVertexBuffer( graphicsDevice,
                                sizeInBytes, BufferUsage.WriteOnly );
      }
    }

    public WritableVertexBuffer( GraphicsDevice graphicsDevice,
                                 Type vertexType, int elementCount, int bufferCount )
    {
      writeOffsets = new int[bufferCount];
      vertexBuffers = new DynamicVertexBuffer[bufferCount];
      for ( int i = 0; i < bufferCount; ++i )
      {
        vertexBuffers[i] = new DynamicVertexBuffer( graphicsDevice,
                            vertexType, elementCount, BufferUsage.WriteOnly );
      }
    }

    public int SetData<T>( T[] data ) where T : struct
    {
      return SetData<T>( data, 0, data.Length, Marshal.SizeOf( typeof( T ) ) );
    }

    public int SetData<T>( T[] data, int startIndex, int elementCount )
                                                                where T : struct
    {
      return SetData<T>( data, startIndex, elementCount,
                                              Marshal.SizeOf( typeof( T ) ) );
    }

    public int SetData<T>( T[] data, int startIndex,
                            int elementCount, int vertexStride ) where T : struct
    {
      int writeSize = elementCount * vertexStride;

      if ( vertexBuffers.Length == 1 )
      {
        if ( writeSize * 2 > vertexBuffers[bufferIndex].SizeInBytes )
        {
          throw new ArgumentOutOfRangeException(
              "Wrote more than half the buffer size." );
        }
      }
      else
      {
        if ( writeSize > vertexBuffers[bufferIndex].SizeInBytes )
        {
          throw new ArgumentOutOfRangeException(
              "Write size is greater than buffer size." );
        }
      }

      int size = vertexBuffers[bufferIndex].SizeInBytes;
      if ( writeOffsets[bufferIndex] + writeSize > size )
        writeOffsets[bufferIndex] = 0;

      vertexBuffers[bufferIndex].SetData<T>( writeOffsets[bufferIndex], data,
          startIndex, elementCount, vertexStride, SetDataOptions.NoOverwrite );

      int result = writeOffsets[bufferIndex];
      writeOffsets[bufferIndex] += writeSize;

      return result;
    }

    public void Flip()
    {
      bufferIndex++;
      if ( bufferIndex >= vertexBuffers.Length )
        bufferIndex = 0;
    }
  }
}
