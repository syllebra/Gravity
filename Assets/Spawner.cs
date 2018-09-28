﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public GameObject prefab;
    public float radius = 100f;
	// Use this for initialization
	void Spawn ()
    {
        var initPos = Attractor.biggest != null ? Attractor.biggest.transform.position : Vector3.zero;
        var initScale = Attractor.biggest != null ? Attractor.biggest.transform.localScale.magnitude : 1F;

        var s = Instantiate(prefab, initPos + new Vector3(Random.Range(-0.5F, 0.5F), Random.Range(-0.5F, 0.5F), Random.Range(-0.5F, 0.5F)) * radius * initScale, Quaternion.identity);

        s.transform.parent = transform;
        
        var ratio = Random.Range(0.01f, 5.0f);
        var attractor = s.GetComponent<Attractor>();

        attractor.Mass = attractor.Mass * ratio;
	}

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
            Spawn();
    }
}