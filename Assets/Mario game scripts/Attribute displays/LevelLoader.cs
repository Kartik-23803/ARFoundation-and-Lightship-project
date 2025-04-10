using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    int currentSceneIndex = 0;

    public void ReloadScene()
    {
        SceneManager.LoadScene(currentSceneIndex);
    }
}