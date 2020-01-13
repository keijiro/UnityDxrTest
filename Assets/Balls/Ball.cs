using UnityEngine;
using Unity.Mathematics;

class Ball : MonoBehaviour
{
    [SerializeField] float _force = 1;
    [SerializeField] float _noiseFrequency = 1;
    [SerializeField] float _noiseAnimation = 1;
    [SerializeField] float _noiseAmplitude = 1;

    Rigidbody _rigidBody;

    static readonly int _idBaseColor = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();

        var color = Color.HSVToRGB(UnityEngine.Random.value, 0.5f, 1);
        var material = GetComponent<MeshRenderer>().material;
        material.SetColor(_idBaseColor, color);
    }

    void FixedUpdate()
    {
        var parent = (float3)transform.parent.position;
        var pos = (float3)transform.position;

        var noffs = _noiseAnimation * Time.time;
        var np1 = pos * _noiseFrequency + math.float3(0, noffs, 0);
        var np2 = math.float3(3.24f, 7.12f, 4.34f) - np1.yzx;

        float3 grad1, grad2;
        noise.snoise(np1, out grad1);
        noise.snoise(np2, out grad2);
        var dfn = math.cross(grad1, grad2) * _noiseAmplitude;

        _rigidBody.AddForce((parent - pos) * _force + dfn);
    }
}
