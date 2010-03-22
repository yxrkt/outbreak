//-----------------------------------------------------------------------------
// BasicEffectで使われているパラメーターの宣言と平行光源計算
//-----------------------------------------------------------------------------

// 基本テクスチャ
uniform const texture BasicTexture;

uniform const sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (BasicTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

//-----------------------------------------------------------------------------
// フォグ設定
//-----------------------------------------------------------------------------

uniform const float		FogEnabled		: register(c0);
uniform const float		FogStart		: register(c1);
uniform const float		FogEnd			: register(c2);
uniform const float3	FogColor		: register(c3);

// ワールド座標での視点位置
uniform const float3	EyePosition		: register(c4);

//-----------------------------------------------------------------------------
// マテリアル設定
//-----------------------------------------------------------------------------

uniform const float3	DiffuseColor	: register(c5) = 0.8f;
uniform const float		Alpha			: register(c6) = 1;
uniform const float3	EmissiveColor	: register(c7) = 0;
uniform const float3	SpecularColor	: register(c8) = 1;
uniform const float		SpecularPower	: register(c9) = 16;

//-----------------------------------------------------------------------------
// 光源設定
// Directionはワールド座標で設定し、単位ベクトルを設定すること
//-----------------------------------------------------------------------------

// 環境光
uniform const float3	AmbientLightColor		: register(c10) =
{ 0.05333332f, 0.09882354f, 0.1819608f };

// 平行光源０の方向、拡散光、鏡面光
uniform const float3	DirLight0Direction		: register(c11) =
{ -0.5265408f, -0.5735765f, -0.6275069f };
uniform const float3	DirLight0DiffuseColor	: register(c12) =
{ 1, 0.9607844f, 0.8078432f };
uniform const float3	DirLight0SpecularColor	: register(c13) =
{ 1, 0.9607844f, 0.8078432f };

// 平行光源１の方向、拡散光、鏡面光
uniform const float3	DirLight1Direction		: register(c14) =
{ 0.7198464f, 0.3420201f, 0.6040227f };
uniform const float3	DirLight1DiffuseColor	: register(c15) =
{ 0.9647059f, 0.7607844f, 0.4078432f };
uniform const float3	DirLight1SpecularColor	: register(c16) = 0;

// 平行光源２の方向、拡散光、鏡面光
uniform const float3	DirLight2Direction		: register(c17) =
{ 0.4545195f, -0.7660444f, 0.4545195f };
uniform const float3	DirLight2DiffuseColor	: register(c18) =
{ 0.3231373f, 0.3607844f, 0.3937255f };
uniform const float3	DirLight2SpecularColor	: register(c19) =
{ 0.3231373f, 0.3607844f, 0.3937255f };

//-----------------------------------------------------------------------------
// 行列
//-----------------------------------------------------------------------------

// ワールド行列
uniform const float4x4	World		: register(vs, c20);	// 20 - 23

// ビュー行列
uniform const float4x4	View		: register(vs, c24);	// 24 - 27

// 射影行列
uniform const float4x4	Projection	: register(vs, c28);	// 28 - 31


//-----------------------------------------------------------------------------
// 平行光源の計算
//-----------------------------------------------------------------------------

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};

//-----------------------------------------------------------------------------
// Compute lighting
// E: 視線ベクトル
// N: 法線(ワールド座標内)
//-----------------------------------------------------------------------------
ColorPair ComputeLights(float3 E, float3 N)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;

	// 平行光源０の計算
	float3 L = -DirLight0Direction;
	float3 H = normalize(E + L);
	float2 ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
	result.Diffuse += DirLight0DiffuseColor * ret.x;
	result.Specular += DirLight0SpecularColor * ret.y;
	
	// 平行光源１の計算
	L = -DirLight1Direction;
	H = normalize(E + L);
	ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
	result.Diffuse += DirLight1DiffuseColor * ret.x;
	result.Specular += DirLight1SpecularColor * ret.y;
	
	// 平行光源２の計算
	L = -DirLight2Direction;
	H = normalize(E + L);
	ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
	result.Diffuse += DirLight2DiffuseColor * ret.x;
	result.Specular += DirLight2SpecularColor * ret.y;
		
	result.Diffuse *= DiffuseColor;
	result.Diffuse	+= EmissiveColor;
	result.Specular	*= SpecularColor;
		
	return result;
}
