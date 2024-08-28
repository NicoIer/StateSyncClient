Shader "Universal/Tutorial06-1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}// 主贴图
        _Color ("Color", Color) = (1,1,1,1) // 颜色
        _CutOff ("CutOff", Range(0,1)) = 0.5 // 阈值
    }
    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest" // 用于指明渲染队列
            "IgnoreProjector"="True" // 用于指明是否忽略投影器
            "RenderType"="TransparentCutout" // 用于指明渲染类型
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
            float4 _Color;
            float _CutOff;
        CBUFFER_END

        TEXTURE2D(_MainTex); // 声明纹理  
        SAMPLER(sampler_MainTex); // 声明纹理采样器


        // a2v -> attribute to vertex
        struct a2v
        {
            float4 positionOS: POSITION;
            float2 uv : TEXCOORD0;
            float3 normal : NORMAL;
        };

        // v2f -> vertex to fragment
        struct v2f
        {
            float4 positionCS: SV_POSITION;
            float2 uv: TEXCOORD0;
            float3 worldNormal : TEXCOORD1;
            float3 worldPos : TEXCOORD2;
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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.worldPos = TransformObjectToWorld(v.positionOS).xyz;
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                clip(col.a - _CutOff);

                Light light = GetMainLight();
                half3 lambert = LightingLambert(light.color, light.direction, i.worldNormal);
                half3 ambient = _GlossyEnvironmentColor.rgb;


                return half4(col.rgb * _Color * (lambert + ambient), 1.0);
            }
            ENDHLSL
        }
    }
}