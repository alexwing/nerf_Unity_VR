/**
@author: Alejandro Aranda Moreno
@mail: alexwing@gmail.com

@references:
    - https://github.com/mattatz/unity-volume-rendering/blob/master/Assets/VolumeRendering/Shaders/VolumeRendering.cginc
    - https://kwea123.github.io/nerf_pl/


This shader is a custom shader for rendering 3D volumes. It uses the raymarching technique to render the 3D volume. It's designed to be used with a 3D data texture that represents the volume, and it provides a number of adjustable properties to control how the volume is rendered.

The main properties of this shader are:

- _Iteration: This controls the number of iterations that the raymarching algorithm will use to render the volume. More iterations will produce a higher-quality rendering, but will also be slower.
- _DataTex: This is the 3D data texture that represents the volume that will be rendered.
- _MinVal and _MaxVal: These properties control the minimum and maximum values that will be used to map the data in the 3D texture to colors in the final rendering.
- _alphaTransition: This property controls the transition between transparent and opaque regions of the volume.
- _alphaFactor: This property controls the overall transparency of the volume.
- _deptFactor: This property controls the depth of the volume.
- _lightFactor: This property controls the intensity of the lighting applied to the volume.
- _lighting: This property toggles the use of lighting in the rendering.
- _Noise: This property toggles the use of noise in the rendering.
- _NoiseTex: This is the 2D data texture that represents the noise that will be used in the rendering.
- _noiseFactor: This property controls the intensity of the noise.   
- _MinX, _MaxX, _MinY, _MaxY, _MinZ, _MaxZ: These properties control the size of the volume. 

The shader also includes a number of helper functions for performing common tasks related to volume rendering, such as finding intersections between rays and bounding boxes, and computing the direction of view rays in both perspective and orthographic projections.
*/

