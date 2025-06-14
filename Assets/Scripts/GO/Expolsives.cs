
using UnityEngine;

public class Expolsives : Damageable
{
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float force = 2000;
    [SerializeField] private float explosionRadius = 5;
    [SerializeField] private float explosionDamage = 150;
    private bool exploded = false;


    public override void TakeDamage(float damage)
    {
        if (exploded) return; 
        exploded = true;

        Instantiate(explosionEffect, transform.position, transform.rotation);
        Collider[] collidersToDestory = Physics.OverlapSphere(transform.position, explosionRadius);

       foreach(Collider nearbyObject in collidersToDestory) //loop nearby objects to take damage
       {
            Damageable damageable = nearbyObject.GetComponent<Damageable>();
            if (damageable != null && damageable.gameObject != gameObject)
            {
                damageable.TakeDamage(explosionDamage);
            }
       }

        Collider[] collidersToMove = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach(Collider nearbyObject in collidersToMove) //loop again to apply forces for items like boxes that need to take damage first to break
        {

            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(force, transform.position, explosionRadius);
        }

        Destroy(gameObject);
    }
}
