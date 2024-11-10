using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ContactDamage : MonoBehaviour
{
    public int damageDealt;

    public LayerMask layerMask;

    public SoundEffectSO contactSound;

    private bool isColliding = false;

    private SpriteRenderer playerSprite;

    private void Awake()
    {
        playerSprite = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Collision detected");
        if (isColliding) return;

        if (layerMask == (layerMask | (1 << collision.gameObject.layer)))
        {
            isColliding = true;
            ApplyDamageToPlayer();
            SoundEffectManager.Instance.PlaySoundEffect(contactSound);
            StartCoroutine(ResetCollision());
        }
    }

    /// <summary>
    /// Resets the isColliding flag after a short delay to prevent multiple hits.
    /// </summary>
    private IEnumerator ResetCollision()
    {
        yield return new WaitForSeconds(1.0f); // Adjust the delay as needed
        isColliding = false;
    }


    private void ApplyDamageToPlayer()
    {
        GameManager.Instance.GetPlayer().health.TakeDamage(damageDealt);
        StartCoroutine(TakeDamageEffect());
    }

    private IEnumerator TakeDamageEffect()
    {
        Color defaultColor = playerSprite.color;
        playerSprite.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        playerSprite.color = defaultColor;
    }
}
