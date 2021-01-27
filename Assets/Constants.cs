namespace Reoria.Framework
{
    /// <summary>
    /// Defines constants used by scripts.
    /// </summary>
    public partial class Constants
    {
        /// <summary>
        /// Defines the number of pixels per unity unit.
        /// </summary>
        public const int PIXELS_PER_UNIT = 32;

        /// <summary>
        /// Defines the range (dead zone) at which lerp values should snap to the target value.
        /// </summary>
        public const float LERP_SNAP = 0.1f;

        /// <summary>
        /// Defines constants used by camera scripts.
        /// </summary>
        public partial class Camera
        {
            /// <summary>
            /// Defines the target camera resolution width.
            /// </summary>
            public const float TARGET_RESOLUTION_WIDTH = 1920f;
            /// <summary>
            /// Defines the target camera resolution height.
            /// </summary>
            public const float TARGET_RESOLUTION_HEIGHT = 1080f;
        }
    }
}
