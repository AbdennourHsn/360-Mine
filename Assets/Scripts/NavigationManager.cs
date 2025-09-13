using System.Collections;
using DG.Tweening;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
    
    [Space(10)]
    [SerializeField]
    private GameObject _3dEnv;
    [SerializeField] private Material[] _envMatsTransaparent;

    [Space(10)] [Header("Anchor points")] 
    [SerializeField] private Transform[] _anchorPoints;

    [Space(10)]
    [SerializeField] private Material _skybox;
    
    [SerializeField] float _transitionDuration = 1f;
    [SerializeField] float _stretchValue = 1f; // current shader value

    private int _currentAnchorPoint = 0;
    private float _blendValue=0;
    public int x;
    
    [SerializeField] GameObject pointer; // optional, leave empty for world forward

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        _camera.transform.position = _anchorPoints[0].position;
        _skybox.SetFloat("_Blend", 0f);
        _currentAnchorPoint = 0;
        SetActiveMeshs(false);
        //SetMaterialsAlpha(1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            TweenMaterialsAlpha(0);
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            SetMaterialsAlpha(1);
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveForward();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            MoveBackward();
        }
        
        
    }

    private IEnumerator TransitionToTarget(int target)
    {
        SetActiveMeshs(true);
        SetMaterialsAlpha(0.8f);
        _camera.transform.DOMove(_anchorPoints[target].position , _transitionDuration).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                //_skybox.SetFloat("_Blend", target);
                TweenMaterialsAlpha(0 , _transitionDuration);

            });
        
        DOTween.To(
            () => _blendValue,
            v =>
            {
                _blendValue = v;
                _skybox.SetFloat("_Blend", _blendValue);
            },
            target,
            _transitionDuration
        ).SetEase(Ease.OutQuad);
        
        _currentAnchorPoint = (int)target;
        yield return new WaitForSeconds(_transitionDuration);
        //SetActiveMeshs(false);

    }

    public void Step()
    {
        Debug.Log("Step");
        Vector3 camForward = _camera.transform.forward;
        Vector3 referenceForward = Quaternion.Euler(0, 90, 0) * Vector3.forward;
        float dot = Vector3.Dot(referenceForward, camForward);
        if (dot > 0f) MoveForward();
        else MoveBackward();
    }

    private void MoveForward()
    {
        if (_currentAnchorPoint < 3)
        {
            StartCoroutine(TransitionToTarget(_currentAnchorPoint+1));
        }
    }

    private void MoveBackward()
    {
        if (_currentAnchorPoint > 0)
        {
            StartCoroutine(TransitionToTarget(_currentAnchorPoint-1));
        }
    }

    private void SetActiveMeshs(bool active)
    {
        MeshRenderer[] renderers = _3dEnv.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mesh in renderers)
        {
            mesh.enabled = active;
        }
    }

    private void SetMaterialsAlpha(float alpha)
    {
        for (int i = 0; i < _envMatsTransaparent.Length; i++)
        {
            if (_envMatsTransaparent[i] != null)
            {
                SetMaterialAlpha(_envMatsTransaparent[i], alpha);
            }
        }
    }
    private void TweenMaterialsAlpha(float targetAlpha, float duration = 1f, Ease ease = Ease.OutQuad, System.Action onComplete = null)
    {
        if (_envMatsTransaparent == null || _envMatsTransaparent.Length == 0)
        {
            Debug.LogWarning("No materials assigned to _envMatsTransparent array!");
            return;
        }
        
        // Kill any existing tweens on these materials
        DOTween.Kill(this);
        
        // Create a sequence to tween all materials simultaneously
        Sequence sequence = DOTween.Sequence();
        
        for (int i = 0; i < _envMatsTransaparent.Length; i++)
        {
            if (_envMatsTransaparent[i] != null)
            {
                float currentAlpha = GetMaterialAlpha(_envMatsTransaparent[i]);
                int materialIndex = i; // Capture for closure
                
                Tween alphaTween = DOTween.To(
                    () => currentAlpha,
                    alpha => SetMaterialAlpha(_envMatsTransaparent[materialIndex], alpha),
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
    
    /// <summary>
    /// Fade materials in (alpha to 1)
    /// </summary>
    private void FadeIn(float duration = 1f, Ease ease = Ease.OutQuad, System.Action onComplete = null)
    {
        TweenMaterialsAlpha(1f, duration, ease, onComplete);
    }
    
    /// <summary>
    /// Fade materials out (alpha to 0)
    /// </summary>
    public void FadeOut(float duration = 1f, Ease ease = Ease.OutQuad, System.Action onComplete = null)
    {
        TweenMaterialsAlpha(0f, duration, ease, onComplete);
    }
    private float GetMaterialAlpha(Material mat)
    {
        return mat.color.a;
    }
    
    private void SetMaterialAlpha(Material mat, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        
        if (mat.HasProperty("_Color"))
        {
            Color color = mat.color;
            color.a = alpha;
            mat.color = color;
        }
        else if (mat.HasProperty("_BaseColor"))
        {
            Color color = mat.GetColor("_BaseColor");
            color.a = alpha;
            mat.SetColor("_BaseColor", color);
        }
        else if (mat.HasProperty("_MainColor"))
        {
            Color color = mat.GetColor("_MainColor");
            color.a = alpha;
            mat.SetColor("_MainColor", color);
        }
        else if (mat.HasProperty("_Alpha"))
        {
            mat.SetFloat("_Alpha", alpha);
        }
    }
}
