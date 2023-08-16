using UnityEngine;
using System.Collections.Generic;

namespace UnityDxrTest.Instancing {

public sealed class InstancingBenchmark : MonoBehaviour
{
    [field:SerializeField] GameObject Prefab { get; set; }
    [field:SerializeField] Vector3Int Dimensions { get; set; } = new Vector3Int(8, 8, 8);
    [field:SerializeField] float Interval { get; set; } = 1;
    [field:SerializeField] bool EnableSrpBatcher { get; set; } = true;

    List<GameObject> _instances = new List<GameObject>();

    void Start()
    {
        var q = Quaternion.identity;
        var mpb = new MaterialPropertyBlock();
        for (var i = 0u; i < Dimensions.x; i++)
        {
            var x = Interval * (i - 0.5f * (Dimensions.x - 1));
            for (var j = 0u; j < Dimensions.y; j++)
            {
                var y = Interval * (j - 0.5f * (Dimensions.y - 1));
                for (var k = 0u; k < Dimensions.z; k++)
                {
                    var z = Interval * (k - 0.5f * (Dimensions.z - 1));
                    var p = new Vector3(x, y, z);
                    var go = Instantiate(Prefab, p, q);
                    go.hideFlags = HideFlags.HideAndDontSave;
                    if (!EnableSrpBatcher)
                        go.GetComponent<Renderer>().SetPropertyBlock(mpb);
                    _instances.Add(go);
                }
            }
        }
    }

    void OnDestroy()
    {
        foreach (var go in _instances) Destroy(go);
        _instances.Clear();
    }
}

} // namespace UnityDxrTest.Instancing
