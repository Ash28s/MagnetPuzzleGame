using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag=="Ball")
        {
            Debug.Log("Won");
            gameManager.TriggerWin();
        }
    }
}
