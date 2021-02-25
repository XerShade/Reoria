using System.Collections.Generic;

namespace Reoria.Extensions.System.Collections.Generic
{
    /// <summary>
    /// Defines extensions for the <see cref="Dictionary{TKey, TValue}"/> class.
    /// </summary>
    public static class DictionaryExtensions
    {
        public static TValue Add<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value, bool overwrite)
        {
            // Check to see if we already have an entry for this key.
            if (!dictionary.ContainsKey(key))
            {
                // Lock the dictionary so other threads can't change it.
                lock(dictionary)
                {
                    // Add the value to the dictionary.
                    dictionary.Add(key, value);

                    // Return the value we just added to the caller.
                    return dictionary[key];
                }
            }
            // Okay we already have one of these, check if they want to overwrite it.
            else if (overwrite)
            {
                // Lock the dictionary so other threads can't change it.
                lock (dictionary)
                {
                    // Remove the existing value from the dictionary.
                    dictionary.Remove(key);

                    // Add the value to the dictionary.
                    dictionary.Add(key, value);

                    // Return the value we just added to the caller.
                    return dictionary[key];
                }
            }

            // No operations were valid, return default to the caller.
            return default;
        }
    }
}
