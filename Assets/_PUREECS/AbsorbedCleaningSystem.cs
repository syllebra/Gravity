using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;



using UnityEngine.Jobs;

public class CleaningBarrier : BarrierSystem { }

[UpdateAfter(typeof(CollisionsSolverSystem))]
public class AbsorbedCleaningSystem : JobComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        public ComponentDataArray<MatterObject> MatterObjects;
    }
    [Inject] private Data m_Data;
    [Inject] private CleaningBarrier m_Barrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new DestructionJob()
        {
            entityCommandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent(),
            Entities = m_Data.Entities,
            matterObjects = m_Data.MatterObjects
        }.Schedule(m_Data.Length, 1, inputDeps);
        return job;
    }

    [BurstCompile]
    struct DestructionJob : IJobParallelFor
    {
        //Since it's a write only, it should have no problem running in parallel
        [WriteOnly] public EntityCommandBuffer.Concurrent entityCommandBuffer;
        [ReadOnly] public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<MatterObject> matterObjects;

        public void Execute(int index)
        {
            if(matterObjects[index].Mass<=0)
                entityCommandBuffer.DestroyEntity(index, Entities[index]);
        }
    }
}