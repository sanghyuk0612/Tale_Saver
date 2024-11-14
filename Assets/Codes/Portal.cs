using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Portal : MonoBehaviour
{
    void  OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(GameManager.instance.stage ==4 || GameManager.instance.stage==7){

                GameManager.instance.stage++;
            }
            else if(GameManager.instance.stage == 10 ){
                
                GameManager.instance.stage++;
                
            }
            else{
                LoadSceneByIndex(1);
            }
            
        }
    }
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
