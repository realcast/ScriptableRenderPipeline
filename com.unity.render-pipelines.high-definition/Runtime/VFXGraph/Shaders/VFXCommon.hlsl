#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/AtmosphericScattering/AtmosphericScattering.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Builtin/BuiltinData.hlsl"

void VFXEncodeMotionVector(float2 motionVec, out float4 outBuffer)
{
    EncodeMotionVector(motionVec, outBuffer);
    outBuffer.zw = 1.0f;
}

float4 VFXTransformPositionWorldToNonJitteredClip(float3 posWS)
{
#if VFX_WORLD_SPACE
    posWS = GetCameraRelativePositionWS(posWS);
#endif
    return mul(_NonJitteredViewProjMatrix, float4(posWS, 1.0f));
}

float4 VFXTransformPositionWorldToPreviousClip(float3 posWS)
{
#if VFX_WORLD_SPACE
    posWS = GetCameraRelativePositionWS(posWS);
#endif
    return mul(_PrevViewProjMatrix, float4(posWS, 1.0f));
}

#ifdef VFX_VARYING_PS_INPUTS
void VFXTransformPSInputs(inout VFX_VARYING_PS_INPUTS input)
{
#if IS_TRANSPARENT_PARTICLE && defined(VFX_VARYING_POSCS)
    // We need to readapt the SS position as our screen space positions are for a low res buffer, but we try to access a full res buffer.
    input.VFX_VARYING_POSCS.xy = _OffScreenRendering > 0 ? (input.VFX_VARYING_POSCS.xy * _OffScreenDownsampleFactor) : input.VFX_VARYING_POSCS.xy;
#endif
}
#endif

float4 VFXTransformPositionWorldToClip(float3 posWS)
{
#if VFX_WORLD_SPACE
    posWS = GetCameraRelativePositionWS(posWS);
#endif
    return TransformWorldToHClip(posWS);
}

float4 VFXTransformPositionObjectToNonJitteredClip(float3 posOS)
{
    float3 posWS = TransformObjectToWorld(posOS);
    return mul(_NonJitteredViewProjMatrix, float4(posWS, 1.0f));
}

float4 VFXTransformPositionObjectToPreviousClip(float3 posOS)
{
    float3 posWS = TransformPreviousObjectToWorld(posOS);
    return mul(_PrevViewProjMatrix, float4(posWS, 1.0f));
}

float4 VFXTransformPositionObjectToClip(float3 posOS)
{
    float3 posWS = TransformObjectToWorld(posOS);
    return VFXTransformPositionWorldToClip(posWS);
}

float3 VFXTransformPositionWorldToView(float3 posWS)
{
#if VFX_WORLD_SPACE
    posWS = GetCameraRelativePositionWS(posWS);
#endif
    return TransformWorldToView(posWS);
}

float4x4 VFXGetObjectToWorldMatrix()
{
    return GetObjectToWorldMatrix();
}

float4x4 VFXGetWorldToObjectMatrix()
{
    return GetWorldToObjectMatrix();
}

float3x3 VFXGetWorldToViewRotMatrix()
{
    return (float3x3)GetWorldToViewMatrix();
}

float3 VFXGetViewWorldPosition()
{
    float3 pos = GetCurrentViewPosition();
#if VFX_WORLD_SPACE
    pos = GetAbsolutePositionWS(pos);
#endif
    return pos;
}

float4x4 VFXGetViewToWorldMatrix()
{
    float4x4 viewToWorld = UNITY_MATRIX_I_V;
    viewToWorld._14_24_34 = VFXGetViewWorldPosition();
    return viewToWorld;
}

float VFXSampleDepth(float4 posSS)
{
    return LoadCameraDepth(posSS.xy);
}

float VFXLinearEyeDepth(float depth)
{
    return LinearEyeDepth(depth,_ZBufferParams);
}

void VFXApplyShadowBias(inout float4 posCS, inout float3 posWS, float3 normalWS)
{
}

void VFXApplyShadowBias(inout float4 posCS, inout float3 posWS)
{
}

float4 VFXApplyFog(float4 color,float4 posCS,float3 posWS)
{
#if VFX_WORLD_SPACE
    posWS = GetCameraRelativePositionWS(posWS); // posWS is absolute in World Space
#endif
    PositionInputs posInput = GetPositionInput(posCS.xy, _ScreenSize.zw, posCS.z, posCS.w, posWS, uint2(0,0));

    float3 V = GetWorldSpaceNormalizeViewDir(posWS);

    float3 volColor, volOpacity;
    EvaluateAtmosphericScattering(posInput, V, volColor, volOpacity); // Premultiplied alpha

#if VFX_BLENDMODE_ALPHA
    color.rgb = color.rgb * (1 - volOpacity) + volColor * color.a;
#elif VFX_BLENDMODE_ADD
    color.rgb = color.rgb * (1.0 - volOpacity);
#elif VFX_BLENDMODE_PREMULTIPLY
    color.rgb = color.rgb * (1 - volOpacity) + volColor * color.a;
    // Note: this formula for color is correct, assuming we apply the Over operator afterwards
    // (see the appendix in the Deep Compositing paper). But do we?
    // Additionally, we do not modify the alpha here, which is most certainly WRONG.
#endif

    return color;
}

#ifdef VFX_VARYING_PS_INPUTS
float4 VFXApplyPreExposure(float4 color, VFX_VARYING_PS_INPUTS input)
{
#ifdef VFX_VARYING_EXPOSUREWEIGHT
	float exposure = lerp(1.0f, GetCurrentExposureMultiplier(),input.VFX_VARYING_EXPOSUREWEIGHT);
#else
    float exposure = GetCurrentExposureMultiplier();
#endif
	color.xyz *= exposure;
    return color;
}
#endif
