#region File Description
//-----------------------------------------------------------------------------
// InstancedModelReader.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// このソースコードはクリエータークラブオンラインのMeshInstancingの
// コードのコメントを翻訳したもの
// http://creators.xna.com/en-US/sample/meshinstancing
//-----------------------------------------------------------------------------
#endregion

#region Using ステートメント

using Microsoft.Xna.Framework.Content;

#endregion

namespace ContentTypes
{
  /// <summary>
  /// InstanceModelを生成するためのコンテントタイプリーダー
  /// </summary>
  public class InstancedModelReader : ContentTypeReader<InstancedModel>
  {
    /// <summary>
    /// Reads instanced model data from an XNB file.
    /// </summary>
    protected override InstancedModel Read( ContentReader input,
                                           InstancedModel existingInstance )
    {
      return new InstancedModel( input );
    }
  }
}
