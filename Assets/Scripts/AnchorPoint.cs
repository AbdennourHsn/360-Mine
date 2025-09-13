using System;
using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    [SerializeField] private Texture cubeMapTexture;
    public int id;
    [Range(-180, 180)] [SerializeField] private float _RotationX;
    [Range(-180, 180)] [SerializeField] private float _RotationY;
    [Range(-180, 180)] [SerializeField] private float _RotationZ;
    [SerializeField] private bool enabled = false;
    [SerializeField] private GameObject selector;

    public void SetAnchorPoint(bool active)
    {
        selector.SetActive(active);
    }

    public AnchorPointData GetCubeMap()
    {
        return new AnchorPointData()
        {
            id = this.id,
            cubeMapTexture = this.cubeMapTexture,
            _RotationX = this._RotationX,
            _RotationY = this._RotationY,
            _RotationZ = this._RotationZ,
        };
    }
}

[System.Serializable]
public class AnchorPointData
{
    public int id;
    public Texture cubeMapTexture;
    public float _RotationX;
    public float _RotationY;
    public float _RotationZ;
}