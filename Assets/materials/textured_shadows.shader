// Shadow passes based on only_shadows_and_atten.shader

Shader "Custom/textured_shadows"
{
    Properties
    {
        _shadowColor ("Shadow Color", Color) = (0,1,0,1)
        _litColor ("LitColor", Color) = (0,0,0,1)
		_MainTex ("Shadow texture", 2D) = "white" {}
		[Toggle(SCREEN_SPACE_COORDINATES)]
        _ScreenSpaceCoordinates ("Screen space", Float) = 0
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
			// Toggle between texture modes: world space and screen space
			#pragma shader_feature SCREEN_SPACE_COORDINATES

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

            float4 _shadowColor;
            float4 _litColor;
			sampler2D _MainTex;
            float4 _MainTex_ST;
			float2 _MainTex_TexelSize;

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
				#ifdef SCREEN_SPACE_COORDINATES
					fixed4 shadow_texture = tex2D(_MainTex, TRANSFORM_TEX(IN.pos.xy * _MainTex_TexelSize, _MainTex));
				#else
					float2 tex_uvs = IN.worldPos.xz * 0.2;
					fixed4 shadow_texture = tex2D(_MainTex, TRANSFORM_TEX(tex_uvs, _MainTex));
				#endif

				UNITY_LIGHT_ATTENUATION(atten, IN, IN.worldPos)
                float brightness = step(0.9, 1-atten) * shadow_texture;
		
                return lerp(_litColor, _shadowColor, brightness);
                return lerp(_litColor, _shadowColor, atten);
			}

			ENDCG
		}

		// Support for casting shadows from this shader. Remove if not needed.
		UsePass "VertexLit/SHADOWCASTER"
	}
}