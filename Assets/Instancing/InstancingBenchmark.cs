using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;

namespace UnityDxrTest.Instancing {

public sealed class InstancingBenchmark : MonoBehaviour
{
    #region Public properties

    [field:SerializeField]
    public GameObject Prefab { get; set; }

    [field:SerializeField]
    public Vector3Int Dimensions { get; set; } = new Vector3Int(8, 8, 8);

    [field:SerializeField]
    public float Interval { get; set; } = 1;

    [field:SerializeField]
    public bool EnableSrpBatcher { get; set; } = true;

    #endregion

    #region Local arrays

    List<GameObject> _instances = new List<GameObject>();
    List<Transform> _xforms = new List<Transform>();
    List<Vector4> _temp = new List<Vector4>();

    #endregion

    #region Private properties and methods

    int TotalInstanceCount
      => Dimensions.x * Dimensions.y * Dimensions.z;

    void UpdateTemp()
    {
        var count = 0;
        for (var i = 0u; i < Dimensions.x; i++)
        {
            var x = Interval * (i - 0.5f * (Dimensions.x - 1));
            for (var j = 0u; j < Dimensions.y; j++)
            {
                var y = Interval * (j - 0.5f * (Dimensions.y - 1));
                for (var k = 0u; k < Dimensions.z; k++)
                {
                    var z = Interval * (k - 0.5f * (Dimensions.z - 1));
                    _temp[count++] = new Vector4(x, y, z, 0.1f);
                }
            }
        }
    }

    void UpdateXforms()
    {
        Profiler.BeginSample("Update Temp");
        UpdateTemp();
        Profiler.EndSample();

        Profiler.BeginSample("Update Transforms");
        for (var i = 0; i < TotalInstanceCount; i++)
        {
            _xforms[i].localPosition = _temp[i];
            _xforms[i].localScale = Vector3.one * _temp[i].w;
        }
        Profiler.EndSample();
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        var sheet = new MaterialPropertyBlock();

        _instances.Capacity = _xforms.Capacity = TotalInstanceCount;
        _temp.AddRange(new Vector4[TotalInstanceCount]);

        for (var i = 0; i < TotalInstanceCount; i++)
        {
            var go = Instantiate(Prefab);

            go.hideFlags = HideFlags.HideAndDontSave;

            if (!EnableSrpBatcher)
                go.GetComponent<Renderer>().SetPropertyBlock(sheet);

            _instances.Add(go);
            _xforms.Add(go.transform);
        }

        UpdateXforms();
    }

    void Update()
      => UpdateXforms();

    void OnDestroy()
    {
        foreach (var go in _instances) Destroy(go);
        _instances.Clear();
        _xforms.Clear();
    }

    #endregion
}

} // namespace UnityDxrTest.Instancing
