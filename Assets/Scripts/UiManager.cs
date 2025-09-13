using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;
    public bool uiActive = false;
    public RectTransform panel;
    public Button clickBtn;
    private Vector3 startPosition;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI moouseSensText;
    
    public MouseLookCamera mouseLookCamera;
    public TextMeshProUGUI colider;
    public TextMeshProUGUI light;
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        startPosition = panel.transform.position;
    }

    public void SetColiderDistance(float distance)
    {
        colider.text = $"Colider distance : {distance:F02}";
    }

    public void ShowUi()
    {
        panel.DOMoveY(86, 0.3f);
        uiActive = true;
        clickBtn.GetComponent<Image>().enabled = false;
    }

    public void HideUi()
    {
        if (uiActive)
        {
            panel.DOMoveY(startPosition.y, 0.3f);
            uiActive = false;
            clickBtn.GetComponent<Image>().enabled = true;
        }
    }

    public void ChangeSpeed(float speed)
    {
        speedText.text = $"Movement Speed: {(2 + 6*speed):F2}";
        GameManager.instance.SetSpeed(speed);
    }

    public void ChangeMouseSensitivity(float sensitivity)
    {
        float value = (1400 + (1000 * sensitivity));
        moouseSensText.text = $"Mouse sensitivity: {value/10:F2}";
        mouseLookCamera.SetMouseSensitivity(sensitivity);
    }
    
    public void ChangeSkyboxLight(float sensitivity)
    {
        light.text = $"Exposure: {8*sensitivity:F2}";
        GameManager.instance.ChangeSkyboxLight(sensitivity);
    }
}
