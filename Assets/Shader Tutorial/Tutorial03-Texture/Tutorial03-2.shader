Shader "Universal/CustomTutorial03-2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}// 主贴图
        _DiffuseColor("Diffuse Color", Color) = (0.5, 0.5, 0.5, 1)
        _SpecularColor("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Glossiness("Glossiness", Range(8, 256)) = 20

        _BumpMap("Normal Map", 2D) = "bump" {}// 法线贴图
        _BumpScale("Bump Scale", Range(-1,1)) = 0
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


        TEXTURE2D(_MainTex); // 声明纹理  
        SAMPLER(sampler_MainTex); // 声明纹理采样器

        TEXTURE2D(_BumpMap); // 声明法线贴图
        SAMPLER(sampler_BumpMap); // 声明法线贴图采样器

        // 支持SRP Batcher
        CBUFFER_START(UnityPerMaterial)
            //声明变量
            float4 _MainTex_ST;
            float4 _DiffuseColor;
            float4 _SpecularColor;
            float _Glossiness;
            float4 _BumpMap_ST;
            float _BumpScale;
        CBUFFER_END


        // a2v -> attribute to vertex
        struct a2v
        {
            float4 positionOS: POSITION;
            float4 uv : TEXCOORD0;
            float4 tangent : TANGENT; // 切线
            float3 normal : NORMAL;
        };

        // v2f -> vertex to fragment
        struct v2f
        {
            float4 positionCS: SV_POSITION;
            float4 uv: TEXCOORD0;
            float3 tangentLightDir : TEXCOORD1;
            float3 tangentViewDir : TEXCOORD2;
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

                // 可以用float4 存储 两个图的uv 可以减少插值的次数
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv, _BumpMap);

                // 下面的三个向量实际上是一个三维空间的基向量, 通过这三个向量可以构造一个从切线空间到世界空间的旋转矩阵
                float3 worldNormal = TransformObjectToWorldNormal(v.normal); // y
                float3 worldTangent = TransformObjectToWorldDir(v.tangent.xyz); // x
                float3 worldBinormal = (cross(worldNormal, worldTangent) * v.tangent.w); // z
                // 构造旋转矩阵 从切线空间到世界空间
                float3x3 worldToTangent = float3x3(worldTangent, worldBinormal, worldNormal);


                // 切线空间的光线方向
                float3 positionWS = TransformObjectToWorld(v.positionOS).xyz;
                o.tangentLightDir = mul(worldToTangent, _MainLightPosition.xyz - positionWS);
                o.tangentViewDir = mul(worldToTangent, TransformWorldToViewDir(positionWS));


                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                half4 dumpNormal = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv.zw);

                //解压法线： 1 = x^2 + y^2 + z^2
                half3 tangentNormal = UnpackNormal(dumpNormal);

                // 强度缩放
                tangentNormal.xy *= _BumpScale;
                // 重新计算z
                tangentNormal.z = sqrt(
                    1 - saturate(tangentNormal.x * tangentNormal.x + tangentNormal.y * tangentNormal.y));
                // tangentNormal = normalize(tangentNormal);

                // 存疑 这里拿的
                Light light = GetMainLight();
                float diffuse = LambertDiffuse(tangentNormal, i.tangentLightDir);
                half3 diffuseColor = _DiffuseColor.rgb * diffuse * light.color.rgb;
                half3 ambient = _GlossyEnvironmentColor;
                half3 halfDir = normalize(i.tangentLightDir + i.tangentViewDir);
                half specular = pow(max(0, dot(halfDir, tangentNormal)), _Glossiness);
                half3 specularColor = _SpecularColor.rgb * specular * light.color.rgb;


                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
                half3 color = col * (ambient + diffuseColor) + specularColor;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}