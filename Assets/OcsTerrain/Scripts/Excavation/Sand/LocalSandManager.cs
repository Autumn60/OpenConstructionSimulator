using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalSandManager : MonoBehaviour
{
    [SerializeField] private GameObject _sandObj;
    [SerializeField] private Transform _sandParent;
    [SerializeField] private int _maxSandCount;
    [SerializeField] private float _sandSize;

    [SerializeField] private Transform _anchor;
    [SerializeField] private int _spawnZone_resolution;
    [SerializeField] private float _spawnZone_width;
    [SerializeField] private float _spawnZone_depth;

    [SerializeField] private DeformableTerrain _terrain;
    [SerializeField] private LayerMask _terrainLayer;

    [SerializeField] private bool _debug_drawGizmos;

    private Transform _transform;
    private Sand[] _sands;

    private List<Sand>[,] _sandPillar;
    private Vector3[,] _spawnPos;
    private Vector3[,] _spawnPos_old;
    private float[,] _height;

    private float _wpr;
    private float _wpr2;
    private float _spawnZone_width_2;

    private void Awake()
    {
        _transform = this.transform;
        _sands = new Sand[_maxSandCount];
        _sandPillar = new List<Sand>[_spawnZone_resolution, _spawnZone_resolution];
        for (int i = 0; i < _spawnZone_resolution; i++)
            for (int j = 0; j < _spawnZone_resolution; j++)
                _sandPillar[i, j] = new List<Sand>();
        _spawnPos = new Vector3[_spawnZone_resolution, _spawnZone_resolution];
        _spawnPos_old = new Vector3[_spawnZone_resolution, _spawnZone_resolution];
        _height = new float[_spawnZone_resolution, _spawnZone_resolution];
    }

    private void Start()
    {
        for(int i = 0; i < _maxSandCount; i++)
        {
            var obj = Instantiate(_sandObj, Vector3.zero, Quaternion.identity);
            obj.name = "sand";
            if(_sandParent)
                obj.transform.parent = _sandParent;
            obj.SetActive(false);
            _sands[i] = obj.GetComponent<Sand>();
            _sands[i].terrain = _terrain;
        }

        for(int i = 0; i < _spawnZone_resolution; i++)
        {
            for(int j = 0; j < _spawnZone_resolution; j++)
            {
                _height[i, j] = -1000.0f;
            }
        }

        _wpr = _spawnZone_width / _spawnZone_resolution;
        _wpr2 = _wpr * _wpr;
        _spawnZone_width_2 = _spawnZone_width * 0.5f;
    }

    private void Update()
    {
        Vector3 anchor_pos = _anchor.position;

        for (int i = 0; i < _spawnZone_resolution; i++)
        {
            for (int j = 0; j < _spawnZone_resolution; j++)
            {
                _spawnPos[i, j] = new Vector3(
                                            (UnsignedFloor(anchor_pos.x) * (_spawnZone_resolution - 1) + i * _wpr) % _spawnZone_width,
                                            0,
                                            (UnsignedFloor(anchor_pos.z) * (_spawnZone_resolution - 1) + j * _wpr) % _spawnZone_width);
                _spawnPos[i, j] += new Vector3(Floor(anchor_pos.x, _wpr) - _spawnZone_width_2, 0, Floor(anchor_pos.z, _wpr) - _spawnZone_width_2);

                if ((_spawnPos[i, j] - _spawnPos_old[i, j]).sqrMagnitude > _wpr2 || _height[i, j] < -999.0f)
                {
                    _spawnPos_old[i, j] = _spawnPos[i, j];
                    foreach (Sand sand in _sandPillar[i, j])
                    {
                        if (!sand) continue;
                        if (sand.isActive) continue;
                        sand.gameObject.SetActive(false);
                    }
                    _sandPillar[i, j].Clear();

                    float ray_offset = 5.0f;
                    Ray ray = new Ray(_spawnPos[i, j] + Vector3.up * ray_offset, Vector3.down);
                    RaycastHit hit;
                    if(Physics.Raycast(ray, out hit, float.MaxValue, _terrainLayer))
                    {
                        _height[i, j] = ray_offset - hit.distance;
                        for(float depth = _sandSize*0.5f; depth < _spawnZone_depth; depth+=_sandSize)
                        {
                            int sandIndex = GetUnusedSandIndex();
                            if (sandIndex == -1) break;
                            _sandPillar[i, j].Add(_sands[sandIndex]);
                            _sands[sandIndex].SetPosition(_spawnPos[i, j].x, _height[i, j] - depth, _spawnPos[i, j].z);
                            _sands[sandIndex].gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        _height[i, j] = -1000.0f;
                    }
                }
                else
                {
                    foreach (Sand sand in _sandPillar[i, j])
                    {
                        if (!sand) continue;
                        if (sand.isActive) continue;
                        sand.SetPosition(_spawnPos[i, j].x, _spawnPos[i, j].z);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!_debug_drawGizmos) return;
        if (_spawnPos == null) return;
        for (int i = 0; i < _spawnZone_resolution; i++)
        {
            for (int j = 0; j < _spawnZone_resolution; j++)
            {
                Gizmos.DrawSphere(_spawnPos[i, j], _wpr*0.2f);
            }
        }
    }

    float Floor(float val, float interval)
    {
        return interval * (int)(val / interval) + (val < 0 ? - interval : 0);
    }

    float UnsignedFloor(float val)
    {
        return _wpr * (int)(val / _wpr) + (val < 0 ? -Floor(val, _spawnZone_width) - _wpr : 0);
    }

    int GetUnusedSandIndex()
    {
        for(int i = 0; i < _maxSandCount; i++)
        {
            if (!_sands[i].isUsed) return i;
        }
        return -1;
    }
}
