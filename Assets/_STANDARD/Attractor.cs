﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Attractor : MonoBehaviour
{
    public const float MinEarthMassesForStar = 332946F * 0.08F;

    public static Attractor biggest = null;

    protected Light starLight = null;
    protected LensFlare starFlare = null;
    protected Material starLightMat = null;

    public static Gradient colorGradient = null;

    // Try to get a color frommass of a star (in EarthMass units)
    // here we only approximate in "MainSequence" from a Hertsprung-Russe diagram
     // (assuming star temperature is directly related to mass in this sequence)
     // TODO: light.colorTemperature
    static Color GetColorFromMass(float earthMasses)
    {
        if (colorGradient == null)
        {
            colorGradient = new Gradient();
            GradientColorKey[] gck;
            GradientAlphaKey[] gak;
            gck = new GradientColorKey[4];
            gck[0].time = 0.00001F;
            gck[0].color = Color.red;
            gck[1].time = 0.001F;
            gck[1].color = Color.yellow;
            gck[2].time = 0.010F;
            gck[2].color = Color.white;
            gck[3].time = 1.0F;
            gck[3].color = Color.blue;

            gak = new GradientAlphaKey[4];
            gak[0].alpha = 1.0F;
            gak[0].time = 0.00001F;
            gak[1].alpha = 1.0F;
            gak[1].time = 0.001F;
            gak[2].alpha = 1.0F;
            gak[2].time = 0.01F;
            gak[3].alpha = 1.0F;
            gak[3].time = 1.0F;
            colorGradient.SetKeys(gck, gak);
        }

        float sunMasses = earthMasses / 332946F;
      
        return colorGradient.Evaluate(sunMasses* 0.001F);
    }

    void UpdateLight()
    {
        if (starLight == null && Mass > MinEarthMassesForStar)
        {
            var star = Instantiate(Resources.Load("Star")) as GameObject;
            star.transform.parent = transform;
            star.transform.position = transform.position;

            starLight = star.GetComponent<Light>();// gameObject.AddComponent<Light>();
            starFlare = star.GetComponent<LensFlare>();
            Debug.Log("A star is born");
            starLight.shadows = LightShadows.Hard;
            starLightMat = Instantiate(Resources.Load("StarLight")) as Material;
            GetComponent<Renderer>().material = starLightMat;
        }
        if (starLight == null)
            return;

        starLight.range = Mass * 0.1f;
        starLight.intensity = Mass * 0.0005f;
        starLight.color = GetColorFromMass(Mass);
        if (starLightMat != null)
        {
            //starLightMat.color = starLight.color;
            starLightMat.SetColor("_EmissionColor", starLight.color* starLight.intensity);
        }

        if (starFlare != null)
            starFlare.color = starLight.color;
    }

    public float Mass
    {
        get { return rb == null ? 0F : rb.mass; }
        set
        {
            if (rb != null)
                rb.mass = value;

            if (biggest == null || Mass > biggest.Mass)
                biggest = this;

            UpdateLight();

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
        float V = rb.mass / density;
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
