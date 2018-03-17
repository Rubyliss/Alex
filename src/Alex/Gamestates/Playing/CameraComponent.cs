﻿using System;
using System.Numerics;
using Alex.Blocks;
using Alex.Engine;
using Alex.Entities;
using Alex.Graphics;
using Alex.Rendering.Camera;
using Alex.Utils;
using Alex.Worlds;
using MiNET.Utils;
using Veldrid;
using Veldrid.Sdl2;

namespace Alex.Gamestates.Playing
{
    public class CameraComponent
    {
        public const float Gravity = 0.08f;
        public const float DefaultDrag = 0.02f;
        public const float Acceleration = 0.02f;

        public const float MouseSpeed = 0.25f;

	    private float FlyingSpeed = 10f;

        private MouseState PreviousMouseState { get; set; }
        private float _leftrightRot = MathF.PI;
        private float _updownRot = -MathF.PI / 10.0f;

        public bool IsJumping { get; private set; }
        public bool IsFreeCam { get; set; }
        private Vector3 Velocity { get; set; }
        private Vector3 Drag { get; set; }

        private FirstPersonCamera Camera { get; }
        private GraphicsDevice Graphics { get; }
        private World World { get; }
        private Settings GameSettings { get; }

		public CameraComponent(FirstPersonCamera camera, GraphicsDevice graphics, World world, Settings settings)
        {
            Camera = camera;
            Graphics = graphics;
            World = world;
            GameSettings = settings;

            IsFreeCam = true;

            Velocity = Vector3.Zero;
            Drag = Vector3.Zero;

           // Mouse.SetPosition(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
         //   PreviousMouseState = Mouse.GetState();
        }

		private bool _inActive = true;
        public void Update(GameTime gameTime, bool checkInput)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;


            bool originalJumpValue = IsJumping;
            var moveVector = Vector3.Zero;
            if (checkInput)
            {
                var currentKeyboardState = Alex.Instance.Window.PumpEvents();
                if (currentKeyboardState.IsKeyDown(KeyBinds.Forward))
                    moveVector.Z = 1;

                if (currentKeyboardState.IsKeyDown(KeyBinds.Backward))
                    moveVector.Z = -1;

                if (currentKeyboardState.IsKeyDown(KeyBinds.Left))
                    moveVector.X = 1;

                if (currentKeyboardState.IsKeyDown(KeyBinds.Right))
                    moveVector.X = -1;

                if (IsFreeCam)
                {
                    if (currentKeyboardState.IsKeyDown(KeyBinds.Up))
                        moveVector.Y = 1;

                    if (currentKeyboardState.IsKeyDown(KeyBinds.Down))
                        moveVector.Y = -1;

	                if (currentKeyboardState.IsKeyDown(KeyBinds.IncreaseSpeed))
		                FlyingSpeed += 1;

	                if (currentKeyboardState.IsKeyDown(KeyBinds.DecreaseSpeed))
		                FlyingSpeed -= 1;

	                if (currentKeyboardState.IsKeyDown(KeyBinds.ResetSpeed))
		                FlyingSpeed = 10f;
                }
				else
                {
                    if (currentKeyboardState.IsKeyDown(KeyBinds.Up) && !IsJumping && IsOnGround(Velocity))
                    {
	                    moveVector.Y = 1;
						//ApplyForce(new Vector3(0, 0.42f * 10, 0), dt);
                        IsJumping = true;
                    }
                }
            }

            if (!IsFreeCam)
            {
	            DoPhysics(originalJumpValue, moveVector, dt);
            }
            else if (IsFreeCam)
            {
                if (moveVector != Vector3.Zero) // If we moved
                {
                    moveVector *= FlyingSpeed * dt;

                    Camera.Move(moveVector);
                }
            }

	        if (checkInput)
	        {
		    /*    if (_inActive)
		        {
			        _inActive = false;
					Mouse.SetPosition(Graphics.Viewport.Width / 2, Graphics.Viewport.Height / 2);
			        PreviousMouseState = Mouse.GetState();
		        }
		      //  MouseState currentMouseState = Mouse.GetState();
		        if (currentMouseState != PreviousMouseState)
		        {
			        float xDifference = currentMouseState.X - PreviousMouseState.X;
			        float yDifference = currentMouseState.Y - PreviousMouseState.Y;

			        float mouseModifier = (float) (MouseSpeed * GameSettings.MouseSensitivy);

			        _leftrightRot -= mouseModifier * xDifference * dt;
			        _updownRot -= mouseModifier * yDifference * dt;
			        _updownRot = MathUtils.Clamp(_updownRot, MathUtils.ToRadians(-89.0f),
				        MathUtils.ToRadians(89.0f));

					Camera.Rotation = new Vector3(-_updownRot, MathHelper.WrapAngle(_leftrightRot), 0);
		        }

		        Mouse.SetPosition(Graphics.Viewport.Width / 2, Graphics.Viewport.Height / 2);
				*/
		      //  PreviousMouseState = Mouse.GetState();
	        }
	        else if (!_inActive)
	        {
		        _inActive = true;
	        }
        }

