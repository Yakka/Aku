// This shader may be applied on painting stuff
Shader "Custom/TriPaintReveal" {
	Properties {
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_Color1 ("Color1", Color) = (1.0, 0.0, 0.0) 
		_Color2 ("Color2", Color) = (0.0, 1.0, 0.0) 
		_Color3 ("Color3", Color) = (0.0, 0.0, 1.0) 
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent"}
		LOD 200
		
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
	
			sampler2D _MainTex;
			sampler2D _HiddenPaint;
			half4 _Color1;
			half4 _Color2;
			half4 _Color3;
			
			struct VertexOutput
			{
				float4 pos : SV_POSITION; // Position on screen
				float2 uv : TEXCOORD0; // Texture coordinates
				float4 screenPos : TEXCOORD1; // Readable position on screen for texturing
				float4 vcolor : TEXCOORD2; // Color
			};
			
			VertexOutput vert(appdata_full v)
			{
				VertexOutput o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				o.screenPos = o.pos;
				o.vcolor = v.color;
				return o;
			}
			
			half4 frag(VertexOutput i): COLOR
			{
				// Get pixel position (values converted from [-1,1] to [0,1] intervals)
				half2 screenPos01 = (i.screenPos.xy / i.screenPos.w + 1) * 0.5; 

				// Color of the source pixel from paint texture and vertex color : 
				// expected to be R, G or B only 
				half4 src = tex2D(_MainTex, i.uv) * i.vcolor;
				
				// Get color of the same pixel from hidden space
				half4 mask = tex2D(_HiddenPaint, screenPos01);
				
				// Get difference between hidden and visible world colors
				// Note : src may exclusively be composed of R xor G xor B.
				half4 tmp = src - mask;
				
				// Map final color
				half4 o = _Color1*src.r + _Color2*src.g + _Color3*src.b;
				
				// Hide stuff
				o.a = src.a * max(tmp.r, max(tmp.g, tmp.b));
				
				return o;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse"
}
