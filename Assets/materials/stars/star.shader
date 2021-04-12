Shader "Unlit/star"
{
    Properties {
        _MainColor ("Color", Color) = (1,1,1,1)
        fogDensity ("Fog Density", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _MainColor;
            float fogDensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = i.vertex.z * _ProjectionParams.z;
                float fog = smoothstep(fogDensity, 0.0, dist);

                return lerp(_MainColor, float4(0,0,0,1), fog);
            }
            ENDCG
        }
    }
}
