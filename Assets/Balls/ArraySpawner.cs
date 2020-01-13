using UnityEngine;
using Unity.Mathematics;

class ArraySpawner : MonoBehaviour
{
    [SerializeField] GameObject _prefab = null;
    [SerializeField] uint2 _resolution = new uint2(10, 10);
    [SerializeField] float _interval = 1;

    void Start()
    {
        var parent = transform;
        var rot = quaternion.identity;

        for (var ix = 0; ix < _resolution.x; ix++)
        {
            for (var iy = 0; iy < _resolution.y; iy++)
            {
                var xy = math.float2(ix, iy) - (float2)_resolution * 0.5f;
                var pos = math.float3(xy * _interval, 0);
                pos = parent.TransformPoint(pos);
                Instantiate(_prefab, pos, rot, parent);
            }
        }
    }
}
