Shader "Unlit/Galaxy"
{
  Properties {
    
    
    _Dis("Dis",float) = 0.1
    _MaxDis("MaxDis",float ) = 10
    _MinDis("MinDis",float ) = 1
    _MainTex("MainTex", 2D) = "white" {}  
    MySrcMode ("SrcMode", Float) = 0
    MyDstMode ("DstMode", Float) = 0
    
  }
  SubShader {
    cull Off

    Pass {

      Tags { "Queue"="Transparent"   "RenderType"="Transparent"   "IgnoreProjection" = "True"}
      Blend [MySrcMode] [MyDstMode]
      Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
      
      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag
      #pragma target 4.5
      //#pragma multi_compile_instancing  
      #include "UnityCG.cginc"
      #include "Assets/Common/Shaders/Math.cginc"
      #include "Assets/ComputeShader/GPUParticle.cginc"

      UNITY_DECLARE_TEX2DARRAY(_TexArr);


      sampler2D _MainTex;
      fixed4 _Color;
      float _RADIUSBUCE;
      float4 _WHScale;
      
      float _Dis;
      float _MaxDis;
      float _MinDis;

      
      #if SHADER_TARGET >= 45
        StructuredBuffer<PosAndDir> positionBuffer;
        StructuredBuffer<float4> colorBuffer;
      #endif

      struct v2f
      {
        float4 pos : SV_POSITION;
        float2 uv_MainTex : TEXCOORD0;
        float2 RadiusBuceVU : TEXCOORD1;
        float distance :TEXCOORD2;
        uint index:SV_InstanceID;//告诉片元，输送实例ID
        
      };
      
      //根据参数适当缩放面片大小
      float3 processDistance(float3 pos, float3 worldPos)
      {

        float distance = length(_WorldSpaceCameraPos - worldPos);

        if(distance>=_MaxDis)distance = _MaxDis;
        else if(distance<=_MinDis) distance = _MinDis;

        pos *= (distance *_Dis);

        return pos;
      }
      float3 getNewPos(float3 v,float3 worldPos)
      {
        float3 dir =   worldPos -_WorldSpaceCameraPos ;

        float4x4 i_v = inverse(UNITY_MATRIX_V);
        float3 up = float3(0,1,0);

        up =  mul(i_v,up);
        
        
        float4 q = QuaternionLookRotation(dir,up);

        v = rotate_vector_at(v,float3(0,0,0),q);

        return v;
      }
      v2f vert (appdata_base v, uint instanceID : SV_InstanceID)
      {
        #if SHADER_TARGET >= 45
          float4 data = positionBuffer[instanceID].position;
        #else
          float4 data = 0;
        #endif
        v2f o;

        

        float3 initialVelocity = positionBuffer[instanceID].initialVelocity;//获取宽高
        
        float3 worldPos =  data.xyz;

        v.vertex.x *= (_WHScale.x * initialVelocity.x);
        v.vertex.y *= (_WHScale.y * initialVelocity.y);
        
        float3 newVector = getNewPos(v.vertex.xyz,worldPos);

        newVector = newVector * data.w;

        newVector = processDistance(newVector,worldPos);

        float3 localPos = data.xyz + newVector;

        o.pos = UnityObjectToClipPos(float4(localPos,v.vertex.w));
        
        o.distance = length(worldPos -_WorldSpaceCameraPos );
        o.uv_MainTex = v.texcoord;
        o.index = instanceID;
        return o;
      }

      fixed4 frag (v2f i, uint instanceID : SV_InstanceID) : COLOR
      {
        fixed4 col = fixed4(1,0,0,1);
        fixed4 col2 = tex2D(_MainTex,i.uv_MainTex);

        int index = positionBuffer[instanceID].picIndex;
        col = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.uv_MainTex, index));
        
        float dis = i.distance;
        if(dis <= 20)
        {
          float f = 1- (20-dis)/(20);

          fixed4 col3 = lerp(col,col2,f);

          if(col3.a>=0.3)col3.a =1;
          return col3;
        }

        return col2;
      }

      ENDCG
    }
    
  }

  FallBack "Diffuse" 
}
