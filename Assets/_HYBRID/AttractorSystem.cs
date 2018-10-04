using Unity.Entities;
using UnityEngine;

public class AttractorSystem : ComponentSystem
{
    public const double G = 66.74;

    private struct AttractorFilter
    {
        public Rigidbody rigidbody;
        public AttractorHybridComponent attractorComponent;
    }

    void Attract(AttractorFilter a, AttractorFilter b)
    {
        var diff = b.rigidbody.position - a.rigidbody.position;
        var d2 = diff.sqrMagnitude;

        if (d2 <= 0)
            return;

        var Fmag = G * (a.attractorComponent.Mass * b.attractorComponent.Mass) / d2;

        a.rigidbody.AddForce(diff.normalized * (float)Fmag);
    }

    protected override void OnUpdate()
    {
        //var deltaTime = Time.deltaTime;
        var attractors = GetEntities<AttractorFilter>();
        foreach (var a in attractors)
        {
            foreach (var b in attractors)
            {
                if (a.rigidbody != b.rigidbody)
                    Attract(a, b);
            }
        }
    }
}