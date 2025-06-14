
using UnityEngine;

public class Breakables : Damageable
{

    [SerializeField] GameObject destoryedVersion;
    private bool broken = false;
    public override void TakeDamage(float damage)
    {
        if(broken) return;
        Instantiate(destoryedVersion, transform.position, transform.rotation);
        broken = true;
        Destroy(gameObject);
    }
}
