// WinTrigger.cs

using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public GameObject winTextUI;

    private void Start()
    {
        // Ensure the win text is hidden at the start
        if (winTextUI != null)
        {
            winTextUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the player
        if (other.CompareTag("Player"))
        {
            if (winTextUI != null)
            {
                winTextUI.SetActive(true);
                Debug.Log("Player has reached the end!");
                Time.timeScale = 0;
            }
        }
    }
}