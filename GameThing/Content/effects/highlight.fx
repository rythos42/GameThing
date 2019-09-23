Texture2D Texture;
SamplerState TextureSampler
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

static const float BloomThreshold = 0.25;
static const float BloomIntensity = 1.25;
static const float BaseIntensity = 1;
static const float BloomSaturation = 1;
static const float BaseSaturation = 1;

float4 AdjustSaturation(float4 color, float saturation)
{
	float grey = dot(color, float3(0.3, 0.59, 0.11));

	return lerp(grey, color, saturation);
}

float4 BloomAndCombine(float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float4 base = Texture.Sample(TextureSampler, texCoord);
	float4 bloom = saturate((base - BloomThreshold) / (1 - BloomThreshold));
    
    bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
    base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
    
    base *= (1 - saturate(bloom));

	return base + bloom;
}

technique Highlight
{
    pass BloomAndCombine
    {
		PixelShader = compile ps_2_0 BloomAndCombine();
    }
}
