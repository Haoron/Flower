Shader "Hidden/FMColorFast"
{
	Properties
	{
	}

	CGINCLUDE

#include "UnityCG.cginc"
#define IF(a, b, c) lerp(b, c, step((half) (a), 0));

	struct v2f
	{
		half4 pos : SV_POSITION;
		float2 uv  : TEXCOORD0;
	};
    
    uniform sampler2D _MainTex;
    uniform half4 _MainTex_TexelSize;
    
    uniform half _Scale;
    uniform half _Offset;
    
    uniform sampler3D _ClutTex;
    uniform half _EffectLut;
	uniform half4 _TintColor;

	uniform half _Hue;
	uniform half _Saturation;
	uniform half _Brightness;
	uniform half _Contrast;
    uniform half _Sharpness;

    uniform half _PixelSize;
    uniform half _CelCuts;

    uniform half _EffectGrain;
    uniform half _GrainSize;
    
    
    uniform half3 _ScanlineColor;
    uniform half _EffectScanline;
    uniform half _ScanlineX;
    uniform half _ScanlineY;

    uniform half _EffectVignette;
    uniform half3 _VignetteColor;
    
    uniform half _DebugSlider;

    inline half GetLuma(half3 c) { return sqrt(dot(c, half3(0.299, 0.587, 0.114))); }
	inline half3 ApplyHue(half3 c, half hue)
	{
		half angle = radians(hue);
		half3 k = half3(0.57735, 0.57735, 0.57735);
		half cosAngle = cos(angle);      
		return c * cosAngle + cross(k, c) * sin(angle) + k * dot(k, c) * (1 - cosAngle);
	}   
    inline half3 ApplyContrast(half3 c) { return (((c - 0.5f) * _Contrast) + 0.5f); }
    inline half3 ApplyBrightness(half3 c) { return c * _Brightness; }
    inline half3 ApplySaturation(half3 c) { return lerp(dot(c, half3(0.299, 0.587, 0.114)), c, _Saturation); }

    inline float2 EdgeUV(float2 uv, half OffX, half OffY) { return uv + float2(_MainTex_TexelSize.x * OffX, _MainTex_TexelSize.y * OffY); }
	inline half3 ApplySharpness(half3 c, float2 uv)
	{
        half3 sum_col = c;
        sum_col += tex2D(_MainTex, EdgeUV(uv, -1,0)).rgb;
        sum_col += tex2D(_MainTex, EdgeUV(uv, 1,0)).rgb;
        sum_col += tex2D(_MainTex, EdgeUV(uv, 0,-1)).rgb;
        sum_col += tex2D(_MainTex, EdgeUV(uv, 0,1)).rgb;
        return saturate((c * 2) - (sum_col/5));
	}

    inline half3 ApplyScanline(half3 c, float2 uv)
    {
        if(_ScanlineX == 0 && _ScanlineY == 0) return c;
        half3 ScanlineColor = _ScanlineColor;
        float2 texel = _MainTex_TexelSize.xy;
        
        ScanlineColor = IF(uv.x % (texel.x*_ScanlineX*2) < texel.x*_ScanlineX, c, ScanlineColor);
        ScanlineColor = IF(uv.y % (texel.y*_ScanlineY*2) < texel.y*_ScanlineY, c, ScanlineColor);
        return lerp(c, ScanlineColor, _EffectScanline);
    }

    inline half random (float2 p) { return frac(sin(dot(p.xy, float2(_Time.y % 10 * 0.1, 65.115))) * 2773.8856); }
    inline half3 ApplyGrain(half3 c, float2 uv)
    {
        half Luminance = GetLuma(c);
        float2 normUV = uv; 
        normUV.x *= (_MainTex_TexelSize.z) / _GrainSize;
        normUV.y *= (_MainTex_TexelSize.w) / _GrainSize;

        float2 ipos = floor(normUV);  // get the integer coords
        half rand = random(ipos);
        half3 mv = half3(1,0,0);
        mv = IF(c.g > c.r && c.g > c.b, half3(0,1,0), mv);
        mv = IF(c.b > c.r && c.b > c.g, half3(0,0,1), mv);

        half3 Noise = ApplyHue(mv* c, rand * 360);
        return lerp(c, Noise * (0.05 + 0.2 * (0.5-abs(0.5-Luminance))) + c * 0.9, _EffectGrain);
    }
    inline half3 ApplyVignette(half3 c, float2 uv)
    {
        float2 coord = (uv - 0.5) * 2;
        half rf = sqrt(dot(coord, coord)) * _EffectVignette;
        half rf2_1 = rf * rf + 1.0;
        half e = 1.0 / (rf2_1 * rf2_1);
        return lerp(c, _VignetteColor, (1-e));
    }

    inline half3 ApplyEffects(half3 c, float2 uv)
    {
        //Cel Shading
        if(GetLuma(c) > 0) c = IF(_CelCuts < 255, normalize(c) * ceil(GetLuma(c) * _CelCuts) * (1/_CelCuts), c);

        c = IF(_Hue > 0, ApplyHue(c, _Hue), c);
        c = IF(_Contrast != 1, ApplyContrast(c), c);
        c = IF(_Brightness != 1, ApplyBrightness(c), c);

        //Lut
        c = IF(_EffectLut > 0, lerp(c, tex3D(_ClutTex, c * _Scale + _Offset).rgb, _EffectLut), c);
        c *= _TintColor;

        c = IF(_EffectScanline > 0, ApplyScanline(c, uv), c);
        
        c = IF(_EffectGrain > 0, ApplyGrain(c, uv), c);
        c = IF(_EffectVignette > 0, ApplyVignette(c, uv), c);
        c = IF(_Saturation != 1, ApplySaturation(c), c);
        return c;
    }
    
	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;      
		return o;
	}

    inline half3 ApplyPixelate(half3 c, float2 uv)
    {
        float2 PS = (_PixelSize / _ScreenParams.xy);
        return IF(( ((uv.x+PS.x*0.5) % PS.x < PS.x *0.1) || ((uv.y+PS.y*0.5) % PS.y < PS.y * 0.1) ), c * GetLuma(c), c);
    }

    inline half3 ApplyDebug(half3 c, half x)
    {
        return IF(x > _DebugSlider, half3(0.9,0.9,0.9), c);
    }

	fixed4 frag(v2f i) : SV_Target
	{
        float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
        half4 c = tex2D(_MainTex, uv);

		c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);    
		
        c.rgb = ApplyEffects(c.rgb, uv);
        c.rgb = IF(_PixelSize>=6, ApplyPixelate(c.rgb, i.uv), c.rgb);

        c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(tex2D(_MainTex, i.uv).rgb, i.uv.x), c);
        return c;
	}

	fixed4 fragLinear(v2f i) : SV_Target
	{
        float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
        half4 c = tex2D(_MainTex, uv);

        c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);

        c.rgb = sqrt(c.rgb);
        c.rgb = ApplyEffects(c.rgb, uv);
        c.rgb *= c.rgb;

        c.rgb = IF(_PixelSize >= 6, ApplyPixelate(c.rgb, i.uv), c.rgb);

        c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(tex2D(_MainTex, i.uv).rgb, i.uv.x), c);
        return c;
	}

	ENDCG

	Subshader
	{
        ZTest Always Cull Off ZWrite Off  
		Pass
		{       
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			ENDCG
		}

		Pass
		{    
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragLinear
			ENDCG
		}      
	}

	Fallback off
}
