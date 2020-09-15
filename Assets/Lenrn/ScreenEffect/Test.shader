Shader "Unlit/Test"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    _Dis("Dis",float) = 0.1
    _MaxDis("MaxDis",float ) = 10
    _MinDis("MinDis",float ) = 1
    MySrcMode ("SrcMode", Float) = 0
    MyDstMode ("DstMode", Float) = 0
    MyOff ("Off", Float) = 0
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Lighting Off ZWrite  [MyOff]  Fog { Color (0,0,0,0) }
    Blend [MySrcMode] [MyDstMode]
    LOD 100

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      

      #include "UnityCG.cginc"
      #include "Assets/Common/Shaders/Math.cginc"
      #include "Assets/ComputeShader/GPUParticle.cginc"
      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float2 uv : TEXCOORD0;
        
        float4 vertex : SV_POSITION;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      float _Dis;
      float _MaxDis;
      float _MinDis;
      
      

      


      float3 getNewPos(float3 v)
      {
        float3 worldPos =  mul(unity_ObjectToWorld, float4(0,0,0,1));

        float3 dir =   worldPos -_WorldSpaceCameraPos ;

        float4x4 i_v = inverse(UNITY_MATRIX_V);
        float3 up = float3(0,1,0);

        up =  mul(i_v,up);
        
        
        float4 q = QuaternionLookRotation(dir,up);

        v = rotate_vector_at(v,float3(0,0,0),q);

        return v;
      }

      v2f vert (appdata v)
      {
        v2f o;

        v.vertex.xyz = getNewPos(v.vertex.xyz);

        o.vertex = UnityObjectToClipPos(v.vertex);

        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        
        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        // sample the texture
        fixed4 col = tex2D(_MainTex, i.uv);
        // if(col.a<=0.1)col.a =0;
        
        return col;
      }
      ENDCG
    }
  }
}
