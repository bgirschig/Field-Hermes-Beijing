Shader "Hidden/Monochrome_outlines" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _intensity ("intensity", Range (0, 1)) = 1
        _lineThickness("Line thickness", Range(0,3)) = 1
        _EdgeThreshold("Edge threshold", Range(0,1)) = 0.9
        _lineColor("Line color", Color) = (0,0,0)
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            // #include "SimplexNoise2D.cginc"

            uniform sampler2D _MainTex;
            uniform float _intensity;
            uniform float _lineThickness;
            uniform float3 _lineColor;
            uniform float _EdgeThreshold;

            float sampleEdge(sampler2D tex, float2 uv, float Offset) {
                
                float mi = 1000.;
                float ma = -100.;
                float x, y;
                for (y = -1.; y <= 1.; y += 1.0)
                {
                    for (x = -1.; x <= 1.; x += 1.0)
                    {
                        float offsets = Offset / _ScreenParams.xy;
 
                        float v = tex2D(tex, uv + float2(x, y)*offsets );
                        mi = min(v, mi);
                        ma = max(v, ma);
                    }
                }
 
                return abs(ma - mi);
            }
 
            float map(float value, float min1, float max1, float min2, float max2) {
               return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
            }

            float4 frag(v2f_img i) : COLOR {
                float4 source_color = tex2D(_MainTex, i.uv);

                // get the source brightness
                float birghtness = source_color.r*.3 + source_color.g*.59 + source_color.b*.11;
                birghtness = map(birghtness, 0,1, 0.8,1);
                float4 output = float4( birghtness, birghtness, birghtness, source_color.a ); 
                
                // compute and apply edge
                float edge_mask = sampleEdge(_MainTex, i.uv, _lineThickness);
                output.rgb = lerp(output.rgb, _lineColor, edge_mask);
                
                // tune the effect intensity
                output.rgb = lerp(source_color.rgb, output.rgb, _intensity);

                return output;
            }
            ENDCG
        }
    }
}