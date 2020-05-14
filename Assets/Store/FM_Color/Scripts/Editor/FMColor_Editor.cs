using System;
using System.Collections;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMColor))]
[CanEditMultipleObjects]
public class FMColor_Editor : Editor
{
    GUISkin skin;
    private Texture2D logo;

    private bool ShowPreset = false;
    private bool ShowBasicSettings = false;
    private bool ShowFresnelSettings = false;
    private bool ShowFogSettings = false;
    private bool ShowGrainSettings = false;
    private bool ShowVignetteSettings = false;
    private bool ShowOutlineSettings = false;
    private bool ShowScanlineSettings = false;
    private bool ShowFXAASettings = false;

    private FMColor FC;
    private LutPack LutMode;
    private float LutContribution;
    private Texture2D LookupTexture;

    private int PixelSize;
    private int CelCuts;
    private int PaintBlurLevel;
    private int PaintRadius;

    private float DebugSlider;
    private bool EnableTouchSlider;


    private FMFilterBasic BasicSettings = new FMFilterBasic();
    private FMFilterFresnel FresnelSettings = new FMFilterFresnel();
    private FMFilterFog FogSettings = new FMFilterFog();
    private FMFilterGrain GrainSettings = new FMFilterGrain();
    private FMFilterVignette VignetteSettings = new FMFilterVignette();
    private FMFilterOutline OutlineSettings = new FMFilterOutline();
    private FMFilterScanline ScanlineSettings = new FMFilterScanline();
    private FMFilterFXAA FXAASettings = new FMFilterFXAA();

