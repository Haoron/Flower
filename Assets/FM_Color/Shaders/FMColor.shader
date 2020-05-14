Shader "Hidden/FMColor"
{
    Properties
    {
        //_MainTex("Base", 2D) = "" {}
        //_EffectLut("Lut Effect", Range(0,1)) = 1.0

        //_PixelSize("Pixel Size", Range(1, 512)) = 0
        //_CelCuts("Cel Cuts", Range(2, 255)) = 0

        //_Tint("Tint (RGB)", Color) = (1, 1, 1, 1)      
        //_Hue("Hue", Range(0,360)) = 0
        //_Saturation("Saturation", Range(0,2)) = 1.0
        //_Brightness("Brightness", Range(0,3)) = 1.0
        //_Contrast("Contrast", Range(0,2)) = 1.0
        //_Sharpness("Sharpness", Range(0,1)) = 0

        //_EffectFresnel("Fresnel Effect", Range(0, 1)) = 0
        //_FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
        //_FresnelPower ("Fresnel Power", Range(0.001, 10)) = 1
        
        //_FresnelScanlineX ("FresnelScanlineX", Range(0, 32)) = 0
        //_FresnelScanlineY ("FresnelScanlineY", Range(0, 32)) = 0

        //_EffectFog("Fog Effect", Range(0, 1)) = 0
        //_DepthClippingFar ("Depth Clipping Far", Range(0.001, 1)) = 1
        //_DepthLevel ("Depth Level", Range(1, 5)) = 1
        //_FogColor ("Fog Color", Color) = (1,1,1,1)
        //_ForegroundColor ("Foreground Color", Color) = (1,1,1,1)
        
        //_EffectGrain ("Grain Effect", Range(0, 1)) = 0
        //_GrainSize ("Grain Size", Range(1, 3)) = 0

        //_EffectVignette ("Vignette Effect", Range(0, 1)) = 0
        //_VignetteColor ("Vignette Color", Color) = (0,0,0,1)

        //==================outline===================
        //_OutlineColor ("Outline Color", Color) = (0,0,0,1)
        //_EffectOutline ("Outline Effect", Range(0, 1)) = 0
        //_NormalMult ("Normal Outline Multiplier", Range(0,4)) = 1
        //_NormalBias ("Normal Outline Bias", Range(1,4)) = 1
        //_DepthMult ("Depth Outline Multiplier", Range(0,4)) = 1
        //_DepthBias ("Depth Outline Bias", Range(1,4)) = 1
        //==================outline===================

        //ScanlineColor
        //_ScanlineColor ("Scanline Color", Color) = (0,0,0,1)
        //_EffectScanline ("Scanline Effect", Range(0, 1)) = 0
        //_ScanlineX ("ScanlineX", Range(0, 32)) = 0
        //_ScanlineY ("ScanlineY", Range(0, 32)) = 0
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

    
    uniform sampler2D _MainTexStencil;
    
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
    
    uniform half _PaintRadius;
    uniform half _PaintBlurLevel;

    uniform half _EffectGrain;
    uniform half _GrainSize;
    
    uniform sampler2D _CameraDepthTexture;
    uniform sampler2D _CameraDepthNormalsTexture;
    uniform half4 _CameraDepthNormalsTexture_TexelSize;

    //==================outline===================
    uniform half4 _OutlineColor;
    uniform half _EffectOutline;
    uniform half _NormalMult;
    uniform half _NormalBias;
    uniform half _DepthMult;
    uniform half _DepthBias;
    
    inline half4 GetNormalDepth(float2 uv)
    {
        half4 depthnormal = tex2D(_CameraDepthNormalsTexture, uv);
        half3 normal;
        half depth;
        DecodeDepthNormal(depthnormal, depth, normal);

        return half4(normal, depth);
    }

    inline void Compare(inout half depthOutline, inout half normalOutline, half baseDepth, half3 baseNormal, float2 uv, float2 offset)
    {
        //read neighbor pixel
        half4 neighborNormalDepth = GetNormalDepth(uv + _CameraDepthNormalsTexture_TexelSize.xy * offset);
        half3 neighborNormal = neighborNormalDepth.xyz;
        half neighborDepth = neighborNormalDepth.w;
        neighborDepth = neighborDepth * _ProjectionParams.z;

        half depthDifference = baseDepth - neighborDepth;
        depthOutline = depthOutline + depthDifference;

        half3 normalDifference = baseNormal - neighborNormal;
        normalDifference = normalDifference.r + normalDifference.g + normalDifference.b;
        normalOutline = normalOutline + normalDifference;
    }

    inline half3 EdgeDetection(half3 c, float2 uv)
    {
        half4 DepthNormal = GetNormalDepth(uv);
        half3 normal = DepthNormal.xyz;
        half depth = DepthNormal.w;

        //get depth as distance from camera in units 
        depth = depth * _ProjectionParams.z;

        half depthDifference = 0;
        half normalDifference = 0;

        Compare(depthDifference, normalDifference, depth, normal, uv, float2(1, 0));
        Compare(depthDifference, normalDifference, depth, normal, uv, float2(0, 1));
        Compare(depthDifference, normalDifference, depth, normal, uv, float2(0, -1));
        Compare(depthDifference, normalDifference, depth, normal, uv, float2(-1, 0));

        depthDifference = depthDifference * _DepthMult;
        depthDifference = saturate(depthDifference);
        depthDifference = pow(depthDifference, _DepthBias);

        normalDifference = normalDifference * _NormalMult;
        normalDifference = saturate(normalDifference);
        normalDifference = pow(normalDifference, _NormalBias);

        half outline = normalDifference + depthDifference;
        return lerp(c, _OutlineColor, outline * _EffectOutline);
    }
    //==================outline===================

    uniform half _EffectFresnel;
    uniform half3 _FresnelColor;
    uniform half _FresnelPower;
    uniform half _FresnelScanlineX;
    uniform half _FresnelScanlineY;

    uniform half _EffectFog;
    uniform half3 _FogColor;
    uniform half3 _ForegroundColor;
    uniform half _DepthLevel;
    uniform half _DepthClippingFar;

    
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
    inline half3 ApplyFresnel(half3 c, half3 normals, float2 uv)
    {
        //perspective projection
        float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
        // conver the uvs into view space
        half3 viewDir = -normalize(half3((uv * 2 - 1) / p11_22, -1));
        half fresnel = 1.0 - saturate(dot(viewDir.xyz, normals));
        fresnel = pow(fresnel, _FresnelPower) * _EffectFresnel;
        
        half3 FresnelColor = _FresnelColor;
        if(_FresnelScanlineX == 0 && _FresnelScanlineY == 0) return lerp(c, FresnelColor, fresnel);

        float2 texel = _MainTex_TexelSize.xy;
        texel = IF(_PaintBlurLevel>0, texel/(1+_PaintBlurLevel), texel);

        FresnelColor = IF(uv.x % (texel.x*_FresnelScanlineX*2) < texel.x*_FresnelScanlineX, c, FresnelColor);
        FresnelColor = IF(uv.y % (texel.y*_FresnelScanlineY*2) < texel.y*_FresnelScanlineY, c, FresnelColor);

        return lerp(c, FresnelColor, fresnel);
    }
    inline half3 ApplyScanline(half3 c, float2 uv)
    {
        if(_ScanlineX == 0 && _ScanlineY == 0) return c;
        half3 ScanlineColor = _ScanlineColor;

        float2 texel = _MainTex_TexelSize.xy;
        texel = IF(_PaintBlurLevel>0, texel/(1+_PaintBlurLevel), texel);

        ScanlineColor = IF(uv.x % (texel.x*_ScanlineX*2) < texel.x*_ScanlineX, c, ScanlineColor);
        ScanlineColor = IF(uv.y % (texel.y*_ScanlineY*2) < texel.y*_ScanlineY, c, ScanlineColor);
        return lerp(c, ScanlineColor, _EffectScanline);
    }
    inline half3 ApplyFog(half3 c, half depth)
    {
        depth = saturate(pow(depth, _DepthLevel) / _DepthClippingFar);
        half3 DepthFog = lerp(_ForegroundColor, _FogColor, depth);
        return lerp(c, DepthFog, depth * _EffectFog);
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

        c = IF(_EffectFresnel > 0, ApplyFresnel(c, GetNormalDepth(uv).xyz, uv), c);
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

    inline half3 ApplyPaintEffect(float2 uv)
    {
        float3 mean[4];
        float3 sigma[4];
        mean[0]=mean[1]=mean[2]=mean[3]=sigma[0]=sigma[1]=sigma[2]=sigma[3]=float3(0,0,0);      
 
        float2 start[4] = {{-_PaintRadius, -_PaintRadius}, {-_PaintRadius, 0}, {0, -_PaintRadius}, {0, 0}};
        float2 pos;
        float3 col;
        for (int k = 0; k < 4; k++) 
        {
            for(int i = 0; i <= _PaintRadius; i++) 
            {
                for(int j = 0; j <= _PaintRadius; j++) 
                {
                    pos = float2(i, j) + start[k];
                    col = tex2D(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                    mean[k] += col;
                    sigma[k] += col * col;
                }
            }
        }
 
        float sigma2;
        float n = pow(_PaintRadius + 1, 2);
        half3 c = float3(1,1,1);
        float min = 1;
 
        for (int l = 0; l < 4; l++) 
        {
            mean[l] /= n;
            sigma[l] = abs(sigma[l] / n - mean[l] * mean[l]);
            sigma2 = sigma[l].r + sigma[l].g + sigma[l].b;
 
            //if (sigma2 < min) 
            //{
                //min = sigma2;
                //c = mean[l];
            //}
            
            c = IF(sigma2 < min, mean[l], c);
            min = IF(sigma2 < min, sigma2, min);
        }
        //if(GetLuma(c) < GetLuma(tex2D(_MainTex, uv))) c *=0.8 * (GetLuma(tex2D(_MainTex, uv)) * 0.5 + 0.5);
        //if((GetLuma(c) - GetLuma(tex2D(_MainTex, uv))) * (GetLuma(c) - GetLuma(tex2D(_MainTex, uv))) > 0.0001) c*=0.5;

        return c;
    }

    inline half3 ApplyDebug(half3 c, half x)
    {
        return IF(x > _DebugSlider - (_MainTex_TexelSize.x*10 / (1+_PaintBlurLevel)), half3(0.9,0.9,0.9), c);
    }

    fixed4 frag(v2f i) : SV_Target
    {
        float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
        half4 c = tex2D(_MainTex, uv);

        c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);    
        
        c.rgb = ApplyEffects(c.rgb, uv);

        c.rgb = IF(_EffectOutline > 0 && _PixelSize == 1, EdgeDetection(c.rgb, uv), c.rgb);
        c.rgb = IF(_EffectFog > 0, ApplyFog(c.rgb, GetNormalDepth(uv).w), c.rgb);
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

        c.rgb = IF(_EffectOutline > 0 && _PixelSize == 1, EdgeDetection(c.rgb, uv), c.rgb);
        c.rgb = IF(_EffectFog > 0, ApplyFog(c.rgb, GetNormalDepth(uv).w), c.rgb);
        c.rgb = IF(_PixelSize >= 6, ApplyPixelate(c.rgb, i.uv), c.rgb);

        c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(tex2D(_MainTex, i.uv).rgb, i.uv.x), c);
        return c;
    }

    fixed4 fragPaint(v2f i) : SV_Target
    {
        float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
        half4 c = half4(ApplyPaintEffect(uv),1);

        c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);    
        
        c.rgb = ApplyEffects(c.rgb, uv);

        c.rgb = IF(_EffectOutline > 0 && _PixelSize == 1, EdgeDetection(c.rgb, uv), c.rgb);
        c.rgb = IF(_EffectFog > 0, ApplyFog(c.rgb, GetNormalDepth(uv).w), c.rgb);
        c.rgb = IF(_PixelSize>=6, ApplyPixelate(c.rgb, i.uv), c.rgb);

        c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(tex2D(_MainTex, i.uv).rgb, i.uv.x), c);
        return c;
    }

    fixed4 fragLinearPaint(v2f i) : SV_Target
    {
        float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
        half4 c = half4(ApplyPaintEffect(uv),1);

        c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);

        c.rgb = sqrt(c.rgb);
        c.rgb = ApplyEffects(c.rgb, uv);
        c.rgb *= c.rgb;

        c.rgb = IF(_EffectOutline > 0 && _PixelSize == 1, EdgeDetection(c.rgb, uv), c.rgb);
        c.rgb = IF(_EffectFog > 0, ApplyFog(c.rgb, GetNormalDepth(uv).w), c.rgb);
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
            //#pragma target 3.0
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragLinear
            //#pragma target 3.0
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragPaint
            //#pragma target 3.0
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragLinearPaint
            //#pragma target 3.0
            ENDCG
        }
    }

    Fallback off
}
