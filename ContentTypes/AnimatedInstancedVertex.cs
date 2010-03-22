using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ContentTypes
{
  /// <summary>
  /// This vertex holds transformation data for an entire
  /// instance of an instanced model.
  /// </summary>
  [StructLayout( LayoutKind.Sequential )]
  public struct AnimatedInstanceVertex
  {
    public Vector3 Position;
    public float Scale;
    public Vector3 RotateAxis;
    public float Rotation;
    public float ClipOffset;
    public float HitEffect;

    public static VertexElement[] VertexElements =
    {
      new VertexElement( 1,  0, VertexElementFormat.Vector4,
                                VertexElementMethod.Default,
                                VertexElementUsage.TextureCoordinate, 1 ),

      new VertexElement( 1, 16, VertexElementFormat.Vector4,
                                VertexElementMethod.Default,
                                VertexElementUsage.TextureCoordinate, 2 ),

      new VertexElement( 1, 32, VertexElementFormat.Vector2,
                                VertexElementMethod.Default,
                                VertexElementUsage.TextureCoordinate, 3 ),
    };

    public static int SizeInBytes
    {
      get { return Marshal.SizeOf( typeof( AnimatedInstanceVertex ) ); }
    }
  }
}