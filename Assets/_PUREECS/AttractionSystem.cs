using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AttractionSystem : JobComponentSystem
{
    public const float G = 66.74F;


    static float3 AttractionForce(float3 pa, float3 pb, float massA, float massB)
    {
        var diff = pb - pa;
        var d2 = math.lengthsq(diff);

        if (d2 <= 0)
            return new float3(0, 0, 0);

        var Fmag = G * (massA * massB) / d2;
        return diff * Fmag / math.sqrt(d2);
    }

     private struct AttractionJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<MatterObject> matterObjects;
        [ReadOnly] public ComponentDataArray<Position> matterObjectsPositions;
        public ComponentDataArray<DynamicObject> dynamicObjects;

        public void Execute(int index)
        {
            var attract = new float3(0, 0, 0);
            for (int i = 0; i < matterObjects.Length; i++)
                if (i != index)
                {
                    attract += AttractionForce(matterObjectsPositions[index].Value, matterObjectsPositions[i].Value,
                        matterObjects[index].Mass, matterObjects[i].Mass);
                }
            var tmp = dynamicObjects[index];
            tmp.forces = attract;
            dynamicObjects[index] = tmp;
        }
    }

    private struct Data
    {
        public readonly int Length;
        public ComponentDataArray<MatterObject> MatterObjects;
        public ComponentDataArray<DynamicObject> DynamicObjects;
        public ComponentDataArray<Position> MatterObjectsPositions;
    }

    [Inject] private Data _data;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new AttractionJob
        {
            matterObjects = _data.MatterObjects,
            dynamicObjects = _data.DynamicObjects,
            matterObjectsPositions = _data.MatterObjectsPositions,

        }.Schedule(_data.Length, 64, inputDeps);
    }

}