    public override void OnInspectorGUI()
    {
        if (FC == null) FC = (FMColor)target;

        if (logo == null) logo = Resources.Load<Texture2D>("Logo/" + "Logo_FMColor");
        if (logo != null)
        {
            const float maxLogoWidth = 430.0f;
            EditorGUILayout.Separator();
            float w = EditorGUIUtility.currentViewWidth;
            Rect r = new Rect();
            r.width = Math.Min(w - 40.0f, maxLogoWidth);
            r.height = r.width / 4.886f;
            Rect r2 = GUILayoutUtility.GetRect(r.width, r.height);
            r.x = r2.x;
            r.y = r2.y;
            GUI.DrawTexture(r, logo, ScaleMode.ScaleToFit);
            if (GUI.Button(r, "", new GUIStyle()))
            {
                Application.OpenURL("http://frozenmist.com");
            }
            EditorGUILayout.Separator();
        }

        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        GUILayout.BeginVertical("box");
        if (!ShowPreset)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Templates")) ShowPreset = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("- Templates")) ShowPreset = false;
            GUILayout.EndHorizontal();

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(FC.LutMode.ToString());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal("box");
            //GUILayout.FlexibleSpace();
            //if (GUILayout.Button(" < ")) FC.Action_PreviousLUT();
            //GUILayout.Label(" Change LUT ");
            //if (GUILayout.Button(" > ")) FC.Action_NextLUT();
            //GUILayout.FlexibleSpace();
            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Minimum")) FC.Action_TemplateMinimum();
            if (GUILayout.Button("Mono")) FC.Action_TemplateMono();
            if (GUILayout.Button("Toon")) FC.Action_TemplateToon();
            if (GUILayout.Button("Contrast")) FC.Action_TemplateContrast();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Scanline")) FC.Action_TemplateScanline();
            if (GUILayout.Button("Grid")) FC.Action_TemplateGrid();
            if (GUILayout.Button("Pixelate")) FC.Action_TemplatePixelate();
            if (GUILayout.Button("Film Grain")) FC.Action_TemplateFilmGrain();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fresnel line")) FC.Action_TemplateFresnelScanline();
            if (GUILayout.Button("Fresnel Grid")) FC.Action_TemplateFresnelGrid();
            if (GUILayout.Button("Fresnel")) FC.Action_TemplateFresnelColor();
            if (GUILayout.Button("Outline")) FC.Action_TemplateOutline();
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint Soft")) FC.Action_TemplatePaintSoft();
            if (GUILayout.Button("Paint Hard")) FC.Action_TemplatePaintHard();
            if (GUILayout.Button("Paint Mono")) FC.Action_TemplatePaintMono();
            if (GUILayout.Button("Paint Draft")) FC.Action_TemplatePaintDraft();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint Soft(M)")) FC.Action_TemplatePaintSoftMobile();
            if (GUILayout.Button("Paint Hard(M)")) FC.Action_TemplatePaintHardMobile();
            if (GUILayout.Button("Paint Mono(M)")) FC.Action_TemplatePaintMonoMobile();
            if (GUILayout.Button("Paint Draft(M)")) FC.Action_TemplatePaintDraftMobile();
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(" < ")) FC.Action_PreviousLUT();
        GUILayout.Label(" Change LUT ");
        if (GUILayout.Button(" > ")) FC.Action_NextLUT();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //init and begin checking undo:
        Init();
        EditorGUI.BeginChangeCheck();
        //Lut
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        LutContribution = EditorGUILayout.Slider("LUT Contribution", FC.LutContribution, 0, 1);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        LutMode = (LutPack)EditorGUILayout.EnumPopup("LUT Mode",FC.LutMode);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        LookupTexture = (Texture2D)EditorGUILayout.ObjectField("Look Up Texture", FC.LookupTexture, typeof(Texture2D), GUILayout.Height(50));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        //Cel
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        PixelSize = EditorGUILayout.IntSlider("Pixel Size", FC.PixelSize, 1, 512);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        CelCuts = EditorGUILayout.IntSlider("Cel Cuts", FC.CelCuts, 2, 255);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        PaintRadius = EditorGUILayout.IntSlider(new GUIContent("Paint: Radius", "Recommended: 1~3 for mobile for performance"), PaintRadius, 0, 10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        PaintBlurLevel = EditorGUILayout.IntSlider(new GUIContent("Paint: Blur Level", "Recommended: 2~3 with Paint Effect 1~3"), PaintBlurLevel, 0, 8);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        //Basic Settings
        GUILayout.BeginVertical("box");
        if (!ShowBasicSettings)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Basic Settings")) ShowBasicSettings = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("- Basic Settings")) ShowBasicSettings = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            BasicSettings.TintColor = EditorGUILayout.ColorField("Tint Color", FC.BasicSettings.TintColor);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            BasicSettings.Hue = EditorGUILayout.Slider("Hue", FC.BasicSettings.Hue, 0, 360);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            BasicSettings.Saturation = EditorGUILayout.Slider("Saturation", FC.BasicSettings.Saturation, -1, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            BasicSettings.Brightness = EditorGUILayout.Slider("Brightness", FC.BasicSettings.Brightness, -1, 3);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            BasicSettings.Contrast = EditorGUILayout.Slider("Contrast", FC.BasicSettings.Contrast, -1, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            BasicSettings.Sharpness = EditorGUILayout.Slider("Sharpness", FC.BasicSettings.Sharpness, 0, 1);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        //Fresnel Settings
        GUILayout.BeginVertical("box");
        if (!ShowFresnelSettings)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Fresnel Settings")) ShowFresnelSettings = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("- Fresnel Settings")) ShowFresnelSettings = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            FresnelSettings.Contribution = EditorGUILayout.Slider("Contribution", FC.FresnelSettings.Contribution, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FresnelSettings.FresnelPower = EditorGUILayout.Slider("Power", FC.FresnelSettings.FresnelPower, 0.001f, 10f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FresnelSettings.FresnelColor = EditorGUILayout.ColorField("Color", FC.FresnelSettings.FresnelColor);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FresnelSettings.FresnelScanlineX = EditorGUILayout.IntSlider("ScanlineX", FC.FresnelSettings.FresnelScanlineX, 0, 32);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FresnelSettings.FresnelScanlineY = EditorGUILayout.IntSlider("ScanlineY", FC.FresnelSettings.FresnelScanlineY, 0, 32);
            GUILayout.EndHorizontal();

        }
        GUILayout.EndVertical();

