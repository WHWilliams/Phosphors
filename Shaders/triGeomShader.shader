Shader "Custom/triGeomShader" 
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
			#pragma geometry geomLine
			
			struct agentData
			{
				float2 pos;
				float2 vel;				
				float life;
				int targetIndex;
				int energy;
			};

            uniform StructuredBuffer<agentData> agentBuffer;
			uniform StructuredBuffer<int> triBuffer;            
			uniform float4 col;		
			uniform float unitScale;
			

            struct v2g
            {
				float4  pos : SV_POSITION;			   
				float2 vNorm : TEXCOORD0;
            };

			struct g2f
			{
				float4 pos  : SV_POSITION;
			};
			
            v2g vert(uint id : SV_VertexID)
            {
                 v2g OUT;
				 int i = triBuffer[id];
				 OUT.pos = float4(agentBuffer[i].pos,0, 1);				 
				 OUT.vNorm = normalize(agentBuffer[i].vel);

                 return OUT;
            }
			
			// solid version
			[maxvertexcount(3)]
			void geomSolid(point v2g P[1], inout TriangleStream<g2f> stream)
			{
				
				g2f output;
				float4 forward = float4(P[0].vNorm,0,0)/2 * unitScale;
				float4 left = forward.yxzw;
				left.x = -left.x;
				left -= forward;
				float4 right = forward.yxzw;
				right.y = -right.y;
				right -= forward;
				

				output.pos = mul(UNITY_MATRIX_VP,P[0].pos + forward);
				stream.Append(output);
				output.pos = mul(UNITY_MATRIX_VP,P[0].pos + right);
				stream.Append(output);
				output.pos = mul(UNITY_MATRIX_VP,P[0].pos + left);
				stream.Append(output);
				stream.RestartStrip();

			}

			// line version
			[maxvertexcount(4)]
			void geomLine(point v2g P[1], inout LineStream<g2f> stream)
			{
				g2f output;
				float4 forward = float4(P[0].vNorm,0,0)/2 * unitScale;
				float4 left = forward.yxzw;
				left.x = -left.x;
				left -= forward;
				float4 right = forward.yxzw;
				right.y = -right.y;
				right -= forward;				

				output.pos = mul(UNITY_MATRIX_VP,P[0].pos + forward);
				stream.Append(output);
				output.pos = mul(UNITY_MATRIX_VP,P[0].pos + right);
				stream.Append(output);				
				output.pos = mul(UNITY_MATRIX_VP,P[0].pos + left);
				stream.Append(output);				
				output.pos = mul(UNITY_MATRIX_VP,P[0].pos + forward);
				stream.Append(output);

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
