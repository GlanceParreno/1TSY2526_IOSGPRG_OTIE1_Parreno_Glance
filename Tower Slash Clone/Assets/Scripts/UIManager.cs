using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;

    private void Start()
    {
        UpdateScore(0);
        UpdateHP(3);
    }

    public void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + newScore;
    }

    public void UpdateHP(int hp)
    {
        if (hpText != null)
            hpText.text = "HP: " + hp;
    }
}
