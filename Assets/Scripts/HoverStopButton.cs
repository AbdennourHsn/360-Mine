using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Script for the button - attach this to your Button GameObject
public class HoverStopButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Movement Control")]
    public static bool isMovementPaused = false;
    
    [Header("UI References")]
    public Text statusText;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMovementPaused = true;
        Debug.Log("Movement STOPPED - Mouse over button");
        
        if (statusText != null)
            statusText.text = "Movement: PAUSED";
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isMovementPaused = false;
        Debug.Log("Movement RESUMED - Mouse left button");
        
        if (statusText != null)
            statusText.text = "Movement: ACTIVE";
    }
}

public class MovingObject : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 moveSpeed = new Vector3(2f, 0f, 1f);
    public bool useRandomMovement = false;
    
    [Header("Boundary Settings")]
    public float boundaryX = 10f;
    public float boundaryZ = 10f;
    
    private Vector3 currentDirection;
    
    void Start()
    {
        if (useRandomMovement)
        {
            currentDirection = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
        }
        else
        {
            currentDirection = moveSpeed.normalized;
        }
    }
    
    void Update()
    {
        if (!HoverStopButton.isMovementPaused)
        {
            MoveObject();
            CheckBoundaries();
        }
    }
    
    void MoveObject()
    {
        if (useRandomMovement)
        {
            transform.position += currentDirection * moveSpeed.magnitude * Time.deltaTime;
        }
        else
        {
            transform.position += moveSpeed * Time.deltaTime;
        }
    }
    
    void CheckBoundaries()
    {
        Vector3 pos = transform.position;
        
        // Bounce off boundaries
        if (Mathf.Abs(pos.x) > boundaryX)
        {
            currentDirection.x *= -1;
            moveSpeed.x *= -1;
        }
        
        if (Mathf.Abs(pos.z) > boundaryZ)
        {
            currentDirection.z *= -1;
            moveSpeed.z *= -1;
        }
        
        transform.position = new Vector3(
            Mathf.Clamp(pos.x, -boundaryX, boundaryX),
            pos.y,
            Mathf.Clamp(pos.z, -boundaryZ, boundaryZ)
        );
    }
}

public class MovingRigidbody : MonoBehaviour
{
    [Header("Rigidbody Movement")]
    public Vector3 forceDirection = new Vector3(5f, 0f, 5f);
    public ForceMode forceMode = ForceMode.Force;
    
    private Rigidbody rb;
    private Vector3 savedVelocity;
    private Vector3 savedAngularVelocity;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        if (HoverStopButton.isMovementPaused)
        {
            if (!rb.isKinematic)
            {
                savedVelocity = rb.linearVelocity;
                savedAngularVelocity = rb.angularVelocity;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
        else
        {
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
                rb.linearVelocity = savedVelocity;
                rb.angularVelocity = savedAngularVelocity;
            }
            
            rb.AddForce(forceDirection * Time.deltaTime, forceMode);
        }
    }
}