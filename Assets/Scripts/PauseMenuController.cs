using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;

    private bool isPaused;
    private GameManager manager;

    private void Awake()
    {
        manager = FindAnyObjectByType<GameManager>();
    }

    private void Start()
    {
        if (manager != null && manager.IsGameCompleted)
        {
            HideForGameCompletion();
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (manager != null && manager.IsGameCompleted)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (manager != null && manager.IsGameCompleted)
            return;

        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (manager != null && manager.IsGameCompleted)
            return;

        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void HideForGameCompletion()
    {
        isPaused = false;
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void OnDisable()
    {
        if (manager == null || !manager.IsGameCompleted)
            Time.timeScale = 1f;
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu");
    }
}
