#region File Description
//-----------------------------------------------------------------------------
// InstancedModel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ContentTypes
{
  public enum InstancingTechnique
  {
    HardwareInstancing,
    ShaderInstancing,
  }

  public class InstancedModel
  {
    #region Properties

    public InstancingTechnique InstancingTechnique { get; private set; }

    public object Tag { get; set; }

    public List<InstancedModelPart> ModelParts { get { return modelParts; } }

    #endregion

    #region Fields

    List<InstancedModelPart> modelParts = new List<InstancedModelPart>();

    GraphicsDevice graphicsDevice;

    #endregion

    #region Initialization

    internal InstancedModel( ContentReader input )
    {
      graphicsDevice = RenderHelper.GetGraphicsDevice( input );

      int partCount = input.ReadInt32();

      for ( int i = 0; i < partCount; i++ )
      {
        modelParts.Add( new InstancedModelPart( this, input, graphicsDevice ) );
      }

      Tag = input.ReadObject<object>();
    }

    #endregion

    #region Public Methods

    public bool SetInstancingTechnique(
                InstancingTechnique technique,
                VertexElement[] instanceVertexElements )
    {
      if ( !IsTechniqueSupported( technique ) )
        return false;

      InstancingTechnique = technique;

      foreach ( InstancedModelPart modelPart in modelParts )
      {
        modelPart.Initialize( technique, instanceVertexElements );
      }

      return true;
    }

    public bool IsTechniqueSupported( InstancingTechnique technique )
    {
      if ( technique == InstancingTechnique.HardwareInstancing )
      {
        GraphicsDeviceCapabilities caps =
                                    graphicsDevice.GraphicsDeviceCapabilities;
        return caps.PixelShaderVersion.Major >= 3;
      }

      return true;
    }

    #endregion
  }
}
