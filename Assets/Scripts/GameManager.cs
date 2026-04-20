using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Attach to the GameManager empty GameObject.
// Singleton — any script can call GameManager.Instance
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public Text scoreText;
    public GameObject gameOverPanel;

    [Header("References")]
    public Transform player;

    public bool IsPlaying { get; private set; } = true;

    public float currentSpeed; // Updated by PlayerController

    private float score = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        { Destroy(gameObject); return; }
        Instance = this;
        
        // Find player if not assigned
        if (player == null)
            player = FindObjectOfType<PlayerController>()?.transform;
    }

    void Start()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (!IsPlaying) return;
        score += currentSpeed * Time.deltaTime;
        if (scoreText) scoreText.text = "Score: " + Mathf.FloorToInt(score);
    }

    public void TriggerGameOver()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("Game Over! Score: " + Mathf.FloorToInt(score));
    }

    // Wire this to a Restart button in your UI
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}