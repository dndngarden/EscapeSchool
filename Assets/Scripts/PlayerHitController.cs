using System.Collections;
using UnityEngine;

public class PlayerHitController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerController playerController;

    [Header("Hit Sprites")]
    [SerializeField] private Sprite[] hitDown;
    [SerializeField] private Sprite[] hitLeft;
    [SerializeField] private Sprite[] hitRight;
    [SerializeField] private Sprite[] hitUp;

    [Header("Hit Settings")]
    [SerializeField] private int repeatCount = 3;
    [SerializeField] private float frameTime = 0.22f;

    [Header("Sorting")]
    [SerializeField] private bool overrideSortingWhileHitting = true;
    [SerializeField] private int hittingOrderInLayer = 50;

    private bool isHitting;
    private int originalOrderInLayer;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (spriteRenderer != null)
        {
            originalOrderInLayer = spriteRenderer.sortingOrder;
        }
    }

    public bool IsHitting()
    {
        return isHitting;
    }

    public IEnumerator PlayHitMotion()
    {
        if (isHitting)
        {
            yield break;
        }

        isHitting = true;

        if (playerController != null)
        {
            playerController.SetActionLocked(true);
        }

        if (spriteRenderer != null && overrideSortingWhileHitting)
        {
            originalOrderInLayer = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = hittingOrderInLayer;
        }

        PlayerController.PlayerDirection direction = PlayerController.PlayerDirection.Down;

        if (playerController != null)
        {
            direction = playerController.LastDirection;
        }

        Sprite[] hitSprites = GetHitSpritesByDirection(direction);

        if (hitSprites == null || hitSprites.Length == 0)
        {
            Debug.LogWarning("연필 타격 스프라이트가 연결되지 않았습니다: " + direction);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            for (int repeat = 0; repeat < repeatCount; repeat++)
            {
                for (int i = 0; i < hitSprites.Length; i++)
                {
                    spriteRenderer.sprite = hitSprites[i];
                    yield return new WaitForSeconds(frameTime);
                }
            }
        }

        if (spriteRenderer != null && overrideSortingWhileHitting)
        {
            spriteRenderer.sortingOrder = originalOrderInLayer;
        }

        if (playerController != null)
        {
            playerController.SetActionLocked(false);
            playerController.ShowIdleSprite();
        }

        isHitting = false;
    }

    private Sprite[] GetHitSpritesByDirection(PlayerController.PlayerDirection direction)
    {
        switch (direction)
        {
            case PlayerController.PlayerDirection.Down:
                return hitDown;

            case PlayerController.PlayerDirection.Left:
                return hitLeft;

            case PlayerController.PlayerDirection.Right:
                return hitRight;

            case PlayerController.PlayerDirection.Up:
                return hitUp;

            default:
                return hitDown;
        }
    }
}
