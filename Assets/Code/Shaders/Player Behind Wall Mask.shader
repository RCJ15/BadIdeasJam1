Shader "Custom/Player Behind Wall Mask"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay+2"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Lighting Off
        ZWrite Off
        ZTest Always
        Blend One OneMinusSrcAlpha
        
        Pass
        {
        CGPROGRAM
            #include "UnitySprites.cginc"

            fixed4 CoolFrag(v2f IN) : SV_Target
            {
                fixed4 s = SampleSpriteTexture(IN.texcoord);

                fixed4 c = IN.color;
                c.rgb *= c.a;
                c *= s.a;

                if (s.a <= 0) {
                    return c;
                }

                float light = (s.r + s.g + s.b) / 3;

                fixed4 c2 = c * 2;

                return lerp(c2, c, light);
            }

            #pragma vertex SpriteVert
            #pragma fragment CoolFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        ENDCG
        }
    }
}