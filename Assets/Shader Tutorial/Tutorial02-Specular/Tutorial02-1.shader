Shader "Universal/Tutorial02-1"
{
    Properties //着色器的输入 
    {
        _Diffuse ("Color", Color) = (1,1,1,1) // 主颜色
        _Specular ("Specular", Color) = (1,1,1,1) // 高光颜色
        _Glossiness ("Smoothness", Range(8,256)) = 20 // 高光强度
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

        #include "Assets/Plugins/UnityToolkit/Shader//UnityToolkit.hlsl"
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

            // 支持SRP Batcher
            CBUFFER_START(UnityPerMaterial)
                //声明变量
                half3 _Specular;
                float4 _Diffuse;
                float _Glossiness;
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
                float3 ambient = _GlossyEnvironmentColor;
                // 坐标转换 将模型的法线从 模型空间转换到世界空间
                float3 worldNormal = mul(v.normalOS, (float3x3)unity_WorldToObject);
                // 获取主光源
                Light light = GetMainLight();
                // 计算漫反射
                half diffuse = LambertDiffuse(worldNormal, light.direction);
                // 计算高光
                half3 reflectDir = normalize(reflect(-light.direction, worldNormal));
                // 计算视线方向 相机位置减去顶点位置 这个算出来很大概率不是单位向量 所以要normalize
                half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.positionOS).xyz);

                half specular = pow(saturate(dot(reflectDir, viewDir)), _Glossiness);

                o.color = ambient +
                    diffuse * _Diffuse.rgb * light.color +
                    specular * _Specular.rgb * light.color;

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