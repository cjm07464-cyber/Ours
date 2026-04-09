Shader "Custom/WhiteToTransparentSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Threshold ("White Threshold", Range(0.8, 1.0)) = 0.95
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Threshold;

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 흰색에 가까우면 투명 처리
                float brightness = (col.r + col.g + col.b) / 3.0;
                if (brightness > _Threshold)
                    col.a = 0;

                return col;
            }
            ENDCG
        }
    }
}