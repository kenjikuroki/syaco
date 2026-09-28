Shader "Mantis/Distant Reef" {
 Properties { _BaseColor("Color",Color)=(.25,.43,.40,1) }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;
 CBUFFER_END
 struct A { float4 p:POSITION; float3 n:NORMAL; };
 struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; };
 V vert(A a) { V o; o.p=TransformObjectToHClip(a.p.xyz); o.world=TransformObjectToWorld(a.p.xyz); o.normal=TransformObjectToWorldNormal(a.n); return o; }
 half4 frag(V i):SV_Target {
 float haze=smoothstep(14,58,distance(i.world,_WorldSpaceCameraPos));
 float light=.72+.28*saturate(dot(normalize(i.normal),normalize(float3(-.3,.8,-.4))));
 return half4(lerp(_BaseColor.rgb*light,float3(.11,.37,.43),haze*.96),1);
 }
 ENDHLSL
 }
 }
}
