using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject wonPanel;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        if(wonPanel==null)
        {
            wonPanel = GameObject.FindGameObjectWithTag("WonPanel");
            Debug.Log(wonPanel==null);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag=="Ball")
        {
            Debug.Log("Won");
            StartCoroutine(PopupWonPanel());
        }
    }

    IEnumerator PopupWonPanel()
    {
        yield return new WaitForSeconds(0.5f);
        Time.timeScale=0f;
        int level = PlayerPrefs.GetInt("Level")+1;
        PlayerPrefs.SetInt("Level",level);
        wonPanel.SetActive(true);
    }
}
