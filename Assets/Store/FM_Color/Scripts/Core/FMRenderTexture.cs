using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FMRenderTexture : MonoBehaviour
{
    Camera Cam;
    RenderTexture rt;
    Vector2 resolution = new Vector2(512, 512);

    public RawImage RawImg;

	// Update is called once per frame
	void Update ()
    {
        if (Cam == null) Cam = GetComponent<Camera>();
        resolution = new Vector2(Screen.width, Screen.height);

        if (rt == null)
        {
            rt = new RenderTexture(Mathf.RoundToInt(resolution.x), Mathf.RoundToInt(resolution.y), 16, RenderTextureFormat.ARGB32);
        }
        else
        {
            if (rt.width != Mathf.RoundToInt(resolution.x) || rt.height != Mathf.RoundToInt(resolution.y))
            {
                Destroy(rt);
                rt = new RenderTexture(Mathf.RoundToInt(resolution.x), Mathf.RoundToInt(resolution.y), 16, RenderTextureFormat.ARGB32);
            }
        }
        if (Cam != null) Cam.targetTexture = rt;
        if (rt != null && RawImg != null) RawImg.texture = rt;
        if (RawImg != null) RawImg.enabled = true;


        CheckCommandBuffer();
        if (RawImg != null)
        {
            if (RTA != null) RawImg.material.SetTexture("_MainTexStencil", RTA);

            if (RawImg.material.shader != Shader.Find("FMCOLOR/FMStencialAlpha"))
            {
                RawImg.material = new Material(Shader.Find("FMCOLOR/FMStencialAlpha"));
            }
        }

    }

    private CommandBuffer commandBuffer;
    RenderTexture RTA;
    private void CheckCommandBuffer()
    {
        if (Cam == null) Cam = GetComponent<Camera>();
        if (commandBuffer == null)
        {
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "commandBuffer";

            int cachedScreenImageID = Shader.PropertyToID("_Temp");
            commandBuffer.GetTemporaryRT(cachedScreenImageID, -1, -1, 32);
            Cam.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
        }

        if (RTA == null)
        {
            RTA = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            if (commandBuffer != null) commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, RTA);
        }
        else
        {
            if (RTA.width != Screen.width || RTA.height != Screen.height)
            {
                if (RTA != null) RTA.Release();
                DestroyImmediate(RTA);

                RTA = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
                if (commandBuffer != null) commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, RTA);
            }
        }
    }


    private void OnDisable()
    {
        RemoveBuffer();
    }

    void RemoveBuffer()
    {
        if (Cam != null && commandBuffer != null)
        {
            Cam.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);

            Cam.RemoveAllCommandBuffers();

            commandBuffer.Clear();
            commandBuffer = null;
        }

        if (Cam != null) Cam.targetTexture = null;

        if (RTA != null)
        {
            RTA.Release();
            DestroyImmediate(RTA);
        }

        if(rt != null)
        {
            rt.Release();
            DestroyImmediate(rt);
        }


        if (RawImg != null) RawImg.enabled = false;
    }
}
