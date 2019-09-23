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

float4 Shade(float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	return Texture.Sample(TextureSampler, texCoord) * float4(0.6, 0.6, 0.6, 1);
}

technique Shade
{
    pass Shade
    {
		PixelShader = compile ps_2_0 Shade();
    }
}
