using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(CollisionsDetectionSystem))]
public class CollisionsSolverSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var job = new CollisionSolvingJob
        {
        };
        job.Schedule(this).Complete();
    }

    private struct CollisionSolvingJob : IJobProcessComponentData<MatterObject, PendingMatter>
    {
        public void Execute(ref MatterObject obj, ref PendingMatter pending)
        {
            obj.Mass = obj.Mass + pending.MassToAdd;
            pending.MassToAdd = 0F;
        }
    }
}
