Shader "Universal/Tutorial04-1-Stencil"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _StencilRef("Stencil Ref", Int) = 0
        _OutlineWidth("Outline Width", Range(0, 0.1)) = 0
    }

    SubShader
    {


        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
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
            half4 _Color;
            int _StencilRef;
            float _OutlineWidth;
        CBUFFER_END

        struct a2v
        {
            float4 positionOS: POSITION; // 模型的顶点的位置
            float3 normalOS: NORMAL; // 模型的顶点的法线
        };

        struct v2f
        {
            float4 positionCS: SV_POSITION;
        };
        ENDHLSL



        Pass
        {
            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
                Pass IncrSat
            }
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            v2f vert(a2v v)
            {
                v.positionOS += float4(v.normalOS * _OutlineWidth,0);
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                return _Color;
            }
            ENDHLSL

        }
    }
}