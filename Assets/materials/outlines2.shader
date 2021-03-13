    Shader "Outlined/ShadowSensitiveOutline" {
     
        Properties
        {
     
            _Color("Color", Color) = (1, 1, 1, 1)
            _Glossiness("Smoothness", Range(0, 1)) = 0.5
            _Metallic("Metallic", Range(0, 1)) = 0
     
            _OutlineWidth("Outline Width", Range(0, 10)) = 0.03
       
     
        }
     
        Subshader
        {
     
        Tags
        {
            "RenderType" = "Opaque"
        }
     
        CGPROGRAM
            //Basic Surface Shader
            #pragma surface surf Standard fullforwardshadows
     
            struct Input
            {
                float4 color : COLOR;
            };
     
            half4 _Color;
            half _Glossiness;
            half _Metallic;
     
            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                o.Albedo = _Color.rgb * IN.color.rgb;
                o.Smoothness = _Glossiness;
                o.Metallic = _Metallic;
                o.Alpha = _Color.a * IN.color.a;
            }
     
     
        ENDCG
            //Outline Pass
            Pass{
                Tags{ "LightMode" = "ForwardBase" }
                Cull Front
     
                CGPROGRAM
     
                    #pragma vertex vert
                    #pragma fragment frag
     
                    #include "UnityCG.cginc"
                    #include "Lighting.cginc"
     
                    #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
                    #include "AutoLight.cginc"
     
                    struct v2f
                    {
                        float4 pos : SV_POSITION;
                        SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                    };
     
                    half _OutlineWidth;
                    v2f vert(float4 position : POSITION, float3 normal : NORMAL)
                    {
                       
                        v2f o;
     
                        //Send the base vertex positions to frag for sampling
                        o.pos = UnityObjectToClipPos(position);
     
                        float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M, normal));
                        //Extrude in 2 dimensions, normalizing x and y and counter foreshadowing with .w and adjust for aspect ratio
                        float2 offset = normalize(clipNormal.xy) / _ScreenParams.xy * _OutlineWidth * o.pos.w * 2;
                        float2 inset = normalize(clipNormal.xy) / _ScreenParams.xy * 1 * o.pos.w * 2;
     
                        //1 pixel inset
                        o.pos.xy -= inset;
                        TRANSFER_SHADOW(o)
     
     
                        //Add the outline offsets after the shadows have been sampled
                        o.pos.xy += offset;
                        return o;
                    }
     
                    fixed4 frag(v2f i) : SV_TARGET
                    {
                        // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                        fixed shadow = SHADOW_ATTENUATION(i);
                        //Use Step(x,y) instead of if statments
                        return fixed4(step(shadow, 0.5), step(shadow, 0.5), step(shadow, 0.5), 1.0);
                    }
     
            ENDCG
            }
            // shadow casting support
            UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
        }
       
    }
