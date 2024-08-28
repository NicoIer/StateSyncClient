Shader "Universal/CustomTutorial03-1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}// 主贴图
        _DiffuseColor("Diffuse Color", Color) = (0.5, 0.5, 0.5, 1)
        _SpecularColor("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Glossiness("Glossiness", Range(8, 256)) = 20
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeLine"="UniversalRenderPipeline" //用于指明使用URP来渲染
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Assets/Plugins/UnityToolkit/Shader/UnityToolkit.hlsl"


        // 支持SRP Batcher
        CBUFFER_START(UnityPerMaterial)
            //声明变量
            float4 _MainTex_ST;
            float4 _DiffuseColor;
            float4 _SpecularColor;
            float _Glossiness;
        CBUFFER_END

        TEXTURE2D(_MainTex); // 声明纹理  
        SAMPLER(sampler_MainTex); // 声明纹理采样器


        // a2v -> attribute to vertex
        struct a2v
        {
            float4 positionOS: POSITION;
            float2 uv : TEXCOORD0;
            float3 normalOS : NORMAL;
        };

        // v2f -> vertex to fragment
        struct v2f
        {
            float4 positionCS: SV_POSITION;
            float2 uv: TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float3 positionWS : TEXCOORD2;
        };
        ENDHLSL

        Pass
        {
            // Pass的标签
            Tags {}
            HLSLPROGRAM
            // 编译指令 
            #pragma vertex vert // vertex shader 的函数 是 vert
            #pragma fragment frag // fragment shader 的函数 是 frag


            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                Light light = GetMainLight();
                float diffuse = LambertDiffuse(i.normalWS, light.direction);
                half3 diffuseColor = _DiffuseColor.rgb * diffuse * light.color.rgb;
                half3 ambient = _GlossyEnvironmentColor;
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.positionWS.xyz);
                half3 halfDir = normalize(light.direction + viewDir);
                half specular = pow(max(0, dot(halfDir, i.normalWS)), _Glossiness);
                half3 specularColor = _SpecularColor.rgb * specular * light.color.rgb;

                half3 color = col * (ambient + diffuseColor) + specularColor;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}