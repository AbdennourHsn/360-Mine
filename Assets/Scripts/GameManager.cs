using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool _isMoving;
    public Camera MainCamera;

    [Header("Anchors")] public AnchorPoint[] _initialAnchorPoint;

    [Header("Cube maps")] public AnchorPointData currentCubeMap;

    [Space(10)] public Material skybox_material;
    public TreeDWorldManager _3dWord;
    
    public float animationCameraSpeed;
    private float _skyboxBland = 0;

    [Range(0, 1)] public float startfeeding;
    public float skyboxFadeDuration;
    public float _strechFactor;
    [SerializeField] private float _cameraMoveSpeed = 3f;

    private Vector3 _initialPos;
    private Quaternion _intialRotation;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _initialPos = MainCamera.transform.position;
        _intialRotation = MainCamera.transform.rotation;
        
        StartCoroutine(AnimateCameraWithLookAt());
    }

    public void ResetGamePlay()
    {
        RenderSettings.skybox = null;
        MainCamera.transform.position = _initialPos;
        MainCamera.transform.rotation = _intialRotation;
        _isMoving = true;
        currentCubeMap = _initialAnchorPoint[0].GetCubeMap();
        SetCubeMapMaterialTargetData(_initialAnchorPoint[0].GetCubeMap());
        StartCoroutine(AnimateCameraWithLookAt());
    }
    
    private IEnumerator AnimateCameraWithLookAt()
    {
        Vector3 targetPosition = _initialAnchorPoint[0].transform.position;
        _3dWord.SetAlpha(1);
        _3dWord.EnableMeshRenderer(true);
        Sequence cameraSequence = DOTween.Sequence();
        
        cameraSequence.Append(MainCamera.transform.DOMove(targetPosition, animationCameraSpeed));
        
        cameraSequence.Join(MainCamera.transform.DOLookAt(
            _3dWord.transform.position, 
            animationCameraSpeed, 
            AxisConstraint.None, 
            Vector3.up
        ));
        
        cameraSequence.SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(animationCameraSpeed*0.8f);
        StartNavigation();
    }

    public void ChangeSkyboxLight(float skyboxLightIntensity)
    {
        skybox_material.SetFloat("_Exposure", 8*skyboxLightIntensity);
    }

    
    private void StartNavigation()
    {
        _isMoving = false;
        if (_3dWord == null) _3dWord = FindFirstObjectByType<TreeDWorldManager>();
        RenderSettings.skybox = skybox_material;
        MainCamera.GetComponent<MouseLookCamera>().enabled = true;
        SetCubeMapMaterialTargetData(currentCubeMap);
        MainCamera.transform.position = _initialAnchorPoint[0].transform.position;
        _3dWord.FadeAlphaTo(0 , 0.5f);
        _3dWord.EnableMeshRenderer(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            moveForward();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            moveBackward();
        }
    }

    private void moveForward()
    {
        if (currentCubeMap.id < _initialAnchorPoint.Length - 1)
            StartCoroutine(MoveToAnchorPoint(_initialAnchorPoint[currentCubeMap.id + 1]));
    }

    private void moveBackward()
    {
        if (currentCubeMap.id > 0)
            StartCoroutine(MoveToAnchorPoint(_initialAnchorPoint[currentCubeMap.id - 1]));
    }

    private void EnableAllAnchorsExcept(AnchorPoint anchor, bool enable)
    {
        foreach (var anchorPoint in _initialAnchorPoint)
        {
            if (anchorPoint == anchor)
                continue;
            anchorPoint.SetAnchorPoint(enable);
        }
    }


    public IEnumerator MoveToAnchorPoint(AnchorPoint anchorPoint)
    {
        _3dWord.SetAlpha(0.7f);
        SetCubeMapMaterialTargetData(anchorPoint.GetCubeMap());

        _isMoving = true;
        EnableAllAnchorsExcept(anchorPoint, false);
        _3dWord.EnableMeshRenderer(true);


        Vector3 startPos = MainCamera.transform.position;
        Vector3 endPos = anchorPoint.transform.position;
        float distance = Vector3.Distance(startPos, endPos);
        float speed = Mathf.Max(0.0001f, _cameraMoveSpeed);
        float moveDur = distance / speed; 

        DOTween.Kill(MainCamera.transform);
        DOTween.Kill("SkyboxBlend");
        DOTween.Kill("SkyboxStritch");

        Sequence seq = DOTween.Sequence();

        var camTween = MainCamera.transform
            .DOMove(endPos, moveDur)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _3dWord.SetAlpha(1);
                _3dWord.EnableMeshRenderer(false);
                print("End");
            });

        var skyboxTween = DOTween.To(
                () => _skyboxBland,
                v =>
                {
                    _skyboxBland = v;
                    skybox_material.SetFloat("_Blend", _skyboxBland);
                },
                1f,
                moveDur
            )
            .SetEase(Ease.Linear)
            .SetId("SkyboxBlend")
            .OnComplete(() =>
            {
                _skyboxBland = 0f;
                currentCubeMap = anchorPoint.GetCubeMap();
                SetCubeMapMaterialTargetData(currentCubeMap);
            });
        var skyboxStretch = DOTween.To(
                () => _strechFactor,
                v =>
                {
                    _strechFactor = v;
                    skybox_material.SetFloat("_StretchBlend_1", _strechFactor);
                },
                0.5f,
                moveDur
            )
            .SetEase(Ease.Linear)
            .SetId("SkyboxStritch")
            .OnComplete(() =>
            {
                skybox_material.SetFloat("_StretchBlend_1", 0);
                _strechFactor = 0;
            });

        seq.Join(camTween);
        seq.Join(skyboxTween);
        seq.Join(skyboxStretch);

        yield return new WaitForSeconds(moveDur * startfeeding);
        _3dWord.FadeAlphaTo(0, skyboxFadeDuration);
        yield return new WaitForSeconds(skyboxFadeDuration);

        anchorPoint.SetAnchorPoint(false);
        EnableAllAnchorsExcept(anchorPoint, true);
        _isMoving = false;
    }

    public void SetCubeMapMaterialTargetData(AnchorPointData anchorPoint)
    {
        if (skybox_material != null)
        {
            skybox_material.SetTexture("_Tex_1", currentCubeMap.cubeMapTexture);
            skybox_material.SetFloat("_RotationX_1", currentCubeMap._RotationX);
            skybox_material.SetFloat("_RotationY_1", currentCubeMap._RotationY);
            skybox_material.SetFloat("_RotationZ_1", currentCubeMap._RotationZ);
            skybox_material.SetFloat("_StretchBlend_1", 0);


            skybox_material.SetTexture("_Tex_2", anchorPoint.cubeMapTexture);
            skybox_material.SetFloat("_RotationX_2", anchorPoint._RotationX);
            skybox_material.SetFloat("_RotationY_2", anchorPoint._RotationY);
            skybox_material.SetFloat("_RotationZ_2", anchorPoint._RotationZ);

            skybox_material.SetFloat("_Blend", 0);
        }
    }

    public void SetSpeed(float speed)
    {
        _cameraMoveSpeed = 2 + 6*speed;
    }
}