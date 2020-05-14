using System;
using UnityEngine;
using System.Collections;

using UnityEngine.Rendering;

[Serializable]
public class FMFilterBasic
{
    public Color TintColor = Color.white;
    [Range(0, 360)]
    public float Hue = 0.0f;
    [Range(-1, 1)]
    public float Saturation = 0.0f;
    [Range(-1, 3)]
    public float Brightness = 0.0f;
    [Range(-1, 1)]
    public float Contrast = 0.0f;
    [Range(0, 1)]
    public float Sharpness = 0.0f;
}

[Serializable]
public class FMFilterFog
{
    [Range(0f, 1f)]
    public float Contribution = 0f;
    [Range(0.001f, 3f)]
    public float DepthLevel = 1f;
    [Range(0.001f, 1f)]
    public float DepthClippingFar = 1f;
    public Color FogColor = new Color(1f, 0.95f, 0.9f);
    public Color ForegroundColor = new Color(1f, 0.8f, 0.4f);
}


[Serializable]
public class FMFilterFresnel
{
    [Range(0, 1f)]
    public float Contribution = 0;
    [Range(0.001f, 10f)]
    public float FresnelPower = 1;
    public Color FresnelColor = new Color(1f, 0.875f, 0f, 1f);

    [Range(0, 32f)]
    public int FresnelScanlineX = 0;
    [Range(0, 32f)]
    public int FresnelScanlineY = 0;
}

[Serializable]
public class FMFilterGrain
{
    [Range(0, 1f)]
    public float Contribution = 0;
    [Range(1, 3)]
    public float Size = 1f;
}

[Serializable]
public class FMFilterVignette
{
    [Range(0, 1f)]
    public float Contribution = 0;
    public Color Color = Color.black;
}

[Serializable]
public class FMFilterOutline
{
    [Range(0, 1f)]
    public float Contribution = 0;
    public Color Color = Color.black;
    [Range(0, 4f)]
    public float NormalMult = 1;
    [Range(1, 4f)]
    public float NormalBias = 1;
    [Range(0, 4f)]
    public float DepthMult = 1;
    [Range(1, 4f)]
    public float DepthBias = 1;
}

[Serializable]
public class FMFilterScanline
{
    [Range(0, 1f)]
    public float Contribution = 0;
    public Color Color = new Color(0f, 0f, 1f, 1f);

    [Range(0, 32f)]
    public int ScanlineX = 0;
    [Range(0, 32f)]
    public int ScanlineY = 0;
}

