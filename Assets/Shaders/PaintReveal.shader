// This shader may be applied on painting stuff
Shader "Custom/PaintReveal" {
	Properties {
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_Color ("Tint color (_Color)", Color) = (1.0, 1.0, 1.0, 1.0)
		_Spread ("Spread effect (_Spread, 0=max, 1=min)", float) = 1.0
		_Coeff ("Spread coeff (_Coeff, 1=disabled, usually 4)", float) = 4.0
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
			half4 _Color;
			half _Spread;
			half _Coeff;
			
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
				o.vcolor = v.color * _Color;
				return o;
			}
			
			half4 frag(VertexOutput i): COLOR
			{
				// Get some variables
				half4 o = tex2D(_MainTex, i.uv) * i.vcolor; // Color of the paint texture
				half2 screenPos01 = (i.screenPos.xy / i.screenPos.w + 1) * 0.5; // From [-1,1] to [0,1]
				half4 mask = tex2D(_HiddenPaint, screenPos01); // Mask from hidden ink
				
				// Spread effect
				// TODO : this makes pixelpaint disappear, need to investigate
//				half s = clamp(_Spread, 0, 1); // TODO remove it on final prod
//				o.a = clamp(_Coeff * (o.a - s) - s, 0, 1);
				
				// Reveal effect
				o.a = o.a * (1f - max(mask.r, max(mask.g, mask.b)));
				//o.a = o.a * (1f - max(o.r * mask.r, max(o.g * mask.g, o.b * mask.b)));

				return o;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse"
}
