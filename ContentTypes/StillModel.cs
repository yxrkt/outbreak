#region File Description
//-----------------------------------------------------------------------------
// CustomModel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace ContentTypes
{
  public class StillModel
  {
    #region Fields

    // Disable compiler warning that we never initialize these fields.
    // That's ok, because the XNB deserializer initialises them for us!
#pragma warning disable 649

    // Internally our custom model is made up from a list of model parts.
    [ContentSerializer]
    List<StillModelPart> modelParts;

#pragma warning restore 649

    #endregion


    private StillModel()
    {
    }


    public void Draw( Matrix world, Matrix view, Matrix projection )
    {
      foreach ( StillModelPart modelPart in modelParts )
      {
        // Look up the effect, and set effect parameters on it. This sample
        // assumes the model will only be using BasicEffect, but a more robust
        // implementation would probably want to handle custom effects as well.
        BasicEffect effect = (BasicEffect)modelPart.Effect;

        effect.EnableDefaultLighting();

        effect.World = world;
        effect.View = view;
        effect.Projection = projection;

        // Set the graphics device to use our vertex declaration,
        // vertex buffer, and index buffer.
        GraphicsDevice device = effect.GraphicsDevice;

        device.VertexDeclaration = modelPart.VertexDeclaration;

        device.Vertices[0].SetSource( modelPart.VertexBuffer, 0,
                                     modelPart.VertexStride );

        device.Indices = modelPart.IndexBuffer;

        // Begin the effect, and loop over all the effect passes.
        effect.Begin();

        foreach ( EffectPass pass in effect.CurrentTechnique.Passes )
        {
          pass.Begin();

          // Draw the geometry.
          device.DrawIndexedPrimitives( PrimitiveType.TriangleList,
                                        0, 0, modelPart.VertexCount,
                                        0, modelPart.TriangleCount );

          pass.End();
        }

        effect.End();
      }
    }
  }

#pragma warning disable 649

  class StillModelPart
  {
    public int TriangleCount;
    public int VertexCount;
    public int VertexStride;

    public VertexDeclaration VertexDeclaration;
    public VertexBuffer VertexBuffer;
    public IndexBuffer IndexBuffer;

    [ContentSerializer( SharedResource = true )]
    public Effect Effect;
  }
}

#pragma warning restore 649
