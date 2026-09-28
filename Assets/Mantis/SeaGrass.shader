Shader "Mantis/Sea Grass"
{
 Properties { _BaseColor("Color", Color)=(0.12,0.42,0.30,1) }
 SubShader {
 Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
 Pass {
 Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;
 CBUFFER_END
 struct A { float4 p:POSITION; float4 c:COLOR; };
 struct V { float4 p:SV_POSITION; float shade:TEXCOORD0; };
 V vert(A a) {
 V o; float3 p=a.p.xyz;
 p.x+=sin(_Time.y*.8+p.x*1.3+p.z)*.18*a.c.r*a.c.r;
 p.z+=cos(_Time.y*.63+p.x+p.z)*.09*a.c.r;
 o.p=TransformObjectToHClip(p); o.shade=.6+a.c.r*.5; return o;
 }
 half4 frag(V i):SV_Target {return half4(_BaseColor.rgb*i.shade,1);}
 ENDHLSL
 }
 }
}
