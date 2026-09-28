Shader "Mantis/Water Sky" {
 SubShader { Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
 Cull Off ZWrite Off
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct V { float4 p:SV_POSITION; float3 direction:TEXCOORD0; };
 V vert(float4 p:POSITION) { V o; o.p=UnityObjectToClipPos(p); o.direction=p.xyz; return o; }
 fixed4 frag(V i):SV_Target {
 float y=normalize(i.direction).y;
 float3 color=lerp(float3(.06,.22,.29),float3(.11,.37,.43),smoothstep(-.5,0,y));
 color=lerp(color,float3(.38,.72,.76),smoothstep(0,.85,y));
 return fixed4(color,1);
 }
 ENDCG
 }
 }
}
