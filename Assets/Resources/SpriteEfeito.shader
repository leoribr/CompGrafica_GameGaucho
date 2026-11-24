Shader "GameGaucho/SpriteEfeito"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura", 2D) = "white" {}
        _CorContorno ("Cor do contorno", Color) = (1, 0.86, 0.3, 1)
        _Contorno ("Contorno", Float) = 0
        _Branco ("Branco", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "CanUseSpriteAtlas" = "True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _CorContorno;
            float _Contorno;
            float _Branco;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 cor = tex2D(_MainTex, i.uv) * i.color;
                if (_Contorno > 0.5 && cor.a < 0.1)
                {
                    float2 d = _MainTex_TexelSize.xy;
                    float vizinhos = tex2D(_MainTex, i.uv + float2(d.x, 0)).a + tex2D(_MainTex, i.uv - float2(d.x, 0)).a
                        + tex2D(_MainTex, i.uv + float2(0, d.y)).a + tex2D(_MainTex, i.uv - float2(0, d.y)).a;
                    if (vizinhos > 0.1)
                        return fixed4(_CorContorno.rgb, _CorContorno.a * i.color.a);
                }
                cor.rgb = lerp(cor.rgb, fixed3(1, 1, 1), _Branco);
                return cor;
            }
            ENDCG
        }
    }
}
