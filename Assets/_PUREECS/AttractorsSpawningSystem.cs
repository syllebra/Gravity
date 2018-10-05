using System.Linq.Expressions;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AttractorsSpawningSystem : JobComponentSystem
{
    [Inject] private AttractorSpawningBarrier _barrier;

    int nb = 0;
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle last = inputDeps;
        for (int i = 0; i < Bootstrap.Instance.SpawnNumber; i++)
        {
            float radius = Bootstrap.Instance != null ? Bootstrap.Instance.spawningRange * Bootstrap.Instance.mostmassiveObjectRadius : 30F;
            float minMassRange = Bootstrap.Instance != null ? Bootstrap.Instance.minMassRange : 0.5F;
            float maxMassRange = Bootstrap.Instance != null ? Bootstrap.Instance.maxMassRange : 20F;
            float3 velocity = Bootstrap.Instance != null ?
               new float3(Bootstrap.Instance.mostmassiveObjectVelocity.x, Bootstrap.Instance.mostmassiveObjectVelocity.y, Bootstrap.Instance.mostmassiveObjectVelocity.z)
                : new float3(0F) ;
            var job = new AttractorSpawningJob
            {
                num = ++nb,

                randomize = new float3(UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius)),
                mass = UnityEngine.Random.Range(minMassRange, maxMassRange),
                vel = velocity,
                entityCommandBuffer = _barrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, last);
            last = job;
        }
        return last;
    }

    private struct AttractorSpawningJob : IJobProcessComponentData<MatterSpawning, Position, Rotation>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public float3 randomize, vel;
        public float mass;
        public int num;
        public void Execute(ref MatterSpawning spawning, ref Position position, ref Rotation rotation)
        {
            entityCommandBuffer.CreateEntity(num);
            entityCommandBuffer.AddSharedComponent(num, Bootstrap.MatterRenderer);
            //entityCommandBuffer.AddComponent(num, new TransformMatrix());
            //entityCommandBuffer.AddSharedComponent(num, new MoveForward());
            var spawnPos = position;
            spawnPos.Value = spawnPos.Value + randomize;
            entityCommandBuffer.AddComponent(num, spawnPos);
            entityCommandBuffer.AddComponent(num, rotation);
            entityCommandBuffer.AddComponent(num, new Scale() {Value = new float3(1F)});
            entityCommandBuffer.AddComponent(num, new MatterObject() { Mass = mass, Density = 1F });
            entityCommandBuffer.AddComponent(num, new DynamicObject() { velocity = new float3(0, 0, 0), forces = new float3(0, 0, 0) });
            entityCommandBuffer.AddComponent(num, new PendingMatter() { MassToAdd = 0F });
        }
    }

    private class AttractorSpawningBarrier : BarrierSystem
    {
    }
}
