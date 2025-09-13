using UnityEngine;
using UnityEngine.Serialization;

public class MouseSurfaceFollower : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private LayerMask surfaceLayerMask = -1; 
    [SerializeField] private float maxRaycastDistance = 100f;
    
    [Header("Pointer Settings")]
    [SerializeField] private GameObject pointerObject;
    [SerializeField] private float offsetFromSurface = 0.1f;
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool showOnlyOnHit = true;
    
    [Space(20)]
    [Header("Anchor Point detecter Settings")]
    [SerializeField] private LayerMask anchorPointTargetLayer = -1;
    [SerializeField] private float maxDistance = 100f;
    private GameObject hoveredObject;
    private Renderer hoveredRenderer;
    private void Start()
    {
        if (raycastCamera == null)
        {
            raycastCamera = Camera.main;
            if (raycastCamera == null)
            {
                raycastCamera = FindObjectOfType<Camera>();
            }
        }
        
        // If no pointer object is assigned, use this gameobject
        if (pointerObject == null)
        {
            pointerObject = gameObject;
        }
        
        // Initially hide the pointer if showOnlyOnHit is true
        if (showOnlyOnHit)
        {
            pointerObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        UpdatePointerPosition();
        HandleHover();
        HandleClick();
        float distance = Vector3.Distance(raycastCamera.transform.position, pointerObject.transform.position);
        UiManager.instance.SetColiderDistance(distance);
    }
    
    private void UpdatePointerPosition()
    {
        if (Input.GetMouseButton(0) || GameManager.instance._isMoving || HoverStopButton.isMovementPaused || UiManager.instance.uiActive)
        {
            pointerObject.SetActive(false);
            return;
        }
        Vector3 mousePosition = Input.mousePosition;
        
        Ray ray = raycastCamera.ScreenPointToRay(mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, surfaceLayerMask))
        {
            if (showOnlyOnHit && !pointerObject.activeInHierarchy)
            {
                pointerObject.SetActive(true);
            }
            
            Vector3 targetPosition = hit.point + hit.normal * offsetFromSurface;
            
            pointerObject.transform.position = targetPosition;
            
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            
            if (smoothRotation)
            {
                pointerObject.transform.rotation = Quaternion.Slerp(
                    pointerObject.transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                pointerObject.transform.rotation = targetRotation;
            }

        }
        else
        {
            if (showOnlyOnHit && pointerObject.activeInHierarchy)
            {
                pointerObject.SetActive(false);
            }
        }
    }
    
    public void PointerRenderers(bool active)
    {
        foreach(Renderer r in pointerObject.GetComponentsInChildren<Renderer>())
            r.enabled = active;
    }
    
    private void HandleHover()
    {
        if( GameManager.instance._isMoving) return;
        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxDistance, anchorPointTargetLayer))
        {
            if (hoveredObject != hit.collider.gameObject)
            {
                ResetHover(); 
                
                hoveredObject = hit.collider.gameObject;
                hoveredRenderer = hoveredObject.GetComponent<Renderer>();
                
                AnchorPointRing anchorPoint = hit.collider.gameObject.GetComponent<AnchorPointRing>();
                if (anchorPoint != null)
                {
                    anchorPoint.OnHoverEnter();
                    PointerRenderers(false);
                }

            }
        }
        else
        {
            if (hoveredObject != null)
            {
                ResetHover();
            }
        }
    }
    
    private void HandleClick()
    {
        if( GameManager.instance._isMoving) return;

        if (Input.GetMouseButtonDown(0) && hoveredObject != null)
        {
            AnchorPointRing anchorPoint = hoveredObject.gameObject.GetComponent<AnchorPointRing>();
            if(anchorPoint != null) anchorPoint.OnClick();
        }
    }
    
    private void ResetHover()
    {
        if (hoveredObject != null)
        {
            PointerRenderers(true);
            AnchorPointRing anchorPoint = hoveredObject.gameObject.GetComponent<AnchorPointRing>();
            if(anchorPoint != null)
                anchorPoint.OnHoverExit();
            hoveredObject = null;
            hoveredRenderer = null;
        }
    }
}