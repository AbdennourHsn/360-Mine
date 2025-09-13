using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    [Range(10f, 179f)]
    public float mainFOV = 85f;
    [Range(10f, 179f)]
    public float minFOV = 85f;
    [Range(10f, 179f)]
    public float maxFOV = 110f;
    
    [Header("Zoom Speed")]
    public float zoomSpeed = 10f;
    
    [Header("Smooth Zooming")]
    public bool smoothZoom = true;
    public float smoothSpeed = 5f;
    
    public Camera cam;
    private float targetFOV;
    
    void Start()
    {
        
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        targetFOV = cam.fieldOfView;
        
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        cam.fieldOfView = mainFOV;
    }
    
    void Update()
    {
        HandleZoomInput();
        
        if (smoothZoom)
        {
            ApplySmoothZoom();
        }
    }
    
    void HandleZoomInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput != 0f)
        {

            targetFOV -= scrollInput * zoomSpeed;
            
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
            
            if (!smoothZoom)
            {
                cam.fieldOfView = targetFOV;
            }
        }
    }
    
    void ApplySmoothZoom()
    {
        if (Mathf.Abs(cam.fieldOfView - targetFOV) > 0.01f)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, smoothSpeed * Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = targetFOV;
        }
    }
    
    public void SetZoom(float fov)
    {
        targetFOV = Mathf.Clamp(fov, minFOV, maxFOV);
        if (!smoothZoom)
            cam.fieldOfView = targetFOV;
    }
    
    public void ZoomIn(float amount = 5f)
    {
        targetFOV = Mathf.Clamp(targetFOV - amount, minFOV, maxFOV);
        if (!smoothZoom)
            cam.fieldOfView = targetFOV;
    }
    
    public void ZoomOut(float amount = 5f)
    {
        targetFOV = Mathf.Clamp(targetFOV + amount, minFOV, maxFOV);
        if (!smoothZoom)
            cam.fieldOfView = targetFOV;
    }
    
    public void ResetZoom()
    {
        targetFOV = (minFOV + maxFOV) / 2f;
        if (!smoothZoom)
            cam.fieldOfView = targetFOV;
    }
    
    public float GetZoomPercentage()
    {
        return (cam.fieldOfView - minFOV) / (maxFOV - minFOV);
    }
}