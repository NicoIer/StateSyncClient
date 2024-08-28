Shader "Universal/CustomTutorial02-3"
{
    Properties
    {
        _DiffuseColor("Diffuse Color", Color) = (0.5, 0.5, 0.5, 1)
        _SpecularColor("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Glossiness("Glossiness", Range(8, 256)) = 20
    }

    SubShader
    {

        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeLine"="UniversalRenderPipeline"
        }


        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Assets/Plugins/UnityToolkit/Shader/UnityToolkit.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _DiffuseColor;
            float4 _SpecularColor;
            float _Glossiness;
        CBUFFER_END


        struct a2v
        {
            float4 positionOS: POSITION;
            float3 normalOS: NORMAL;
        };


        struct v2f
        {
            float4 positionCS: SV_POSITION;
            float3 normalWS : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
        };
        ENDHLSL

        Pass
        {

            Tags {}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS = mul((float3x3)unity_WorldToObject, v.normalOS);
                o.positionWS = mul(unity_ObjectToWorld, v.positionOS).xyz; // 4x4 * 4x1 = 4x1
                return o;
            }


            half4 frag(v2f i) : SV_Target
            {
                Light light = GetMainLight();
                float diffuse = LambertDiffuse(i.normalWS, light.direction);
                half3 diffuseColor = _DiffuseColor.rgb * diffuse * light.color.rgb;
                half3 ambient = _GlossyEnvironmentColor;
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.positionWS.xyz);
                half3 halfDir = normalize(light.direction + viewDir);
                half specular = pow(max(0, dot(halfDir, i.normalWS)), _Glossiness);
                half3 specularColor = _SpecularColor.rgb * specular * light.color.rgb;

                half3 color = ambient + diffuseColor + specularColor;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}