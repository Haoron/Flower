using System;
using UnityEngine;
using System.Collections;

using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FMColorFast : MonoBehaviour
{
    Camera Cam;
    [HideInInspector]
    public Material material;

    [Header("~~~~~~~~ COLOR TONE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~")]
    [Space(10)]
    [Range(0, 1)]
    public float LutContribution = 1.0f;
    public LutPack LutMode = LutPack.LUT_RGB;
    private LutPack previousLutMode;

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
    public FMFilterGrain GrainSettings = new FMFilterGrain();
    [Space]
    public FMFilterVignette VignetteSettings = new FMFilterVignette();
    [Space]
    public FMFilterScanline ScanlineSettings = new FMFilterScanline();

    [Space(10)]
    [Range(0, 1)]
    public float DebugSlider = 0;
    public bool EnableTouchSlider = false;
    bool AllowSlide = false;

    private void Start()
    {
        if (Cam == null) Cam = GetComponent<Camera>();
		Cam.depthTextureMode = DepthTextureMode.None;

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
        {
            Graphics.Blit(source, destination);
            return;
        }

		int pass = (QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0);
		material.SetTexture("_MainTex", source);
		Graphics.Blit(source, destination, material, pass);
	}

    public void UpdateMaterialSettings()
    {
		if (material == null) material = new Material(Shader.Find("Hidden/FMColorFast"));

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

        SetMaterial(material);

#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
    }

    void SetMaterial(Material _mat)
    {
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

        //================Basic===============
        BasicSettings.TintColor = Color.white;
        BasicSettings.Hue = 0f;
        BasicSettings.Saturation = 0f;
        BasicSettings.Brightness = 0f;
        BasicSettings.Contrast = 0f;
        BasicSettings.Sharpness = 0f;

        
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

    #endregion
}



