using System.Collections;
using UnityEngine;

public class GoldenKeyPickup : MonoBehaviour
{
    private GameManager manager;
    private bool playerNearby;
    private bool isReady;
    private bool isCollected;

    public bool IsPlayerNearby =>
        playerNearby && isReady && !isCollected;

    void Awake()
    {
        manager = FindAnyObjectByType<GameManager>();
    }

    void OnEnable()
    {
        playerNearby = false;
        isReady = false;
        StartCoroutine(EnablePickupAfterRise());
    }

    void OnDisable()
    {
        manager?.SetGoldenKeyInPickupRange(this, false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null || isCollected)
            return;

        playerNearby = true;
        RegisterIfReady();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null || isCollected)
            return;

        playerNearby = true;
        RegisterIfReady();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null)
            return;

        playerNearby = false;
        manager?.SetGoldenKeyInPickupRange(this, false);
    }

    IEnumerator EnablePickupAfterRise()
    {
        yield return new WaitForSeconds(1.5f);

        isReady = true;
        RegisterIfReady();
    }

    void RegisterIfReady()
    {
        if (IsPlayerNearby)
            manager?.SetGoldenKeyInPickupRange(this, true);
    }

    public void Collect()
    {
        if (!IsPlayerNearby || manager == null || manager.HasGoldenKey)
            return;

        isCollected = true;
        manager.CollectGoldenKey();
        manager.SetGoldenKeyInPickupRange(this, false);
        gameObject.SetActive(false);
    }
}
