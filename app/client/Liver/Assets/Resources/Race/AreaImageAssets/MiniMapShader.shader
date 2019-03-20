// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Liver/MiniMap" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    
    _GroundColor ("Ground", Color) = (1, 1, 1, 1)
    _PathColor ("Path", Color)  = (1, 1, 1, 1)
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // 地面の色と道の色
            fixed4 _GroundColor;
            fixed4 _PathColor;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                // Alpha値→RGB値
                // NOTE 元画像はAlha値0で塗り潰してからレンダリングしている
                //      何かしら物体がある場合はAlpha値が0以外になる
                
                // ２つの色を線形補間
                return lerp(_GroundColor, _PathColor, col.a);
            }
        ENDCG
    }
}

}
