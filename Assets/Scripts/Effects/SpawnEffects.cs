using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnEffects : MonoBehaviour
{
    [SerializeField] GameObject HitEffect;

    public void SpawnHitEffect(RaycastHit hit)
    {
        //Instantiate(HitEffect, positon, gameObject.transform.rotation);
        Instantiate(HitEffect, hit.point, Quaternion.LookRotation(hit.normal));

    }
}