	    private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(CameraComponent));

        private void DoPhysics(bool originalJumpValue, Vector3 direction, float dt)
        {
			//Apply Gravity.
			//Velocity += new Vector3(0, -Gravity * dt, 0);
			//speed = 0.25f
			//mp = 0.7f

	        var oldVelocity = new Vector3(Velocity.X, Velocity.Y, Velocity.Z);

			float currentDrag = GetCurrentDrag() * 2.5f;
	        float speedFactor = (0.25f * 0.7f * 0.7f);

	        bool onGround = false;
	        if (IsOnGround(Velocity))
	        {
		        onGround = true;
		        if (Velocity.Y < 0)
		        {
			        Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
			        IsJumping = false;
		        }
	        }
	        else
	        {
		    //    currentDrag *= 0.05f;
				Velocity -= new Vector3(0, Gravity, 0);
			}

	        if (direction.Y > 0)
	        {
		        direction.Y = 0;
		        Velocity += new Vector3(0, (0.55f / Gravity), 0);
	        }

	        Drag = new Vector3(Velocity.X * currentDrag, 0, Velocity.Z * currentDrag);

	        Velocity += (direction * speedFactor) - Drag;

	        if (Velocity.LengthSquared() < 0.000001f)
	        {
				Velocity = Vector3.Zero;
	        }

	        var groundSpeedSquared = Velocity.X * Velocity.X + Velocity.Z * Velocity.Z;
	        if (groundSpeedSquared > 4.7f)
	        {
		        var groundSpeed = (float)Math.Sqrt(groundSpeedSquared);
		        var correctionScale = 4.7f / groundSpeed;
				Velocity *= new Vector3(correctionScale, 1, correctionScale);
	        }
		  //  Velocity -= Drag;

			var v = (oldVelocity + Velocity) * 0.5f * dt;
			//Matrix.CreateLookAt(v, Vector3.Forward, )
			if (v != Vector3.Zero) //Only if we moved.
			{
                var preview = Camera.PreviewMove(v);

                var headBlock = (Block)World.GetBlock(preview);
                var headBoundingBox = headBlock.GetBoundingBox(preview.Floor());

                var feetBlockPosition = preview.Floor() - new Vector3(0, 1, 0);
                var feetBlock = (Block)World.GetBlock(feetBlockPosition);
                var feetBoundingBox = feetBlock.GetBoundingBox(feetBlockPosition);

				var difference = (preview.Y) - (feetBoundingBox.Min.Y);
				//Log.Debug($"{difference}");
                var playerBoundingBox = GetPlayerBoundingBox(preview);

                if (!headBlock.Solid && !IsColiding(playerBoundingBox, headBoundingBox) &&
                    !feetBlock.Solid && !IsColiding(playerBoundingBox, feetBoundingBox))
                {
                    Camera.Move(v);
                }
                else if (!headBlock.Solid && !IsColiding(playerBoundingBox, headBoundingBox) && feetBlock.Solid &&
                         (difference <= 0.5f))
                {
                    Camera.Move((v) + new Vector3(0, Math.Abs(difference), 0));
                }
                else
                {
					Velocity = Vector3.Zero;
	                Drag = Vector3.Zero;
                }
            }
        }

        private float GetCurrentDrag()
        {
	        return DefaultDrag;
            Vector3 applied = Camera.Position.Floor();
            applied -= new Vector3(0, Player.EyeLevel, 0);

            if (applied.Y > 255) return DefaultDrag;
            if (applied.Y < 0) return DefaultDrag;

            return World.GetBlock(applied.X, applied.Y, applied.Z).Drag;
        }

        private bool IsOnGround(Vector3 velocity)
        {
            var playerPosition = Camera.Position;

            Vector3 applied = Camera.Position.Floor();
            applied -= new Vector3(0, Player.EyeLevel, 0);

            if (applied.Y > 255) return false;
            if (applied.Y < 0) return false;

            var block = (Block)World.GetBlock(applied.X, applied.Y, applied.Z);
            var boundingBox = block.GetBoundingBox(applied);

            if (block.Solid)
            {
                if (IsColidingGravity(GetPlayerBoundingBox(playerPosition), boundingBox))
                {
                    return true;
                }

                if (IsColidingGravity(GetPlayerBoundingBox(playerPosition + velocity), boundingBox))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsColidingGravity(BoundingBox box, BoundingBox blockBox)
        {
            return box.Min.Y >= blockBox.Min.Y;
        }

        private bool IsColiding(BoundingBox box, BoundingBox blockBox)
        {
            var a = new System.Drawing.Rectangle((int)box.Min.X, (int)box.Min.Z, (int)(box.Max.X - box.Min.X), (int)(box.Max.Z - box.Min.Z));
            var b = new System.Drawing.Rectangle((int)blockBox.Min.X, (int)blockBox.Min.Z, (int)(blockBox.Max.X - blockBox.Min.X), (int)(blockBox.Max.Z - blockBox.Min.Z));
            return a.IntersectsWith(b);
        }

        private BoundingBox GetPlayerBoundingBox(Vector3 position)
        {
            return new BoundingBox(position - new Vector3(0.15f, 0, 0.15f), position + new Vector3(0.15f, 1.8f, 0.15f));
        }
    }
}
