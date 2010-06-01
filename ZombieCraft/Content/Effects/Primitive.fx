float4x4 View;
float4x4 Projection;

float4  Color = { 1, 1, 1, 1 };
texture Texture;
sampler TextureSampler = 
sampler_state
{
  Texture = (Texture);
  
  MipFilter = LINEAR;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
};


struct ColorVertexShaderInput
{
  float4 Position : POSITION0;
  float4 Color    : COLOR0;
};

struct ColorVertexShaderOutput
{
  float4 Position : POSITION0;
  float4 Color    : COLOR0;
};

struct TextureVertexShaderInput
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
};

struct TextureVertexShaderOutput
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
};

ColorVertexShaderOutput ColorVertexShader( ColorVertexShaderInput input )
{
  ColorVertexShaderOutput output;

  float4 viewPosition = mul( input.Position, View );
  output.Position = mul( viewPosition, Projection );
  
  output.Color = input.Color;

  return output;
}

float4 ColorPixelShader( ColorVertexShaderOutput input ) : COLOR0
{
  return input.Color * Color;
}

TextureVertexShaderOutput TextureVertexShader( TextureVertexShaderInput input )
{
  TextureVertexShaderOutput output;

  float4 viewPosition = mul( input.Position, View );
  output.Position = mul( viewPosition, Projection );
  
  output.TexCoord = input.TexCoord;

  return output;
}

float4 TexturePixelShader( TextureVertexShaderOutput input ) : COLOR0
{
  return tex2D( TextureSampler, input.TexCoord ) * Color;
}


technique Color
{
  pass Pass1
  {
    VertexShader = compile vs_3_0 ColorVertexShader();
    PixelShader  = compile ps_3_0 ColorPixelShader();
  }
}

technique Texture
{
  pass Pass1
  {
    VertexShader = compile vs_3_0 TextureVertexShader();
    PixelShader  = compile ps_3_0 TexturePixelShader();
  }
}
