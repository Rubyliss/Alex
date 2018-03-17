using System;
using System.Drawing.Drawing2D;
using System.Numerics;
using Alex.Utils;
using Veldrid.Utilities;


namespace Alex.Rendering.Camera
{
    public class Camera
    {
        public BoundingFrustum BoundingFrustum => new BoundingFrustum(ViewMatrix * ProjectionMatrix);

        /// <summary>
        /// 
        /// </summary>
        public Matrix4x4 ProjectionMatrix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Matrix4x4 ViewMatrix { get; set; }

	    /// <summary>
        /// 
        /// </summary>
        public Vector3 Target { get; private set; }
        private Vector3 _position;
        /// <summary>
        /// Our current position.
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
				UpdateLookAt();
            }
        }

        private Vector3 _rotation;
        /// <summary>
        /// Our current rotation
        /// </summary>
        public Vector3 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
				UpdateLookAt();
            }
        }

	    public float Yaw
	    {
		    get
			{
			 	Vector3 v = Direction;
				return MathUtils.RadianToDegree((float)Math.Atan2(v.X, v.Z));
			}
	    }

	    public float Pitch;

        public Vector3 Direction;
        /// <summary>
        /// Updates the camera's looking vector.
        /// </summary>
        protected void UpdateLookAt()
        {
	        Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationX(Rotation.X) *
                                    Matrix4x4.CreateRotationY(Rotation.Y);

            Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);

            Target = Position + lookAtOffset;

            Direction = Vector3.Transform(Vector3.UnitZ, rotationMatrix);

			ViewMatrix = Matrix4x4.CreateLookAt(Position, Target, Vector3.UnitY);
		}
    }
}