Shader "Custom/SemanticColors"
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
            nointerpolation float4 col : TEXCOORD0;
         };
        struct VertIn
        {
            float4 position : POSITION;
            fixed2 uv : TEXCOORD0;
        };

         vertexOutput vert(VertIn input)
             // vertex shader 
          {
             uint scol = (int)input.uv.x; //get semantic color stored in per-vertex UV coordinates
             float r = ((scol & 0x00ff0000) >> 16) / 255.0;
             float g = ((scol & 0x0000ff00) >> 8) / 255.0;
             float b = (scol & 0x000000ff) / 255.0;
             vertexOutput output; // we don't need to type 'struct' here
             output.pos = UnityObjectToClipPos(input.position);
             output.col = float4(r, g, b, 1.0);
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
