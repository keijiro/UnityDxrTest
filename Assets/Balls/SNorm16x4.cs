using Unity.Mathematics;

public struct SNorm16x4
{
    public uint lo;
    public uint hi;

    public SNorm16x4(float3 v)
    {
        var vi = math.clamp(v, -1, 1) * 0x7fff;
        lo = (uint)((ushort)vi.x) | ((uint)((ushort)vi.y) << 16);
        hi = (ushort)vi.z;
    }

    public static implicit operator SNorm16x4(float3 v) => new SNorm16x4(v);
}
