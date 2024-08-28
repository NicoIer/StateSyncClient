
// URP 多Pass渲染
Shader "Universal/Tutorial04-2"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _OutlineCol("OutlineCol", Color) = (1,0,0,1)
        _OutlineFactor("OutlineFactor", Range(0,10)) = 0.1
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
            half4 _OutlineCol;
            half _OutlineFactor;
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
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            v2f vert(a2v v)
            {
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

        Pass
        {
            Cull Front
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            v2f vert(a2v v)
            {
                v2f o;
                v.positionOS.xyz = v.positionOS.xyz + v.normalOS * _OutlineFactor;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return _OutlineCol;
            }
            ENDHLSL
        }
    }

}