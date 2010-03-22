using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ContentTypes;

namespace ZombieCraft
{
  static class ModelDrawer
  {
    static Dictionary<InstancedModel, InstancedModelRenderData> instancedModels;
    static List<StillModel> stillModels;

    public static Camera Camera;

    static ModelDrawer()
    {
      instancedModels = new Dictionary<InstancedModel, InstancedModelRenderData>( 4 );
      stillModels = new List<StillModel>( 10 );
    }

    public static InstanceTransformRef GetInstanceRef( InstancedModel model )
    {
      if ( !instancedModels.ContainsKey( model ) )
        instancedModels.Add( model, new InstancedModelRenderData( model ) );
      return instancedModels[model].AddInstance();
    }

    public static void ReleaseInstanceRef( InstanceTransformRef reference )
    {
      foreach ( var kvp in instancedModels )
      {
        if ( kvp.Value.ContainsRef( reference ) )
          kvp.Value.RemoveInstance( reference );
      }
    }

    public static void Draw()
    {
      Matrix view = Camera.ViewMatrix;
      Matrix projection = Camera.ProjectionMatrix;

      GraphicsDevice device = ZombieCraft.Instance.GraphicsDevice;
      RenderState renderState = device.RenderState;

      renderState.DepthBufferEnable = true;

      foreach ( var kvp in instancedModels )
        kvp.Value.DrawInstances( ref view, ref projection, ref Camera.Position );

      // Draw regular models
      //...
    }
  }
}