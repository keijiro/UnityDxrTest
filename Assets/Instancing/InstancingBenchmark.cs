using System.Linq;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace UnityDxrTest.Instancing {

public sealed class InstancingBenchmark : MonoBehaviour
{
    #region Public properties

    [field:SerializeField]
    public GameObject Prefab { get; set; }

    [field:SerializeField]
    public int3 Dimensions { get; set; } = 8;

    [field:SerializeField]
    public float Interval { get; set; } = 1;

    [field:SerializeField]
    public bool EnableSrpBatcher { get; set; } = true;

    #endregion

    #region Private properties and methods

    GameObject[] _instances;
    TransformAccessArray _xforms;

    int TotalInstanceCount
      => Dimensions.x * Dimensions.y * Dimensions.z;

    void UpdateXforms()
      => new XformJob(){ Dimensions = Dimensions }.Schedule(_xforms);

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        var sheet = new MaterialPropertyBlock();
        _instances = new GameObject[TotalInstanceCount];

        for (var i = 0; i < TotalInstanceCount; i++)
        {
            var go = Instantiate(Prefab);
            go.hideFlags = HideFlags.HideAndDontSave;

            if (!EnableSrpBatcher)
                go.GetComponent<Renderer>().SetPropertyBlock(sheet);

            _instances[i] = go;
        }

        _xforms = new TransformAccessArray
          (_instances.Select(go => go.transform).ToArray());

        UpdateXforms();
    }

    void Update()
      => UpdateXforms();

    void OnDestroy()
    {
        foreach (var go in _instances) Destroy(go);
        _instances = null;
        _xforms.Dispose();
    }

    #endregion
}

[BurstCompile]
struct XformJob : IJobParallelForTransform
{
    public int3 Dimensions;

    [BurstCompile]
    public void Execute(int index, TransformAccess transform)
    {
        var p = math.float3(index % Dimensions.x,
                            index / Dimensions.x % Dimensions.y,
                            index / (Dimensions.x * Dimensions.y));
        transform.position = (p - (float3)(Dimensions - 1) * 0.5f) * 0.1f;
        transform.localScale = (float3)0.1f;
    }
}

} // namespace UnityDxrTest.Instancing
