using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(AttractionSystem))]
public class PhysicsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var job = new ApplyDynamicsJob
        {
            dt = Time.deltaTime
        };
        job.Schedule(this).Complete();
    }

    [BurstCompile]
    private struct ApplyDynamicsJob : IJobProcessComponentData<MatterObject, DynamicObject, Position>
    {
        public float dt;
        public void Execute(ref MatterObject obj, ref DynamicObject dyn, ref Position pos)
        {
            if (obj.Mass <= 0)
                return;
            float3 acceleration = dyn.forces / obj.Mass;
            //float3 acceleration =  new float3(0F,-9.81F,0F);
            dyn.forces = acceleration;
            dyn.velocity += acceleration * dt;
            pos.Value = pos.Value + dyn.velocity * dt;
        }
    }
}