// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Icon"
{
	Properties
	{
		_Icon("Icon", 2D) = "white" {}
		_Tiling("Tiling", Float) = 1
		_IconColor("Icon Color", Color) = (1,1,1,1)
		_BackgroundColor("Background Color", Color) = (0,0,0,1)
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _BackgroundColor;
		uniform sampler2D _Icon;
		uniform float _Tiling;
		uniform float4 _IconColor;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (_Tiling).xx;
			float2 uv_TexCoord15 = i.uv_texcoord * temp_cast_0;
			float4 tex2DNode1 = tex2D( _Icon, uv_TexCoord15 );
			float4 lerpResult17 = lerp( _BackgroundColor , ( tex2DNode1 * _IconColor ) , tex2DNode1.r);
			o.Albedo = lerpResult17.rgb;
			o.Smoothness = ( ( 1.0 - tex2DNode1 ) * _Smoothness ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	// CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
7;6;1905;1013;2022.096;451.4622;1.361747;True;True
Node;AmplifyShaderEditor.RangedFloatNode;16;-1232,-48;Inherit;False;Property;_Tiling;Tiling;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;15;-1088,-48;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;-784,112;Inherit;False;Property;_IconColor;Icon Color;2;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.85,0.85,0.85,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-864,-80;Inherit;True;Property;_Icon;Icon;0;0;Create;True;0;0;0;False;0;False;-1;None;2add9fc2fb4161f4ba53c9937ba6ed25;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;13;-576,272;Inherit;False;Property;_Smoothness;Smoothness;4;0;Create;True;0;0;0;False;0;False;0.5;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-560,-80;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;12;-480,48;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;2;-634,-260;Inherit;False;Property;_BackgroundColor;Background Color;3;0;Create;True;0;0;0;False;0;False;0,0,0,1;0.1372549,0.3372549,0.4470588,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;17;-352,-256;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-256,48;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-48,-256;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Icon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;15;0;16;0
WireConnection;1;1;15;0
WireConnection;5;0;1;0
WireConnection;5;1;3;0
WireConnection;12;0;1;0
WireConnection;17;0;2;0
WireConnection;17;1;5;0
WireConnection;17;2;1;1
WireConnection;14;0;12;0
WireConnection;14;1;13;0
WireConnection;0;0;17;0
WireConnection;0;4;14;0
ASEEND*/
//CHKSM=0374020A6EA2BE9D2FFCA8DFE62513FAA67874DB