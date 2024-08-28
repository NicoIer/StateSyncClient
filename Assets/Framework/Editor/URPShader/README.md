# URPShader模版
```hlsl
Shader "#NAME#"
{
    Properties //着色器的输入 
    {
        _BaseMap ("Texture", 2D) = "white" {}// 主贴图
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
            float4 _BaseMap_ST;
        CBUFFER_END

        TEXTURE2D(_BaseMap); //贴图采样  
        SAMPLER(sampler_BaseMap);

        // a2v -> attribute to vertex
        struct a2v //顶点着色器
        {
            float4 positionOS: POSITION;
            float3 normalOS: TANGENT;
            half4 vertexColor: COLOR;
            float2 uv : TEXCOORD0;
        };

        // v2f -> vertex to fragment
        struct v2f //片元着色器
        {
            float4 positionCS: SV_POSITION;
            float2 uv: TEXCOORD0;
            half4 vertexColor: COLOR;
        };
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.vertexColor = v.vertexColor;
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                half res = lerp(i.vertexColor, col, i.vertexColor.g).x;
                return half4(res, res, res, 1.0);
            }
            ENDHLSL
        }
    }
}
```




## BuildingIn管线空间变换方法
UnityObjectToClipPos(v.vertex) 将模型空间下的顶点转换到齐次裁剪空间
UnityObjectToWorldNormal(v.normal) 将模型空间下的法线转换到世界空间(已归一化)
UnityObjectToWorldDir (v.tangent) 将模型空间下的切线转换到世界空间(已归一化)
UnityWorldSpaceLightDir (i.worldPos) 世界空间下顶点到灯光方向的向量(未归一化)
UnityWorldSpaceViewDir (i.worldPos) 世界空间下顶点到视线方向的向量(未归一化)

## URP管线空间变换方法
TransformObjectToWorld(float3 positionOS) 模型到世界空间
TransformWorldToObject(float3 positionWS) 世界到模型空间
TransformWorldToView(float3 positionWS) 世界到视图空间
TransformObjectToHClip(float3 positionOS) 模型到裁剪空间
TransformWorldToHClip(float3 positionWS) 世界到裁剪空间
TransformViewToHClip(float3 positionVS) 视图到裁剪空间
TransformObjectToWorldDir(real3 dirOS) 模型到世界空间向量
TransformWorldToObjectDir(real3 dirWS) 世界到模型空间向量
TransformWorldToViewDir(real3 dirWS) 世界到视图空间向量
TransformWorldToHClipDir(real3 directionWS) 世界到裁剪空间向量
TransformObjectToWorldNormal(float3 normalOS) 模型到世界空间法线向量
TransformWorldToObjectNormal(float3 normalWS) 世界到模型空间法线向量
