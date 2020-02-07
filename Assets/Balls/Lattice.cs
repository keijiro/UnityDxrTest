//
// Lattice - An example of vertex animation with C# Job System and New Mesh API
// https://github.com/keijiro/VertexAnimationJob
//

using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[ExecuteInEditMode, RequireComponent(typeof(MeshRenderer))]
public sealed class Lattice : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] int2 _resolution = math.int2(64, 64);
    [SerializeField] float2 _extent = math.float2(10, 10);
    [SerializeField] float _noiseFrequency = 1;
    [SerializeField, Range(1, 8)] int _noiseOctave = 2;
    [SerializeField] float _noiseAmplitude = 0.1f;
    [SerializeField] float _noiseAnimation = 0.5f;

    void OnValidate()
    {
        _resolution = math.max(_resolution, math.int2(3, 3));
        _extent = math.max(_extent, float2.zero);
    }

    #endregion

    #region Internal objects

    Mesh _mesh;
    int2 _meshResolution;

    #endregion

    #region MonoBehaviour implementation

    void OnDestroy()
    {
        if (_mesh != null)
        {
            if (Application.isPlaying)
                Destroy(_mesh);
            else
                DestroyImmediate(_mesh);
        }

        _mesh = null;
    }

    void Update()
    {
        using (var vertexArray = CreateVertexArray())
        {
            if (_meshResolution.Equals(_resolution))
                UpdateVerticesOnMesh(vertexArray);
            else
                ResetMesh(vertexArray);
        }

        UpdateMeshBounds();
    }

    #endregion

    #region Mesh object operations

    void ResetMesh(NativeArray<Vertex> vertexArray)
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.hideFlags = HideFlags.DontSave;

            var mf = GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = gameObject.AddComponent<MeshFilter>();
                mf.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
            }

            mf.sharedMesh = _mesh;
        }
        else
        {
            _mesh.Clear();
        }

        var vertexCount = vertexArray.Length;

        _mesh.SetVertexBufferParams(
            vertexCount,
            new VertexAttributeDescriptor
                (VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor
                (VertexAttribute.Normal, VertexAttributeFormat.SNorm16, 4)
        );
        _mesh.SetVertexBufferData(vertexArray, 0, 0, vertexCount);

        using (var indexArray = CreateIndexArray(vertexCount))
        {
            _mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);
            _mesh.SetIndexBufferData(indexArray, 0, 0, vertexCount);
        }

        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount));

        _meshResolution = _resolution;
    }

    void UpdateVerticesOnMesh(NativeArray<Vertex> vertexArray)
    {
        _mesh.SetVertexBufferData(vertexArray, 0, 0, vertexArray.Length);
    }

    void UpdateMeshBounds()
    {
        var size = math.float3(_extent, _noiseAmplitude * 2);
        _mesh.bounds = new Bounds(Vector3.zero, size);
    }

    #endregion

    #region Index array operations

    NativeArray<uint> CreateIndexArray(int length)
    {
        var buffer = new NativeArray<uint>(
            length, Allocator.Temp,
            NativeArrayOptions.UninitializedMemory
        );

        for (var i = 0; i < length; i++) buffer[i] = (uint)i;

        return buffer;
    }

    #endregion

    #region Jobified vertex animation

    struct Vertex
    {
        public float3 position;
        public SNorm16x4 normal;
    }

    NativeArray<Vertex> CreateVertexArray()
    {
        var triangleCount = 2 * _resolution.x * _resolution.y;

        var points = new NativeArray<float3>(
            (_resolution.x + 1) * (_resolution.y + 1),
            Allocator.TempJob, NativeArrayOptions.UninitializedMemory
        );

        var vertices = new NativeArray<Vertex>(
            triangleCount * 3,
            Allocator.TempJob, NativeArrayOptions.UninitializedMemory
        );

        var job_p = new PointCalculationJob {
            rotation = Time.time * 0.3f,
            resolution = _resolution,
            extent = _extent,
            noiseFreq = _noiseFrequency,
            noiseOct = _noiseOctave,
            noiseAmp = _noiseAmplitude,
            noiseRot = Time.time * _noiseAnimation,
            output = points
        };

        var job_v = new VertexConstructionJob {
            resolution = _resolution,
            points = points,
            output = vertices
        };

        var handle_p = job_p.Schedule(points.Length, 64);
        var handle_v = job_v.Schedule(triangleCount, 64, handle_p);
        
        handle_v.Complete();
        points.Dispose();

        return vertices;
    }

    [Unity.Burst.BurstCompile(CompileSynchronously = true)]
    struct PointCalculationJob : IJobParallelFor
    {
        [ReadOnly] public float rotation;
        [ReadOnly] public int2 resolution;
        [ReadOnly] public float2 extent;
        [ReadOnly] public float noiseFreq;
        [ReadOnly] public int noiseOct;
        [ReadOnly] public float noiseAmp;
        [ReadOnly] public float noiseRot;

        [WriteOnly] public NativeArray<float3> output;

        public void Execute(int i)
        {
            var columns = resolution.x + 1;
            var row = i / columns;
            var odd = (row & 1) != 1;

            var p = math.float2(i - row * columns, row);
            p.x += odd ? -0.25f : +0.25f;
            p = (p / resolution - 0.5f) * extent;

            var np = (p + extent) * noiseFreq;
            var nw = 1.0f;
            var z = 0.0f;

            for (var lv = 0; lv < noiseOct; lv++)
            {
                z += noise.srnoise(np, noiseRot) * nw;
                np *= 2;
                nw *= 0.5f;
            }

            output[i] = math.float3(p, z * noiseAmp);
        }
    }

    [Unity.Burst.BurstCompile(CompileSynchronously = true)]
    struct VertexConstructionJob : IJobParallelFor
    {
        [ReadOnly] public int2 resolution;
        [ReadOnly] public NativeArray<float3> points;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<Vertex> output;

        public void Execute(int i)
        {
            var it = i / 2;
            var iy = it / resolution.x;
            var ix = it - iy * resolution.x;

            var i1 = iy * (resolution.x + 1) + ix;
            var i2 = i1;
            var i3 = i1;

            if ((iy & 1) == 0)
            {
                if ((i & 1) == 0)
                {
                    i2 += resolution.x + 1;
                    i3 += 1;
                }
                else
                {
                    i1 += resolution.x + 1;
                    i2 += resolution.x + 2;
                    i3 += 1;
                }
            }
            else
            {
                if ((i & 1) == 0)
                {
                    i2 += resolution.x + 1;
                    i3 += resolution.x + 2;
                }
                else
                {
                    i2 += resolution.x + 2;
                    i3 += 1;
                }
            }

            var V1 = points[i1];
            var V2 = points[i2];
            var V3 = points[i3];

            var N = (SNorm16x4)math.normalize(math.cross(V2 - V1, V3 - V1));

            output[i * 3 + 0] = new Vertex { position = V1, normal = N };
            output[i * 3 + 1] = new Vertex { position = V2, normal = N };
            output[i * 3 + 2] = new Vertex { position = V3, normal = N };
        }
    }

    #endregion
}
