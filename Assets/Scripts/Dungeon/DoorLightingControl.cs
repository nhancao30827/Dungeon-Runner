using System.Collections;
using UnityEngine;


[DisallowMultipleComponent]
public class DoorLightingControl : MonoBehaviour
{
    private bool isLit = false;
    private Door door;

    private void Awake()
    {
        door = GetComponentInParent<Door>();
    }

    /// <summary>
    /// Fades in the door by gradually increasing the alpha value of the door's material.
    /// </summary>
    /// <param name="door">The door to fade in.</param>
    public void FadeInDoor(Door door)
    {
        Material material = new Material(GameResources.Instance.variableLitShader);

        if (!isLit)
        {
            SpriteRenderer[] spriteRendererArray = GetComponentsInParent<SpriteRenderer>();

            foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
            {
                StartCoroutine(FadeInDoorRoutine(spriteRenderer, material));
            }
        }

        isLit = true;
    }

    /// <summary>
    /// Coroutine to gradually increase the alpha value of the material to create a fade-in effect.
    /// </summary>
    /// <param name="spriteRenderer">The sprite renderer to apply the material to.</param>
    /// <param name="material">The material to fade in.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator FadeInDoorRoutine(SpriteRenderer spriteRenderer, Material material)
    {
        spriteRenderer.material = material;

        for (float i = 0.05f; i <= 1f; i += Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        spriteRenderer.material = GameResources.Instance.litMaterial;
    }

    /// <summary>
    /// Trigger event to start fading in the door when a collider enters the trigger.
    /// </summary>
    /// <param name="collision">The collider that entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        FadeInDoor(door);
    }
}
