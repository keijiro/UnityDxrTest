using UnityEngine;
using Unity.Mathematics;

class RandomSpawner : MonoBehaviour
{
    [SerializeField] GameObject _prefab = null;
    [SerializeField] uint _instanceCount = 20;
    [SerializeField] uint _randomSeed = 0xdeadbeef;
    [SerializeField] float3 _extent = math.float3(1, 1, 1);

    void Start()
    {
        var rand = new Unity.Mathematics.Random(_randomSeed);
        var parent = transform;
        var rot = quaternion.identity;

        for (var i = 0; i < _instanceCount; i++)
        {
            var pos = (rand.NextFloat3() - 0.5f) * _extent;
            pos = parent.TransformPoint(pos);
            Instantiate(_prefab, pos, rot, parent);
        }
    }
}
