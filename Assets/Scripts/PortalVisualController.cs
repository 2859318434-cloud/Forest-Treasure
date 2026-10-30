using UnityEngine;

public class PortalVisualController : MonoBehaviour
{
    [SerializeField] private GameManager manager;
    [SerializeField] private MeshRenderer portalRenderer;
    [SerializeField] private Material lockedMaterial;
    [SerializeField] private Material unlockedMaterial;
    [SerializeField] private ParticleSystem unlockParticles;
    [SerializeField] private AudioSource unlockAudioSource;

    private bool lastUnlockedState;

    void Start()
    {
        if (manager == null || portalRenderer == null ||
            lockedMaterial == null || unlockedMaterial == null)
        {
            Debug.LogWarning("PortalVisualController: assign all references in the Inspector.", this);
            enabled = false;
            return;
        }

        ApplyState(manager.IsPortalUnlocked);
    }

    void Update()
    {
        if (manager == null || portalRenderer == null)
            return;

        bool isUnlocked = manager.IsPortalUnlocked;
        if (isUnlocked != lastUnlockedState)
        {
            ApplyState(isUnlocked);

            if (isUnlocked && unlockParticles != null)
                unlockParticles.Play();

            if (isUnlocked)
            {
                if (unlockAudioSource == null)
                    Debug.LogWarning("PortalVisualController: assign PortalEnergy's AudioSource to Unlock Audio Source in the Inspector.", this);
                else if (unlockAudioSource.clip == null)
                    Debug.LogWarning("PortalVisualController: assign PortalUnlockSFX to the unlock AudioSource's AudioClip.", this);
                else
                    unlockAudioSource.PlayOneShot(unlockAudioSource.clip);
            }
        }
    }

    void ApplyState(bool isUnlocked)
    {
        portalRenderer.sharedMaterial = isUnlocked ? unlockedMaterial : lockedMaterial;
        lastUnlockedState = isUnlocked;
    }
}
