Shader "Custom/cloudShader" 
{
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
 
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
			#pragma geometry cloud
						
            uniform float4 col;
			float x;
			float y;
			float scale;
			float increment;
			float rotationSpeed;
			float pulsationSpeed;
			float timeOffset;
			uint count;
			
            struct v2g
            {
				float4  pos : SV_POSITION;
            };

			struct g2f
			{
				float4 pos  : SV_POSITION;
			};
			
            v2g vert()
            {
                 v2g OUT;				 
				 OUT.pos = float4(x,y,0,1);				 				 

                 return OUT;
            }
			
		
				
			[maxvertexcount(16)]
			void cloud(point v2g P[1], inout PointStream<g2f> stream)
			{
				float Time = _Time + timeOffset;

				g2f OUT;
				float I = 0;
				
				for(uint i =0;i<count;i++)
				{
					float xi, yi;
					xi = cos(I+Time*rotationSpeed);
					yi = sin(I+Time*rotationSpeed);
										
					OUT.pos = mul(UNITY_MATRIX_VP,P[0].pos + float4(xi,yi,0.0f,0.0f)*scale*cos(I+Time*pulsationSpeed));
					
					stream.Append(OUT);
					I += increment;					
				}
				stream.RestartStrip();	
				
							
			}

            float4 frag(g2f IN) : COLOR
            {
                return col;
            }
 
            ENDCG
        }
    }
}
