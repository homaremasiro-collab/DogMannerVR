// Copyright (c) 2022 momoma
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

Shader "MomomaShader/LookAtBillboard/Surface_Cutout"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset][Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpScale ("Normal Scale", Float) = 1.0
		[NoScaleOffset] _MetallicGlossMap ("Mask Map (MOXS)", 2D) = "white" {}
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_OcclusionStrength ("Occlusion", Range(0,1)) = 1.0
		[ToggleUI] _XRotaion ("X Rotation", Float ) = 0
		_ClipAlpha ("Clip Alpha", Range(0.0,1.0)) = 0.5
	}
	Subshader
	{
		Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" "DisableBatching" = "True" }

		AlphaToMask On

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows addshadow vertex:vert alphatest:_
		#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		UNITY_DECLARE_TEX2D(_MainTex);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicGlossMap);

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP(half, _BumpScale)
		UNITY_DEFINE_INSTANCED_PROP(half, _Metallic)
		UNITY_DEFINE_INSTANCED_PROP(half, _Glossiness)
		UNITY_DEFINE_INSTANCED_PROP(half, _OcclusionStrength)
		UNITY_DEFINE_INSTANCED_PROP(half, _ClipAlpha)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		UNITY_DEFINE_INSTANCED_PROP(fixed, _XRotaion)
		UNITY_INSTANCING_BUFFER_END(Props)

		#include "MS_LookAtBillboard.cginc"

		void surf (in Input IN, inout SurfaceOutputStandard o)
		{
			float4 c = UNITY_SAMPLE_TEX2D(_MainTex, IN.uv_MainTex);
			c *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
			o.Albedo = c.rgb;
			o.Alpha = saturate((c.a - UNITY_ACCESS_INSTANCED_PROP(Props, _ClipAlpha)) / max(fwidth(c.a), 0.0001) + 0.5);
			o.Normal = UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, IN.uv_MainTex), UNITY_ACCESS_INSTANCED_PROP(Props, _BumpScale));
			float4 moxg = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap, _MainTex, IN.uv_MainTex);
			o.Metallic = moxg.r * UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic);
			o.Occlusion = LerpOneTo(moxg.g, UNITY_ACCESS_INSTANCED_PROP(Props, _OcclusionStrength));
			o.Smoothness = moxg.a * UNITY_ACCESS_INSTANCED_PROP(Props, _Glossiness);
		}
		ENDCG
	}
	FallBack "Standard"
}