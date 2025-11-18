using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage);
}
public class Projectile : MonoBehaviour
{
    [HideInInspector] public bool isPlayerProjectile = false;  // Owner: true = player bắn, false = enemy bắn
    public float lifeTime = 5f;

    private Animator animator;
    private bool hasHit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // PLAYER PHI TIÊU: Hit ENEMY
        if (isPlayerProjectile && other.CompareTag("Enemy"))
        {
            HitTarget(other);
            return;
        }

        // ENEMY PHI TIÊU: Hit PLAYER
        if (!isPlayerProjectile && other.CompareTag("Player"))
        {
            HitTarget(other);
            return;
        }
    }

    void HitTarget(Collider2D target)
    {
        hasHit = true;

        // Gây damage cho target
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(1);  // Damage = 1
        }

        // Dừng + Animation hit
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        StartCoroutine(DestroyAfterAnimation(0.5f));
    }

    IEnumerator DestroyAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}