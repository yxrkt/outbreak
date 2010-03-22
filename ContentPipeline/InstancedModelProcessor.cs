#region File Description
//-----------------------------------------------------------------------------
// InstancedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

#endregion

namespace ContentPipeline
{
  [ContentProcessor( DisplayName = "Instanced Model" )]
  public class InstancedModelProcessor : ContentProcessor<NodeContent,
                                                          InstancedModelContent>
  {
    #region Properties

    [DefaultValue( true )]
    [DisplayName( "Bake in node transforms" )]
    public bool BakeTransform { get; set; }

    [DisplayName( "Effect File" )]
    public string EffectFilename { get; set; }

    #endregion

    #region Fields

    protected NodeContent rootNode;
    protected ContentProcessorContext context;
    protected InstancedModelContent outputModel;

    Dictionary<MaterialContent, MaterialContent> processedMaterials =
                        new Dictionary<MaterialContent, MaterialContent>();

    #endregion

    public InstancedModelProcessor()
      : base()
    {
      BakeTransform = true;
    }

    public override InstancedModelContent Process( NodeContent input,
                                                   ContentProcessorContext context )
    {
      if ( String.IsNullOrEmpty( EffectFilename ) )
      {
        EffectFilename = "../Effects/InstancedModel.fx";
      }

      this.rootNode = input;
      this.context = context;

      outputModel = new InstancedModelContent();

      ProcessNode( input );

      return outputModel;
    }


    void ProcessNode( NodeContent node )
    {
      if ( BakeTransform )
      {
        MeshHelper.TransformScene( node, node.Transform );
        node.Transform = Matrix.Identity;
      }

      MeshContent mesh = node as MeshContent;

      if ( mesh != null )
        ProcessMesh( mesh );

      foreach ( NodeContent child in node.Children )
      {
        ProcessNode( child );
      }
    }

    void ProcessMesh( MeshContent mesh )
    {
      MeshHelper.OptimizeForCache( mesh );

      foreach ( GeometryContent geometry in mesh.Geometry )
      {
        ProcessGeometry( geometry );
      }
    }

    void ProcessGeometry( GeometryContent geometry )
    {
      int indexCount = geometry.Indices.Count;
      int vertexCount = geometry.Vertices.VertexCount;

      if ( vertexCount > ushort.MaxValue )
      {
        throw new InvalidContentException(
            string.Format( "There are too many vertices." ) );
      }

      if ( vertexCount > ushort.MaxValue / 8 )
      {
        context.Logger.LogWarning( null, rootNode.Identity,
                        "The number of vertices cannot be drawn in one batch." );
      }

      VertexChannelCollection vertexChannels = geometry.Vertices.Channels;

      for ( int i = 1; i <= 4; i++ )
      {
        if ( vertexChannels.Contains( VertexChannelNames.TextureCoordinate( i ) ) )
        {
          throw new InvalidContentException(
              string.Format( "The model's texture coordinate {0} is already in use.", i ) );
        }
      }

      VertexBufferContent vertexBufferContent;
      VertexElement[] vertexElements;

      geometry.Vertices.CreateVertexBuffer( out vertexBufferContent,
                                            out vertexElements,
                                            context.TargetPlatform );

      int vertexStride =
                  VertexDeclaration.GetVertexStrideSize( vertexElements, 0 );

      MaterialContent material = ProcessMaterial( geometry.Material );

      outputModel.AddModelPart( indexCount, vertexCount, vertexStride,
                                vertexElements, vertexBufferContent,
                                geometry.Indices, material );
    }

    MaterialContent ProcessMaterial( MaterialContent material )
    {
      if ( !processedMaterials.ContainsKey( material ) )
      {
        EffectMaterialContent instancedMaterial = AddEffectMaterial( material );

        processedMaterials[material] =
            context.Convert<MaterialContent,
                            MaterialContent>( instancedMaterial,
                                              "MaterialProcessor" );
      }

      return processedMaterials[material];
    }

    protected virtual EffectMaterialContent AddEffectMaterial(
                                                        MaterialContent material )
    {
      EffectMaterialContent instancedMaterial = new EffectMaterialContent();

      instancedMaterial.Effect = new ExternalReference<EffectContent>(
                                      EffectFilename, rootNode.Identity );

      if ( material.Textures.ContainsKey( "Texture" ) )
      {
        instancedMaterial.Textures.Add( "BasicTexture",
                                            material.Textures["Texture"] );
      }

      string[] paramNames =
      {
        "Alpha",
        "DiffuseColor",
        "EmissiveColor",
        "SpecularColor",
        "SpecularPower"
      };

      foreach ( string paramName in paramNames )
      {
        if ( material.OpaqueData.ContainsKey( paramName ) )
        {
          instancedMaterial.OpaqueData.Add(
                                  paramName, material.OpaqueData[paramName] );
        }
      }

      return instancedMaterial;
    }
  }
}
