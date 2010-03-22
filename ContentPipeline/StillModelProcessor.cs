#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace CustomModelPipeline
{
  [ContentProcessor( DisplayName = "Still Model" )]
  public class StillModelProcessor : ContentProcessor<NodeContent,
                                                       StillModelContent>
  {
    #region Fields

    ContentProcessorContext context;
    StillModelContent outputModel;

    // A single material may be reused on more than one piece of geometry.
    // This dictionary keeps track of materials we have already converted,
    // to make sure we only bother processing each of them once.
    Dictionary<MaterialContent, MaterialContent> processedMaterials =
                        new Dictionary<MaterialContent, MaterialContent>();

    #endregion


    public override StillModelContent Process( NodeContent input,
                                                ContentProcessorContext context )
    {
      this.context = context;

      outputModel = new StillModelContent();

      ProcessNode( input );

      return outputModel;
    }


    void ProcessNode( NodeContent node )
    {
      MeshHelper.TransformScene( node, node.Transform );

      node.Transform = Matrix.Identity;

      MeshContent mesh = node as MeshContent;

      if ( mesh != null )
      {
        MeshHelper.OptimizeForCache( mesh );

        foreach ( GeometryContent geometry in mesh.Geometry )
        {
          ProcessGeometry( geometry );
        }
      }

      foreach ( NodeContent child in node.Children )
      {
        ProcessNode( child );
      }
    }


    void ProcessGeometry( GeometryContent geometry )
    {
      int triangleCount = geometry.Indices.Count / 3;
      int vertexCount = geometry.Vertices.VertexCount;

      VertexBufferContent vertexBufferContent;
      VertexElement[] vertexElements;

      geometry.Vertices.CreateVertexBuffer( out vertexBufferContent,
                                            out vertexElements,
                                            context.TargetPlatform );

      int vertexStride = VertexDeclaration.GetVertexStrideSize( vertexElements, 0 );

      MaterialContent material = ProcessMaterial( geometry.Material );

      outputModel.AddModelPart( triangleCount, vertexCount, vertexStride,
                                vertexElements, vertexBufferContent,
                                geometry.Indices, material );
    }


    /// <summary>
    /// Converts an input material by chaining to the built-in MaterialProcessor
    /// class. This will automatically go off and build any effects or textures
    /// that are referenced by the material. When you load the resulting material
    /// at runtime, you will get back an Effect instance that has the appropriate
    /// textures already loaded into it and ready to go.
    /// </summary>
    MaterialContent ProcessMaterial( MaterialContent material )
    {
      if ( !processedMaterials.ContainsKey( material ) )
      {
        processedMaterials[material] =
            context.Convert<MaterialContent,
                            MaterialContent>( material, "MaterialProcessor" );
      }

      return processedMaterials[material];
    }
  }
}
