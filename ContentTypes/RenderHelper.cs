#region Using ステートメント

using System;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ContentTypes
{
  /// <summary>
  /// レンダリング用ヘルパークラス
  /// </summary>
  public static class RenderHelper
  {
    /// <summary>
    /// ContentReaderからGraphicsDeviceを取得する
    /// </summary>
    public static GraphicsDevice GetGraphicsDevice( ContentReader input )
    {
      IServiceProvider serviceProvider = input.ContentManager.ServiceProvider;

      IGraphicsDeviceService deviceService =
          (IGraphicsDeviceService)serviceProvider.GetService(
                                      typeof( IGraphicsDeviceService ) );

      return deviceService.GraphicsDevice;
    }

    /// <summary>
    /// 頂点宣言クラスに新しい頂点要素を追加する
    /// </summary>
    /// <param name="vertexDeclaration">追加先の頂点宣言</param>
    /// <param name="extraElements">追加する頂点要素</param>
    /// <returns>新要素追加後の頂点宣言</returns>
    public static VertexDeclaration ExtendVertexDeclaration(
                VertexDeclaration vertexDeclaration, VertexElement[] extraElements )
    {
      VertexElement[] originalElements = vertexDeclaration.GetVertexElements();

      return ExtendVertexDeclaration( vertexDeclaration.GraphicsDevice,
                                          originalElements, extraElements );
    }

    /// <summary>
    /// 複数の頂点要素を合成する
    /// </summary>
    /// <param name="graphicsDevice">頂点宣言を生成する為のGraphicsDevice</param>
    /// <param name="originalElements">元の頂点要素</param>
    /// <param name="extraElements">追加する頂点要素</param>
    /// <returns>新要素追加後の頂点宣言</returns>
    public static VertexDeclaration ExtendVertexDeclaration(
                                        GraphicsDevice graphicsDevice,
                                        VertexElement[] originalElements,
                                        VertexElement[] extraElements )
    {
      int length = originalElements.Length + extraElements.Length;

      VertexElement[] elements = new VertexElement[length];

      originalElements.CopyTo( elements, 0 );
      extraElements.CopyTo( elements, originalElements.Length );

      return new VertexDeclaration( graphicsDevice, elements );
    }

  }
}
