#ifndef _PARTICLE_COMPUTE_COMMON_INCLUDED_
  #define _PARTICLE_COMPUTE_COMMON_INCLUDED_

  #define THREAD_X 8
  #define THREAD_Y 1
  #define THREAD_Z 1




  RWStructuredBuffer<PosAndDir> positionBuffer;
  RWStructuredBuffer<float4> colorBuffer;
  //�߽���buff
  RWStructuredBuffer<float4> boundaryBuffer;

  float x;

  float y;

  float z;

  float RadianRatio =  57.29578;

  float GetCross(float2 p1, float2 p2, float2 p)
  {
    return (p2.x - p1.x) * (p.y - p1.y) - (p.x - p1.x) * (p2.y - p1.y);
  }
  
  bool ContainsQuadrangle(float2 leftDownP2, float2 leftUpP1, float2 rightDownP3, float2 rightUpP4, float2 p)
  {

    float value1 = GetCross(leftUpP1, leftDownP2, p);

    float value2 = GetCross(rightDownP3, rightUpP4, p);

    if (value1 * value2 < 0) return false;

    float value3 = GetCross(leftDownP2, rightDownP3, p);

    float value4 = GetCross(rightUpP4, leftUpP1, p);

    if (value3 * value4 < 0) return false;

    return true;
  }

  float3 CalculateCubicBezierPoint(float t, float3 p0, float3 p1, float3 p2)
  {
    float u = 1 - t;
    float tt = t * t;
    float uu = u * u;

    float3 p = uu * p0;
    p += 2 * u * t * p1;
    p += tt * p2;

    return p;
  }

  //获得贝塞尔曲线的数组
  float3 CalculateCubicBezierPoint(float t, float3 p0, float3 p1, float3 p2,float3 p3)
  {
    float u = 1 - t;
    float uu = u * u;
    float uuu = u * u * u;
    float tt = t * t;
    float ttt = t * t * t;
    float3 p = p0 * uuu;
    p += 3 * p1 * t * uu;
    p += 3 * p2 * tt * u;
    p += p3 * ttt;
    return p;


    
  }





#endif // _PARTICLE_COMPUTE_COMMON_INCLUDED_
