using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(AbsorbedCleaningSystem))]
public class ScaleObjectsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var job = new ScaleJob
        {
        };
        job.Schedule(this).Complete();
    }

    public const float VA = 3F / (4F * 3.14159F);
    public const float VB = 1F / 3F;

    [BurstCompile]
    private struct ScaleJob : IJobProcessComponentData<MatterObject, Scale>
    {
        public void Execute(ref MatterObject obj, ref Scale sc)
        {
            float V = obj.Mass / obj.Density;
            float rad = math.pow(V * VA, VB);
            sc.Value = new float3(rad);
        }
    }
}