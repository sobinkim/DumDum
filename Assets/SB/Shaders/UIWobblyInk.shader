Shader "UI/WobblyInk"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _WobbleStrength ("Wobble Strength", Range(0, 0.03)) = 0.006
        _WobbleSpeed ("Wobble Speed", Range(0, 10)) = 2.2
        _WobbleScale ("Wobble Scale", Range(1, 80)) = 28
        _InkThreshold ("Ink Threshold", Range(0, 1)) = 0.28
        _InkSoftness ("Ink Softness", Range(0.001, 0.5)) = 0.18
        _PhaseOffset ("Phase Offset", Range(0, 20)) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _WobbleStrength;
            float _WobbleSpeed;
            float _WobbleScale;
            float _InkThreshold;
            float _InkSoftness;
            float _PhaseOffset;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            float2 BuildWobble(float2 uv)
            {
                float t = _Time.y * _WobbleSpeed + _PhaseOffset;
                float waveA = sin((uv.y * _WobbleScale) + t);
                float waveB = sin((uv.x * (_WobbleScale * 0.73)) - (t * 1.37));
                float waveC = sin(((uv.x + uv.y) * (_WobbleScale * 0.47)) + (t * 0.61));
                return float2(waveA + waveC * 0.45, waveB - waveC * 0.35) * _WobbleStrength;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 baseColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                float brightestChannel = max(baseColor.r, max(baseColor.g, baseColor.b));
                float inkMask = 1.0 - smoothstep(_InkThreshold, _InkThreshold + _InkSoftness, brightestChannel);
                inkMask *= baseColor.a;

                float2 wobbleUv = IN.texcoord + BuildWobble(IN.texcoord) * inkMask;
                fixed4 wobbledColor = (tex2D(_MainTex, wobbleUv) + _TextureSampleAdd) * IN.color;
                fixed4 color = lerp(baseColor, wobbledColor, inkMask);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
