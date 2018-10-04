using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class CleanupMatterSpawningSystem : JobComponentSystem
{
    private struct CleanupMatterSpawningJob : IJobParallelFor
    {
        [ReadOnly] public EntityArray entities;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public float currentTime;
        public ComponentDataArray<MatterSpawning> spawnings;


        public void Execute(int index)
        {
            if(currentTime - spawnings[index].firedAt < 0.5f)
                entityCommandBuffer.RemoveComponent<MatterSpawning>(index, entities[index]);
        }
    }

    private struct Data
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentDataArray<MatterSpawning> Spawnings;
    }

    [Inject] private Data _data;
    [Inject] private CleanupMatterSpawningBarrier _barrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new CleanupMatterSpawningJob()
        {
            entities = _data.Entities,
            entityCommandBuffer = _barrier.CreateCommandBuffer().ToConcurrent(),
            currentTime = Time.time,
            spawnings = _data.Spawnings
        }.Schedule(_data.Length, 64, inputDeps);
    }
}

public class CleanupMatterSpawningBarrier : BarrierSystem
{

}