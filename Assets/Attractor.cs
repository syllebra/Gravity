using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Attractor : MonoBehaviour
{
    public static Attractor biggest = null;

    public float Mass
    {
        get { return rb == null ? 0F : rb.mass; }
        set
        {
            if (rb != null)
                rb.mass = value;

            if (biggest == null || Mass > biggest.Mass)
                biggest = this;

            RecomputeRadiusFromMass();
        }
    }

    public float density = 1f;
    public const double G = 66.74;
    public Rigidbody rb = null;

    public bool beeingDestroyed = false;

    public static List<Attractor> Attractors = null;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        RecomputeRadiusFromMass();
    }

    void FixedUpdate()
    {
        if (Attractors == null)
            return;
        foreach (var a in Attractors)
        {
            if (a != this)
                Attract(a);
        }

    }

    void Attract(Attractor b)
    {
        var diff = b.transform.position - transform.position;
        var d2 = diff.sqrMagnitude;

        if (d2 <= 0)
            return;

        var Fmag = G * (Mass * b.Mass) / d2;

        rb.AddForce(diff.normalized * (float)Fmag);
    }

    private void OnEnable()
    {
        if (Attractors == null)
            Attractors = new List<Attractor>();
        Attractors.Add(this);
    }

    private void OnDisable()
    {
        if (Attractors != null)
            Attractors.Remove(this);
    }

    void RecomputeRadiusFromMass()
    {
        float V = rb.mass * density;
        float rad = Mathf.Pow((V / Mathf.PI) * (3F / 4F), 1F / 3F);
        transform.localScale = Vector3.one * rad;
    }

    static void Fuse(Attractor a, Attractor b)
    {
        float sA = a.transform.localScale.magnitude * 0.5f;
        float sB = b.transform.localScale.magnitude * 0.5f;

        Attractor A = a, B = b;

        if (sA < sB)
        {
            A = b;
            B = a;
        }

        // transfer mass
        float tmp = B.Mass;
        B.beeingDestroyed = true;
        Destroy(B.gameObject);
        A.Mass += tmp;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (beeingDestroyed)
            return;
        var other = collision.collider.GetComponent<Attractor>();
        if (other == null)
            return;

        if (other.beeingDestroyed)
            return;

        Fuse(this, other);
    }

    Queue<Vector3> trajectory = new Queue<Vector3>();
    private void Start()
    {
    }

    void Update()
    {
        trajectory.Enqueue(transform.position);
        if (trajectory.Count > 150)
            trajectory.Dequeue();
        RecomputeRadiusFromMass();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || trajectory == null || trajectory.Count < 2)
            return;
        Gizmos.color = Color.red;

        Vector3 pLast = trajectory.Peek();
        foreach(var p in trajectory)
        {
            Gizmos.DrawLine(pLast, p);
            pLast = p;
            //Gizmos.DrawCube(p, Vector3.one * 4F);
        }
    }
}
