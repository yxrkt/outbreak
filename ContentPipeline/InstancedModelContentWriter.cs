#region File Description
//-----------------------------------------------------------------------------
// InstancedModelContentWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// このソースコードはクリエータークラブオンラインのMeshInstancingの
// プロセッサーコードのコメントを翻訳、修正したもの
// http://creators.xna.com/en-US/sample/meshinstancing
//-----------------------------------------------------------------------------
#endregion

#region Using ステートメント

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

#endregion

namespace ContentPipeline
{
  /// <summary>
  /// InstancedModelContentを書き出す為のコンテントタイプライター
  /// </summary>
  [ContentTypeWriter]
  public class InstancedModelContentWriter : ContentTypeWriter<InstancedModelContent>
  {
    /// <summary>
    /// XNBファイルにインスタンスモデルを書き出す
    /// </summary>
    protected override void Write( ContentWriter output, InstancedModelContent value )
    {
      value.Write( output );
    }

    /// <summary>
    /// 実行時のCLR型をコンテント･パイプラインに伝える
    /// </summary>
    public override string GetRuntimeType( TargetPlatform targetPlatform )
    {
      return typeof( ContentTypes.InstancedModel ).AssemblyQualifiedName;
    }

    /// <summary>
    /// 実行時に使用するコンテントタイプリーダーの型を
    /// コンテント･パイプラインに伝える
    /// </summary>
    public override string GetRuntimeReader( TargetPlatform targetPlatform )
    {
      return typeof( ContentTypes.InstancedModelReader ).AssemblyQualifiedName;
    }
  }
}
