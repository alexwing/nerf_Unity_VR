Shader "Unlit/volumeShad2"  // ref https://github.com/mattatz/unity-volume-rendering/blob/master/Assets/VolumeRendering/Shaders/VolumeRendering.cginc
{
	Properties
	{
		[Header(Rendering)]
		_Volume("Volume", 3D) = "" {}
		_Iteration("Iteration", Int) = 10
		//rgb alpha bool
		[MaterialToggle] _rgbAlpha("rgbAlpha", Float) = 0
		[MaterialToggle] _cutoff ("cutoff", Float) = 0
		_alphaTransition("Alpha Transition", Range(0, 1)) = 0.5

		_AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.5
		[MaterialToggle] _Dissolve("Dissolve", Float) = 0
		[MaterialToggle] _Normalized("Normalized", Float) = 0

		[Header(Ranges)]
		_MinX("MinX", Range(0, 1)) = 0.0
		_MaxX("MaxX", Range(0, 1)) = 1.0
		_MinY("MinY", Range(0, 1)) = 0.0
		_MaxY("MaxY", Range(0, 1)) = 1.0
		_MinZ("MinZ", Range(0, 1)) = 0.0
		_MaxZ("MaxZ", Range(0, 1)) = 1.0
	}
	SubShader
	{
		Tags
		{ "Queue" = "Transparent"
		  "RenderType" = "Transparent"
		}
		Cull Front
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 localPos : TEXCOORD0;
			};

			sampler3D _Volume;
			int _Iteration;
			float _AlphaCutoff;
			float _cutoff;
			float _alphaTransition;
			float _rgbAlpha;
			fixed _MinX, _MaxX, _MinY, _MaxY, _MinZ, _MaxZ;
			fixed _Dissolve;
			fixed _Normalized;
			

			float4 sample(float3 pos) // clip the volume
			{			
				if (pos.x < _MinX || pos.x > _MaxX || pos.y < _MinY || pos.y > _MaxY || pos.z < _MinZ || pos.z > _MaxZ)
					return float4(0, 0, 0, 0);

				if (_cutoff){
					if (_rgbAlpha) {
						//rgb / 3 = alpha
							//evaluate bigger rgb value
							float4 color = tex3D(_Volume, pos);
							float alpha = max(max(color.r, color.g), color.b);							
							if (alpha > _AlphaCutoff){
								return float4(tex3D(_Volume, pos).rgb, alpha);
							} else {
								return float4(tex3D(_Volume, pos).rgb, alpha* _alphaTransition);
							}				
					}else{
							//if _alphaTransition > 0  = alpha = 0, else alpha transition =  1 - _alphaTransition
							float4 color = tex3D(_Volume, pos);
							float alpha = color.a;
							if (alpha > _AlphaCutoff){
								return float4(color.rgb, alpha);
							} else {
								return float4(color.rgb, alpha * _alphaTransition);
							}	
					}
				}
				return  tex3D(_Volume, pos);
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.localPos = v.vertex;
				return o;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				float3 rayOrigin = i.localPos + 0.5;
				float3 rayDir  = ObjSpaceViewDir(i.localPos);
				float rayLength = length(rayDir);
				//remove normalize run faster
				if (_Normalized){
				//	rayDir = rayDir / rayLength;
					rayDir = normalize(rayDir);
				}
				float t = 1.732 / _Iteration; // step size for one iteration

				float4 finalColor = 0.0;
				[loop]
				for (int j = 0; j < _Iteration; ++j)
				{
					float step = t * j;
					if (step >= rayLength) // do not render volume that is behind the camera
						break;
					if (_Dissolve) {
						step *= (1 + sin(_Time.z / 2))*0.5;
					}
					float3 curPos = rayOrigin + rayDir * step;
					float4 color = sample(curPos);
					// use back to front composition	
					//simplified version			
					finalColor = float4(color.a * color.rgb + (1-color.a) * finalColor.rgb, color.a + (1 - color.a) * finalColor.a);
					if (finalColor.a > 1) break;
				}
				return finalColor;

			}
			ENDCG
		}
	}
}
