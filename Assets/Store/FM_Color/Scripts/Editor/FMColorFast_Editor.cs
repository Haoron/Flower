using System;
using System.Collections;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMColorFast))]
[CanEditMultipleObjects]
public class FMColorFast_Editor : Editor
{
    GUISkin skin;
    private Texture2D logo;

    private bool ShowPreset = false;
    private bool ShowBasicSettings = false;
    private bool ShowGrainSettings = false;
    private bool ShowVignetteSettings = false;
    private bool ShowScanlineSettings = false;

    private FMColorFast FC;
    private LutPack LutMode;
    private float LutContribution;
    private Texture2D LookupTexture;

    private int PixelSize;
    private int CelCuts;

    private float DebugSlider;
    private bool EnableTouchSlider;

    private FMFilterBasic BasicSettings = new FMFilterBasic();
    private FMFilterGrain GrainSettings = new FMFilterGrain();
    private FMFilterVignette VignetteSettings = new FMFilterVignette();
    private FMFilterScanline ScanlineSettings = new FMFilterScanline();

    public override void OnInspectorGUI()
    {
        if (FC == null) FC = (FMColorFast)target;

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

            FC.DebugSlider = DebugSlider;
            FC.EnableTouchSlider = EnableTouchSlider;

            FC.BasicSettings.TintColor = BasicSettings.TintColor;
            FC.BasicSettings.Hue = BasicSettings.Hue;
            FC.BasicSettings.Saturation = BasicSettings.Saturation;
            FC.BasicSettings.Brightness = BasicSettings.Brightness;
            FC.BasicSettings.Contrast = BasicSettings.Contrast;
            FC.BasicSettings.Sharpness = BasicSettings.Sharpness;

            FC.GrainSettings.Contribution = GrainSettings.Contribution;
            FC.GrainSettings.Size = GrainSettings.Size;

            FC.VignetteSettings.Contribution = VignetteSettings.Contribution;
            FC.VignetteSettings.Color = VignetteSettings.Color;

            FC.ScanlineSettings.Contribution = ScanlineSettings.Contribution;
            FC.ScanlineSettings.Color = ScanlineSettings.Color;
            FC.ScanlineSettings.ScanlineX = ScanlineSettings.ScanlineX;
            FC.ScanlineSettings.ScanlineY = ScanlineSettings.ScanlineY;

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

        DebugSlider = FC.DebugSlider;
        EnableTouchSlider = FC.EnableTouchSlider;

        BasicSettings.TintColor = FC.BasicSettings.TintColor;
        BasicSettings.Hue = FC.BasicSettings.Hue;
        BasicSettings.Saturation = FC.BasicSettings.Saturation;
        BasicSettings.Brightness = FC.BasicSettings.Brightness;
        BasicSettings.Contrast = FC.BasicSettings.Contrast;
        BasicSettings.Sharpness = FC.BasicSettings.Sharpness;

        GrainSettings.Contribution = FC.GrainSettings.Contribution;
        GrainSettings.Size = FC.GrainSettings.Size;

        VignetteSettings.Contribution = FC.VignetteSettings.Contribution;
        VignetteSettings.Color = FC.VignetteSettings.Color;
       
        ScanlineSettings.Contribution = FC.ScanlineSettings.Contribution;
        ScanlineSettings.Color = FC.ScanlineSettings.Color;
        ScanlineSettings.ScanlineX = FC.ScanlineSettings.ScanlineX;
        ScanlineSettings.ScanlineY = FC.ScanlineSettings.ScanlineY;

    }
}
