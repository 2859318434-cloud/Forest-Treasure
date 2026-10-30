using UnityEngine;

public class ForestPortalTrigger : MonoBehaviour
{
    [SerializeField] private GameManager manager;

    void Awake()
    {
        if (manager == null)
            manager = FindAnyObjectByType<GameManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        if (manager != null && manager.IsPortalUnlocked)
            manager.CompleteGame();
    }
}
