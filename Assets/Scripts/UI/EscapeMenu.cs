using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeMenu : MonoBehaviour
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start() { }

  // Update is called once per frame
  void Update()
  {
    if (InputManager.instance.GetExitPressed())
    {
      // save the game anytime before loading a new scene
      DataPersistenceManager.instance.SaveGame();
      // load the main menu scene
      SceneManager.LoadSceneAsync("MainMenu");
    }
  }
}
