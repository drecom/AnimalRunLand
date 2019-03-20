// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Liver/MinimapTransparent" {
Properties {
	[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
	_Color ("Tint", Color) = (1,1,1,1)
    _MaskTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    
    _OffsetX ("Texture Offset", Float) = 0
    _OffsetY ("Texture Offset", Float) = 0
    _Scale   ("Texture Scale", Float)  = 1
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    
    Cull Off
    ZWrite Off
    Lighting Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float2 mask_texcoord : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            fixed4 _Color;
            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            
            float _OffsetX;
            float _OffsetY;
            float _Scale;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.mask_texcoord = (v.texcoord - float2(_OffsetX, _OffsetY)) * float2(_Scale, _Scale);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            	fixed4 mcol = tex2D(_MaskTex, i.mask_texcoord);
                fixed4 col  = tex2D(_MainTex, i.texcoord) * _Color * mcol;
                return col;
            }
        ENDCG
    }
}

}
