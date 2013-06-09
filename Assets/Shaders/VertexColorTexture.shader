Shader "Custom/VertexColorTexture" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
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
			//#include "UnityCG.cginc"
	
			sampler2D _MainTex;
	
			struct data {
				float4 vertex: POSITION;
				fixed4 color: COLOR;
				float2 texcoord : TEXCOORD0; // Texture coordinates
			};
			
			data vert(data v) {
				v.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return v;
			}
			
			fixed4 frag(data f) : COLOR {
				return f.color * tex2D(_MainTex, f.texcoord);
			}
			
			ENDCG
		}		
	} 
	FallBack "Diffuse"
}
