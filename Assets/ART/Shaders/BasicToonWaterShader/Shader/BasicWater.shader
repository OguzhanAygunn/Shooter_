Shader "Custom/BasicWaterShader"
{
    Properties
    {
        _Color ("Background Color", Color) = (0.1, 0.4, 0.8, 0.8)
        _TextureColor ("Texture Color", Color) = (1, 1, 1, 1)
        _MainTex ("Water Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Float) = 0.5
        _WaveStrength ("Wave Strength", Range(0, 0.1)) = 0.01
        _WaveAmount ("Wave Amount", Float) = 0.1
        _WaveFrequency ("Wave Frequency", Float) = 1
        _TextureDistortion ("Texture Distortion", Range(0, 1)) = 0.5
        _CartoonFactor ("Cartoon Factor", Range(0, 1)) = 0.5
        _TransparencySpeed ("Transparency Animation Speed", Float) = 1.0
        _TransparencyStrength ("Transparency Strength", Range(0, 1)) = 0.5
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamAmount ("Foam Amount", Range(0, 1)) = 0.1
        _FoamCutoff ("Foam Cutoff", Range(0, 1)) = 0.5
        _FoamSpeed ("Foam Speed", Float) = 0.1
        _FoamNoiseScale ("Foam Noise Scale", Float) = 20
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade
        #pragma target 3.0

        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _CameraDepthTexture;
        fixed4 _Color, _TextureColor, _FoamColor;
        float _WaveSpeed, _WaveStrength, _WaveAmount, _WaveFrequency;
        float _TextureDistortion, _CartoonFactor;
        float _TransparencySpeed, _TransparencyStrength;
        float _FoamAmount, _FoamCutoff, _FoamSpeed, _FoamNoiseScale;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float4 screenPos;
        };

        // Improved pseudo-random function
        float2 random2(float2 st)
        {
            st = float2(dot(st, float2(127.1, 311.7)),
                        dot(st, float2(269.5, 183.3)));
            return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
        }

        // Gradient noise function
        float gradientNoise(float2 st) 
        {
            float2 i = floor(st);
            float2 f = frac(st);

            float2 u = f*f*(3.0-2.0*f);

            return lerp( lerp( dot( random2(i + float2(0.0,0.0) ), f - float2(0.0,0.0) ),
                               dot( random2(i + float2(1.0,0.0) ), f - float2(1.0,0.0) ), u.x),
                         lerp( dot( random2(i + float2(0.0,1.0) ), f - float2(0.0,1.0) ),
                               dot( random2(i + float2(1.0,1.0) ), f - float2(1.0,1.0) ), u.x), u.y);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            float2 uv = IN.uv_MainTex;
            
            // Generate smoother wave movement
            float2 waveOffset = float2(
                gradientNoise(uv * _WaveFrequency + _Time.y * _WaveSpeed),
                gradientNoise(uv * _WaveFrequency * 1.2 + _Time.y * _WaveSpeed * 1.1)
            ) * _WaveAmount;

            // Apply distortion with control
            float2 distortedUV = uv + waveOffset * _WaveStrength * _TextureDistortion;

            // Use texture derivatives for better sampling
            fixed4 c = tex2D(_MainTex, distortedUV, ddx(uv), ddy(uv));
            
            // Blend distorted texture with original
            c = lerp(tex2D(_MainTex, uv), c, _TextureDistortion);
            
            // Apply texture color
            c *= _TextureColor;

            // Pulsating transparency (only for texture)
            float transparencyPulse = (sin(_Time.y * _TransparencySpeed) + 1) * 0.5;
            float textureTransparency = lerp(1, transparencyPulse, _TransparencyStrength);

            // Calculate foam
            float2 foamUV = IN.worldPos.xz * _FoamNoiseScale + _Time.y * _FoamSpeed;
            float foamNoise = gradientNoise(foamUV);

            // Get depth texture
            float4 screenPos = IN.screenPos;
            float depth = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos)));
            float foamLine = 1 - saturate(_FoamAmount * (depth - screenPos.w));

            // Combine foam noise with foam line
            float foam = saturate(foamNoise + foamLine);
            foam = smoothstep(_FoamCutoff, 1, foam);

            // Blend colors, apply transparency and foam
            fixed3 finalColor = lerp(_Color.rgb, c.rgb, c.a * textureTransparency);
            finalColor = lerp(finalColor, _FoamColor.rgb, foam);

            o.Albedo = finalColor;
            o.Alpha = lerp(_Color.a, c.a * _TextureColor.a, c.a * textureTransparency);

            // Simple normal calculation for lighting
            o.Normal = float3(0, 0, 1);
        }
        ENDCG
    }
    FallBack "Transparent/VertexLit"
}