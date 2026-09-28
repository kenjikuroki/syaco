Shader "Mantis/Shallow Water Sand"
{
 Properties { _BaseColor("Sand", Color)=(0.62,0.66,0.48,1) }
 SubShader {
 Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
 Pass {
 Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;
 CBUFFER_END
 struct A { float4 p:POSITION; float3 n:NORMAL; };
 struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; float3 local:TEXCOORD1; float3 normal:TEXCOORD2; };
 V vert(A a) { V o; o.p=TransformObjectToHClip(a.p.xyz); o.world=TransformObjectToWorld(a.p.xyz); o.local=a.p.xyz; o.normal=TransformObjectToWorldNormal(a.n); return o; }
 half4 frag(V i):SV_Target {
 float2 p=i.local.xz;
 float ripple=sin(p.x*15+sin(p.y*1.7)*1.5)*.035;
 float grain=frac(sin(dot(floor(p*160),float2(127.1,311.7)))*43758.5453)*.035;
 float t=_Time.y*.22;
 float a=sin(p.x*2.3+sin(p.y*2.1+t))+sin(p.y*2.8+sin(p.x*1.7-t));
 float b=sin(p.x*2.9-sin(p.y*1.8-t))+sin(p.y*2.2+sin(p.x*2.3+t));
 float caustic=pow(saturate(1-abs(a)*3),5)*.13+pow(saturate(1-abs(b)*3),5)*.08;
 Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
 float lighting=.62+.38*saturate(dot(normalize(i.normal),sun.direction))*sun.shadowAttenuation;
 float depth=smoothstep(5,14,length(p));
 float3 sand=(_BaseColor.rgb+ripple+grain)*lighting+caustic*float3(.65,1,.9)*sun.shadowAttenuation;
 return half4(lerp(sand,float3(.10,.32,.34),depth*.72),1);
 }
 ENDHLSL
 }
 }
}
