Shader "Custom/Clearer" {
	Properties {}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 200
		
		Pass
		{
			//Blend SrcAlpha OneMinusSrcAlpha
			//Cull off
			//Lighting off
			//ZWrite off
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
		
			struct data {
				float4 vertex: POSITION;
			};
			
			data vert(data v) {
				//v.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return v;
			}
			
			fixed4 frag(data f) : COLOR {
				return fixed4(0.0, 0.0, 0.0, 0.0);
			}
			
			ENDCG
		}		
	} 
	FallBack "Diffuse"
}
