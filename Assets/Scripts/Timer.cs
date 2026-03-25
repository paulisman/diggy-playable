using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    /// <summary>
    /// Total time in seconds.
    /// </summary>
    [Range(1, 60)]
    public float timeRemaining = 25f;

    /// <summary>
    /// Flag if the timer is running.
    /// </summary>
    public bool timerIsRunning = false;

    [Header("UI elements")]
    /// <summary>
    /// UI element displaying remaining time.
    /// </summary>
    public TextMeshProUGUI timerText;

    /// <summary>
    /// UI element of restart game button.
    /// </summary>
    public Button restartButton;

    /// <summary>
    /// Main cleaning manager script instance.
    /// </summary>
    public CleaningManager cleaningManager;

    private void Start()
    {
        timerIsRunning = true;
        restartButton.gameObject.SetActive(false);
    }

    void Update()
    {
        // Updating the text of timer
        int wholeMinutes = Mathf.FloorToInt(timeRemaining / 60);
        int remainingSeconds = Mathf.FloorToInt(timeRemaining % 60);

        string wholeMinutesText = wholeMinutes.ToString().Length > 1 ? wholeMinutes.ToString() : "0" + wholeMinutes.ToString();
        string remainingSecondsText = remainingSeconds.ToString().Length > 1 ? remainingSeconds.ToString() : "0" + remainingSeconds.ToString();

        timerText.text = wholeMinutesText + ":" + remainingSecondsText;

        if (timeRemaining > 0.1f)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            if (timerIsRunning)
            {
                cleaningManager.EndGame(restartButton);
            }
            timerIsRunning = false;
        }
    }
}