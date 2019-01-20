float4x4 viewProj;
//float4x4 world;

struct VS_IN
{
	float4 pos : POSITION;
	//float4 col : COLOR;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	//float4x4 worldViewProj = mul(world, viewProj);
	float4x4 worldViewProj = viewProj;

	output.pos = mul(input.pos, worldViewProj);
	//output.pos = input.pos;
	//output.col = input.col;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	//return input.col;
	float4 col = {1.0f, 0, 0, 1.0f};
	return col;
}

technique10 Render
{
	pass P0
	{
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetPixelShader(CompileShader(ps_4_0, PS()));
	}
}