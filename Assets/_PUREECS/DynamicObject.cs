using Unity.Entities;
using Unity.Mathematics;

public struct DynamicObject : IComponentData
{
    public float3 forces;
    public float3 velocity;
}
