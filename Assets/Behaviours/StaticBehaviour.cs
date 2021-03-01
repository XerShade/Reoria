using UnityEngine;

namespace Reoria.Behaviours
{
    /// <summary>
    /// Defnies functions and properties for static game objects.
    /// </summary>
    public class StaticBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Called when the script is first created.
        /// </summary>
        void Awake()
        {
            // Set the object to DontDestroyOnLoad so we can keep it between scenes.
            DontDestroyOnLoad(gameObject);
        }
    }
}
