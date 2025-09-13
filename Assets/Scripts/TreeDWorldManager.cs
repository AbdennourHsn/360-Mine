using DG.Tweening;
using UnityEngine;

public class TreeDWorldManager : MonoBehaviour
{
    public Material[] materials;

    public void EnableMeshRenderer(bool enable)
    {
        MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mesh in renderers)
        {
            mesh.enabled = enable;
        }
    }

    public void SetAlpha(float alpha)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null)
            {
                SetMaterialAlpha(materials[i], alpha);
            }
        }
    }

    public void FadeAlphaTo(float targetAlpha, float duration = 1f, Ease ease = Ease.OutQuad,
        System.Action onComplete = null)
    {
        if (materials == null || materials.Length == 0)
        {
            Debug.LogWarning("No materials assigned to _envMatsTransparent array!");
            return;
        }

        // Kill any existing tweens on these materials
        DOTween.Kill(this);

        // Create a sequence to tween all materials simultaneously
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null)
            {
                float currentAlpha = GetMaterialAlpha(materials[i]);
                int materialIndex = i; // Capture for closure

                Tween alphaTween = DOTween.To(
                    () => currentAlpha,
                    alpha => SetMaterialAlpha(materials[materialIndex], alpha),
                    targetAlpha,
                    duration
                ).SetEase(ease);

                // Add to sequence (Join makes them run simultaneously)
                if (i == 0)
                    sequence.Append(alphaTween);
                else
                    sequence.Join(alphaTween);
            }
        }

        // Set callback and target for killing tweens
        sequence.SetTarget(this);
        if (onComplete != null)
            sequence.OnComplete(() => onComplete());
    }


    private float GetMaterialAlpha(Material mat)
    {
        return mat.GetFloat("_Visibility");
    }

    private void SetMaterialAlpha(Material mat, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        
        mat.SetFloat("_Visibility", alpha);
        
    }
}