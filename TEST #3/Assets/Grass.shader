Shader "Roystan/Grass"
{
	Properties
	{
		[Header(Shading)]
		_TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		[Space]
		_TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
		_TessellationEdgeLength("Tessellation Edge Length", Range(5, 100)) = 50
		_TessellationMinDist("Tessellation Min Dist", Float) = 10
		_BendRotationRandom("Bend Rotation Random", Range(0,1)) = 0.2
		_PlayerPosition("Player Position", Vector) = (0,0,0,0)
		_PlayerStrength("Player Strength", Float) = 10
		[Header(Blades)]
		_BladeWidth("Blade Width", Float) = 0.05
		_BladeWidthRandom("Blade Width Random", Float) = 0.02
		_BladeHeight("Blade Height", Float) = 0.5
		_BladeHeightRandom("Blade Height Random", Float) = 0.3
		_BladeTipOffset("Blade Tip Offset", Range(0, 1)) = 0.5
		[Header(Wind)]
		_WindDistortionMap("Wind Distortion Map", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Float) = 1
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	//#include "Autolight.cginc"	
	#include "Shaders/CustomTessellation.cginc"

	float _BendRotationRandom;
	float _BladeWidth;
	float _BladeWidthRandom;
	float _BladeHeight;
	float _BladeHeightRandom;
	sampler2D _WindDistortionMap;
	float4 _WindDistortionMap_ST;
	float2 _WindFrequency;
	float _WindStrength;
	float _BladeTipOffset;
	float3 _PlayerPosition;
	float _PlayerStrength;

	// Simple noise function, sourced from http://answers.unity.com/answers/624136/view.html
	// Extended discussion on this function can be found at the following link:
	// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
	// Returns a number in the 0...1 range.
	float rand(float3 co)
	{
		return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
	}

	// Construct a rotation matrix that rotates around the provided axis, sourced from:
	// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
	float3x3 AngleAxis3x3(float angle, float3 axis)
	{
		float c, s;
		sincos(angle, s, c);

		float t = 1 - c;
		float x = axis.x;
		float y = axis.y;
		float z = axis.z;

		return float3x3(
			t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			t * x * z - s * y, t * y * z + s * x, t * z * z + c
			);
	}

	struct geometryOutput
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	geometryOutput VertexOutput(float3 pos, float2 uv)
	{
		geometryOutput o;
		o.pos = UnityObjectToClipPos(pos);
		o.uv = uv;
		return o;
	}

	#define BLADE_SEGMENTS 2

	geometryOutput GenerateGrassVertex(float3 pos, float width, float height, float2 uv, float3x3 transformMatrix)
	{
		float3 tangentPoint = float3(width, 0, height);

		float3 localPosition = pos + mul(transformMatrix, tangentPoint);
		return VertexOutput(localPosition, uv);
	}

	TriangleStream<geometryOutput> CreateGrassBlade(float3 pos, float3 normal, inout TriangleStream<geometryOutput> triStream)
	{
		float3 vTangent = float3(0, 0, 1);
		vTangent = vTangent - normal * dot(normal, vTangent);
		float3 vBinormal = cross(normal, vTangent);

		float3x3 tangentToLocal = float3x3
			(
				vTangent.x, vBinormal.x, normal.x,
				vTangent.y, vBinormal.y, normal.y,
				vTangent.z, vBinormal.z, normal.z
				);

		float3x3 facingRotMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));
		float3x3 bendRotMatrix = AngleAxis3x3(rand(pos.zzx) * _BendRotationRandom * UNITY_PI * 0.5, float3(-1, 0, 0));

		float2 uv = pos.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
		float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength;
		float3 wind = normalize(float3(windSample.x, windSample.y, 0));

		float3x3 windRotation = AngleAxis3x3(UNITY_PI * windSample, wind);

		/*float3 grassDirection = normalize(_PlayerPosition - pos);
		grassDirection.y = 0;
		float3 grassPerpen = float3(grassDirection.z, 0, -grassDirection.x);
		float3x3 playerRotation = AngleAxis3x3((1 - (min(distance(_PlayerPosition, pos), 1))) * _PlayerStrength, grassPerpen);*/

		float3x3 transformationMatrix = mul(mul(mul(tangentToLocal, windRotation), facingRotMatrix), bendRotMatrix);
		float3x3 transformationMatrixFacing = mul(tangentToLocal, facingRotMatrix);

		float height = ((rand(pos.zyx) * 2 - 1) * _BladeHeightRandom + _BladeHeight) * ((min(distance(_PlayerPosition, pos), 1)));
		float width = (rand(pos.xzy) * 2 - 1) * _BladeWidthRandom + _BladeWidth;

		for (int i = 0; i < BLADE_SEGMENTS; i++)
		{
			float t = i / ((float)BLADE_SEGMENTS - _BladeTipOffset);

			float segmentHeight = height * (t);
			float segmentWidth = width;

			float3x3 transformMatrix = (transformationMatrixFacing * (i == 0)) +  (transformationMatrix * (i != 0));

			triStream.Append(GenerateGrassVertex(pos, segmentWidth, segmentHeight, float2(0, t), transformMatrix));
			triStream.Append(GenerateGrassVertex(pos, -segmentWidth, segmentHeight, float2(1, t), transformMatrix));
		}
		triStream.Append(GenerateGrassVertex(pos, 0, height, float2(0.5, 1), transformationMatrix));

		return triStream;
	}
	
	[maxvertexcount((BLADE_SEGMENTS * 2 + 1) * 3)]
	void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> triStream)
	{
		CreateGrassBlade(IN[0].vertex, IN[0].normal, triStream);
		triStream.RestartStrip();
		CreateGrassBlade(IN[1].vertex, IN[1].normal, triStream);
		triStream.RestartStrip();
		CreateGrassBlade(IN[2].vertex, IN[2].normal, triStream);
	}

	ENDCG

	SubShader
	{
		Cull Off		

		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				//"LightMode" = "Vertex"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geo
			#pragma multi_complile_fog
			#pragma target 4.6
			#pragma hull hull
			#pragma domain domain

			#include "Lighting.cginc"

			float4 _TopColor;
			float4 _BottomColor;

			float4 frag(geometryOutput i, fixed facing : VFACE) : SV_Target
			{
				return lerp (_BottomColor, _TopColor, i.uv.y);
				//return tex2D(_MainTex, i.uv);
				//return float4(1,1,1,1);
			}
			ENDCG
		}
	}
}