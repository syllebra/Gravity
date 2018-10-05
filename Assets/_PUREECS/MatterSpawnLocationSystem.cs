using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class MatterSpawnLocationSystem : JobComponentSystem
{
    private struct AttractorSpawnJob : IJobParallelFor
    {
        [ReadOnly] public EntityArray entityArray;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public float currentTime;
        public void Execute(int index)
        {
            entityCommandBuffer.AddComponent(index, entityArray[index], new MatterSpawning() { firedAt = currentTime });
        }
    }

    private struct Data
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentDataArray<MatterSpawnPosition> MatterSpawnPositions;
        public SubtractiveComponent<MatterSpawning> Spawnings;
    }

    [Inject] private Data _data;
    [Inject] private MatterSpawnLocationBarrier _barrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (Input.GetKey(KeyCode.Return))
        {
            return new AttractorSpawnJob
            {
                entityArray = _data.Entities,
                entityCommandBuffer = _barrier.CreateCommandBuffer().ToConcurrent(),
                currentTime = Time.time
            }.Schedule(_data.Length, 64, inputDeps);
        }
        return base.OnUpdate(inputDeps);
    }
}

public class MatterSpawnLocationBarrier : BarrierSystem
{
}