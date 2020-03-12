using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = TrueCraft.API.Vector3;

namespace TrueCraft.Client.Rendering
{
    /// <summary>
    ///     Represents a camera for use with rendering.
    /// </summary>
    public class Camera
    {
        // _Camera settings
        private float _aspectRatio;
        private float _fov, _nearZ, _farZ;

        // Dependent variables
        private readonly BoundingFrustum _frustum;
        private bool _isDirty; // Whether dependent variables need to be recalculated.
        private float _pitch, _yaw;

        // Position/rotation
        private Vector3 _position;
        private Matrix _view, _projection;

        /// <summary>
        ///     Creates a new camera from the specified values.
        /// </summary>
        /// <param name="aspectRatio"></param>
        /// <param name="fov"></param>
        /// <param name="nearZ"></param>
        /// <param name="farZ"></param>
        public Camera(float aspectRatio, float fov, float nearZ, float farZ)
            : this(aspectRatio, fov, nearZ, farZ, Vector3.Zero, 0.0f, 0.0f)
        {
        }

        /// <summary>
        ///     Creates a new camera from the specified values.
        /// </summary>
        /// <param name="aspectRatio"></param>
        /// <param name="fov"></param>
        /// <param name="nearZ"></param>
        /// <param name="farZ"></param>
        /// <param name="position"></param>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        public Camera(float aspectRatio, float fov, float nearZ, float farZ, Vector3 position, float pitch, float yaw)
        {
            AspectRatio = aspectRatio;
            Fov = fov;
            NearZ = nearZ;
            FarZ = farZ;

            Position = position;
            Pitch = pitch;
            Yaw = yaw;

            _frustum = new BoundingFrustum(Matrix.Identity);
            _view = _projection = Matrix.Identity;
            _isDirty = true;
        }

        /// <summary>
        ///     Gets or sets the aspect ratio for this camera.
        /// </summary>
        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
                _aspectRatio = value;
                _isDirty = true;
            }
        }

        /// <summary>
        ///     Gets or sets the field of view for this camera, in degrees.
        /// </summary>
        public float Fov
        {
            get => _fov;
            set
            {
                _fov = value;
                _isDirty = true;
            }
        }

        /// <summary>
        ///     Gets or sets the near Z clipping plane for this camera.
        /// </summary>
        public float NearZ
        {
            get => _nearZ;
            set
            {
                _nearZ = value;
                _isDirty = true;
            }
        }

        /// <summary>
        ///     Gets or sets the far Z clipping plane for this camera.
        /// </summary>
        public float FarZ
        {
            get => _farZ;
            set
            {
                _farZ = value;
                _isDirty = true;
            }
        }

        /// <summary>
        ///     Gets or sets the position of this camera.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                _isDirty = true;
            }
        }

        /// <summary>
        ///     Gets or sets the pitch for this camera, in degrees.
        /// </summary>
        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                _isDirty = true;
            }
        }

        /// <summary>
        ///     Gets or sets the yaw for this camera, in degrees.
        /// </summary>
        public float Yaw
        {
            get => _yaw;
            set
            {
                _yaw = value;
                _isDirty = true;
            }
        }

        /// <summary>
        ///     Returns the bounding frustum calculated for this camera.
        /// </summary>
        /// <returns></returns>
        public BoundingFrustum Frustum
        {
            get
            {
                if (_isDirty)
                    Recalculate();
                return _frustum;
            }
        }

        /// <summary>
        ///     Applies this camera to the specified effect.
        /// </summary>
        /// <param name="effect">The effect to apply this camera to.</param>
        public void ApplyTo(IEffectMatrices effectMatrices)
        {
            if (_isDirty)
                Recalculate();

            effectMatrices.View = _view;
            effectMatrices.Projection = _projection;
        }

        /// <summary>
        ///     Returns the view matrix calculated for this camera.
        /// </summary>
        /// <returns></returns>
        public Matrix GetViewMatrix()
        {
            if (_isDirty)
                Recalculate();
            return _view;
        }

        /// <summary>
        ///     Gets the projection matrix calculated for this camera.
        /// </summary>
        /// <returns></returns>
        public Matrix GetProjectionMatrix()
        {
            if (_isDirty)
                Recalculate();
            return _projection;
        }

        /// <summary>
        ///     Recalculates the dependent variables for this camera.
        /// </summary>
        private void Recalculate()
        {
            var origin = new Microsoft.Xna.Framework.Vector3(
                (float) _position.X,
                (float) _position.Y,
                (float) _position.Z);

            var direction = Microsoft.Xna.Framework.Vector3.Transform(Microsoft.Xna.Framework.Vector3.UnitZ,
                Matrix.CreateRotationX(MathHelper.ToRadians(_pitch)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(-(_yaw - 180) + 180)));

            _view = Matrix.CreateLookAt(origin, origin + direction, Microsoft.Xna.Framework.Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(_fov), _aspectRatio, _nearZ, _farZ);
            _frustum.Matrix = _view * _projection;
            _isDirty = false;
        }
    }
}