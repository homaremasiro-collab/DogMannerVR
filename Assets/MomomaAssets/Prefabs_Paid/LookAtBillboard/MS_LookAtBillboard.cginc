// Copyright (c) 2022 momoma
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

void vert(inout appdata_full i)
{
	#if defined(USING_STEREO_MATRICES)
		float3 cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
	#else
		float3 cameraPos = _WorldSpaceCameraPos;
	#endif

	float3 direction = mul(unity_WorldToObject, float4(cameraPos, 1)).xyz;
	direction.y = UNITY_ACCESS_INSTANCED_PROP(Props, _XRotaion) ? direction.y : 0;
	direction = normalize(-direction);

	float3x3 billboardMatrix;
	billboardMatrix[2] = direction;
	billboardMatrix[0] = normalize(float3(direction.z, 0, -direction.x));
	billboardMatrix[1] = normalize(cross(direction, billboardMatrix[0]));
	billboardMatrix = transpose(billboardMatrix);

	i.vertex.xyz = mul(billboardMatrix, i.vertex.xyz);
	i.normal = mul(billboardMatrix, i.normal);
	i.tangent.xyz = mul(billboardMatrix, i.tangent.xyz);
}
