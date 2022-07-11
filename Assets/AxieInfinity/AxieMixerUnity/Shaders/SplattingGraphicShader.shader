﻿// Upgrade NOTE: upgraded instancing buffer 'InstanceProperties' to new syntax.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Texture Splatting Graphic Palette" {

	Properties {
		[NoScaleOffset] _MainTex ("Main Tex", 2D) = "white" {}
		[NoScaleOffset] _LineTex ("Line Tex", 2D) = "(0,0,0,0)" {}
		[NoScaleOffset] _Splat0Tex ("Splat0 Tex", 2D) = "(0,0,0,0)" {}
		[NoScaleOffset] _Splat1Tex ("Splat1 Tex", 2D) = "(0,0,0,0)" {}
		[NoScaleOffset] _SwapTex ("Swap Tex", 2D) = "(0,0,0,0)" {}

		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector][Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255

		[HideInInspector] _ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	SubShader {
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass {
			Name "Normal"

			CGPROGRAM
			// #pragma multi_compile_instancing
			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _LineTex, _Splat0Tex, _Splat1Tex, _SwapTex;
			
			struct VertexData {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
				// UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			Interpolators MyVertexProgram (VertexData v) {
				Interpolators i;
				UNITY_INITIALIZE_OUTPUT(Interpolators, i);
				// UNITY_SETUP_INSTANCE_ID(v);
				// UNITY_TRANSFER_INSTANCE_ID(v, i);

				i.position = UnityObjectToClipPos(v.position);
				i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				i.vertexColor = v.vertexColor;
				return i;
			}

			float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
				//UNITY_SETUP_INSTANCE_ID(i);

				/*float4 dstColor1 = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DstColor1);
				float4 dstColor2 = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DstColor2);
				float4 dstColor3 = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DstColor3);
				float4 dstColor4 = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DstColor4);
				float4 dstColor5 = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DstColor5);*/

				float4 dstColor1 = tex2D(_SwapTex, float2(i.vertexColor.r,  0.0 / 8.0 + i.vertexColor.g * 256.0 / 8.0));
				float4 dstColor2 = tex2D(_SwapTex, float2(i.vertexColor.r,  1.0 / 8.0 + i.vertexColor.g * 256.0 / 8.0));
				float4 dstColor3 = tex2D(_SwapTex, float2(i.vertexColor.r,  2.0 / 8.0));
				float4 dstColor4 = tex2D(_SwapTex, float2(i.vertexColor.r,  3.0 / 8.0));
				float4 dstColor5 = tex2D(_SwapTex, float2(i.vertexColor.r,  4.0 / 8.0));

				float4 mainColor = tex2D(_MainTex, i.uv);
				float4 lineColor = tex2D(_LineTex, i.uv);

				float3 splat0Color = tex2D(_Splat0Tex, i.uv).rgb;
				float3 splat1Color = tex2D(_Splat1Tex, i.uv).rgb;

				float3 color = float3(0, 0, 0);

				float mainBackColorA = mainColor.a * splat0Color.b;
				float mainColorA = mainColor.a * (1 - splat0Color.b);

				color = color * (1 - mainBackColorA) + mainColor.rgb * mainBackColorA;
				color = color * (1 - splat0Color.r) + dstColor1 * splat0Color.r;
				color = color * (1 - splat0Color.g) + dstColor2 * splat0Color.g;
				color = color * (1 - splat1Color.b) + dstColor4 * splat1Color.b;
				color = color * (1 - splat1Color.g) + dstColor3 * splat1Color.g;
				color = color * (1 - splat1Color.r) + dstColor4 * splat1Color.r;
				color = color * (1 - mainColorA) + mainColor.rgb * i.vertexColor.a * mainColorA;
				color = color * (1 - lineColor.a) + lineColor.rgb * dstColor5 * lineColor.a;
				
				float4 baseColor = 0;
				baseColor.rgb = color;
				baseColor.a = clamp(mainBackColorA + splat0Color.r + splat0Color.g + splat1Color.g + splat1Color.b + splat1Color.r + mainColorA + lineColor.a, 0, 1) * i.vertexColor.a;
				#ifdef UNITY_UI_ALPHACLIP
				clip (baseColor.a - 0.001);
				#endif
				return baseColor;
				
			}

			ENDCG
		}
	}
}
