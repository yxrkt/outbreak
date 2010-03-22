#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace CustomModelPipeline
{
  [ContentSerializerRuntimeType( "ContentTypes.StillModel, ContentTypes" )]
  public class StillModelContent
  {
    [ContentSerializer]
    List<StillModelPart> modelParts = new List<StillModelPart>();


    public void AddModelPart( int triangleCount, int vertexCount, int vertexStride,
                              VertexElement[] vertexElements,
                              VertexBufferContent vertexBufferContent,
                              IndexCollection indexCollection,
                              MaterialContent materialContent )
    {
      StillModelPart modelPart = new StillModelPart();

      modelPart.TriangleCount = triangleCount;
      modelPart.VertexCount = vertexCount;
      modelPart.VertexStride = vertexStride;
      modelPart.VertexElements = vertexElements;
      modelPart.VertexBufferContent = vertexBufferContent;
      modelPart.IndexCollection = indexCollection;
      modelPart.MaterialContent = materialContent;

      modelParts.Add( modelPart );
    }
  }

  [ContentSerializerRuntimeType( "ContentTypes.StillModelPart, ContentTypes" )]
  class StillModelPart
  {
    public int TriangleCount;
    public int VertexCount;
    public int VertexStride;

    public VertexElement[] VertexElements;
    public VertexBufferContent VertexBufferContent;
    public IndexCollection IndexCollection;

    [ContentSerializer( SharedResource = true )]
    public MaterialContent MaterialContent;
  }
}
