using Unity.Entities;
using Unity.Mathematics;

public struct MatterObject : IComponentData
{
    public float Mass;
    public float Density;
}
