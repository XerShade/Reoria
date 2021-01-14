using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reoria.Framework.Unity
{
    /// <summary>
    /// Changes a GameObject to be static and loaded between scenes.
    /// </summary>
    public class StaticBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Checks to see if the object has already been set to DontDestroyOnLoad.
        /// </summary>
        [SerializeField]
        private bool dontDestroyOnLoad;

        /// <summary>
        /// Called when the object is first created.
        /// </summary>
        void Awake()
        {
            // Check to see if DontDestroyOnLoad is set yet.
            if (!dontDestroyOnLoad)
            {
                // Set the object to DontDestroyOnLoad so we can keep it between scenes.
                DontDestroyOnLoad(gameObject);
                dontDestroyOnLoad = true;

                // Move it back to the _GameEngine scene, this is the only place static object should exist.
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("_GameEngine"));
            }
        }
    }
}
