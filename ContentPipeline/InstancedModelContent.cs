#region File Description
//-----------------------------------------------------------------------------
// InstancedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// このソースコードはクリエータークラブオンラインのMeshInstancingの
// プロセッサーコードのコメントを翻訳したもの
// http://creators.xna.com/en-US/sample/meshinstancing
//-----------------------------------------------------------------------------

#endregion

#region Using ステートメント

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

#endregion

namespace ContentPipeline
{
  /// <summary>
  /// コンテント･パイプライン用のクラスでランタイムのInstancedModelクラスに
  /// 相当するもの。
  /// このクラスにはInstancedModelProcessorから出力される。
  /// InstancedModelに近いものだがGPUのデータを直接格納するかわりに、
  /// 管理しやすいデータを格納している。
  /// </summary>
  public class InstancedModelContent
  {
    // インスタンスモデルは内部でModelPartを持つ
    List<ModelPart> modelParts = new List<ModelPart>();

    public object Tag;

    // それぞれのモデルパートは１つのエフェクトに対応するジオメトリを格納する
    class ModelPart
    {
      // インデックス数
      public int IndexCount;

      // 頂点数
      public int VertexCount;

      // 頂点ストライドサイズ
      public int VertexStride;

      // 頂点要素、実行時にはVertexDeclarationになる
      public VertexElement[] VertexElements;

      // 頂点バッファコンテント、実行時にはVertexBufferになる
      public VertexBufferContent VertexBufferContent;

      // インデックスコレクション、実行時にはIndexBufferになる
      public IndexCollection IndexCollection;

      // マテリアルコンテント、実行時にはEffectになる
      public MaterialContent MaterialContent;
    }


    /// <summary>
    /// ModelPartを追加する
    /// InstancedModelProcessorが使用するヘルパーメソッド
    /// </summary>
    public void AddModelPart( int indexCount, int vertexCount, int vertexStride,
                             VertexElement[] vertexElements,
                             VertexBufferContent vertexBufferContent,
                             IndexCollection indexCollection,
                             MaterialContent materialContent )
    {
      ModelPart modelPart = new ModelPart();

      modelPart.IndexCount = indexCount;
      modelPart.VertexCount = vertexCount;
      modelPart.VertexStride = vertexStride;
      modelPart.VertexElements = vertexElements;
      modelPart.VertexBufferContent = vertexBufferContent;
      modelPart.IndexCollection = indexCollection;
      modelPart.MaterialContent = materialContent;

      modelParts.Add( modelPart );
    }


    /// <summary>
    /// XNBファイルにInstancedModelを書き出す
    /// </summary>
    public void Write( ContentWriter output )
    {
      output.Write( modelParts.Count );

      foreach ( ModelPart modelPart in modelParts )
      {
        output.Write( modelPart.IndexCount );
        output.Write( modelPart.VertexCount );
        output.Write( modelPart.VertexStride );

        output.WriteObject( modelPart.VertexElements );
        output.WriteObject( modelPart.VertexBufferContent );
        output.WriteObject( modelPart.IndexCollection );

        output.WriteSharedResource( modelPart.MaterialContent );
      }

      output.WriteObject( Tag );
    }
  }
}
