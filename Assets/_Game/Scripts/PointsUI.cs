using System;
using UnityEngine;

public class PointsUI : MonoBehaviour
{
    public static PointsUI instance;
    [SerializeField] private TMPro.TextMeshProUGUI pointsText;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateText(int points)
    {
        pointsText.text = points.ToString();
    }
}
