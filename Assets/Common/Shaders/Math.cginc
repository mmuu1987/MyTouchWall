#ifndef _MATH_INCLUDED_
  #define _MATH_INCLUDED_

  #define QUATERNION_IDENTITY float4(0, 0, 0, 1)


  //#define RadianRatio 57.29578;
  //����ת�Ƕ�
  #define RadianRatio 57.29578
  #define P 3.141592653

  // Quaternion multiplication
  // http://mathworld.wolfram.com/Quaternion.html
  float4 qmul(float4 q1, float4 q2) {
    return float4(
    q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
    q1.w * q2.w - dot(q1.xyz, q2.xyz)
    );
  }

  // Vector rotation with a quaternion
  // http://mathworld.wolfram.com/Quaternion.html
  float3 rotate_vector(float3 v, float4 r) {
    float4 r_c = r * float4(-1, -1, -1, 1);
    return qmul(r, qmul(float4(v, 0), r_c)).xyz;
  }

  float3 rotate_vector_at(float3 v, float3 center, float4 r) {
    float3 dir = v - center;
    return center + rotate_vector(dir, r);
  }

  // A given angle of rotation about a given axis
  float4 rotate_angle_axis(float angle, float3 axis) {
    float sn = sin(angle * 0.5);
    float cs = cos(angle * 0.5);
    return float4(axis * sn, cs);
  }


  float4 q_conj(float4 q) {
    return float4(-q.x, -q.y, -q.z, q.w);
  }

  float4x4 look_at_matrix(float3 at, float3 eye, float3 up) {
    float3 zaxis = normalize(at - eye);
    float3 xaxis = normalize(cross(up, zaxis));
    float3 yaxis = cross(zaxis, xaxis);
    return float4x4(
    xaxis.x, yaxis.x, zaxis.x, 0,
    xaxis.y, yaxis.y, zaxis.y, 0,
    xaxis.z, yaxis.z, zaxis.z, 0,
    0, 0, 0, 1
    );
  }

  // http://stackoverflow.com/questions/349050/calculating-a-lookat-matrix
  float4x4 look_at_matrix(float3 forward, float3 up) {
    float3 xaxis = cross(forward, up);
    float3 yaxis = up;
    float3 zaxis = forward;
    return float4x4(
    xaxis.x, yaxis.x, zaxis.x, 0,
    xaxis.y, yaxis.y, zaxis.y, 0,
    xaxis.z, yaxis.z, zaxis.z, 0,
    0, 0, 0, 1
    );
  }

  // http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
  float4 matrix_to_quaternion(float4x4 m) {

    float tr = m[0][0] + m[1][1] + m[2][2];
    float4 q = float4(0, 0, 0, 0);

    if (tr > 0) {
      float s = sqrt(tr + 1.0) * 2; // S=4*qw 
      q.w = 0.25 * s;
      q.x = (m[2][1] - m[1][2]) / s;
      q.y = (m[0][2] - m[2][0]) / s;
      q.z = (m[1][0] - m[0][1]) / s;
      } else if ((m[0][0] > m[1][1]) && (m[0][0] > m[2][2])) {
      float s = sqrt(1.0 + m[0][0] - m[1][1] - m[2][2]) * 2; // S=4*qx 
      q.w = (m[2][1] - m[1][2]) / s;
      q.x = 0.25 * s;
      q.y = (m[0][1] + m[1][0]) / s;
      q.z = (m[0][2] + m[2][0]) / s;
      } else if (m[1][1] > m[2][2]) {
      float s = sqrt(1.0 + m[1][1] - m[0][0] - m[2][2]) * 2; // S=4*qy
      q.w = (m[0][2] - m[2][0]) / s;
      q.x = (m[0][1] + m[1][0]) / s;
      q.y = 0.25 * s;
      q.z = (m[1][2] + m[2][1]) / s;
      } else {
      float s = sqrt(1.0 + m[2][2] - m[0][0] - m[1][1]) * 2; // S=4*qz
      q.w = (m[1][0] - m[0][1]) / s;
      q.x = (m[0][2] + m[2][0]) / s;
      q.y = (m[1][2] + m[2][1]) / s;
      q.z = 0.25 * s;
    }

    return q;
  }

  float4 QuaternionLookRotation(float3 forward, float3 up)
  {
    

    

    float3 vector1 = normalize(forward);
    float3 vector2 = normalize(cross(up, vector1));
    float3 vector3 = cross(vector1, vector2);
    float m00 = vector2.x;
    float m01 = vector2.y;
    float m02 = vector2.z;
    float m10 = vector3.x;
    float m11 = vector3.y;
    float m12 = vector3.z;
    float m20 = vector1.x;
    float m21 = vector1.y;
    float m22 = vector1.z;


    float num8 = (m00 + m11) + m22;
    float4 quaternion =float4(0,0,0,0);
    if (num8 > 0)
    {
      float num = (float)sqrt(num8 + 1);
      quaternion.w = num * 0.5f;
      num = 0.5f / num;
      quaternion.x = (m12 - m21) * num;
      quaternion.y = (m20 - m02) * num;
      quaternion.z = (m01 - m10) * num;
      return quaternion;
    }
    if ((m00 >= m11) && (m00 >= m22))
    {
      float num7 = (float)sqrt(((1 + m00) - m11) - m22);
      float num4 = 0.5f / num7;
      quaternion.x = 0.5f * num7;
      quaternion.y = (m01 + m10) * num4;
      quaternion.z = (m02 + m20) * num4;
      quaternion.w = (m12 - m21) * num4;
      return quaternion;
    }
    if (m11 > m22)
    {
      float num6 = (float)sqrt(((1 + m11) - m00) - m22);
      float num3 = 0.5f / num6;
      quaternion.x = (m10 + m01) * num3;
      quaternion.y = 0.5f * num6;
      quaternion.z = (m21 + m12) * num3;
      quaternion.w = (m20 - m02) * num3;
      return quaternion;
    }
    float num5 = (float)sqrt(((1 + m22) - m00) - m11);
    float num2 = 0.5f / num5;
    quaternion.x = (m20 + m02) * num2;
    quaternion.y = (m21 + m12) * num2;
    quaternion.z = 0.5f * num5;
    quaternion.w = (m01 - m10) * num2;
    return quaternion;
  }



  float4x4 inverse(float4x4 input)
  {
    #define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
    
    float4x4 cofactors = float4x4(
    minor(_22_23_24, _32_33_34, _42_43_44), 
    -minor(_21_23_24, _31_33_34, _41_43_44),
    minor(_21_22_24, _31_32_34, _41_42_44),
    -minor(_21_22_23, _31_32_33, _41_42_43),
    
    -minor(_12_13_14, _32_33_34, _42_43_44),
    minor(_11_13_14, _31_33_34, _41_43_44),
    -minor(_11_12_14, _31_32_34, _41_42_44),
    minor(_11_12_13, _31_32_33, _41_42_43),
    
    minor(_12_13_14, _22_23_24, _42_43_44),
    -minor(_11_13_14, _21_23_24, _41_43_44),
    minor(_11_12_14, _21_22_24, _41_42_44),
    -minor(_11_12_13, _21_22_23, _41_42_43),
    
    -minor(_12_13_14, _22_23_24, _32_33_34),
    minor(_11_13_14, _21_23_24, _31_33_34),
    -minor(_11_12_14, _21_22_24, _31_32_34),
    minor(_11_12_13, _21_22_23, _31_32_33)
    );
    #undef minor
    return transpose(cofactors) / determinant(input);
  }


  float EaseInQuad(float start, float end, float value)
  {
    end -= start;
    return end * value * value + start;
  }

  float EaseOutQuad(float start, float end, float value)
  {
    end -= start;
    return -end * value * (value - 2) + start;
  }

  


#endif // _MATH_INCLUDED_
