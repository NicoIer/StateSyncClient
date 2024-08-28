Shader "Universal/Tutorial01-1"
{
    Properties //着色器的输入 
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
        // URP所有的光照相关函数都在Lighting.hlsl中
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        #include "Assets//Plugins/UnityToolkit/Shader/UnityToolkit.hlsl"
        ENDHLSL

        Pass
        {
            Tags
            {
                // LightMode指定了这个Pass使用的光照模式，这里使用了UniversalForward
                "LightMode" = "UniversalForward"
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END


            // a2v -> attribute to vertex
            struct a2v
            {
                float4 positionOS: POSITION; // 模型的顶点的位置
                float3 normalOS: NORMAL; // 模型的顶点的法线
            };

            // v2f -> vertex to fragment
            struct v2f
            {
                float4 positionCS: SV_POSITION;
                float3 color: COLOR;
                // float2 uv: TEXCOORD0;
            };


            v2f vert(a2v v)
            {
                v2f o;
                // TransformObjectToHClip 将模型空间的顶点位置转换到裁剪空间
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                // UNITY_LIGHTMODEL_AMBIENT是Unity中的环境光 可以在Lighting Inspector中设置
                // 这里是获取环境光的颜色
                // float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                half3 ambient = _GlossyEnvironmentColor.rgb;
                // 坐标转换 将模型的法线从 模型空间转换到世界空间
                float3 worldNormal = normalize(mul(v.normalOS, (float3x3)unity_WorldToObject));
                // 获取主光源
                Light light = GetMainLight();

                // dot(worldNormal,light.direction) 拿到这个顶点的法线和光线的夹角
                // saturate 限制值在0-1之间
                // saturate(dot(worldNormal,light.direction)) 计算出的值表示漫反射的强度 越垂直越亮
                // light.color * saturate(dot(worldNormal,light.direction)) 计算出的值表示漫反射的颜色
                half diffuse = LambertDiffuse(worldNormal, light.direction);
                o.color = light.color * diffuse * _BaseColor.rgb + ambient;
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                return half4(i.color, 1.0);
            }
            ENDHLSL
        }
    }
}