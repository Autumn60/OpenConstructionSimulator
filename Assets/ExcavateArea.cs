using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcavateArea : MonoBehaviour
{
    [SerializeField] private DeformableTerrain _terrain;

    private void Awake()
    {
        if(!_terrain) _terrain = UnityEngine.Object.FindObjectOfType<DeformableTerrain>(); ;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Sand")
        {
            var sand = other.GetComponent<Sand>();
            sand.Activate();
            if (_terrain.GetHeight(other.transform.position) > other.transform.position.y - sand.minRadius) {
                _terrain.SetHeight(other.transform.position, other.transform.position.y - sand.minRadius);
                _terrain.OnHeightmapChanged();
            }
        }
    }
}
