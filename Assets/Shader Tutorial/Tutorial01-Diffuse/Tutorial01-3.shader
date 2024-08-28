Shader "Universal/CustomTutorial01-3"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1) // 主颜色
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
            float4 _BaseColor;
        CBUFFER_END


        struct a2v
        {
            float4 positionOS: POSITION;
            half3 normalOS : NORMAL;
        };

        struct v2f
        {
            float4 positionCS: SV_POSITION;
            half3 normalWS : TEXCOORD0;
        };
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            // 编译指令 
            #pragma vertex vert // vertex shader 的函数 是 vert
            #pragma fragment frag // fragment shader 的函数 是 frag


            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                Light light = GetMainLight();
                half diffuse = LambertDiffuse(i.normalWS, light.direction);
                float3 color = _GlossyEnvironmentColor.rgb + _BaseColor.rgb * diffuse * light.color;
                
                return half4(color, 1); 
            }
            ENDHLSL
        }
    }
}