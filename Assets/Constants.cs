namespace Reoria
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

            /// <summary>
            /// Defines the Z position value that cameras should be positioned at.
            /// </summary>
            public const float CAMERA_Z_POSITION = -10f;
        }
    }
}
