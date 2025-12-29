Shader "Unlit/stimShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorOutterCircle ("Color outter circle", Color) = (0,1,0,1)
        _ColorInnerCircle ("Color inner circle", Color) = (1,1,1,1)
        _ColorHole ("Color hole", Color) = (0,0,0,1)
        _RadiusOutterCircle("Radius outter circle", float) = 0.25
        _RadiusInnerCircle("Radius inner circle", float) = 0.05
        _RadiusHole("Radius hole", float) = 0.005
        _HolePos("hole position", vector) = (0,0,0,0)
        _ShallMakeHole("Shall make hole feedback dots", int) = 1

    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"

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

            float4 _ColorInnerCircle;
            float4 _ColorOutterCircle;
            float4 _ColorHole;
            float _RadiusOutterCircle;
            float _RadiusInnerCircle;
            float _RadiusHole;
            float4 _HolePos;
            int _ShallMakeHole;



            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //float4 col = tex2D(_MainTex, i.uv);
                float4 col = _ColorOutterCircle;

                //center coordinate to middle of raw image
                float a = i.uv.x-0.5;
                float b = i.uv.y-0.5;

                //make a circle
                if (a*a + b*b > pow(_RadiusOutterCircle,2))
                {
                    return col = float4(0,0,0,0);
                }
                
                else if ( a*a+b*b < pow(_RadiusInnerCircle,2))
                {
                    col = _ColorInnerCircle;
                }

                if (_ShallMakeHole){
                    if (pow(a-_HolePos.x, 2) + pow(b-_HolePos.y,2) < pow(_RadiusHole,2))
                    {
                    col = _ColorHole;
                    }
                }
                return col;
            }
            ENDCG
        }
    }
}


