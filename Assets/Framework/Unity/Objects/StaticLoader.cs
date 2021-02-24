using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reoria.Framework.Unity.Objects
{
    /// <summary>
    /// Static loader script, loads static objects for the game's use.
    /// </summary>
    public class StaticLoader : MonoBehaviour
    {
        /// <summary>
        /// Called when the script is started.
        /// </summary>
        void Start()
        {
            // Check to see if the main camera is loaded.
            if (MainCamera.Instance is null)
                MainCamera.Instantiate();
        }
        
        /// <summary>
        /// Called after the script is updated.
        /// </summary>
        void LateUpdate()
        {
            // Check to see if we are on the static loader scene.
            if(SceneManager.GetActiveScene().name.Equals("_StaticObjects"))
            {
                // We are, default to the main menu.
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
