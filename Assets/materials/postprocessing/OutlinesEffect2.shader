// Applies an outline effect to the _MainTex.
// Requires DepthNormals to be enabled on the camera (so that _CameraDepthNormalsTexture is available)

Shader "Postprocessing/OutlinesEffect2"
{
    Properties
    {
        [HideInInspector]
        _MainTex ("Texture", 2D) = "white" {}

        [Header(General)]
        _reveal("reveal", Range(0, 1)) = 1

        [Space]
        [Header(Line)]
        _lineColor ("Line color", Color) = (1,1,1,1)
        _lineThickness("Line thickness", Range(0, 2)) = 1
        _stepa("bightness min", Range(0, 1)) = 1
        _stepb("brightness max", Range(0, 1)) = 1

        [Space]
        [Header(Levels)]
        _levelInLow("In low", Range(0, 1)) = 0
        _levelInHigh("In high", Range(0, 1)) = 1
        _levelOutLow("Out low", Range(0, 1)) = 0
        _levelOutHigh("Out high", Range(0, 1)) = 1

        [Space]
        [Header(Nois)]
        _noise ("Noise texture", 2D) = "white" {}
        _noiseAmount ("Noise strength", Range(0,1)) = 0.1
        _noiseScale ("Noise scale", Range(0,10)) = 1
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
            sampler2D _SelectionBuffer;

            float4 _MainTex_TexelSize;
            float _reveal;
            // line
            float4 _lineColor;
            float _lineThickness;
            float _stepa;
            float _stepb;
            // levels
            float _levelInLow;
            float _levelInHigh;
            float _levelOutLow;
            float _levelOutHigh;
            // noise
            sampler2D _noise;
            float2 _noise_TexelSize;
            float _noiseAmount;
            float _noiseScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float getBrightness(float4 color) {
                return saturate(color.r*.3 + color.g*.59 + color.b*.11);
            }

            void Compare(inout float colorOutline, float baseBrightness, float2 uv, float2 offset, float factor) {
                // color                
                float4 neighborColor = tex2D(_MainTex, uv + _MainTex_TexelSize.xy * offset * _lineThickness);
                float neighborBrightness = getBrightness(neighborColor);
                float colorDifference = baseBrightness - neighborBrightness;
                colorOutline = colorOutline + colorDifference * factor;
                // colorOutline = neighborBrightness;
            }

            float Edges(float2 uv, float sourceBrightness) {
                float colorOutline = 0;

                Compare(colorOutline, sourceBrightness, uv, float2(1, 0), 1);
                Compare(colorOutline, sourceBrightness, uv, float2(2, 0), .75);
                Compare(colorOutline, sourceBrightness, uv, float2(-1, 0), -1);
                Compare(colorOutline, sourceBrightness, uv, float2(-2, 0), -.75);

                Compare(colorOutline, sourceBrightness, uv, float2(0, 1), 1);
                Compare(colorOutline, sourceBrightness, uv, float2(0, 2), .75);
                Compare(colorOutline, sourceBrightness, uv, float2(0, -1), -1);
                Compare(colorOutline, sourceBrightness, uv, float2(0, -2), -.75);

                colorOutline = smoothstep(_stepa, _stepb, colorOutline / 6);
                return colorOutline;
            }

            float map(float value, float min1, float max1, float min2, float max2) {
               return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 sourceColor = tex2D(_MainTex, i.uv);
                float brightness = getBrightness(sourceColor);
                if (i.uv.y > _reveal) return sourceColor;
                // return brightness;

                float outline = Edges(i.uv, brightness);
                float outputBrightness = map(saturate(brightness), _levelInLow, _levelInHigh, _levelOutLow, _levelOutHigh);
                float4 output = lerp(outputBrightness, _lineColor, outline);

                // add noise
                float2 screenUV = i.uv.xy * _ScreenParams.xy * _noise_TexelSize.xy / _noiseScale;
                float4 noise = tex2D(_noise, screenUV);
                output = lerp(output, noise*output, _noiseAmount);

                float selection = tex2D(_SelectionBuffer, i.uv);
                return lerp(sourceColor, output, selection);

            }

            ENDCG
        }
    }
}
