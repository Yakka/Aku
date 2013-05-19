// This shader expands 3D objects by using their normals.
// Currently only a test.
Shader "Custom/Expander" {
	Properties {
		_Color ("Tint color (_Color)", Color) = (1.0, 1.0, 1.0, 1.0)
		_Expand ("Expand factor (_Expand)", float) = 0
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent"}
		LOD 200
		
		Pass
		{
			//Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
	
			half4 _Color;
			half _Expand;
			
			struct VertexOutput
			{
				float4 pos : SV_POSITION; // Position on screen
				float4 vcolor : TEXCOORD2; // Color
			};
			
			VertexOutput vert(appdata_full v)
			{
				VertexOutput o;
				float4 pos = v.vertex + _Expand*float4(v.normal.xyz,1);
				o.pos = mul (UNITY_MATRIX_MVP, pos);
				o.vcolor = v.color * _Color;
				return o;
			}
			
			half4 frag(VertexOutput i): COLOR
			{
				return i.vcolor;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse"
}
