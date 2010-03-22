#region Using Statements

using System;

using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ContentTypes
{
  class InstancedIndexBuffer : IndexBuffer
  {
    #region Properties

    public int MaxInstances { get; private set; }

    #endregion

    public static InstancedIndexBuffer Create( IndexBuffer orignalIndexBuffer,
                                               int leastInstances, bool allow32bitIndex )
    {
      if ( orignalIndexBuffer.IndexElementSize != IndexElementSize.SixteenBits )
      {
        throw new ArgumentOutOfRangeException( "32-bit index buffer is not supported." );
      }

      GraphicsDevice gd = orignalIndexBuffer.GraphicsDevice;

      int indexCount = orignalIndexBuffer.SizeInBytes / sizeof( ushort );
      ushort[] originalIndices = new ushort[indexCount];
      orignalIndexBuffer.GetData<ushort>( originalIndices );

      int instanceCount = Math.Max( ushort.MaxValue / indexCount, leastInstances );

      int newSize = instanceCount * indexCount;

      InstancedIndexBuffer indexBuffer = null;

      if ( newSize >= ushort.MaxValue )
      {
        if ( allow32bitIndex == false )
        {
          throw new InvalidOperationException(
              "Number of indices cannot fit in a 16-bit index buffer." +
              "HWInstancePart.cs, Allow32BitIndexBuffer should be true." +
              "Or reduce the number of vertices in the model and pray." );
        }

        uint[] newIndices = new uint[newSize];

        int index = 0;
        for ( int i = 0; i < instanceCount; ++i )
        {
          uint packInstanceValue = (uint)( i * indexCount );

          for ( int j = 0; j < originalIndices.Length; ++j )
            newIndices[index++] = packInstanceValue + originalIndices[j];
        }

        indexBuffer = new InstancedIndexBuffer( gd, newSize * sizeof( uint ),
                                                IndexElementSize.ThirtyTwoBits );

        indexBuffer.SetData<uint>( newIndices );
      }
      else
      {
        // using 16-bit index buffer
        ushort[] newIndices = new ushort[newSize];

        int index = 0;
        for ( int i = 0; i < instanceCount; ++i )
        {
          ushort packInstanceValue = (ushort)( i * indexCount );

          for ( int j = 0; j < originalIndices.Length; ++j )
          {
            newIndices[index++] =
                (ushort)( packInstanceValue + originalIndices[j] );
          }
        }

        indexBuffer = new InstancedIndexBuffer( gd, newSize * sizeof( ushort ),
                                                IndexElementSize.SixteenBits );

        indexBuffer.SetData<ushort>( newIndices );
      }

      indexBuffer.MaxInstances = instanceCount;

      return indexBuffer;
    }

    protected InstancedIndexBuffer( GraphicsDevice graphicsDeivce, int sizeInBytes,
                                    IndexElementSize indexElementSize )
      : base( graphicsDeivce, sizeInBytes, BufferUsage.None, indexElementSize )
    {
    }
  }
}
