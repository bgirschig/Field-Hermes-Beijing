// Draws a Colored silhouette of a mesh
// Used for effect-masking by the BlitEffect script
Shader "Hidden/silhouette"
{
    Properties
    {
        _mainColor ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {

        Pass
        {            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _mainColor = float4(1,1,1,1);

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _mainColor;
            }
            ENDCG
        }
    }
}
