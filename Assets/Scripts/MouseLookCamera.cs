using UnityEngine;

public class MouseLookCamera : MonoBehaviour
{
    [Header("Mouse Look Settings")] [SerializeField]
    private float baseSensitivity = 100f;

    [SerializeField] private float verticalLookLimit = 80f; // Maximum up/down angle

    [Header("Screen Size Sensitivity")] [SerializeField]
    private bool useScreenSizeAdjustment = true;

    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    [SerializeField] private SensitivityScaleMode scaleMode = SensitivityScaleMode.Diagonal;

    [Header("Input Settings")] [SerializeField]
    private KeyCode lookButton = KeyCode.Mouse0;


    [Header("Smoothing (Optional)")] [SerializeField]
    private bool useSmoothLook = false;

    [SerializeField] private float smoothTime = 0.1f;

    // Enum for different scaling modes
    public enum SensitivityScaleMode
    {
        Width,
        Height,
        Diagonal,
        Area,
        Average
    }

    // Private variables
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    private float currentSensitivity;
    private Vector2 lastScreenSize;
    private bool wasFocused = true;

    private void Start()
    {
        Vector3 currentRotation = transform.eulerAngles;
        yRotation = currentRotation.y;
        xRotation = currentRotation.x;

        if (xRotation > 180f)
        {
            xRotation -= 360f;
        }

        UpdateSensitivity();
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        baseSensitivity = 1400 + (1000 * sensitivity);
        UpdateSensitivity();
    }

    private void Update()
    {
        if (HoverStopButton.isMovementPaused || UiManager.instance.uiActive) return;
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize != lastScreenSize)
        {
            UpdateSensitivity();
            lastScreenSize = currentScreenSize;
        }

        HandleMouseLook();
    }

    private void UpdateSensitivity()
    {
        if (!useScreenSizeAdjustment)
        {
            currentSensitivity = baseSensitivity;
            return;
        }

        float scaleFactor = CalculateScaleFactor();
        currentSensitivity = baseSensitivity * scaleFactor;
    }

    private float CalculateScaleFactor()
    {
        Vector2 currentResolution = new Vector2(Screen.width, Screen.height);

        switch (scaleMode)
        {
            case SensitivityScaleMode.Width:
                return referenceResolution.x / currentResolution.x;

            case SensitivityScaleMode.Height:
                return referenceResolution.y / currentResolution.y;

            case SensitivityScaleMode.Diagonal:
                float referenceDiagonal = Mathf.Sqrt(referenceResolution.x * referenceResolution.x +
                                                     referenceResolution.y * referenceResolution.y);
                float currentDiagonal = Mathf.Sqrt(currentResolution.x * currentResolution.x +
                                                   currentResolution.y * currentResolution.y);
                return referenceDiagonal / currentDiagonal;

            case SensitivityScaleMode.Area:
                float referenceArea = referenceResolution.x * referenceResolution.y;
                float currentArea = currentResolution.x * currentResolution.y;
                return Mathf.Sqrt(referenceArea / currentArea);

            case SensitivityScaleMode.Average:
                float widthScale = referenceResolution.x / currentResolution.x;
                float heightScale = referenceResolution.y / currentResolution.y;
                return (widthScale + heightScale) / 2f;

            default:
                return 1f;
        }
    }


    private void HandleMouseLook()
    {
        bool isFocused = Application.isFocused;
        if (!wasFocused && isFocused)
        {
            wasFocused = isFocused;
            return;
        }

        wasFocused = isFocused;

        if (!isFocused || !Input.GetMouseButton(0))
        {
            if (useSmoothLook)
            {
                currentMouseDelta = Vector2.zero;
                currentMouseDeltaVelocity = Vector2.zero;
            }

            Cursor.lockState = CursorLockMode.None;
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Input.GetMouseButton(0))
        {
            // Get mouse movement input
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector2 targetMouseDelta = new Vector2(mouseX, mouseY);

            if (useSmoothLook)
            {
                currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta,
                    ref currentMouseDeltaVelocity, smoothTime);
            }
            else
            {
                currentMouseDelta = targetMouseDelta;
            }

            currentMouseDelta *= currentSensitivity * Time.deltaTime;
            yRotation += currentMouseDelta.x;
            xRotation -= currentMouseDelta.y;
            xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
        else if (useSmoothLook)
        {
            currentMouseDelta = Vector2.zero;
            currentMouseDeltaVelocity = Vector2.zero;
        }
    }
}