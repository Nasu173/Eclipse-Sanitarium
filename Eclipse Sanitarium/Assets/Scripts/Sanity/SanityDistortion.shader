Shader "Hidden/SanityDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Distortion ("Distortion Intensity", Range(-2, 2)) = 0
        _Vignette ("Vignette Intensity", Range(0, 2)) = 0
        _Chromatic ("Chromatic Aberration", Range(0, 0.2)) = 0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Distortion;
            float _Vignette;
            float _Chromatic;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 distort(float2 uv, float strength)
            {
                float2 d = uv - 0.5;
                float r2 = dot(d, d);
                return uv + d * (strength * r2);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                fixed4 col;
                // 色彩分离 + 扭曲
                col.r = tex2D(_MainTex, distort(uv, _Distortion + _Chromatic)).r;
                col.g = tex2D(_MainTex, distort(uv, _Distortion)).g;
                col.b = tex2D(_MainTex, distort(uv, _Distortion - _Chromatic)).b;
                col.a = 1.0;

                // 强制加黑效果 (更加激进)
                float2 dist = (i.uv - 0.5) * 1.5;
                float vig = 1.0 - dot(dist, dist) * _Vignette;
                vig = saturate(vig);
                
                col.rgb *= vig;

                return col;
            }
            ENDCG
        }
    }
}
