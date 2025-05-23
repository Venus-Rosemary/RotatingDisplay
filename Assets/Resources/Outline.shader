Shader "Custom/TransparentOutline" {
    Properties {
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.03
        _Transparency ("Transparency", Range(0, 1)) = 1.0
    }
    
    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
        }
        
        // 第一个Pass：绘制描边
        Pass {
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
            };
            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _Transparency;
            
            v2f vert (appdata v) {
                v2f o;
                float3 normal = normalize(v.normal);
                float3 pos = v.vertex + normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(pos);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                return float4(_OutlineColor.rgb, _OutlineColor.a * _Transparency);
            }
            ENDCG
        }
        
        // 第二个Pass：透明主体
        Pass {
            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
            };
            
            float _Transparency;
            
            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                return fixed4(0,0,0,0); // 完全透明
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
