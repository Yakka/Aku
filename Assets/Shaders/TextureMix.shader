Shader "Blend 2 by Vertex Alpha" {
 
	Properties
	{
	   _MainTex ("Texture 1  (vertex A  = white)", 2D) = ""
	   _Texture2 ("Texture 2  (vertex A = black)", 2D) = ""
	}
	 
	SubShader
	{
	   BindChannels
	   {
	      Bind "vertex", vertex
	      Bind "color", color
	      Bind "texcoord", texcoord
	   }
	   
	   Pass
	   {
	      SetTexture [_MainTex]
	      SetTexture [_Texture2] {combine previous lerp(primary) texture}
	   }
	}
 
}