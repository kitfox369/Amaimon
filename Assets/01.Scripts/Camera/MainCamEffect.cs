using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class MainCamEffect : MonoBehaviour
{
    public Material material;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(material == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit(source, destination,material);
    }

}
