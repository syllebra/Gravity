using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBiggestECS : MonoBehaviour
{
    float distanceRatio = 10F;
    public Transform toFollow = null;

    // Update is called once per frame
    void Update()
    {
        if (toFollow == null)
            return;

        transform.LookAt(toFollow.position);

        var dir = toFollow.position - transform.position;
        float biggestRadius = toFollow.localScale.magnitude;
        transform.position = toFollow.position - dir.normalized * distanceRatio * biggestRadius;
    }

}