[Serializable]
public class FMFilterFXAA
{
    public bool Enable = false;
    //[Range(0.0312f, 0.0833f)]
    //public float ContrastThreshold = 0.0312f;
    //[Range(0.063f, 0.333f)]
    //public float RelativeThreshold = 0.063f;
    [Range(0f, 1f)]
    public float SubpixelBlending = 1f;
    public bool LowQuality = false;
}

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FMColor : MonoBehaviour
{
    Camera Cam;
    [HideInInspector]
    public Material material;

    [HideInInspector]
    public Material FXAAMat;

    [Header("~~~~~~~~ COLOR TONE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~")]
    [Space(10)]
    [Range(0, 1)]
    public float LutContribution = 1.0f;
    public LutPack LutMode = LutPack.LUT_RGB;
    private LutPack previousLutMode;

    public int PaintRadius = 0;
    public int PaintBlurLevel = 0;

    //[HideInInspector]
    public Texture2D LookupTexture;
    private Texture2D previousTexture;
    private Texture3D Converted3DLut;
    private int LutSize;

    [Header("~~~~~~~~ PIXELATE & CEL ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~")]
    [Space(10)]
    [Range(1, 512)]
    public int PixelSize = 1;
    [Range(2, 255)]
    public int CelCuts = 255;

    [Header("~~~~~~~~ COLOR EFFECTS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~")]
    [Space(10)]
    public FMFilterBasic BasicSettings = new FMFilterBasic();
    [Space]
    public FMFilterFresnel FresnelSettings = new FMFilterFresnel();
    [Space]
    public FMFilterFog FogSettings = new FMFilterFog();
    [Space]
    public FMFilterGrain GrainSettings = new FMFilterGrain();
    [Space]
    public FMFilterVignette VignetteSettings = new FMFilterVignette();
    [Space]
    public FMFilterOutline OutlineSettings = new FMFilterOutline();
    [Space]
    public FMFilterScanline ScanlineSettings = new FMFilterScanline();
    [Space]
    public FMFilterFXAA FXAASettings = new FMFilterFXAA();

    [Space(10)]
    [Range(0, 1)]
    public float DebugSlider = 0;
    public bool EnableTouchSlider = false;
    bool AllowSlide = false;

    DepthTextureMode CamDepthTextureMode;
    private void Start()
    {
        if (Cam == null) Cam = GetComponent<Camera>();
        Cam.depthTextureMode = DepthTextureMode.DepthNormals;
        CamDepthTextureMode = Cam.depthTextureMode;

        CheckLUT();
    }

    void CheckLUT()
    {
        //======================Change Lookup texture======================
        if (LookupTexture == null)
        {
            LookupTexture = Resources.Load<Texture2D>("LUT/" + LutMode.ToString());
        }
        else
        {
            if (LookupTexture.name != LutMode.ToString()) LookupTexture = Resources.Load<Texture2D>("LUT/" + LutMode.ToString());
        }
        //======================Change Lookup texture======================
    }

    private void Update()
    {
        if (Application.isPlaying && EnableTouchSlider)
        {
            float _value = (float)Input.mousePosition.x / (float)Screen.width;
            if (Input.GetMouseButtonDown(0))
            {
                if (Mathf.Abs(DebugSlider - _value) < 0.05) AllowSlide = true;
            }
            if (Input.GetMouseButton(0))
            {
                if (AllowSlide) DebugSlider = _value;
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (AllowSlide) StartCoroutine(DebugSliderLerp(0));
            }
        }

        UpdateMaterialSettings();
    }


    private void OnDestroy()
    {
        if (Converted3DLut != null) DestroyImmediate(Converted3DLut);
        Converted3DLut = null;
    }

    public void SetIdentityLut()
    {
        if (!SystemInfo.supports3DTextures) return;
        if (Converted3DLut != null) DestroyImmediate(Converted3DLut);

        int dim = 32;
        Color[] newC = new Color[dim * dim * dim];
        float oneOverDim = 1.0f / (1.0f * dim - 1.0f);

        for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                for (int k = 0; k < dim; k++)
                {
                    newC[i + (j * dim) + (k * dim * dim)] = new Color((i * 1.0f) * oneOverDim, (j * 1.0f) * oneOverDim, (k * 1.0f) * oneOverDim, 1.0f);
                }
            }
        }

        Converted3DLut = new Texture3D(dim, dim, dim, TextureFormat.RGB24, false);
        Converted3DLut.SetPixels(newC);
        Converted3DLut.Apply();
        LutSize = Converted3DLut.width;
        Converted3DLut.wrapMode = TextureWrapMode.Clamp;
    }

    public bool ValidDimensions(Texture2D tex2d)
    {
        if (tex2d == null) return false;

        int h = tex2d.height;
        if (h != Mathf.FloorToInt(Mathf.Sqrt(tex2d.width))) return false;
        return true;
    }

    internal bool Convert(Texture2D lookupTexture)
    {
        if (!SystemInfo.supports3DTextures) return false;

        if (lookupTexture == null)
        {
            SetIdentityLut();
        }
        else
        {
            if (Converted3DLut != null) DestroyImmediate(Converted3DLut);

            if (lookupTexture.mipmapCount > 1)
            {
                Debug.LogError("Lookup texture must not have mipmaps");
                return false;
            }

            try
            {
                int dim = lookupTexture.height;

                if (!ValidDimensions(lookupTexture))
                {
                    Debug.LogError("Lookup texture dimensions must be a power of two. The height must equal the square root of the width.");
                    return false;
                }

                var c = lookupTexture.GetPixels();
                var newC = new Color[c.Length];

                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        for (int k = 0; k < dim; k++)
                        {
                            int j_ = dim - j - 1;
                            newC[i + (j * dim) + (k * dim * dim)] = c[k * dim + i + j_ * dim * dim];
                        }
                    }
                }

                Converted3DLut = new Texture3D(dim, dim, dim, TextureFormat.ARGB32, false);
                Converted3DLut.SetPixels(newC);
                Converted3DLut.Apply();
                LutSize = Converted3DLut.width;
                Converted3DLut.wrapMode = TextureWrapMode.Clamp;
            }
            catch (Exception ex)
            {
                Debug.LogError("Unable to convert texture to LUT texture, make sure it is read/write. Error: " + ex);
            }
        }

        return true;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Converted3DLut == null) SetIdentityLut();
        if (Converted3DLut == null || material == null)
        //if (Converted3DLut == null || (MatPaint == null && PaintBlurLevel > 0) || (MatBasic == null && PaintBlurLevel == 0))
        {
            Graphics.Blit(source, destination);
            return;
        }

        //check camera depth mode
        if (FogSettings.Contribution > 0 || FresnelSettings.Contribution > 0 || OutlineSettings.Contribution > 0)
        {
            if (CamDepthTextureMode != DepthTextureMode.DepthNormals)
            {
                Cam.depthTextureMode = DepthTextureMode.DepthNormals;
                CamDepthTextureMode = Cam.depthTextureMode;
            }
        }
        else
        {
            if (CamDepthTextureMode != DepthTextureMode.None)
            {
                Cam.depthTextureMode = DepthTextureMode.None;
                CamDepthTextureMode = Cam.depthTextureMode;
            }
        }

        int depth = FogSettings.Contribution > 0 || FresnelSettings.Contribution > 0 || OutlineSettings.Contribution > 0 ? 16 : 0;
        int pass = (QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0) + ((PaintBlurLevel > 0 || PaintRadius > 0) ? 2 : 0);
        if (PaintBlurLevel > 0)
        {
            RenderTexture RTexture = RenderTexture.GetTemporary((source.width / (1 + PaintBlurLevel)), (source.height / (1 + PaintBlurLevel)), depth, source.format);
            Graphics.Blit(source, RTexture);

            material.SetTexture("_MainTex", RTexture);
            //allow FXAA?
            if (FXAASettings.Enable)
            {
                RenderTexture MainTex = RenderTexture.GetTemporary(RTexture.width, RTexture.height, depth, RTexture.format);
                Graphics.Blit(RTexture, MainTex, material, pass);
                Graphics.Blit(MainTex, destination, FXAAMat);
                RenderTexture.ReleaseTemporary(MainTex);
            }
            else
            {
                Graphics.Blit(RTexture, destination, material, pass);
            }
            RenderTexture.ReleaseTemporary(RTexture);
        }
        else
        {
            material.SetTexture("_MainTex", source);
            //allow FXAA?
            if (FXAASettings.Enable)
            {
                RenderTexture MainTex = RenderTexture.GetTemporary(source.width, source.height, depth, source.format);
                Graphics.Blit(source, MainTex, material, pass);
                Graphics.Blit(MainTex, destination, FXAAMat);
                RenderTexture.ReleaseTemporary(MainTex);
            }
            else
            {
                Graphics.Blit(source, destination, material, pass);
            }
        }
    }

    public void UpdateMaterialSettings()
    {
        if (material == null)
        {
            material = new Material(Shader.Find("Hidden/FMColor"));
            //material.hideFlags = HideFlags.HideAndDontSave;
        }

        //if (material.shader != Shader.Find("Hidden/FMColor"))
        //{
        //    material = new Material(Shader.Find("Hidden/FMColor"));
        //}

        if (LutMode != previousLutMode)
        {
            previousLutMode = LutMode;
            if (LutMode != LutPack.LUT_Custom) CheckLUT();
        }

        if (LookupTexture != null)
        {
            if (LookupTexture.name != LutMode.ToString()) LutMode = LutPack.LUT_Custom;
        }
        else
        {
            LutMode = LutPack.LUT_Custom;
        }

        if (LookupTexture != previousTexture)
        {
            previousTexture = LookupTexture;
            Convert(LookupTexture);
        }

        //SetMaterial(PaintRadius > 0 ? MatPaint : MatBasic);
        SetMaterial(material);

        if (FXAASettings.Enable)
        {
            if (FXAAMat == null)
            {
                FXAAMat = new Material(Shader.Find("Hidden/FMFXAA"));
                //FXAAMat.hideFlags = HideFlags.HideAndDontSave;
            }

            FXAAMat.SetFloat("_SubpixelBlending", FXAASettings.SubpixelBlending);
            FXAAMat.SetFloat("_DebugSlider", DebugSlider);

            if (FXAASettings.LowQuality)
            {
                FXAAMat.EnableKeyword("LOW_QUALITY");
            }
            else
            {
                FXAAMat.DisableKeyword("LOW_QUALITY");
            }
        }

#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
    }

    void SetMaterial(Material _mat)
    {
        _mat.SetFloat("_PaintRadius", PaintRadius);
        _mat.SetFloat("_PaintBlurLevel", PaintBlurLevel);

        _mat.SetTexture("_ClutTex", Converted3DLut);
        _mat.SetFloat("_EffectLut", LutContribution);

        _mat.SetFloat("_Scale", (LutSize - 1) / (1.0f * LutSize));
        _mat.SetFloat("_Offset", 1.0f / (2.0f * LutSize));

        _mat.SetColor("_TintColor", BasicSettings.TintColor);
        _mat.SetFloat("_Hue", BasicSettings.Hue);
        _mat.SetFloat("_Saturation", BasicSettings.Saturation + 1.0f);
        _mat.SetFloat("_Brightness", BasicSettings.Brightness + 1.0f);
        _mat.SetFloat("_Contrast", BasicSettings.Contrast + 1.0f);
        _mat.SetFloat("_Sharpness", BasicSettings.Sharpness);

        _mat.SetFloat("_EffectFog", FogSettings.Contribution);
        _mat.SetFloat("_DepthLevel", FogSettings.DepthLevel);
        _mat.SetFloat("_DepthClippingFar", FogSettings.DepthClippingFar);

        _mat.SetColor("_FogColor", FogSettings.FogColor);
        _mat.SetColor("_ForegroundColor", FogSettings.ForegroundColor);

        _mat.SetFloat("_FresnelPower", FresnelSettings.FresnelPower);
        _mat.SetColor("_FresnelColor", FresnelSettings.FresnelColor);
        _mat.SetFloat("_EffectFresnel", FresnelSettings.Contribution);
        _mat.SetFloat("_FresnelScanlineX", FresnelSettings.FresnelScanlineX);
        _mat.SetFloat("_FresnelScanlineY", FresnelSettings.FresnelScanlineY);


        //Pixel Size
        _mat.SetFloat("_PixelSize", PixelSize);
        //CelCuts
        _mat.SetFloat("_CelCuts", CelCuts);

        _mat.SetColor("_ScanlineColor", ScanlineSettings.Color);
        _mat.SetFloat("_EffectScanline", ScanlineSettings.Contribution);
        _mat.SetFloat("_ScanlineX", ScanlineSettings.ScanlineX);
        _mat.SetFloat("_ScanlineY", ScanlineSettings.ScanlineY);


        //_EffectGrain
        _mat.SetFloat("_EffectGrain", GrainSettings.Contribution);
        //_GrainSize
        _mat.SetFloat("_GrainSize", GrainSettings.Size);
        //_EffectVignette
        _mat.SetFloat("_EffectVignette", VignetteSettings.Contribution);
        //_VignetteColor
        _mat.SetColor("_VignetteColor", VignetteSettings.Color);

        _mat.SetColor("_OutlineColor", OutlineSettings.Color);
        _mat.SetFloat("_EffectOutline", OutlineSettings.Contribution);
        _mat.SetFloat("_NormalMult", OutlineSettings.NormalMult);
        _mat.SetFloat("_NormalBias", OutlineSettings.NormalBias);
        _mat.SetFloat("_DepthMult", OutlineSettings.DepthMult);
        _mat.SetFloat("_DepthBias", OutlineSettings.DepthBias);

        _mat.SetFloat("_DebugSlider", DebugSlider);
    }

    IEnumerator DebugSliderLerp(float _value)
    {
        AllowSlide = false;
        while (Mathf.Abs(DebugSlider - _value) > 0.01)
        {
            DebugSlider = Mathf.Lerp(DebugSlider, _value, Time.fixedDeltaTime * 10f);
            yield return new WaitForFixedUpdate();
        }

        DebugSlider = _value;
        yield return null;
    }

    public void Action_NextLUT()
    {
        int LutID = (int)LutMode;
        LutID++;
        if (LutID >= Enum.GetNames(typeof(LutPack)).Length - 1) LutID = 0;
        LutMode = (LutPack)LutID;
        LutContribution = 1f;
        UpdateMaterialSettings();
    }

    public void Action_PreviousLUT()
    {
        int LutID = (int)LutMode;
        LutID--;
        if (LutID < 0) LutID = Enum.GetNames(typeof(LutPack)).Length - 2;
        LutMode = (LutPack)LutID;
        LutContribution = 1f;
        UpdateMaterialSettings();
    }

    //templates
    #region Templates
    public void ResetAllSettings()
    {
        //================LUT===============
        LutContribution = 1f;
        PixelSize = 1;
        CelCuts = 255;

        PaintRadius = 0;
        PaintBlurLevel = 0;

        //================Basic===============
        BasicSettings.TintColor = Color.white;
        BasicSettings.Hue = 0f;
        BasicSettings.Saturation = 0f;
        BasicSettings.Brightness = 0f;
        BasicSettings.Contrast = 0f;
        BasicSettings.Sharpness = 0f;

        //================Fresnel===============
        FresnelSettings.Contribution = 0f;
        FresnelSettings.FresnelPower = 1f;
        FresnelSettings.FresnelColor = Color.yellow;
        FresnelSettings.FresnelScanlineX = 0;
        FresnelSettings.FresnelScanlineY = 0;

        //================Fog===============
        FogSettings.Contribution = 0f;
        FogSettings.DepthLevel = 1f;
        FogSettings.DepthClippingFar = 1f;
        FogSettings.FogColor = Color.white;
        FogSettings.ForegroundColor = Color.white;

        //================Grain===============
        GrainSettings.Contribution = 0f;
        GrainSettings.Size = 1f;

        //================Vignette===============
        VignetteSettings.Contribution = 0f;
        VignetteSettings.Color = Color.black;

        //================Scanline===============
        ScanlineSettings.Contribution = 0f;
        ScanlineSettings.Color = Color.blue;
        ScanlineSettings.ScanlineX = 0;
        ScanlineSettings.ScanlineY = 0;


        //================Outline===============
        OutlineSettings.Contribution = 0f;
        OutlineSettings.Color = Color.black;
        OutlineSettings.NormalMult = 1f;
        OutlineSettings.NormalBias = 1f;
        OutlineSettings.DepthMult = 1f;
        OutlineSettings.DepthBias = 1f;

        //================FXAA===============
        FXAASettings.Enable = false;
        FXAASettings.SubpixelBlending = 1f;
        FXAASettings.LowQuality = true;

        UpdateMaterialSettings();
    }

    public void Action_TemplateMinimum()
    {
        ResetAllSettings();
        UpdateMaterialSettings();
    }

    public void Action_TemplateToon()
    {
        ResetAllSettings();

        CelCuts = 8;
        BasicSettings.Saturation = 0.5f;

        GrainSettings.Contribution = 0.5f;
        GrainSettings.Size = 2f;

        UpdateMaterialSettings();
    }
    public void Action_TemplateMono()
    {
        ResetAllSettings();

        BasicSettings.Saturation = -1f;
        GrainSettings.Contribution = 0.5f;
        GrainSettings.Size = 2;

        UpdateMaterialSettings();
    }
    public void Action_TemplateContrast()
    {
        ResetAllSettings();

        CelCuts = 4;
        BasicSettings.Contrast = 0.5f;

        UpdateMaterialSettings();
    }

    public void Action_TemplateScanline()
    {
        ResetAllSettings();

        ScanlineSettings.Contribution = 0.125f;
        ScanlineSettings.ScanlineY = 3;

        UpdateMaterialSettings();
    }
    public void Action_TemplateGrid()
    {
        ResetAllSettings();

        ScanlineSettings.Contribution = 0.125f;
        ScanlineSettings.Color = Color.black;
        ScanlineSettings.ScanlineX = 4;
        ScanlineSettings.ScanlineY = 4;

        UpdateMaterialSettings();
    }
    public void Action_TemplatePixelate()
    {
        ResetAllSettings();

        PixelSize = 8;
        GrainSettings.Contribution = 0.5f;
        GrainSettings.Size = 1f;

        UpdateMaterialSettings();
    }
    public void Action_TemplateFilmGrain()
    {
        ResetAllSettings();

        BasicSettings.Saturation = -0.25f;
        BasicSettings.Brightness = -0.25f;
        BasicSettings.Contrast = 0.25f;

        GrainSettings.Contribution = 0.8f;
        GrainSettings.Size = 2.5f;

        ScanlineSettings.Contribution = 0.1f;
        ScanlineSettings.Color = Color.white;
        ScanlineSettings.ScanlineY = 4;

        VignetteSettings.Contribution = 0.75f;
        VignetteSettings.Color = Color.black;

        UpdateMaterialSettings();
    }

    public void Action_TemplateFresnelScanline()
    {
        ResetAllSettings();

        FresnelSettings.Contribution = 1f;
        FresnelSettings.FresnelPower = 5;
        FresnelSettings.FresnelScanlineY = 2;

        UpdateMaterialSettings();
    }
    public void Action_TemplateFresnelColor()
    {
        ResetAllSettings();

        FresnelSettings.Contribution = 1f;
        FresnelSettings.FresnelPower = 5;

        UpdateMaterialSettings();
    }
    public void Action_TemplateFresnelGrid()
    {
        ResetAllSettings();

        FresnelSettings.Contribution = 1f;
        FresnelSettings.FresnelPower = 5;
        FresnelSettings.FresnelScanlineX = 2;
        FresnelSettings.FresnelScanlineY = 2;

        UpdateMaterialSettings();
    }
    public void Action_TemplateOutline()
    {
        ResetAllSettings();
        OutlineSettings.Contribution = 1f;
        OutlineSettings.NormalMult = 4f;
        OutlineSettings.NormalBias = 2f;
        OutlineSettings.DepthMult = 4f;
        OutlineSettings.DepthBias = 2f;

        UpdateMaterialSettings();
    }

    public void Action_TemplateBest()
    {
        LutContribution = 1;
        PixelSize = 1;
        CelCuts = 12;

        BasicSettings.Hue = 0;
        BasicSettings.Saturation = 0;
        BasicSettings.Brightness = 0;
        BasicSettings.Contrast = 0;
        BasicSettings.Sharpness = 0;

        FresnelSettings.Contribution = 0.25f;
        FogSettings.Contribution = 1f;
        GrainSettings.Contribution = 0.75f;
        VignetteSettings.Contribution = 0.5f;

        FresnelSettings.FresnelScanlineX = 0;
        FresnelSettings.FresnelScanlineY = 2;

        ScanlineSettings.Contribution = 0.1f;
        ScanlineSettings.Color = Color.blue;
        ScanlineSettings.ScanlineX = 2;
        ScanlineSettings.ScanlineY = 0;

        UpdateMaterialSettings();
    }

    public void Action_TemplatePaintSoft()
    {
        ResetAllSettings();

        CelCuts = 64;

        PaintRadius = 2;
        PaintBlurLevel = 3;

        BasicSettings.Contrast = 0.25f;

        ScanlineSettings.Contribution = 0.025f;
        ScanlineSettings.Color = Color.white;
        ScanlineSettings.ScanlineY = 4;

        UpdateMaterialSettings();
    }
    public void Action_TemplatePaintHard()
    {
        ResetAllSettings();

        CelCuts = 64;

        PaintRadius = 2;
        PaintBlurLevel = 3;

        BasicSettings.Contrast = 0.25f;
        BasicSettings.Sharpness = 0.75f;

        ScanlineSettings.Contribution = 0.025f;
        ScanlineSettings.Color = Color.white;
        ScanlineSettings.ScanlineY = 4;

        UpdateMaterialSettings();
    }

    public void Action_TemplatePaintMono()
    {
        ResetAllSettings();

        CelCuts = 64;

        PaintRadius = 2;
        PaintBlurLevel = 3;

        BasicSettings.Saturation = -1f;
        BasicSettings.Contrast = 0.5f;

        ScanlineSettings.Contribution = 0.025f;
        ScanlineSettings.Color = Color.white;
        ScanlineSettings.ScanlineY = 4;

        UpdateMaterialSettings();
    }

    public void Action_TemplatePaintDraft()
    {
        ResetAllSettings();

        CelCuts = 4;

        PaintRadius = 2;
        PaintBlurLevel = 3;

        BasicSettings.Saturation = -1f;
        BasicSettings.Brightness = -0.15f;
        BasicSettings.Contrast = 0.5f;
        BasicSettings.Sharpness = 0.25f;

        FresnelSettings.Contribution = 1f;
        FresnelSettings.FresnelPower = 5f;
        FresnelSettings.FresnelColor = Color.white;
        FresnelSettings.FresnelScanlineY = 2;

        ScanlineSettings.Contribution = 0.025f;
        ScanlineSettings.Color = Color.white;
        ScanlineSettings.ScanlineY = 4;

        OutlineSettings.Contribution = 0.75f;

        UpdateMaterialSettings();
    }

    public void Action_TemplatePaintSoftMobile()
    {
        ResetAllSettings();

        CelCuts = 64;

        PaintRadius = 1;
        PaintBlurLevel = 3;

        BasicSettings.Contrast = 0.25f;

        ScanlineSettings.Contribution = 0.025f;
        ScanlineSettings.Color = Color.white;
        ScanlineSettings.ScanlineY = 4;

        UpdateMaterialSettings();
    }
    public void Action_TemplatePaintHardMobile()
    {
        ResetAllSettings();

        CelCuts = 64;

        PaintRadius = 1;
        PaintBlurLevel = 3;

        BasicSettings.Contrast = 0.25f;
        BasicSettings.Sharpness = 0.75f;

        ScanlineSettings.Contribution = 0.025f;
        ScanlineSettings.Color = Color.white;
        ScanlineSettings.ScanlineY = 4;

        UpdateMaterialSettings();
    }

    public void Action_TemplatePaintMonoMobile()
    {
        ResetAllSettings();

        CelCuts = 64;

        PaintRadius = 1;
        PaintBlurLevel = 3;

        BasicSettings.Saturation = -1f;
        BasicSettings.Contrast = 0.5f;

        ScanlineSettings.Contribution = 0.025f;
        ScanlineSettings.Color = Color.white;
        ScanlineSettings.ScanlineY = 4;

        UpdateMaterialSettings();
    }

    public void Action_TemplatePaintDraftMobile()
    {
        ResetAllSettings();

        CelCuts = 4;

        PaintRadius = 1;
        PaintBlurLevel = 3;

        BasicSettings.Saturation = -1f;
        BasicSettings.Brightness = -0.15f;
        BasicSettings.Contrast = 0.5f;
        BasicSettings.Sharpness = 0.25f;

        FresnelSettings.Contribution = 1f;
        FresnelSettings.FresnelPower = 5f;
        FresnelSettings.FresnelColor = Color.white;
        FresnelSettings.FresnelScanlineY = 2;

        ScanlineSettings.Contribution = 0.025f;
        ScanlineSettings.Color = Color.white;
        ScanlineSettings.ScanlineY = 4;

        OutlineSettings.Contribution = 0.75f;

        UpdateMaterialSettings();
    }

    #endregion
}



