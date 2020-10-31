using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public float maxHealth = 5;
    float currentHealth;
    public Rigidbody2D rigidbody;

    private void Awake()
    {
        currentHealth = maxHealth;
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public void ApplyDamageAndKnockback(float damage, Vector2 knockback)
    {

        currentHealth -= damage;
        rigidbody.AddRelativeForce(knockback);
        
        if(currentHealth <= 0)
        {

            print(this.name + " died");
            Destroy(GetComponent<Rigidbody2D>());
            Destroy(gameObject);

        }

    }

    public Vector2 getCurrentVelocity() { return rigidbody.velocity; }

}
