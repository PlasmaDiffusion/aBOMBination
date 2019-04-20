Shader "Custom/NormalSeeThrough"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		UsePass "Custom/SeeThrough/FORWARD"
		UsePass "Custom/SeeThrough/BASIC"
	}
}
