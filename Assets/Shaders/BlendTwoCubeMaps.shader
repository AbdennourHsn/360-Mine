Shader "Custom/BlendTwoCubeMaps"
{
    Properties
    {
        _Blend ("Blend (0..1)", Range(0,3)) = 0

        [NoScaleOffset] _Tex_1 ("Cubemap 1", Cube) = "grey" {}
        _RotationX_1 ("cube 1 : Rotation X (Pitch)", Range(-180, 180)) = 0
        _RotationY_1 ("cube 1 : Rotation Y (Yaw)", Range(-180, 180)) = 0
        _RotationZ_1 ("cube 1 : Rotation Z (Roll)", Range(-180, 180)) = 0
        _StretchBlend_1 ("Forward Stretch Blend 1", Range(-2, 1)) = 0


        [NoScaleOffset] _Tex_2 ("Cubemap 2", Cube) = "grey" {}
        _RotationX_2 ("cube 2 : Rotation X (Pitch)", Range(-180, 180)) = 0
        _RotationY_2 ("cube 2 : Rotation Y (Yaw)", Range(-180, 180)) = 0
        _RotationZ_2 ("cube 2 : Rotation Z (Roll)", Range(-180, 180)) = 0
        _StretchBlend_2 ("Forward Stretch Blend 2", Range(-2, 1)) = 0

        _StretchIntensity ("Stretch Intensity", Range(1, 5)) = 2
        
        _Tint ("Tint Color", Color) = (1, 1, 1, 1)
        _Exposure ("Exposure", Range(0, 8)) = 1.3
    }

    SubShader
    {
        Tags
        {
            "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float _Blend;

            float _RotationX_1, _RotationY_1, _RotationZ_1;
            float _RotationX_2, _RotationY_2, _RotationZ_2;


            samplerCUBE _Tex_1;
            samplerCUBE _Tex_2;


            float _StretchBlend_1;
            float _StretchBlend_2;



            float _StretchIntensity;
            fixed4 _Tint;
            half _Exposure;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.uv;
                return o;
            }

            float3 ApplyRotation(float3 dir, float rotation)
            {
                float rad = rotation * 3.14159265 / 180.0;
                float cosR = cos(rad);
                float sinR = sin(rad);

                return float3(
                    dir.x * cosR - dir.z * sinR,
                    dir.y,
                    dir.x * sinR + dir.z * cosR
                );
            }

            float3 ApplyRotationEuler(float3 dir, float3 eulerDeg)
            {
                float3 r = radians(eulerDeg);
                float cx = cos(r.x), sx = sin(r.x);
                float cy = cos(r.y), sy = sin(r.y);
                float cz = cos(r.z), sz = sin(r.z);

                // Rx, Ry, Rz
                float3x3 Rx = float3x3(
                    1, 0, 0,
                    0, cx, -sx,
                    0, sx, cx
                );
                float3x3 Ry = float3x3(
                    cy, 0, sy,
                    0, 1, 0,
                    -sy, 0, cy
                );
                float3x3 Rz = float3x3(
                    cz, -sz, 0,
                    sz, cz, 0,
                    0, 0, 1
                );

                // Z * X * Y (ZXY)
                float3x3 R = mul(Rz, mul(Rx, Ry));
                return mul(R, dir);
            }

            float3 ComputeFinalDirection(float3 worldPos, float rotationDeg, float stretchIntensity, float stretchBlend)
            {
                // Base directions
                float3 direction = normalize(worldPos);
                float3 cameraForward = normalize(UNITY_MATRIX_V[2].xyz);
                
                float3 rotatedCameraForward = ApplyRotation(cameraForward, rotationDeg);
                float3 rotatedDirection = ApplyRotation(direction, rotationDeg);
                
                float forwardDot = dot(rotatedDirection, rotatedCameraForward);

                // Stretch logic
                float3 originalDirection = rotatedDirection;
                float3 stretchedDirection = rotatedDirection;

                // Constants (tweak if needed)
                const float kForwardThreshold = 0.8;
                const float kStretchScale = 0.7;

                if (forwardDot < kForwardThreshold)
                {
                    float sideBackAmount = (1.0 - max(0, forwardDot));
                    float stretchFactor = sideBackAmount * stretchIntensity * kStretchScale;

                    stretchedDirection = normalize(lerp(-rotatedCameraForward, rotatedDirection, 1.0 - stretchFactor));
                }

                return normalize(lerp(originalDirection, stretchedDirection, stretchBlend));
            }

            float3 ComputeFinalDirectionEuler(
                float3 worldPos,
                float3 eulerDeg, 
                float stretchIntensity,
                float stretchBlend)
            {
                float3 direction = normalize(worldPos);
                float3 camForwardWS = normalize(UNITY_MATRIX_V[2].xyz);

                float3 rCamFwd = ApplyRotationEuler(camForwardWS, eulerDeg);
                float3 rDir = ApplyRotationEuler(direction, eulerDeg);

                float forwardDot = dot(rDir, rCamFwd);

                float3 originalDirection = rDir;
                float3 stretchedDirection = rDir;

                const float kForwardThreshold = 0.8;
                const float kStretchScale = 0.7;

                if (forwardDot < kForwardThreshold)
                {
                    float sideBackAmount = (1.0 - max(0, forwardDot));
                    float stretchFactor = sideBackAmount * stretchIntensity * kStretchScale;

                    stretchedDirection = normalize(lerp(-rCamFwd, rDir, 1.0 - stretchFactor));
                }
                
                float3 rUp = ApplyRotationEuler(float3(0, 1, 0), eulerDeg);
                float pole = abs(dot(rDir, rUp)); 
                float fade = 1.0 - smoothstep(0.7, 0.95, pole); 

                return normalize(lerp(originalDirection, stretchedDirection, stretchBlend * fade));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 eulerAngles_1 = float3(_RotationX_1, _RotationY_1, _RotationZ_1);
                float3 finalDirection1 = ComputeFinalDirectionEuler(i.worldPos, eulerAngles_1, _StretchIntensity,_StretchBlend_1);

                float3 eulerAngles_2 = float3(_RotationX_2, _RotationY_2, _RotationZ_2);
                float3 finalDirection2 = ComputeFinalDirectionEuler(i.worldPos, eulerAngles_2, _StretchIntensity,_StretchBlend_2);


                fixed4 col1 = texCUBE(_Tex_1, finalDirection1);
                fixed4 col2 = texCUBE(_Tex_2, finalDirection2);

                fixed4 c;

                c = lerp(col1, col2, _Blend);
                
                c.rgb *= _Tint.rgb * _Exposure;

                return c;
            }
            ENDCG
        }
    }
    FallBack Off
}