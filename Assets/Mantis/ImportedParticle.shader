Shader "Mantis/ImportedParticle"
{
    Properties { _MainTex("Texture", 2D) = "white" {} _Tint("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST; half4 _Tint;
            CBUFFER_END
            struct Input { float4 position : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Output { float4 position : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            Output vert(Input v) { Output o; o.position = TransformObjectToHClip(v.position.xyz); o.color = v.color * _Tint; o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; return o; }
            half4 frag(Output i) : SV_Target { return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color; }
            ENDHLSL
        }
    }
}
