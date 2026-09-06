using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeskInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRange = 0.8f;

    [Header("Hit Motion")]
    [SerializeField] private PlayerHitController hitController;

    private bool isInteracting;

    private void Awake()
    {
        if (hitController == null)
        {
            hitController = GetComponent<PlayerHitController>();
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    public void TryInteract()
    {
        // 문제 UI 또는 규칙 UI가 떠 있는 동안에는 상호작용 금지
        if (QuizManager.Instance != null && QuizManager.Instance.IsUIOpen)
        {
            return;
        }

        // 이미 상호작용 중이면 중복 실행 방지
        if (isInteracting)
        {
            return;
        }

        IInteractable nearestInteractable = FindNearestInteractable();

        if (nearestInteractable == null)
        {
            return;
        }

        StartCoroutine(InteractRoutine(nearestInteractable));
    }

    private IEnumerator InteractRoutine(IInteractable target)
    {
        isInteracting = true;

        // 대상이 요구할 때만 연필로 치는 모션을 먼저 실행 (책상: O, 문: X)
        if (target.PlaysHitMotion && hitController != null)
        {
            yield return StartCoroutine(hitController.PlayHitMotion());
        }

        // 모션이 끝난 뒤 대상별 상호작용 실행
        target.Interact();

        isInteracting = false;
    }

    private IInteractable FindNearestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

        IInteractable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable == null)
            {
                continue;
            }

            Component interactableComponent = interactable as Component;

            if (interactableComponent == null)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, interactableComponent.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = interactable;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
