using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace ZombieCraft
{
  public struct VertexPositionNormal
  {
    public Vector3 Position;
    public Vector3 Normal;

    public static readonly VertexElement[] VertexElements =
    {
      new VertexElement( 0, 0, VertexElementFormat.Vector3, 
                               VertexElementMethod.Default, 
                               VertexElementUsage.Position, 0 ),

      new VertexElement( 0, 12, VertexElementFormat.Vector3, 
                                VertexElementMethod.Default, 
                                VertexElementUsage.Normal, 0 ),
    };

    public VertexPositionNormal( Vector3 position, Vector3 normal )
    {
      Position = position;
      Normal = normal;
    }

    public static int SizeInBytes { get { return sizeof( float ) * 6; } }
  }

  public struct VertexPositionNormalColor
  {
    public Vector3 Position;
    public Vector3 Normal;
    public Color Color;

    public static readonly VertexElement[] VertexElements =
    {
      new VertexElement( 0, 0, VertexElementFormat.Vector3, 
                               VertexElementMethod.Default, 
                               VertexElementUsage.Position, 0 ),

      new VertexElement( 0, 12, VertexElementFormat.Vector3, 
                                VertexElementMethod.Default, 
                                VertexElementUsage.Normal, 0 ),

      new VertexElement( 0, 24, VertexElementFormat.Color, 
                                VertexElementMethod.Default, 
                                VertexElementUsage.Color, 0 ),
    };

    public VertexPositionNormalColor( Vector3 position, Vector3 normal, Color color )
    {
      Position = position;
      Normal = normal;
      Color = color;
    }

    public static int SizeInBytes { get { return sizeof( float ) * 6; } }
  }

  public struct VertexPositionNormalTextureTangentBinormal
  {
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TextureCoordinate;
    public Vector3 Tangent;
    public Vector3 Binormal;

    public static readonly VertexElement[] VertexElements =
    {
      new VertexElement( 0, 0, VertexElementFormat.Vector3, 
                               VertexElementMethod.Default, 
                               VertexElementUsage.Position, 0 ),

      new VertexElement( 0, 12, VertexElementFormat.Vector3, 
                                VertexElementMethod.Default, 
                                VertexElementUsage.Normal, 0 ),

      new VertexElement( 0, 24, VertexElementFormat.Vector2, 
                                VertexElementMethod.Default, 
                                VertexElementUsage.TextureCoordinate, 0 ),

      new VertexElement( 0, 32, VertexElementFormat.Vector3, 
                                VertexElementMethod.Default, 
                                VertexElementUsage.Tangent, 0 ),

      new VertexElement( 0, 36, VertexElementFormat.Vector3, 
                                VertexElementMethod.Default, 
                                VertexElementUsage.Binormal, 0 ),
    };

    public VertexPositionNormalTextureTangentBinormal( Vector3 position, Vector3 normal, 
                                                       Vector2 textureCoordinate, Vector3 tangent, Vector3 binormal )
    {
      Position = position;
      Normal = normal;
      TextureCoordinate = textureCoordinate;
      Tangent = tangent;
      Binormal = binormal;
    }

    public static int SizeInBytes { get { return sizeof( float ) * 14; } }
  }

  public struct StaticInstanceVertex
  {
    public Vector3 Position;
    public float Scale;
    public Vector3 Axis;
    public float Angle;

    public StaticInstanceVertex( Vector3 position, float scale, 
                                 Vector3 axis, float angle )
    {
      Position = position;
      Scale = scale;
      Axis = axis;
      Angle = angle;
    }

    public static readonly VertexElement[] VertexElements = 
    {
      new VertexElement( 1, 0, VertexElementFormat.Vector4,
                               VertexElementMethod.Default,
                               VertexElementUsage.TextureCoordinate, 1 ),

      new VertexElement( 1,16, VertexElementFormat.Vector4,
                               VertexElementMethod.Default,
                               VertexElementUsage.TextureCoordinate, 2 ),
    };

    public static int SizeInBytes
    {
      get
      {
        return Marshal.SizeOf( typeof( StaticInstanceVertex ) );
      }
    }
  }
}