        //Fog Settings
        GUILayout.BeginVertical("box");
        if (!ShowFogSettings)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Fog Settings")) ShowFogSettings = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("- Fog Settings")) ShowFogSettings = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            FogSettings.Contribution = EditorGUILayout.Slider("Contribution", FC.FogSettings.Contribution, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FogSettings.DepthLevel = EditorGUILayout.Slider("Depth Level", FC.FogSettings.DepthLevel, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FogSettings.DepthClippingFar = EditorGUILayout.Slider("Depth Clipping Far", FC.FogSettings.DepthClippingFar, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FogSettings.FogColor = EditorGUILayout.ColorField("Fog Color", FC.FogSettings.FogColor);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FogSettings.ForegroundColor = EditorGUILayout.ColorField("Foreground Color", FC.FogSettings.ForegroundColor);
            GUILayout.EndHorizontal();

        }
        GUILayout.EndVertical();

        //Grain Settings
        GUILayout.BeginVertical("box");
        if (!ShowGrainSettings)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Grain Settings")) ShowGrainSettings = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("- Grain Settings")) ShowGrainSettings = false;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GrainSettings.Contribution = EditorGUILayout.Slider("Contribution", FC.GrainSettings.Contribution, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GrainSettings.Size = EditorGUILayout.Slider("Size", FC.GrainSettings.Size, 0, 3);
            GUILayout.EndHorizontal();

        }
        GUILayout.EndVertical();

        //Vignette Settings
        GUILayout.BeginVertical("box");
        if (!ShowVignetteSettings)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Vignette Settings")) ShowVignetteSettings = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("- Vignette Settings")) ShowVignetteSettings = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            VignetteSettings.Contribution = EditorGUILayout.Slider("Contribution", FC.VignetteSettings.Contribution, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            VignetteSettings.Color = EditorGUILayout.ColorField("Color", FC.VignetteSettings.Color);
            GUILayout.EndHorizontal();

        }
        GUILayout.EndVertical();

        //Outline Settings
        GUILayout.BeginVertical("box");
        if (!ShowOutlineSettings)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Outline Settings")) ShowOutlineSettings = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("- Outline Settings")) ShowOutlineSettings = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            OutlineSettings.Contribution = EditorGUILayout.Slider("Contribution", FC.OutlineSettings.Contribution, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            OutlineSettings.Color = EditorGUILayout.ColorField("Color", FC.OutlineSettings.Color);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            OutlineSettings.NormalMult = EditorGUILayout.Slider("Normal Mult", FC.OutlineSettings.NormalMult, 0, 4);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            OutlineSettings.NormalBias = EditorGUILayout.Slider("Normal Bias", FC.OutlineSettings.NormalBias, 1, 4);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            OutlineSettings.DepthMult = EditorGUILayout.Slider("Depth Mult", FC.OutlineSettings.DepthMult, 0, 4);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            OutlineSettings.DepthBias = EditorGUILayout.Slider("Depth Bias", FC.OutlineSettings.DepthBias, 1, 4);
            GUILayout.EndHorizontal();

        }
        GUILayout.EndVertical();

        //Scanline Settings
        GUILayout.BeginVertical("box");
        if (!ShowScanlineSettings)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Scanline Settings")) ShowScanlineSettings = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("- Scanline Settings")) ShowScanlineSettings = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            ScanlineSettings.Contribution = EditorGUILayout.Slider("Contribution", FC.ScanlineSettings.Contribution, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ScanlineSettings.Color = EditorGUILayout.ColorField("Color", FC.ScanlineSettings.Color);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ScanlineSettings.ScanlineX = EditorGUILayout.IntSlider("Scanline X", FC.ScanlineSettings.ScanlineX, 0, 32);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ScanlineSettings.ScanlineY = EditorGUILayout.IntSlider("Scanline Y", FC.ScanlineSettings.ScanlineY, 0, 32);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        //FXAA Settings
        GUILayout.BeginVertical("box");
        if (!ShowFXAASettings)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ FXAA Settings")) ShowFXAASettings = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("- FXAA Settings")) ShowFXAASettings = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            FXAASettings.Enable = EditorGUILayout.Toggle("Enable", FXAASettings.Enable);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FXAASettings.SubpixelBlending = EditorGUILayout.Slider("Subpixel Blending", FC.FXAASettings.SubpixelBlending, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            FXAASettings.LowQuality = EditorGUILayout.Toggle("Low Quality", FXAASettings.LowQuality);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        //Debug
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        DebugSlider = EditorGUILayout.Slider("Debug Slider", FC.DebugSlider, 0, 1);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EnableTouchSlider = EditorGUILayout.Toggle("Touch Slide", FC.EnableTouchSlider);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();


        //DrawDefaultInspector();


        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(FC, "Changed Area Of Effect");
            FC.LutContribution = LutContribution;
            FC.LutMode = LutMode;
            FC.LookupTexture = LookupTexture;

            if (PixelSize > 1 && PixelSize % 2 != 0) PixelSize++;
            FC.PixelSize = PixelSize;
            FC.CelCuts = CelCuts;

            FC.PaintBlurLevel = PaintBlurLevel;
            FC.PaintRadius = PaintRadius;

            FC.DebugSlider = DebugSlider;
            FC.EnableTouchSlider = EnableTouchSlider;

            FC.BasicSettings.TintColor = BasicSettings.TintColor;
            FC.BasicSettings.Hue = BasicSettings.Hue;
            FC.BasicSettings.Saturation = BasicSettings.Saturation;
            FC.BasicSettings.Brightness = BasicSettings.Brightness;
            FC.BasicSettings.Contrast = BasicSettings.Contrast;
            FC.BasicSettings.Sharpness = BasicSettings.Sharpness;

            FC.FresnelSettings.Contribution = FresnelSettings.Contribution;
            FC.FresnelSettings.FresnelPower = FresnelSettings.FresnelPower;
            FC.FresnelSettings.FresnelColor = FresnelSettings.FresnelColor;
            FC.FresnelSettings.FresnelScanlineX = FresnelSettings.FresnelScanlineX;
            FC.FresnelSettings.FresnelScanlineY = FresnelSettings.FresnelScanlineY;

            FC.FogSettings.Contribution = FogSettings.Contribution;
            FC.FogSettings.DepthLevel = FogSettings.DepthLevel;
            FC.FogSettings.DepthClippingFar = FogSettings.DepthClippingFar;
            FC.FogSettings.FogColor = FogSettings.FogColor;
            FC.FogSettings.ForegroundColor = FogSettings.ForegroundColor;

            FC.GrainSettings.Contribution = GrainSettings.Contribution;
            FC.GrainSettings.Size = GrainSettings.Size;

            FC.VignetteSettings.Contribution = VignetteSettings.Contribution;
            FC.VignetteSettings.Color = VignetteSettings.Color;

            FC.OutlineSettings.Contribution = OutlineSettings.Contribution;
            FC.OutlineSettings.Color = OutlineSettings.Color;
            FC.OutlineSettings.NormalMult = OutlineSettings.NormalMult;
            FC.OutlineSettings.NormalBias = OutlineSettings.NormalBias;
            FC.OutlineSettings.DepthMult = OutlineSettings.DepthMult;
            FC.OutlineSettings.DepthBias = OutlineSettings.DepthBias;

            FC.ScanlineSettings.Contribution = ScanlineSettings.Contribution;
            FC.ScanlineSettings.Color = ScanlineSettings.Color;
            FC.ScanlineSettings.ScanlineX = ScanlineSettings.ScanlineX;
            FC.ScanlineSettings.ScanlineY = ScanlineSettings.ScanlineY;

            FC.FXAASettings.Enable = FXAASettings.Enable;
            FC.FXAASettings.SubpixelBlending = FXAASettings.SubpixelBlending;
            FC.FXAASettings.LowQuality = FXAASettings.LowQuality;

            FC.UpdateMaterialSettings();
        }

    }

    void Init()
    {
        LutContribution = FC.LutContribution;
        LutMode = FC.LutMode;
        LookupTexture = FC.LookupTexture;

        PixelSize = FC.PixelSize;
        CelCuts = FC.CelCuts;

        PaintBlurLevel = FC.PaintBlurLevel;
        PaintRadius = FC.PaintRadius;

        DebugSlider = FC.DebugSlider;
        EnableTouchSlider = FC.EnableTouchSlider;

        BasicSettings.TintColor = FC.BasicSettings.TintColor;
        BasicSettings.Hue = FC.BasicSettings.Hue;
        BasicSettings.Saturation = FC.BasicSettings.Saturation;
        BasicSettings.Brightness = FC.BasicSettings.Brightness;
        BasicSettings.Contrast = FC.BasicSettings.Contrast;
        BasicSettings.Sharpness = FC.BasicSettings.Sharpness;

        FresnelSettings.Contribution = FC.FresnelSettings.Contribution;
        FresnelSettings.FresnelPower = FC.FresnelSettings.FresnelPower;
        FresnelSettings.FresnelColor = FC.FresnelSettings.FresnelColor;
        FresnelSettings.FresnelScanlineX = FC.FresnelSettings.FresnelScanlineX;
        FresnelSettings.FresnelScanlineY = FC.FresnelSettings.FresnelScanlineY;

        FogSettings.Contribution = FC.FogSettings.Contribution;
        FogSettings.DepthLevel = FC.FogSettings.DepthLevel;
        FogSettings.DepthClippingFar = FC.FogSettings.DepthClippingFar;
        FogSettings.FogColor = FC.FogSettings.FogColor;
        FogSettings.ForegroundColor = FC.FogSettings.ForegroundColor;

        GrainSettings.Contribution = FC.GrainSettings.Contribution;
        GrainSettings.Size = FC.GrainSettings.Size;

        VignetteSettings.Contribution = FC.VignetteSettings.Contribution;
        VignetteSettings.Color = FC.VignetteSettings.Color;

        OutlineSettings.Contribution = FC.OutlineSettings.Contribution;
        OutlineSettings.Color = FC.OutlineSettings.Color;
        OutlineSettings.NormalMult = FC.OutlineSettings.NormalMult;
        OutlineSettings.NormalBias = FC.OutlineSettings.NormalBias;
        OutlineSettings.DepthMult = FC.OutlineSettings.DepthMult;
        OutlineSettings.DepthBias = FC.OutlineSettings.DepthBias;

        ScanlineSettings.Contribution = FC.ScanlineSettings.Contribution;
        ScanlineSettings.Color = FC.ScanlineSettings.Color;
        ScanlineSettings.ScanlineX = FC.ScanlineSettings.ScanlineX;
        ScanlineSettings.ScanlineY = FC.ScanlineSettings.ScanlineY;

        FXAASettings.Enable = FC.FXAASettings.Enable;
        FXAASettings.SubpixelBlending = FC.FXAASettings.SubpixelBlending;
        FXAASettings.LowQuality = FC.FXAASettings.LowQuality;
    }
}
