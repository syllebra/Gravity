using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBiggest : MonoBehaviour
{
    float distanceRatio = 10F;

	// Update is called once per frame
	void Update ()
    {
        if (Attractor.biggest == null)
            return;

        transform.LookAt(Attractor.biggest.transform);

        var dir = Attractor.biggest.transform.position - transform.position;
        float biggestRadius = Attractor.biggest.transform.localScale.magnitude;
        transform.position = Attractor.biggest.transform.position - dir.normalized * distanceRatio * biggestRadius;


    }
}
