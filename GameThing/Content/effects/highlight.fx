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

Texture2D BaseTexture;
SamplerState BaseTextureSampler
{
	Texture = <BaseTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float BloomIntensity;
float BaseIntensity;
float BloomSaturation;
float BaseSaturation;
float BloomThreshold;

float4 AdjustSaturation(float4 color, float saturation)
{
	float grey = dot(color, float3(0.3, 0.59, 0.11));

	return lerp(grey, color, saturation);
}

float4 BloomAndCombine(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float4 c = Texture.Sample(TextureSampler, texCoord);
	float4 bloom = saturate((c - BloomThreshold) / (1 - BloomThreshold));
	float4 base = BaseTexture.Sample(BaseTextureSampler, texCoord);
    
    bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
    base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
    
    base *= (1 - saturate(bloom));
    
    return base + bloom;
}

technique Highlight
{
    pass BloomAndCombine
    {
        #if SM4
			PixelShader = compile ps_4_0_level_9_1 BloomAndCombine();
		#else
			PixelShader = compile ps_2_0 BloomAndCombine();
		#endif
    }
}
