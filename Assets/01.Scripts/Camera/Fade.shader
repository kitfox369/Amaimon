Shader "Custom/Fade"
{
    Properties {
    _TintColor ("TintColor", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Fade ("Fade", Range(0, 1)) = 1
    }

    SubShader {
        Pass{
            CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Fade;

			fixed4 frag(v2f_img i) : COLOR
			{
				fixed4 currentText = tex2D(_MainTex, i.uv);
				
				fixed4 color = lerp(currentText, 0, _Fade);

				currentText.rgb = color;
				
				return currentText;
            }

        ENDCG
        }
    }
}
