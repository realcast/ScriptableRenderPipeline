Shader "Hidden/HDRP/XROcclusionMesh"
{
    HLSLINCLUDE
        #pragma target 4.5
        #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

        struct Attributes
        {
            float4 vertex : POSITION;
        };

        struct Varyings
        {
            float4 vertex : SV_POSITION;
        };

        Varyings Vert(Attributes input)
        {
            Varyings output;
            output.vertex = mul(GetRawUnityObjectToWorld(), input.vertex);
            return output;
        }

        void Frag(out float4 outColor : SV_Target0, out float outputDepth : SV_Depth)
        {
            outColor = (0.0).xxxx;
            outputDepth = UNITY_NEAR_CLIP_VALUE;
        }
    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }

        Pass
        {
            ZWrite On ZTest Always Blend Off Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDHLSL
        }
    }
    Fallback Off
}
