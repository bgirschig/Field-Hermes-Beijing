// Shadow passes based on only_shadows_and_atten.shader

Shader "Custom/textured_shadows"
{
    Properties
    {
        _shadowColor ("Shadow Color", Color) = (0,1,0,1)
        _litColor ("LitColor", Color) = (0,0,0,1)
        _patternSize ("pattern size", float) = 400
        _patternSpacing ("pattern spacing", float) = 2
    }
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		// main directional light pass.
		Pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				SHADOW_COORDS(0)
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				TRANSFER_SHADOW(o);
				return o;
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				UNITY_LIGHT_ATTENUATION(atten, IN, 0)
                return _LightColor0;
			}

			ENDCG

		}

		// Forward additive pass (only needed if you care about more lights than 1 directional).
		// Can remove if no point/spot light support needed.
		Pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardAdd" }
			ZWrite Off Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// Include shadowing support for point/spot
			#pragma multi_compile_fwdadd_fullshadows
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

            float4 _shadowColor;
            float4 _litColor;
            float _patternSize;
            float _patternSpacing;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD4;
				SHADOW_COORDS(1)
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.vertex+0.5;
				TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
				return o;
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				UNITY_LIGHT_ATTENUATION(atten, IN, IN.worldPos)
                float shadow_pattern = 1-smoothstep(0.4f, 0.5f, length(IN.uv * _patternSize % _patternSpacing - 0.5f));
                float brightness = step(0.99, 1-atten) * shadow_pattern;
		
                return lerp(_litColor, _shadowColor, brightness);
			}

			ENDCG
		}

		// Support for casting shadows from this shader. Remove if not needed.
		UsePass "VertexLit/SHADOWCASTER"
	}
}