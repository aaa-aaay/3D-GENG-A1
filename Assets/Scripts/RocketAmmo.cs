using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketAmmo : MonoBehaviour
{

    [SerializeField] private float speed = 6000.0f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float force = 1000;
    [SerializeField] private float explosionRadius = 5;

    Rigidbody rb;
    private float damage = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
       // RocketLauncher.Onfire += OnMissleFired;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 9) return; // 9 is wepon layer


        Instantiate(explosionEffect, transform.position, transform.rotation);
        Collider[] collidersToDestory = Physics.OverlapSphere(transform.position, explosionRadius); //for area damage

        foreach (Collider nearbyObject in collidersToDestory)
        {
            Damageable damageable = nearbyObject.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }

        Collider[] collidersToMove = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in collidersToMove)
        {

            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(force, transform.position, explosionRadius);
        }


        Destroy(gameObject);
    }

    public void FireMissle(float damage, Ray ray)
    {
        this.damage = damage;
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = (hit.point - transform.position).normalized;
            rb.AddForce(direction * speed, ForceMode.Impulse);
        }
        else
        {
            Vector3 fallbackDirection = ray.direction.normalized;
            rb.AddForce(fallbackDirection * speed, ForceMode.Impulse);
        }

    }

    //private void OnMissleFired(float damage, float range, Ray ray)
    //{
    //    RocketLauncher.Onfire -= OnMissleFired;
    //    StartCoroutine(ApplyForceAfterInitialization(ray, range));
    //}


    //private IEnumerator ApplyForceAfterInitialization(Ray ray, float range)
    //{
    //    yield return new WaitForFixedUpdate();

    //    Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

    //    if (Physics.Raycast(ray, out RaycastHit hit, range))
    //    {
    //        Vector3 direction = (hit.point - transform.position).normalized;
    //        rb.AddForce(direction * speed, ForceMode.Impulse);
    //    }
    //    else
    //    {
    //        Vector3 fallbackDirection = ray.direction.normalized;
    //        rb.AddForce(fallbackDirection * speed, ForceMode.Impulse);
    //    }
    //}

}
