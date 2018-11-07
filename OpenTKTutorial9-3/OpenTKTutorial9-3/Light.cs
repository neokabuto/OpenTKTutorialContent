using OpenTK;

namespace OpenTKTutorial9
{
    /// <summary>
    /// A light in the scene
    /// </summary>
    class Light
    {
        /// <summary>
        /// Create a new light with a given position, color, and intensities
        /// </summary>
        /// <param name="position">Light's position in world space</param>
        /// <param name="color">Color of the light</param>
        /// <param name="diffuseintensity">Intensity of diffuse effects from this light</param>
        /// <param name="ambientintensity">Intensity of ambient lighting from this light</param>
        public Light(Vector3 position, Vector3 color, float diffuseintensity = 1.0f, float ambientintensity = 1.0f)
        {
            Position = position;
            Color = color;

            DiffuseIntensity = diffuseintensity;
            AmbientIntensity = ambientintensity;

            Type = LightType.Point;
            Direction = new Vector3(0, 0, 1);
            ConeAngle = 15.0f;
        }

        /// <summary>
        /// Position of this light, in world space
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Color of this light
        /// </summary>
        public Vector3 Color;

        /// <summary>
        /// Diffuse intensity of this light
        /// </summary>
        public float DiffuseIntensity;

        /// <summary>
        /// Ambient intensity of this light
        /// </summary>
        public float AmbientIntensity;

        /// <summary>
        /// The type of light, i.e. point lights, spotlights and directional lights
        /// </summary>
        public LightType Type;

        /// <summary>
        /// For spot and directional lights, the direction the light shines in
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// For spot lights, the angle the light shines in
        /// </summary>
        public float ConeAngle;

        /// <summary>
        /// Linear attenuation factor, making the light's brightness reduce at an even rate as it travels away from the light
        /// </summary>
        public float LinearAttenuation;

        /// <summary>
        /// Quadratic attenuation factor, making the light's brightness reduce at an increasing rate as it travels away from the light
        /// </summary>
        public float QuadraticAttenuation;
    }

    /// <summary>
    /// Possible types of light
    /// </summary>
    enum LightType { Point, Spot, Directional }
}
