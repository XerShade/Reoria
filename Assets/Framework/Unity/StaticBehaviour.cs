using System.Collections.Generic;
using UnityEngine;

namespace Reoria.Framework.Unity
{
    /// <summary>
    /// Changes a GameObject to be static and loaded between scenes.
    /// </summary>
    public class StaticBehaviour : MonoBehaviour
    {
        /// <summary>
        /// A collection of names of all game objects currently loaded using this script.
        /// </summary>
        private static readonly List<string> staticObjects = new List<string>();

        /// <summary>
        /// Called when the object is first created.
        /// </summary>
        void Awake()
        {
            // Check to see if the object has already been loaded.
            if (!staticObjects.Contains(gameObject.name))
            {
                // Add the object to the collection.
                staticObjects.Add(gameObject.name);

                // Set the object to DontDestroyOnLoad so we can keep it between scenes.
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Woah there, this object is already loaded. This violates the whole static concept.
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Check to see if the object has been loaded.
            if (!staticObjects.Contains(gameObject.name))
            {
                // Remove the object from the collection.
                staticObjects.Remove(gameObject.name);
            }
        }
    }
}
