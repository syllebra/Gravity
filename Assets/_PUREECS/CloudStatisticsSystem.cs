using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

[UpdateAfter(typeof(ScaleObjectsSystem))]
public class CloudStatisticsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        NativeArray<float> BiggestValues = new NativeArray<float>(2, Allocator.TempJob);
        NativeArray<float3> BiggestF3 = new NativeArray<float3>(2, Allocator.TempJob);
        BiggestValues[0] = float.NegativeInfinity;

        var job = new FindMostMassiveJob
        {
            bv = BiggestValues,
            bp = BiggestF3
        };
        job.Schedule(this).Complete();
        //UnityEngine.Debug.Log(BiggestPosition[0].ToString());
        if(Bootstrap.Instance != null)
            Bootstrap.Instance.SetupBiggest(new Vector3(BiggestF3[0].x, BiggestF3[0].y, BiggestF3[0].z), BiggestValues[1], BiggestValues[0],
                new Vector3(BiggestF3[1].x, BiggestF3[1].y, BiggestF3[1].z));
        BiggestValues.Dispose();
        BiggestF3.Dispose();
    }

    public const float VA = 3F / (4F * 3.14159F);
    public const float VB = 1F / 3F;

    [BurstCompile]
    private struct FindMostMassiveJob : IJobProcessComponentData<MatterObject, Position, Scale, DynamicObject>
    {
        public NativeArray<float> bv;
        public NativeArray<float3> bp;

        public void Execute(ref MatterObject obj, ref Position pos, ref Scale sc, ref DynamicObject dyn)
        {
            if (obj.Mass > bv[0])
            {
                bv[0] = obj.Mass;
                bp[0] = pos.Value;
                bv[1] = sc.Value.x;
                bp[1] = dyn.velocity;
            }
        }
    }
}