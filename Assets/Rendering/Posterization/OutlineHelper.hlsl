#ifndef OUTLINE_HELPER
#define OUTLINE_HELPER


void GetOffsetedCoords_float(float2 uv, float2 texelSize,float offsetMul, out float2 bl, out float2 br, out float2 tl, out float2 tr)
{
    bl = uv + texelSize * offsetMul * float2(-1, -1);
    br = uv + texelSize * offsetMul * float2(-1, 1);
    tl = uv + texelSize * offsetMul * float2(1, -1);
    tr = uv + texelSize * offsetMul * float2(1, 1);
}


void NormalEdgeIndicator_float(float3 normalEgdeBias,float3 normal,float3 neighborNormal,float depthDifference,out float indcator)
{
    float normalDiff = dot(normal - neighborNormal, normalEgdeBias);
    float normalIndicator = clamp(smoothstep(-0.01, 0.01, normalDiff), 0.0, 1.0);
    float depthIndicator = clamp(sign(depthDifference * 0.25 + 0.025), 0.0, 1.0);
    indcator = (1.0 - dot(normal, neighborNormal)) * normalIndicator * depthIndicator;
}

void Luminance_float(float3 color, out float lum)
{
    lum = (0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b);
}

#endif