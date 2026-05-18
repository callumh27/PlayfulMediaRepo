using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public void PressPlay()
    {
        SceneManager.LoadScene("MainScene");
    }
    
}
