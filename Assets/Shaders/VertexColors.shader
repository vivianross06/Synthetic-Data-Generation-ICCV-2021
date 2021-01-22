Shader "Custom/VertexColors"
{
    SubShader{
      Pass {
         CGPROGRAM

         #pragma vertex vert // vert function is the vertex shader 
         #pragma fragment frag // frag function is the fragment shader

         // for multiple vertex output parameters an output structure 
         // is defined:
         struct vertexOutput {
            float4 pos : SV_POSITION;
            //nointerpolation float4 col : TEXCOORD0;
            float4 col : TEXCOORD0;
         };
        struct VertIn
        {
            float4 position : POSITION;
            float4 color : COLOR;
        };

         vertexOutput vert(VertIn input)
             // vertex shader 
          {
             vertexOutput output; // we don't need to type 'struct' here
             output.pos = UnityObjectToClipPos(input.position);
             output.col = input.color;
             // Here the vertex shader writes output data
             // to the output structure. 
          return output;
          }

       float4 frag(vertexOutput input) : COLOR // fragment shader
       {
          return input.col;
       // Here the fragment shader returns the "col" input 
       // parameter with semantic TEXCOORD0 as nameless
       // output parameter with semantic COLOR.
 }

 ENDCG
}
    }
}
