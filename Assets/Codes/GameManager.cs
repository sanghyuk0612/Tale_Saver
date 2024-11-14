using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int Score;
    public int stage;
    public int chapter;
    public bool isPlayerInRange;
    private void Awake()
    {
        chapter=1;
        stage =1;
        Score = 0;
        isPlayerInRange=false;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
