using System.Collections;
using UnityEngine;

public class TreasureChestInteraction : MonoBehaviour
{
    [SerializeField] private Animator chestAnimator;
    public GameObject keyRoot;

    private GameManager manager;
    private bool playerNearby;
    private bool isOpened;

    public bool IsPlayerNearby => playerNearby && !isOpened;

    void Awake()
    {
        manager = FindAnyObjectByType<GameManager>();

        if (chestAnimator == null)
            chestAnimator = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null || isOpened)
            return;

        playerNearby = true;
        manager?.SetChestInInteractionRange(this, true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null)
            return;

        playerNearby = false;
        manager?.SetChestInInteractionRange(this, false);
    }

    public void Open()
    {
        if (!IsPlayerNearby)
            return;

        isOpened = true;
        chestAnimator?.SetTrigger("Open");

        if (keyRoot != null)
            StartCoroutine(ShowKeyAfterChestOpens());

        manager?.SetChestInInteractionRange(this, false);
    }

    IEnumerator ShowKeyAfterChestOpens()
    {
        yield return new WaitForSeconds(1f);

        keyRoot.SetActive(true);

        Animator keyAnimator = keyRoot.GetComponent<Animator>();
        if (keyAnimator != null)
            keyAnimator.Play("KeyRise", 0, 0f);
    }
}
