
using UnityEngine;

public class CrystalPickup : MonoBehaviour
{
    private bool playerNearby = false;
    private GameManager manager;

    public bool IsPlayerNearby => playerNearby;

    void Awake()
    {
        manager = FindAnyObjectByType<GameManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            playerNearby = true;
            manager?.SetCrystalInPickupRange(this, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            playerNearby = false;
            manager?.SetCrystalInPickupRange(this, false);
        }
    }

    public void Collect()
    {
        if (!playerNearby)
            return;

        if (manager != null)
            manager.AddCrystal();

        manager?.SetCrystalInPickupRange(this, false);

        Destroy(gameObject);
    }
}
