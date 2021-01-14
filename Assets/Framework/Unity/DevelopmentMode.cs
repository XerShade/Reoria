using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Development mode script, performs actions that only need to be done if Unity is running from the editor.
/// </summary>
public class DevelopmentMode : MonoBehaviour
{
    /// <summary>
    /// Called when the script instance is created.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Awake()
    {
        // Let the developer know we're checkign for the unity editor.
        Debug.Log("Checking to see if the game has been started from the Unity Editor.");

        // Check to see fi we are in the editor, we do not want this running on game normally.
        if (Application.isEditor)
        {
            // Let the developer know we're starting the script.
            Debug.Log("The game has been started from the Unity Editor, starting developer mode scripts.");

            // Create a dictionary to check for global game object status.
            Dictionary<string, bool> verificationChecks = new Dictionary<string, bool>
            {
                // { "Game", GameObject.Find("Game") != null }
            };

            // Output the dictionart results to the console so we know if something is not loading.
            foreach (var v in verificationChecks)
                if (v.Value)
                    Debug.Log($"Checking for instance of object {v.Key}: {v.Value}", GameObject.Find(v.Key));
                else
                    Debug.LogWarning($"Checking for instance of object {v.Key}: {v.Value}", GameObject.Find(v.Key));

            // If a required object is not loaded it usually means the _GameEngine preload scene was not loaded, load it.
            if ((from v in verificationChecks where v.Value.Equals(false) select v.Value).FirstOrDefault())
                SceneManager.LoadScene("_GameEngine");
        }
    }
}