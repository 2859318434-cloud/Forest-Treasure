
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int crystalCount = 0;
    public int targetCrystals = 5;

    public TextMeshProUGUI crystalCountText;
    public GameObject winText;
    public GameObject restartButton;
    public GameObject pickupHintText;
    [SerializeField] private GameObject portalUnlockedText;
    [SerializeField] private TMP_Text questObjectiveText;
    [SerializeField] private AudioSource goldenKeyAudioSource;
    [SerializeField] private AudioClip goldenKeyPickupClip;
    public bool HasGoldenKey { get; private set; }
    public bool IsPortalUnlocked { get; private set; }
    public bool IsGameCompleted => isGameCompleted;

    private AudioSource pickupAudioSource;
    private readonly HashSet<CrystalPickup> nearbyCrystals = new();
    private readonly HashSet<TreasureChestInteraction> nearbyChests = new();
    private readonly HashSet<GoldenKeyPickup> nearbyGoldenKeys = new();
    private TextMeshProUGUI pickupHintLabel;
    private bool isGameCompleted;
    private Coroutine portalUnlockedHintCoroutine;
    private bool hasWarnedMissingQuestObjective;

    void Start()
    {
        UpdateCrystalUI();
        UpdateQuestObjective();

        if (winText != null)
            winText.SetActive(false);

        if (restartButton != null)
            restartButton.SetActive(false);

        if (portalUnlockedText != null)
            portalUnlockedText.SetActive(false);
        else
            Debug.LogWarning("GameManager: assign Canvas/PortalUnlockedText to Portal Unlocked Text in the Inspector.", this);

        if (pickupHintText != null)
            pickupHintLabel = pickupHintText.GetComponent<TextMeshProUGUI>();

        UpdatePickupHint();

        pickupAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isGameCompleted || Time.timeScale <= 0f)
            return;

        UpdatePickupHint();

        if (Input.GetKeyDown(KeyCode.E))
            TryInteractWithNearestTarget();
    }

    public void AddCrystal()
    {
        crystalCount++;
        if (pickupAudioSource != null)
        {
            pickupAudioSource.PlayOneShot(pickupAudioSource.clip);
        }

        Debug.Log("Crystals collected: " + crystalCount);

        UpdateCrystalUI();

        TryUnlockPortal();
        UpdateQuestObjective();
    }

    void UpdateCrystalUI()
    {
        if (crystalCountText != null)
        {
            crystalCountText.text = "Crystals: " + crystalCount;
        }
    }

    void UpdateQuestObjective()
    {
        if (questObjectiveText == null)
        {
            if (!hasWarnedMissingQuestObjective)
            {
                Debug.LogWarning("GameManager: assign Canvas/QuestObjectiveText to Quest Objective Text in the Inspector.", this);
                hasWarnedMissingQuestObjective = true;
            }

            return;
        }

        if (isGameCompleted)
            questObjectiveText.text = "Quest Complete!";
        else if (crystalCount < targetCrystals)
            questObjectiveText.text = $"Collect Magic Crystals ({crystalCount}/{targetCrystals})";
        else if (!HasGoldenKey)
            questObjectiveText.text = "Find the Golden Key";
        else if (IsPortalUnlocked)
            questObjectiveText.text = "Enter the Glowing Portal";
    }

    public void SetCrystalInPickupRange(
        CrystalPickup crystal,
        bool isInRange)
    {
        if (isInRange)
            nearbyCrystals.Add(crystal);
        else
            nearbyCrystals.Remove(crystal);

        UpdatePickupHint();
    }

    public void SetChestInInteractionRange(
        TreasureChestInteraction chest,
        bool isInRange)
    {
        if (isInRange)
            nearbyChests.Add(chest);
        else
            nearbyChests.Remove(chest);

        UpdatePickupHint();
    }

    public void SetGoldenKeyInPickupRange(
        GoldenKeyPickup goldenKey,
        bool isInRange)
    {
        if (isInRange)
            nearbyGoldenKeys.Add(goldenKey);
        else
            nearbyGoldenKeys.Remove(goldenKey);

        UpdatePickupHint();
    }

    public void CollectGoldenKey()
    {
        if (HasGoldenKey)
            return;

        HasGoldenKey = true;
        Debug.Log("Golden key collected.");

        if (goldenKeyAudioSource == null)
            Debug.LogWarning("GameManager: assign GameManager's AudioSource to Golden Key Audio Source in the Inspector.", this);
        else if (goldenKeyPickupClip == null)
            Debug.LogWarning("GameManager: assign GoldenKeyPickupSFX to Golden Key Pickup Clip in the Inspector.", this);
        else
            goldenKeyAudioSource.PlayOneShot(goldenKeyPickupClip);

        TryUnlockPortal();
        UpdateQuestObjective();
    }

    void TryUnlockPortal()
    {
        if (IsPortalUnlocked || crystalCount < targetCrystals || !HasGoldenKey)
            return;

        IsPortalUnlocked = true;
        Debug.Log("Portal Unlocked!");
        UpdateQuestObjective();

        if (portalUnlockedText != null)
            portalUnlockedHintCoroutine = StartCoroutine(ShowPortalUnlockedHint());
    }

    IEnumerator ShowPortalUnlockedHint()
    {
        portalUnlockedText.SetActive(true);
        yield return new WaitForSeconds(3f);

        if (portalUnlockedText != null)
            portalUnlockedText.SetActive(false);

        portalUnlockedHintCoroutine = null;
    }

    void TryInteractWithNearestTarget()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null)
            return;

        TryGetNearestInteraction(
            player.transform.position,
            out CrystalPickup nearestCrystal,
            out TreasureChestInteraction nearestChest,
            out GoldenKeyPickup nearestGoldenKey);

        if (nearestCrystal != null)
            nearestCrystal.Collect();
        else if (nearestChest != null)
            nearestChest?.Open();
        else
            nearestGoldenKey?.Collect();
    }

    bool TryGetNearestInteraction(
        Vector3 playerPosition,
        out CrystalPickup nearestCrystal,
        out TreasureChestInteraction nearestChest,
        out GoldenKeyPickup nearestGoldenKey)
    {
        nearbyCrystals.RemoveWhere(crystal =>
            crystal == null || !crystal.IsPlayerNearby);
        nearbyChests.RemoveWhere(chest =>
            chest == null || !chest.IsPlayerNearby);
        nearbyGoldenKeys.RemoveWhere(goldenKey =>
            goldenKey == null || !goldenKey.IsPlayerNearby);

        nearestCrystal = null;
        nearestChest = null;
        nearestGoldenKey = null;
        float nearestDistanceSqr = float.PositiveInfinity;

        foreach (CrystalPickup crystal in nearbyCrystals)
        {
            float distanceSqr =
                (crystal.transform.position - playerPosition).sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestCrystal = crystal;
            }
        }

        foreach (TreasureChestInteraction chest in nearbyChests)
        {
            float distanceSqr =
                (chest.transform.position - playerPosition)
                .sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestCrystal = null;
                nearestChest = chest;
            }
        }

        foreach (GoldenKeyPickup goldenKey in nearbyGoldenKeys)
        {
            float distanceSqr =
                (goldenKey.transform.position - playerPosition)
                .sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestCrystal = null;
                nearestChest = null;
                nearestGoldenKey = goldenKey;
            }
        }

        return nearestCrystal != null || nearestChest != null ||
            nearestGoldenKey != null;
    }

    void UpdatePickupHint()
    {
        if (isGameCompleted)
        {
            if (pickupHintText != null)
                pickupHintText.SetActive(false);

            return;
        }

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null || !TryGetNearestInteraction(
            player.transform.position,
            out CrystalPickup nearestCrystal,
            out TreasureChestInteraction nearestChest,
            out GoldenKeyPickup nearestGoldenKey))
        {
            if (pickupHintText != null)
                pickupHintText.SetActive(false);

            return;
        }

        if (pickupHintText != null)
            pickupHintText.SetActive(true);

        if (pickupHintLabel != null)
        {
            if (nearestCrystal != null)
                pickupHintLabel.text = "Press E to Collect";
            else if (nearestChest != null)
                pickupHintLabel.text = "Press E to Open";
            else
                pickupHintLabel.text = "Press E to Take Key";
        }
    }

    public void CompleteGame()
    {
        if (!IsPortalUnlocked || isGameCompleted)
            return;

        isGameCompleted = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PauseMenuController pauseMenu = FindAnyObjectByType<PauseMenuController>();
        if (pauseMenu != null)
            pauseMenu.HideForGameCompletion();

        UpdatePickupHint();
        UpdateQuestObjective();

        if (portalUnlockedHintCoroutine != null)
        {
            StopCoroutine(portalUnlockedHintCoroutine);
            portalUnlockedHintCoroutine = null;
        }

        if (portalUnlockedText != null)
            portalUnlockedText.SetActive(false);

        WinGame();
    }

    void WinGame()
    {
        Debug.Log("YOU WIN!");

        if (winText != null)
            winText.SetActive(true);

        if (restartButton != null)
            restartButton.SetActive(true);
    }

    public void RestartGame()
    {
        Debug.Log("RESTART BUTTON CLICKED!");

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
