using UnityEngine;

sealed public class Rainbow : MonoBehaviour
{
    [SerializeField] Mesh _mesh = null;
    [SerializeField] Material _material = null;

    (GameObject gameObject, Material material)[]
        _columns = new (GameObject, Material)[24];

    void Start()
    {
        for (var i = 0; i < _columns.Length; i++)
        {
            var go = new GameObject("Torus");
            var mat = new Material(_material);

            go.AddComponent<MeshFilter>().sharedMesh = _mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;

            var x = (i - _columns.Length / 2 + 1) * 3.0f / _columns.Length;

            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(x, 0, 0);
            go.transform.localRotation = Quaternion.Euler(0, 90, 0);
            go.transform.localScale = Vector3.one * 0.3f;

            _columns[i] = (go, mat);
        }
    }

    void Update()
    {
        for (var i = 0; i < _columns.Length; i++)
        {
            var rz = i * 440.0f / _columns.Length + Time.time * 110;
            var rot = Quaternion.Euler(0, 90, rz);
            var col = Color.HSVToRGB((Time.time + i) * 0.1f % 1, 1, 10, true);
            _columns[i].gameObject.transform.localRotation = rot;
            _columns[i].material.SetColor("_EmissiveColor", col);
        }
    }
}
