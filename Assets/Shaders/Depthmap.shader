Shader "Custom/Depthmap"
{
    SubShader
    {
        Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldpos: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MyColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float MAX_DEPTH = 65.535f; //max value of uint16 converted from mm to m

                fixed4 col = tex2D(_MainTex, i.uv);
                col.xyz = i.worldpos;
                float dist = -mul(UNITY_MATRIX_V, i.worldpos-_WorldSpaceCameraPos).z;
                float display = min(dist, MAX_DEPTH) / MAX_DEPTH;
                col = fixed4(display, display, display, 1+(dist*1000.0f)); //use alpha channel to store dist. Add 1 to ensure opaque color.
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

            ENDCG
        }
    }
}