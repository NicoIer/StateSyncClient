Shader "Universal/CustomTutorial01-4"
{
    /* Properties区域
     * 
     * Shaderlab 提供的一种用于在Inspector中显示Shader属性的机制
     * 通过Properties区域定义的属性，可以在Inspector中显示，并且可以在Shader中使用
     * 支持的类型查看Unity文档 https://docs.unity3d.com/Manual/SL-Properties.html
     * 
     * 如何定义
     * 属性名("Inspector显示名", 类型) = "默认值" { }
     * 
     * 如何与HLSL关联 
     * 需要在HLSL中声明一个同名的变量，这样Unity会自动将Inspector中的属性值赋值给HLSL中的变量
     */

    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }
    /* SubShader是Shader的主要部分，包含了一系列的Pass
     * Pass是渲染管线的一个阶段，包含了一个顶点着色器和一个片元着色器
     * 可能存在多个SubShader，Unity会选择当前环境可用的第一个SubShader
     */
    SubShader
    {
        /* Tag 是一个键值对，它的作用是告诉渲染引擎，应该 什么时候 怎么样 去渲染 
         * https://docs.unity.cn/cn/2022.3/Manual/SL-SubShaderTags.html
         */
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeLine"="UniversalRenderPipeline" //用于指明使用URP来渲染
        }
        /* 通用区域 这里内容可以在多个Pass中共享 */

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Assets/Plugins/UnityToolkit/Shader/UnityToolkit.hlsl"

        // 支持SRP Batcher
        CBUFFER_START(UnityPerMaterial)
            //声明变量
            half4 _BaseColor;
        CBUFFER_END

        struct a2v
        {
            float4 positionOS: POSITION;
            half3 normalOS : NORMAL;
        };

        // v2f -> vertex to fragment
        struct v2f
        {
            float4 positionCS: SV_POSITION;
            half3 normalWS : TEXCOORD0;
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
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                Light mainLight = GetMainLight();
                half diffuse = HalfLambdaDiffuse(i.normalWS, mainLight.direction);
                half4 ambient = _GlossyEnvironmentColor;

                half3 color = _BaseColor.rgb * diffuse * mainLight.color.rgb + ambient.rgb;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}