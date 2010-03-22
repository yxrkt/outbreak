using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombieCraft
{
  internal class InstancedModelRenderData
  {
    public const int MaxInstances = 10000;

    InstancedModel model;
    StaticInstanceVertex[] instances;
    int instanceCount;
    WritableVertexBuffer vertexBuffer;

    public InstancedModelRenderData( InstancedModel model )
    {
      this.model = model;
      this.model.SetInstancingTechnique( InstancingTechnique.HardwareInstancing, 
                                         StaticInstanceVertex.VertexElements );
      instances = new StaticInstanceVertex[MaxInstances];
      vertexBuffer = new WritableVertexBuffer( ZombieCraft.Instance.GraphicsDevice,
                                               typeof( StaticInstanceVertex ),
                                               MaxInstances, 3 );
      instanceCount = 0;
    }

    public bool ContainsRef( InstanceTransformRef reference )
    {
      return ( reference.array == instances && reference.index < instanceCount );
    }

    public InstanceTransformRef AddInstance()
    {
      if ( instanceCount == instances.Length )
        throw new InvalidOperationException( "Cannot have more than " + MaxInstances.ToString() +
                                             " instances. Try raising MaxInstances." );

      return new InstanceTransformRef( instanceCount++, instances );
    }

    public void RemoveInstance( InstanceTransformRef reference )
    {
      instances[reference.index] = instances[--instanceCount];
    }

    public void DrawInstances( ref Matrix view, ref Matrix projection, ref Vector3 eyePosition )
    {
      if ( instanceCount == 0 ) return;

      foreach ( InstancedModelPart meshPart in model.ModelParts )
      {
        meshPart.Begin( view, projection, eyePosition );

        vertexBuffer.Flip();
        int offset = vertexBuffer.SetData( instances, 0, instanceCount );
        GraphicsDevice device = vertexBuffer.VertexBuffer.GraphicsDevice;

        int count = instanceCount;
        while ( count > 0 )
        {
          int numToDraw = Math.Min( count, meshPart.MaxInstances );

          device.Vertices[1].SetSource( vertexBuffer.VertexBuffer, offset, StaticInstanceVertex.SizeInBytes );

          meshPart.Draw( numToDraw );

          offset += numToDraw * StaticInstanceVertex.SizeInBytes;
          count -= numToDraw;
        }

        meshPart.End();
      }
    }
  }
}
