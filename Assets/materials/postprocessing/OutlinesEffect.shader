// Applies an outline effect to the _MainTex.
// Requires DepthNormals to be enabled on the camera (so that _CameraDepthNormalsTexture is available)

Shader "Postprocessing/OutlinesEffect"
{
    Properties
    {
        [HideInInspector]
        _MainTex ("Texture", 2D) = "white" {}
        _noise ("Noise texture", 2D) = "white" {}
        _noiseAmount ("Noise amount", Range(0,1)) = 0.1
        [Space]
        _lineColor ("Line color", Color) = (1,1,1,1)
        _lineThickness("Line thickness", Range(0, 10)) = 1
        [Space]
        _brightnessOutMin ("Brightness min", Range(0,1)) = 0.6
        _brightnessOutMax ("Brightness max", Range(0,5)) = 1
        [Space]
        _brigtnessInfluenceOnLine ("Influence of Brightness on Outline", Range(0,1)) = 1
        _normalInfluenceOnLine ("Influence of Normals on Outline", Range(0,1)) = 1
        _depthInfluenceOnLine ("Influence of Depth on Outline", Range(0,1)) = 1
    }
    SubShader
    {

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _noise;
            sampler2D _SelectionBuffer;
            sampler2D _CameraDepthNormalsTexture;
            float _noiseAmount;
            float4 _CameraDepthNormalsTexture_TexelSize;
            float4 _lineColor;
            float _colorOffset;
            float _brigtnessInfluenceOnLine;
            float _normalInfluenceOnLine;
            float _depthInfluenceOnLine;
            float _lineThickness;

            float _brightnessOutMin;
            float _brightnessOutMax;

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            void Compare(
                    inout float depthOutline, inout float normalOutline, inout float brigthness,
                    float baseDepth, float3 baseNormal, float baseBrightness,
                    float2 uv, float2 offset
                ) {
                // depth and normal (encoded in the same vector by unity)
                float4 neighborDepthnormal = tex2D(_CameraDepthNormalsTexture, uv + _CameraDepthNormalsTexture_TexelSize.xy * offset * _lineThickness);
                float3 neighborNormal;
                float neighborDepth;
                DecodeDepthNormal(neighborDepthnormal, neighborDepth, neighborNormal);
                neighborDepth = neighborDepth * _ProjectionParams.z;

                // color                
                float4 neighborColor = tex2D(_MainTex, uv + _CameraDepthNormalsTexture_TexelSize.xy * offset * _lineThickness);
                float beighborBrightness = neighborColor.r*.3 + neighborColor.g*.59 + neighborColor.b*.11;

                float depthDifference = baseDepth - neighborDepth;
                float normalDifference = 1 - dot(neighborNormal, baseNormal);
                float colorDifference = smoothstep(0.05, 0.1, baseBrightness - beighborBrightness);

                depthOutline = depthOutline + depthDifference;
                normalOutline = normalOutline + normalDifference;
                brigthness = brigthness + colorDifference;
            }

            float Edges(float2 uv, float sourceBrightness) {
                float4 depthnormal = tex2D(_CameraDepthNormalsTexture, uv);
                float3 normal;
                float depth;
                DecodeDepthNormal(depthnormal, depth, normal);
                depth = depth * _ProjectionParams.z;

                float depthOutline = 0;
                float normalOutline = 0;
                float colorOutline = 0;
                Compare(depthOutline, normalOutline, colorOutline, depth, normal, sourceBrightness, uv, float2(1, 0));
                Compare(depthOutline, normalOutline, colorOutline, depth, normal, sourceBrightness, uv, float2(0, 1));
                Compare(depthOutline, normalOutline, colorOutline, depth, normal, sourceBrightness, uv, float2(-1, 0));
                Compare(depthOutline, normalOutline, colorOutline, depth, normal, sourceBrightness, uv, float2(0, -1));

                float outline = saturate(
                    saturate(colorOutline * _brigtnessInfluenceOnLine) +
                    saturate(normalOutline * _normalInfluenceOnLine) +
                    saturate(depthOutline * _depthInfluenceOnLine));
                return outline;
            }

            float map(float value, float min1, float max1, float min2, float max2) {
               return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 sourceColor = tex2D(_MainTex, i.uv);
                float brightness = sourceColor.r*.3 + sourceColor.g*.59 + sourceColor.b*.11;

                float outline = Edges(i.uv, brightness);
                float4 output = lerp(map(brightness, 0, 1, _brightnessOutMin, _brightnessOutMax), _lineColor, outline);

                float4 noise = tex2D(_noise, i.uv);
                output = lerp(output, noise*output, _noiseAmount);

                float selection = tex2D(_SelectionBuffer, i.uv);
                float4 masked = lerp(sourceColor, output, selection);
                return masked;
            }

            ENDCG
        }
    }
}
