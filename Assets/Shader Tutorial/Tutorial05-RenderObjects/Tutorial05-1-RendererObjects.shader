Shader "Universal/Tutorial05-1-RendererObjects"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth("Outline Width", Range(0.0, 0.1)) = 0.01
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
            half4 _OutlineColor;
            float _OutlineWidth;
        CBUFFER_END


        // a2v -> attribute to vertex
        struct a2v
        {
            float4 positionOS: POSITION;
            float4 normalOS : NORMAL;
        };

        // v2f -> vertex to fragment
        struct v2f
        {
            float4 positionCS: SV_POSITION;
        };
        ENDHLSL

        Pass
        {
            Cull Front
            // Pass的标签
            Tags {}
            HLSLPROGRAM
            // 编译指令 
            #pragma vertex vert // vertex shader 的函数 是 vert
            #pragma fragment frag // fragment shader 的函数 是 frag


            v2f vert(a2v v)
            {
                v.positionOS = v.positionOS + v.normalOS * _OutlineWidth;
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);

                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}