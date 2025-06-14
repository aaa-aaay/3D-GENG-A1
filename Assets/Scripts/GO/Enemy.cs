using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : Damageable
{

    //For Movements
    [SerializeField] float movementSpeed;
    [SerializeField] float moveDistance;
    private Vector3 startPosition;
    private int startDirection;
    float randomSpeed;
    float timer = 3;

    //For damage
    [SerializeField] private float health;
    [SerializeField] private float damageDuration;
    private Color damagecolor = Color.red;
    private Color originalColor;
    private Renderer renderer;

    private void Start()
    {
        startPosition = transform.position;
        renderer = GetComponent<Renderer>();
        originalColor = renderer.material.color;

        randomSpeed = Random.Range(-movementSpeed, movementSpeed);

    }
    public void Update()
    {
        float offsetX = Mathf.Sin(Time.time * movementSpeed) * randomSpeed;
        transform.position = new Vector3(startPosition.x + offsetX, startPosition.y, startPosition.z);
    }

    public override void TakeDamage(float damage)
    {
        health -= damage;

        if (renderer != null)
        {
            StartCoroutine(DamageAnimation());
        }
        if (health <= 0)
        {
            Destroy();
        }
    }

    IEnumerator DamageAnimation()
    {
        renderer.material.color = damagecolor;
        float elapsedTime = 0.0f;
        while (elapsedTime <= damageDuration)
        {
            renderer.material.color = Color.Lerp(damagecolor, originalColor, elapsedTime / damageDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        renderer.material.color = originalColor;
        yield return null;
    }

    private void Destroy()
    {


        Destroy(gameObject);
    }
}
