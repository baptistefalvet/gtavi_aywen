#ifndef SPEED_LINES_HELPER
#define SPEED_LINES_HELPER

float3 random3(float3 c)
{
    float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
    float3 r;
    r.z = frac(512.0 * j);
    j *= .125;
    r.x = frac(512.0 * j);
    j *= .125;
    r.y = frac(512.0 * j);
    return r - 0.5;
}

float simplex3d(float3 p)
{
    float3 s = floor(p + dot(p, 0.3333333));
    float3 x = p - s + dot(s, 0.1666667);
    float3 e = step(0.0, x - x.yzx);
    float3 i1 = e * (1.0 - e.zxy);
    float3 i2 = 1.0 - e.zxy * (1.0 - e);
    float3 x1 = x - i1 + 0.1666667;
    float3 x2 = x - i2 + 2.0 * 0.1666667;
    float3 x3 = x - 1.0 + 3.0 * 0.1666667;
    float4 w, d;
    w.x = dot(x, x);
    w.y = dot(x1, x1);
    w.z = dot(x2, x2);
    w.w = dot(x3, x3);
    w = max(0.6 - w, 0.0);
    d.x = dot(random3(s), x);
    d.y = dot(random3(s + i1), x1);
    d.z = dot(random3(s + i2), x2);
    d.w = dot(random3(s + 1.0), x3);
    w *= w;
    w *= w;
    d *= w;
    return dot(d, 52.0);
}

void SpeedLines_float(float2 uv,float2 screenSize,float Time,float raduis,float edge,out float lines)
{
    float time = Time * 2.;
    float scale = 50.0;
    float2 p = float2(0.5 * screenSize.x / screenSize.y, 0.5) + normalize(uv) * min(length(uv), 0.05);
    float3 p3 = scale * 0.25 * float3(p.xy, 0) + float3(0, 0, time * 0.025);
    float noise = simplex3d(p3 * 32.0) * 0.5 + 0.5;
    float dist = abs(clamp(length(uv) / raduis, 0.0, 1.0) * noise * 2. - 1.);
    float stepped = smoothstep(edge - .5, edge + .5, noise * (1.0 - pow(dist, 4.0)));
    float final = smoothstep(edge - 0.05, edge + 0.05, noise * stepped);
    
    lines = final;
}

#endif