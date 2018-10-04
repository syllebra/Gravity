using Unity.Entities;

public struct MatterSpawnPosition : IComponentData
{
}

public class MatterSpawnComponent : ComponentDataWrapper<MatterSpawnPosition>
{
}
