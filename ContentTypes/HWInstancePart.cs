#region Using Statements

using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ContentTypes
{
  partial class InstancedModelPart
  {
    #region Constant Declarations

    const bool Allow32BitIndexBuffer = false;

    const int LeastInstances = 32;

    #endregion

    void InitializeHardwareInstancing( VertexElement[] instanceElements )
    {
#if XBOX360
      if ( !( indexBuffer is InstancedIndexBuffer ) )
      {
        IndexBuffer newIB = InstancedIndexBuffer.Create( indexBuffer,
                                LeastInstances, Allow32BitIndexBuffer );
        indexBuffer.Dispose();
        indexBuffer = newIB;
      }

      MaxInstances = ( (InstancedIndexBuffer)indexBuffer ).MaxInstances;

#else

      MaxInstances = 500;

#endif

      vertexDeclaration = RenderHelper.ExtendVertexDeclaration(
                                          vertexBuffer.GraphicsDevice,
                                          originalVertexDeclaration,
                                          instanceElements );
    }
  }
}
