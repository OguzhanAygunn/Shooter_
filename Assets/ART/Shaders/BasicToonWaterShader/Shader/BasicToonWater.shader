Shader "Custom/BasicToonWaterShader"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.1, 0.4, 0.8, 0.8)
        _MainTex ("Water Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Float) = 0.5
        _WaveStrength ("Wave Strength", Range(0, 0.1)) = 0.01
        _WaveAmount ("Wave Amount", Float) = 0.1
        _WaveFrequency ("Wave Frequency", Float) = 1
        _TextureDistortion ("Texture Distortion", Range(0, 1)) = 0.5
        _CartoonFactor ("Cartoon Factor", Range(0, 1)) = 0.5
        _ColorSteps ("Color Steps", Range(2, 10)) = 4
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.2
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamAmount ("Foam Amount", Range(0, 1)) = 0.1
        _FoamCutoff ("Foam Cutoff", Range(0, 1)) = 0.5
        _FoamSpeed ("Foam Speed", Float) = 0.1
        _FoamNoiseScale ("Foam Noise Scale", Float) = 20
        _FoamSmoothness ("Foam Smoothness", Range(0, 0.5)) = 0.1
        _FoamEdgeSize ("Foam Edge Size", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _OutlineWidth;
            fixed4 _OutlineColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float2 offset = TransformViewToProjection(norm.xy);
                o.pos.xy += offset * _OutlineWidth * o.pos.z;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        CGPROGRAM
        #pragma surface surf ToonRamp alpha
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _CameraDepthTexture;
        fixed4 _Color, _EdgeColor, _FoamColor;
        float _WaveSpeed, _WaveStrength, _WaveAmount, _WaveFrequency;
        float _TextureDistortion, _CartoonFactor, _ColorSteps, _EdgeThreshold;
        float _FoamAmount, _FoamCutoff, _FoamSpeed, _FoamNoiseScale;
        float _FoamSmoothness, _FoamEdgeSize;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
            float4 screenPos;
        };

        // Toon ramp lighting function
        fixed4 LightingToonRamp(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            float diff = dot(s.Normal, lightDir);
            float h = diff * 0.5 + 0.5;
            float ramp = floor(h * _ColorSteps) / _ColorSteps;
            ramp = lerp(h, ramp, _CartoonFactor);

            fixed4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * ramp * atten;
            c.a = s.Alpha;
            return c;
        }

        // Improved pseudo-random function
        float2 random2(float2 st)
        {
            st = float2(dot(st, float2(127.1, 311.7)),
                        dot(st, float2(269.5, 183.3)));
            return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
        }

        // Smooth noise function
        float noise(float2 st)
        {
            float2 i = floor(st);
            float2 f = frac(st);

            float2 u = f * f * (3.0 - 2.0 * f);

            return lerp(lerp(dot(random2(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                             dot(random2(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                        lerp(dot(random2(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                             dot(random2(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
        }

        // Smooth step function for foam
        float smootherstep(float edge0, float edge1, float x) {
            x = saturate((x - edge0) / (edge1 - edge0));
            return x * x * x * (x * (x * 6 - 15) + 10);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            float2 uv = IN.uv_MainTex;
            
            // Generate smoother wave movement
            float2 waveOffset = float2(
                noise(uv * _WaveFrequency + _Time.y * _WaveSpeed),
                noise(uv * _WaveFrequency * 1.2 + _Time.y * _WaveSpeed * 1.1)
            ) * _WaveAmount;

            // Apply distortion with control
            float2 distortedUV = uv + waveOffset * _WaveStrength * _TextureDistortion;

            // Use texture derivatives for better sampling
            fixed4 c = tex2D(_MainTex, distortedUV, ddx(uv), ddy(uv));
            
            // Blend distorted texture with original
            c = lerp(tex2D(_MainTex, uv), c, _TextureDistortion);
            
            // Apply cartoon effect
            float3 normal = UnpackNormal(tex2D(_MainTex, distortedUV));
            float edge = 1 - saturate(dot(normalize(IN.viewDir), normal));
            
            // Calculate foam
            float2 foamUV = IN.worldPos.xz * _FoamNoiseScale + _Time.y * _FoamSpeed;
            float foamNoise = noise(foamUV);

            // Get depth texture
            float4 screenPos = IN.screenPos;
            float depth = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos)));
            float foamLine = 1 - saturate(_FoamAmount * (depth - screenPos.w));

            // Create smooth foam transition
            float foamGradient = smootherstep(_FoamCutoff - _FoamSmoothness, _FoamCutoff + _FoamSmoothness, foamLine + foamNoise);
            float foam = foamGradient * _FoamEdgeSize;

            fixed3 finalColor;
            if (edge > _EdgeThreshold)
            {
                finalColor = lerp(c.rgb * _Color.rgb, _EdgeColor.rgb, _CartoonFactor);
            }
            else
            {
                finalColor = c.rgb * _Color.rgb;
            }

            // Blend in foam
            finalColor = lerp(finalColor, _FoamColor.rgb, foam);

            o.Albedo = finalColor;
            o.Alpha = c.a * _Color.a;
        }
        ENDCG
    }
    FallBack "Transparent/VertexLit"
}