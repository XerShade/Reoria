using Reoria.Models;
using System;

namespace Reoria.EventArgs
{
    /// <summary>
    /// Defines event arguments for the <see cref="IModel.OnGuidChanged"/> event.
    /// </summary>
    public class ModelGuidChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// The new <see cref="System.Guid"/> that the <see cref="IModel"/>'s unique <see cref="System.Guid"/> is being changed to.
        /// </summary>
        public Guid Guid;

        /// <summary>
        /// Constructs a new <see cref="ModelGuidChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="guid">The new <see cref="System.Guid"/> that the <see cref="IModel"/>'s unique <see cref="System.Guid"/> is being changed to.</param>
        public ModelGuidChangedEventArgs(Guid guid)
        {
            // Store the Guid that we are changing too.
            Guid = guid;
        }
    }
}
