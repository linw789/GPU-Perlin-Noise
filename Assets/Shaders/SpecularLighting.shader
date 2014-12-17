Shader "Custom/SpecularLighting" 
{
	Properties 
	{
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_SpecColor ("SpecColor", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess ("Shininess", Float) = 10
	}
	SubShader 
	{
		Tags {"LightMode" = "ForwardBase"}
		Pass 
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			// user defined variables
			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float _Shininess;
			
			// Unity defined variables
			uniform float4 _LightColor0;
			
			// structures
			struct vertexIn
			{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
			};
			
			struct vertexOut
			{
				float4 pos: SV_POSITION;
				float4 posWorld: TEXCOORD0;
				float3 normalDir: TEXCOORD1;
			};
			
			// vertex function
			vertexOut vert(vertexIn vi)
			{
				vertexOut vo;
				
				vo.posWorld = mul(_Object2World, vi.vertex);
				vo.normalDir = normalize(mul(float4(vi.normal, 0.0), _World2Object).xyz);
				
				vo.pos = mul(UNITY_MATRIX_MVP, vi.vertex);
				
				return vo;
			}
			
			// fragment function
			float4 frag(vertexOut vo): COLOR
			{
				float3 normalDir = vo.normalDir;
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - vo.posWorld.xyz);
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				
				float atten = 1.0;
				float theta = max(0.0, dot(normalDir, lightDir));
				float3 lightDirReflect = 2 * theta * normalDir - lightDir;
				float gama = pow(max(0.0, dot(lightDirReflect, viewDir)), _Shininess);
				float3 diffColor = atten * _LightColor0.rgb * theta;
				float3 specColor = atten * _SpecColor.rgb * theta * gama;
				float total = diffColor + specColor + UNITY_LIGHTMODEL_AMBIENT;
				
				return float4(total * _Color.rgb, 1.0);
			}
			
			ENDCG
		}
	}
	Fallback "Diffuse"
}
