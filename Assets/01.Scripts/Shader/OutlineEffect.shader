Shader "Custom/OutlineEffect"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0)
        _Alpha ("Alpha", Range(0, 1)) = 0.5
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _OutLineColor("OutLine color", Color) = (0,0,0,0)
        _OutLineWidth("OutLine Width", Range(0.001, 0.02)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 200

        cull front
        zwrite off

        CGPROGRAM

        #pragma surface surf NoLight vertex:vert noshadow noambient 
        #pragma target 3.0

        float4 _OutLineColor;
        float _OutLineWidth;

        void vert(inout appdata_full v){
            v.vertex.xyz += v.normal.xyz *_OutLineWidth;
        }

        struct Input{
            float4 color;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            
        }

        fixed4 LightingNoLight(SurfaceOutput s, float3 lightDir, float atten){
            fixed4 c;
            c.rgb = _OutLineColor.rgb;
            c.a = _OutLineColor.a;
            return c;
        }
        ENDCG

        cull back
        zwrite on
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Alpha;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a*_Alpha;
        }
        ENDCG
    }
    FallBack "Transparent"
}
