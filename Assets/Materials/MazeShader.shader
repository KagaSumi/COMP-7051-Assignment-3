Shader "Custom/MazeShader"
{
    Properties
    {
        // We ONLY keep variables here that we want to tweak manually per material.
        _MainTex ("Texture", 2D) = "white" {}
        
        // We keep Angle here so you can resize the beam in the Inspector if you want
        _FlashlightAngle ("Flashlight Angle (Cos)", Range(0, 1)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1; 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // --- GLOBAL VARIABLES (Controlled by VisualEffectsController.cs) ---
            // These do not exist in 'Properties', so they listen to C# directly.
            
            float _AmbientIntensity;   // 1.0 = Day, 0.1 = Night
            
            float _FogEnabled;         // 1.0 = On, 0.0 = Off
            float _FogDensity;
            float4 _FogColor;
            
            float _FlashlightEnabled;  // 1.0 = On, 0.0 = Off
            float4 _FlashlightPos;
            float4 _FlashlightDir;
            
            // This one comes from Properties above
            float _FlashlightAngle;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Base Texture Color
                fixed4 col = tex2D(_MainTex, i.uv);

                // --- DAY / NIGHT LOGIC ---
                // Start with ambient intensity. 
                // We use max() to ensure it never goes completely pitch black (safety).
                float effectiveAmbient = max(_AmbientIntensity, 0.05);
                float3 finalLight = float3(effectiveAmbient, effectiveAmbient, effectiveAmbient);

                // --- FLASHLIGHT LOGIC ---
                // We check > 0.5 because floats can be 0.00001
                if (_FlashlightEnabled > 0.5) 
                {
                    float3 toLight = _FlashlightPos.xyz - i.worldPos;
                    float dist = length(toLight);
                    float3 lightDir = normalize(toLight);

                    // Compare look direction vs direction to pixel
                    float dotProd = dot(-lightDir, normalize(_FlashlightDir.xyz));

                    if (dotProd > _FlashlightAngle)
                    {
                        // Attenuation (fade over distance)
                        float intensity = 1.0 / (1.0 + (dist * 0.05));
                        
                        // Soft edges
                        float spotEffect = smoothstep(_FlashlightAngle, _FlashlightAngle + 0.1, dotProd);
                        
                        // Add light
                        finalLight += float3(1, 0.95, 0.8) * intensity * spotEffect * 2.0;
                    }
                }

                // Apply lighting
                col.rgb *= finalLight;

                // --- FOG LOGIC ---
                if (_FogEnabled > 0.5)
                {
                    float dist = distance(_WorldSpaceCameraPos, i.worldPos);
                    float fogFactor = exp2(-_FogDensity * dist);
                    fogFactor = clamp(fogFactor, 0.0, 1.0);
                    col.rgb = lerp(_FogColor.rgb, col.rgb, fogFactor);
                }

                return col;
            }
            ENDCG
        }
    }
}