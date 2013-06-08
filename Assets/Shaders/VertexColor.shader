Shader "Custom/VertexColor" {
	Properties {}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 200
		
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			//Cull off
			//Lighting off
			//ZWrite off
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
		
			struct data {
				float4 vertex: POSITION;
				fixed4 color: COLOR;
			};
			
			data vert(data v) {
				v.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return v;
			}
			
			fixed4 frag(data f) : COLOR {
				return f.color;
			}
			
			ENDCG
		}		
	} 
	FallBack "Diffuse"
}
