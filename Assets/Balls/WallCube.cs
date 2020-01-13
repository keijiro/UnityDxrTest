using UnityEngine;
using Unity.Mathematics;

class WallCube : MonoBehaviour
{
    [SerializeField] float _noiseFrequency = 1;
    [SerializeField] float _noiseAnimation = 1;
    [SerializeField] float _noiseAmplitude = 1;

    void Update()
    {
        var xy = ((float3)transform.localPosition).xy;
        var t = Time.time;
        var z = noise.snoise(math.float3(xy * _noiseFrequency, t * _noiseAnimation));
        transform.localPosition = math.float3(xy, z * _noiseAmplitude);
    }
}