Shader "VolumeRendering/VolumeColorRenderingShader"
{
    Properties
    {
        _Iteration("Iterations", Range(1, 2000)) = 10
        _DataTex ("Data Texture (Generated)", 3D) = "" {}
       
        [Header(Ranges level)]
       
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0

        [Header(Ranges size)]
		_MinX("MinX", Range(0, 1)) = 0.0
		_MaxX("MaxX", Range(0, 1)) = 1.0
		_MinY("MinY", Range(0, 1)) = 0.0
		_MaxY("MaxY", Range(0, 1)) = 1.0
		_MinZ("MinZ", Range(0, 1)) = 0.0
		_MaxZ("MaxZ", Range(0, 1)) = 1.0  

        [Header(Properties)]

        _alphaTransition("Alpha Transition", Range(0, 1)) = 0.5
        _alphaFactor("Alpha Factor", Range(0, 10)) = 1
        _deptFactor("Depth Factor", Range(0.0, 1.0)) = 0.15
		[MaterialToggle] _lighting("Lighting", Float) = 0     
        _lightFactor("Light Factor", Range(0.0, 10.0)) = 1

        [Header(Noise)]
        [MaterialToggle] _Noise("Activate Noise", Float) = 0
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _noiseFactor("Noise Factor", Range(0.0, 4.0)) = 2

    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Cull Front
        ZTest LEqual
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct vert_in
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexLocal : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct frag_out
            {
                float4 colour : SV_TARGET;
                float depth : SV_DEPTH;
            };

            sampler3D _DataTex;
            sampler2D _NoiseTex;

            float _MinVal, _MaxVal;
            float _alphaTransition;
            float _alphaFactor;
            float _lightFactor;
            float _deptFactor;
            float _noiseFactor;

			uniform float _MinX, _MaxX, _MinY, _MaxY, _MinZ, _MaxZ;


            uniform int _lighting;
            uniform int _Iteration;
            uniform int _Noise;


            struct RayInfo
            {
                float3 startPos;
                float3 endPos;
                float3 direction;
                float2 aabbInters;
            };

            struct RaymarchInfo
            {
                RayInfo ray;
                int numSteps;
                float numStepsRecip;
                float stepSize;
            };

            float3 getViewRayDir(float3 vertexLocal)
            {
                if(unity_OrthoParams.w == 0)
                {
                    // Perspective
                    return normalize(ObjSpaceViewDir(float4(vertexLocal, 0.0f)));
                }
                else
                {
                    // Orthographic
                    float3 camfwd = mul((float3x3)unity_CameraToWorld, float3(0,0,-1));
                    float4 camfwdobjspace = mul(unity_WorldToObject, camfwd);
                    return normalize(camfwdobjspace);
                }
            }

            // Find ray intersection points with axis aligned bounding box
            // spanish: encontrar puntos de intersección del rayo con el bounding box alineado con los ejes
            float2 intersectAABB(float3 rayOrigin, float3 rayDir, float3 boxMin, float3 boxMax)
            {
                float3 tMin = (boxMin - rayOrigin) / rayDir;
                float3 tMax = (boxMax - rayOrigin) / rayDir;
                float3 t1 = min(tMin, tMax);
                float3 t2 = max(tMin, tMax);
                float tNear = max(max(t1.x, t1.y), t1.z);
                float tFar = min(min(t2.x, t2.y), t2.z);
                return float2(tNear, tFar);
            };

            // Get a ray for the specified fragment (back-to-front)
            // spanish: coger rayo para el fragmento especificado (de atrás hacia adelante)
            RayInfo getRayBack2Front(float3 vertexLocal)
            {
                RayInfo ray;
                ray.direction = getViewRayDir(vertexLocal);
                ray.startPos = vertexLocal + float3(0.5f, 0.5f, 0.5f);
                // Find intersections with axis aligned boundinng box (the volume)
                // spanish: encontrar intersecciones con el bounding box alineado con los ejes (el volumen)
                ray.aabbInters = intersectAABB(ray.startPos, ray.direction, float3(0.0, 0.0, 0.0), float3(1.0f, 1.0f, 1.0));

                // Check if camera is inside AABB
                // spanish: comprobar si la cámara está dentro del AABB
                const float3 farPos = ray.startPos + ray.direction * ray.aabbInters.y - float3(0.5f, 0.5f, 0.5f);
                float4 clipPos = UnityObjectToClipPos(float4(farPos, 1.0f));
                ray.aabbInters += min(clipPos.w, 0.0);

                ray.endPos = ray.startPos + ray.direction * ray.aabbInters.y;
                return ray;
            }

            // Get a ray for the specified fragment (front-to-back)
            // spanish: coger rayo para el fragmento especificado (de adelante hacia atrás)
            RayInfo getRayFront2Back(float3 vertexLocal)
            {
                RayInfo ray = getRayBack2Front(vertexLocal);
                ray.direction = -ray.direction;
                float3 tmp = ray.startPos;
                ray.startPos = ray.endPos;
                ray.endPos = tmp;
                return ray;
            }

            RaymarchInfo initRaymarch(RayInfo ray, int maxNumSteps)
            {
                RaymarchInfo raymarchInfo;
                raymarchInfo.stepSize = 1.732f/*greatest distance in box*/ / maxNumSteps;
                raymarchInfo.numSteps = (int)clamp(abs(ray.aabbInters.x - ray.aabbInters.y) / raymarchInfo.stepSize, 1, maxNumSteps);
                raymarchInfo.numStepsRecip = 1.0 / raymarchInfo.numSteps;
                return raymarchInfo;
            }

            // Gets the gradient at the specified position
            // spanish: coger gradiente en la posición especificada
            float3 getGradient(float3 pos)
            {
                return tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0f)).rgb;
            }

            // Performs lighting calculations, and returns a modified colour.
            // spanish: realiza cálculos de iluminación, y devuelve un color modificado.
            float3 calculateLighting(float3 col, float3 normal, float3 lightDir, float3 eyeDir, float specularIntensity)
            {
                float ndotl = max(lerp(0.0f, 1.5f, dot(normal, lightDir)), 0.5f); // modified, to avoid volume becoming too dark
                float3 diffuse = ndotl * col;
                float3 v = eyeDir;
                float3 r = normalize(reflect(-lightDir, normal));
                float rdotv = max( dot( r, v ), 0.0 );
                float3 specular = pow(rdotv, 32.0f) * float3(1.0f, 1.0f, 1.0f) * specularIntensity;
                return diffuse + specular;
            }

            // Converts local position to depth value
            // spanish: convierte posición local a valor de profundidad
            float localToDepth(float3 localPos)
            {
                float4 clipPos = UnityObjectToClipPos(float4(localPos, 1.0f));

#if defined(SHADER_API_GLCORE) || defined(SHADER_API_OPENGL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
                return (clipPos.z / clipPos.w) * 0.5 + 0.5;
#else
                return clipPos.z / clipPos.w;
#endif
            }

            frag_in vert_main (vert_in v)
            {
                frag_in o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }


            float4 sample(float3 pos) // clip the volume
			{		
                if (pos.x < _MinX || pos.x > _MaxX || pos.y < _MinY || pos.y > _MaxY || pos.z < _MinZ || pos.z > _MaxZ)
                						return float4(0, 0, 0, 0);
				return  tex3D(_DataTex, pos);
			}

            // Direct Volume Rendering
            // spanish: renderizado de volumen directo
            frag_out frag_dvr(frag_in i)
            {


                RayInfo ray = getRayFront2Back(i.vertexLocal);
                RaymarchInfo raymarchInfo = initRaymarch(ray, _Iteration );

                float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));

                // Create a small random offset in order to remove artifacts
                // spanish: crear un pequeño desplazamiento aleatorio para eliminar artefactos
                if (_Noise){
                 ray.startPos += (_noiseFactor * ray.direction * raymarchInfo.stepSize) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;
                }
                float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float tDepth = raymarchInfo.numStepsRecip * (raymarchInfo.numSteps - 1);
                [loop]
                for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                {
                    const float t = iStep * raymarchInfo.numStepsRecip;
                    const float3 currPos = lerp(ray.startPos, ray.endPos, t);



                    color = sample(currPos);
                
                    // Apply visibility window
                    // spanish: aplicar ventana de visibilidad
                    if (color.a < _MinVal || color.a > _MaxVal) continue;

                    // Calculate gradient (needed for lighting and 2D transfer functions)
                    // spanish: calcular gradiente (necesario para iluminación y funciones de transferencia 2D)
                    if (_lighting){
                        color.rgb = calculateLighting(color.rgb, normalize(getGradient(currPos)), lightDir, -ray.direction, 0.3f);
                    }
                    if (color.a > _deptFactor && t < tDepth) {
                        tDepth = t;
                    }
                    // Early ray termination
                    // spanish: terminación temprana de rayo
                    if (color.a > _alphaTransition) {
                        break;
                    }
                    //reduce the alpha
                   // color = float4(color.r * _alphaFactor, color.g * _alphaFactor, color.b * /_alphaFactor, color.a * _alphaFactor);

                    color  = float4(color.rgb * _lightFactor, color.a * _alphaFactor);
                    if (color.a > 1) break;
                }

                // Write fragment output
                // spanish: escribir salida de fragmento
                frag_out output;
                
                output.colour = color;
                //Aplly DEPTH
                tDepth += (step(color.a, 0.0) * 1000.0); // Write large depth if no hit
                const float3 depthPos = lerp(ray.startPos, ray.endPos, tDepth) - float3(0.5f, 0.5f, 0.5f);
                output.depth = localToDepth(depthPos);
                

                return output;
            }

            
            frag_in vert(vert_in v)
            {
                return vert_main(v);
            }

            frag_out frag(frag_in i)
            {

                return frag_dvr(i);                    

            }

            ENDCG
        }
    }
}
