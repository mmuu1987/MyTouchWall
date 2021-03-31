  Shader "Instanced/MyShaderFluid" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		
		_Color("Color",color) = (1,1,1,1)
    }
    SubShader {

        Pass {

            Tags { "RenderType"="Opaque" "Queue"="Transparent"}

			Blend SrcAlpha  OneMinusSrcAlpha
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "Assets/Common/Shaders/Math.cginc"
            #include "Assets/ComputeShader/GPUParticle.cginc"
          UNITY_DECLARE_TEX2DARRAY(_TexArr);

            sampler2D _MainTex;
			
			fixed4 _Color;
            float4 _WHScale;
      	 

			#if SHADER_TARGET >= 45
            StructuredBuffer<PosAndDir> positionBuffer;
			StructuredBuffer<float4> colorBuffer;
            #endif

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                uint index:SV_InstanceID;//告诉片元，输送实例ID 必须是uint,
             
            };
            v2f vert (appdata_base v, uint instanceID : SV_InstanceID)
            {
            #if SHADER_TARGET >= 45
                float4 data = positionBuffer[instanceID].position;
            #else
                float4 data = 0;
            #endif
                float3 initialVelocity = positionBuffer[instanceID].initialVelocity;//获取宽高
                float3 localPosition = v.vertex * data.w;
                localPosition.x *= _WHScale.x * initialVelocity.x;
                localPosition.y *= _WHScale.y * initialVelocity.y;
                float3 worldPosition = data.xyz + localPosition;

               

                v2f o;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.uv_MainTex = v.texcoord;
				o.index = instanceID;
                return o;
            }

            fixed4 frag (v2f i, uint instanceID : SV_InstanceID) : SV_Target
            {
               int index = positionBuffer[instanceID].picIndex;
               fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.uv_MainTex, index));
			  
               return col;
            }

            ENDCG
        }
		
    }

	FallBack "Diffuse" 
}