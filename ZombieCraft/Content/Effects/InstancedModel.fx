//-----------------------------------------------------------------------------
// InstancedModel.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//=============================================================================
#include "Basic.fx"


// Maxiumum number of instances that can be drawn. This is limited by the 
// number of constant registers available by the current shader model. This
// value must match MaxShaderInstanceCount in ShaderInstancePart.cs.

#define MAX_SHADER_INSTANCE_COUNT 100

// Array of translation and scale data
// xyz = Position, w = Scale

float4 ShaderInstanceParams0[MAX_SHADER_INSTANCE_COUNT];

// Array of rotation data
// xyz = Rotation Axis, w = Angle

float4 ShaderInstanceParams1[MAX_SHADER_INSTANCE_COUNT];

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal	  : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Diffuse	: COLOR0;
    float4 Specular	: COLOR1;
    float2 TexCoord : TEXCOORD0;
};

//-----------------------------------------------------------------------------
// Builds a quaternion from an axis and angle
//=============================================================================
float4 QuaternionFromAxisAngle( float3 axis, float angle )
{
  float halfAngle = angle * 0.5f;
  return float4( axis * sin( halfAngle ), cos( halfAngle ) );
}

//-----------------------------------------------------------------------------
// Transforms a vertex by a quaternions
//=============================================================================
float3 TransformByQuaternion( float3 position, float4 quaternion )
{
  float4 Q = quaternion;
  float3 v = position;
	
	return 
    ( 2.0f * Q.w * Q.w - 1.0f ) * v +
    ( 2.0f * dot( v, Q.xyz ) * Q.xyz ) +
    ( 2.0f * Q.w * cross( Q.xyz, v ) );
}

//-----------------------------------------------------------------------------
// Common method called by all vertex shaders
//=============================================================================
VertexShaderOutput VertexShaderCommon( VertexShaderInput input, float4 instanceParam0,
                                       float4 instanceParam1)
{
  VertexShaderOutput output;
    
  // Vertex transform
  float3 pos_os = instanceParam0.xyz;
  float scale = instanceParam0.w;
  float3 rotationAxis = instanceParam1.xyz;
  float rotation = instanceParam1.w;
    
  float4 Q = QuaternionFromAxisAngle( rotationAxis, rotation );
  float4 pos_ws = float4( pos_os +
                          TransformByQuaternion( input.Position * scale, Q ), 1 );
 
  float4 pos_vs = mul(pos_ws, View);
  output.Position = mul(pos_vs, Projection);

  // Lighting
  float3 N = TransformByQuaternion( input.Normal, Q );
  float3 posToEye = EyePosition - pos_ws;
  float3 E = normalize( posToEye );
  ColorPair lightResult = ComputeLights( E, N );
    
  output.Diffuse = float4( lightResult.Diffuse, 1 );
  output.Specular = float4( lightResult.Specular, 1 );

  // Texture coordinate
  output.TexCoord = input.TexCoord;

  return output;
}

//-----------------------------------------------------------------------------
// Shader instancing
//=============================================================================
VertexShaderOutput ShaderInstancingVS( VertexShaderInput input,
                                       float instanceIndex : TEXCOORD1 )
{
  return VertexShaderCommon( input,
                             ShaderInstanceParams0[instanceIndex], 
                             ShaderInstanceParams1[instanceIndex] );
}

#ifdef XBOX360

//-----------------------------------------------------------------------------
// Hardware vertex shader instancing (Xbox)
//=============================================================================

// Number of vertices in the model
int VertexCount;

VertexShaderOutput HardwareInstancingVS( int index : INDEX )
{
  int vertexIndex = ( index + 0.5 ) % VertexCount;
  int instanceIndex = ( index + 0.5 ) / VertexCount;

  float4 position;
  float4 normal;
  float4 texCoord;
  float4 instanceParam0;
  float4 instanceParam1;

  asm
  {
    vfetch position,		vertexIndex, position0
    vfetch normal,			vertexIndex, normal0
    vfetch texCoord,		vertexIndex, texcoord0

    vfetch instanceParam0,	instanceIndex, texcoord1
    vfetch instanceParam1,	instanceIndex, texcoord2
  };

  VertexShaderInput input;

  input.Position = position;
  input.Normal = normal;
  input.TexCoord = texCoord;

  return VertexShaderCommon( input, instanceParam0, instanceParam1 );
}

#else

//-----------------------------------------------------------------------------
// Hardware instanced vertex shader (Windows)
// Shader model 3.0 or later can use hardware instancing
// Information is read from the stream every two instances of the second peak (??)
//=============================================================================
VertexShaderOutput HardwareInstancingVS( VertexShaderInput input,
                                         float4 instanceParam0 : TEXCOORD1,
                                         float4 instanceParam1 : TEXCOORD2 )
{
  return VertexShaderCommon( input, instanceParam0, instanceParam1 );
}

#endif

//-----------------------------------------------------------------------------
// Pixel shader is shared by all techniques
//=============================================================================
float4 PixelShader( VertexShaderOutput input ) : COLOR0
{
	return tex2D( TextureSampler, input.TexCoord ) * 
					      input.Diffuse + float4( input.Specular.rgb, 0 );
}

//-----------------------------------------------------------------------------
// Techniques
//=============================================================================
technique ShaderInstancing
{
  pass Pass1
  {
    VertexShader = compile vs_2_0 ShaderInstancingVS();
    PixelShader  = compile ps_1_1 PixelShader();
  }
}

// Requires Xbox 360 or SM 3.0 graphics card
technique HardwareInstancing
{
  pass Pass1
  {
    VertexShader = compile vs_3_0 HardwareInstancingVS();
    PixelShader  = compile ps_3_0 PixelShader();
  }
}
