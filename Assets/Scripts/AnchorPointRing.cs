using System;
using UnityEngine;

public class AnchorPointRing : MonoBehaviour
{
    private Animator _animation;
    [SerializeField]
    private Material _mainMaterial;
    [SerializeField]
    private Material _hoverMaterial;
    private void Start()
    {
        _animation = GetComponent<Animator>();
    }

    public void OnHoverEnter()
    {
        if(_animation!=null) _animation.SetBool("animate" ,true);
        this.GetComponent<MeshRenderer>().material = _hoverMaterial;
        
    }
    
    public void OnHoverExit()
    {
        if(_animation!=null) _animation.SetBool("animate" ,false);
        this.GetComponent<MeshRenderer>().material = _mainMaterial;
    }
    
    public void OnClick()
    {
        var anchorPoint = transform.parent.GetComponent<AnchorPoint>();
        if (anchorPoint != null) StartCoroutine(GameManager.instance.MoveToAnchorPoint(anchorPoint));
    }
}
