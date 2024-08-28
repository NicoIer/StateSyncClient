Shader "#NAME#"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}// 主贴图
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


        // 支持SRP Batcher
        CBUFFER_START(UnityPerMaterial)
            //声明变量
            float4 _MainTex_ST;
        CBUFFER_END

        TEXTURE2D(_MainTex); // 声明纹理  
        SAMPLER(sampler_MainTex); // 声明纹理采样器


        // a2v -> attribute to vertex
        struct a2v
        {
            float4 positionOS: POSITION;
            float2 uv : TEXCOORD0;
        };

        // v2f -> vertex to fragment
        struct v2f
        {
            float4 positionCS: SV_POSITION;
            float2 uv: TEXCOORD0;
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
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}