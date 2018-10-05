using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct PendingMatter : IComponentData
{
    public float MassToAdd;
    //public float Density;
}

[UpdateAfter(typeof(PhysicsSystem))]
public class CollisionsDetectionSystem : JobComponentSystem
{
    static bool IsCollision(float3 pa, float3 pb, float ra, float rb)
    {
        var diff = pb - pa;
        var d2 = math.lengthsq(diff);

        return d2 < ra*ra + rb*rb;
    }

    private struct CollisionDetectionJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<MatterObject> matterObjects;
        public ComponentDataArray<PendingMatter> pendingMatters;
        [ReadOnly] public ComponentDataArray<Position> matterObjectsPositions;
        [ReadOnly] public ComponentDataArray<Scale> matterObjectsScales;

        public void Execute(int index)
        {
            var attract = new float3(0, 0, 0);
            for (int i = 0; i < matterObjects.Length; i++)
                if (i != index && IsCollision(matterObjectsPositions[index].Value, matterObjectsPositions[i].Value,
                        matterObjectsScales[index].Value.x, matterObjectsScales[i].Value.x))
                {
                    float matterDiff = matterObjects[i].Mass; // will Absorb
                    if (pendingMatters[index].MassToAdd >=0 && matterObjects[i].Mass > matterObjects[index].Mass)
                        matterDiff = -matterObjects[index].Mass; // will be absorbed
                    float tmp = pendingMatters[index].MassToAdd + matterDiff;
                    pendingMatters[index] = new PendingMatter() { MassToAdd = tmp };
                }
        }
    }

    private struct Data
    {
        public readonly int Length;
        public ComponentDataArray<MatterObject> MatterObjects;
        public ComponentDataArray<PendingMatter> PendingMatters;
        public ComponentDataArray<Position> MatterObjectsPositions;
        public ComponentDataArray<Scale> MatterObjectsScales;
    }

    [Inject] private Data _data;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new CollisionDetectionJob
        {
            matterObjects = _data.MatterObjects,
            matterObjectsScales = _data.MatterObjectsScales,
            matterObjectsPositions = _data.MatterObjectsPositions,
            pendingMatters = _data.PendingMatters,
        }.Schedule(_data.Length, 64, inputDeps);
    }

}