using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum PlayerDirection
    {
        Down,
        Left,
        Right,
        Up
    }

    [Header("Move")]
    public float moveSpeed = 4f;

    [Header("Animation")]
    public float frameTime = 0.15f;

    public Sprite[] walkDown;
    public Sprite[] walkLeft;
    public Sprite[] walkRight;
    public Sprite[] walkUp;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;

    private float animTimer;
    private int frameIndex;

    private PlayerDirection currentDirection = PlayerDirection.Down;

    public PlayerDirection LastDirection { get; private set; } = PlayerDirection.Down;

    public bool IsActionLocked { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (IsActionLocked)
        {
            moveInput = Vector2.zero;
            return;
        }

        ReadInput();
        UpdateDirection();
        UpdateFlipbookAnimation();
    }

    private void FixedUpdate()
    {
        if (IsActionLocked)
        {
            return;
        }

        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void ReadInput()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            x = -1f;
        }
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            x = 1f;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            y = -1f;
        }
        else if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            y = 1f;
        }

        moveInput = new Vector2(x, y).normalized;
    }

    private void UpdateDirection()
    {
        if (moveInput.sqrMagnitude < 0.01f)
        {
            return;
        }

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            if (moveInput.x < 0)
            {
                currentDirection = PlayerDirection.Left;
            }
            else
            {
                currentDirection = PlayerDirection.Right;
            }
        }
        else
        {
            if (moveInput.y < 0)
            {
                currentDirection = PlayerDirection.Down;
            }
            else
            {
                currentDirection = PlayerDirection.Up;
            }
        }

        LastDirection = currentDirection;
    }

    private void UpdateFlipbookAnimation()
    {
        Sprite[] currentSprites = GetSpritesByDirection(currentDirection);

        if (currentSprites == null || currentSprites.Length == 0)
        {
            return;
        }

        if (moveInput.sqrMagnitude < 0.01f)
        {
            frameIndex = 0;
            animTimer = 0f;
            spriteRenderer.sprite = GetSpritesByDirection(LastDirection)[0];
            return;
        }

        animTimer += Time.deltaTime;

        if (animTimer >= frameTime)
        {
            animTimer = 0f;
            frameIndex++;

            if (frameIndex >= currentSprites.Length)
            {
                frameIndex = 0;
            }

            spriteRenderer.sprite = currentSprites[frameIndex];
        }
    }

    private Sprite[] GetSpritesByDirection(PlayerDirection direction)
    {
        switch (direction)
        {
            case PlayerDirection.Down:
                return walkDown;

            case PlayerDirection.Left:
                return walkLeft;

            case PlayerDirection.Right:
                return walkRight;

            case PlayerDirection.Up:
                return walkUp;

            default:
                return walkDown;
        }
    }

    public void SetActionLocked(bool locked)
    {
        IsActionLocked = locked;

        if (locked)
        {
            moveInput = Vector2.zero;
        }
    }

    public void ShowIdleSprite()
    {
        Sprite[] sprites = GetSpritesByDirection(LastDirection);

        if (sprites != null && sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }
}