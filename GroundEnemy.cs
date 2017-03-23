﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Bond_of_Stone
{
    class GroundEnemy : Entity
    {
        float speed = 1f;
        int direction;

        Chunk nextChunk;
        Rectangle gapRect;
        Rectangle wallRect;

        Vector2 previousPosition;

        //PHYSICS
        float acceleration = 13000f; //how fast the player picks up speed from rest
        float maxFallSpeed = 450f; //max effect of gravity
        float maxSpeed = 1200f; //maximum speed

        float drag = .48f; //speed reduction (need this)

        //Animation?
        SpriteEffects facing = SpriteEffects.None;
        float walkingTimer = 0;
        float walkFrameSpeed = 0.05f;
        int walkFrame = 0;
        int walkFramesTotal = 4;

        public bool Grounded;

        public Vector2 velocity;

        public new Rectangle Rect
        {
            get
            {
                return new Rectangle(
                    (int)Position.X,
                    (int)Position.Y,
                    Graphics.PlayerTextures[0].Width * Game1.PIXEL_SCALE,
                    Graphics.PlayerTextures[0].Height * Game1.PIXEL_SCALE
                    );
            }
        }

        public GroundEnemy(Texture2D texture, Vector2 position) : base(texture, position)
        {
            Texture = texture;
            Position = position;
        }

        /// <summary>
        /// Updates player collision and input states.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="keyboardState">Provides a snapshot of inputs.</param>
        /// <param name="prevKeyboardState">Provides a snapshot of the previous frame's inputs.</param>
        public void Update(GameTime gameTime)
        {
            previousPosition = Position;

            //Update pathfinding colliders
            gapRect = new Rectangle(Rect.X + Game1.TILE_SIZE, Rect.Y + Game1.TILE_SIZE, Game1.TILE_SIZE, Game1.TILE_SIZE);
            wallRect = new Rectangle(Rect.X + Game1.TILE_SIZE, Rect.Y, Game1.TILE_SIZE, Game1.TILE_SIZE);
            nextChunk = Game1.Generator.GetEntityChunkID(gapRect);

            //Check for pathfinding (gaps and walls)
            if (CollisionHelper.IsCollidingWithChunk(nextChunk, gapRect) && !CollisionHelper.IsCollidingWithChunk(nextChunk, wallRect))
            {
                direction *= -1;
            }

            //Move it
            Position = Move(new Vector2(speed * direction, 0));

            if (CurrentChunk != null)
                Position = CollisionHelper.DetailedCollisionCorrection(previousPosition, Position, Rect, CurrentChunk);

            //Check collision directions
            Grounded = CheckCardinalCollision(new Vector2(0, 3));


            //Apply the physics
            ApplyPhysics(gameTime);

            
        }

        /// <summary>
        /// Applies all the physics (collisions, etc.) for the player.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="keyboardState">Provides a snapshot of inputs.</param>
        public void ApplyPhysics(GameTime gameTime)
        {
            //Save the elapsed time
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float motion = 0f;

            //Save the previous position
            Vector2 previousPosition = Position;

            //Set the X and Y components of the velocity separately.
            velocity.X += motion * acceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + Game1.GRAVITY.Y * elapsed, -maxFallSpeed, maxFallSpeed);

            //Apply tertiary forces
            velocity.X *= drag;

            //Clamp the velocity
            velocity.X = MathHelper.Clamp(velocity.X, -maxSpeed, maxSpeed);

            //Move the player and correct for collisions
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            if (CurrentChunk != null && Game1.PlayerStats.IsAlive)
                Position = CollisionHelper.DetailedCollisionCorrection(previousPosition, Position, Rect, CurrentChunk);

            //Reset the velocity vector.
            if (Position.X == previousPosition.X)
                velocity.X = 0;


            GetAnimation(elapsed);
        }

        Vector2 Move(Vector2 motion)
        {
            return Position + motion;
        }

        /// <summary>
        /// checks whether the player is "next to" a collidable surface.
        /// </summary>
        /// <param name="offset">The direction of the check.</param>
        /// <returns>Boolean is true when the player's rect offset by "offset" is colliding with the level.</returns>
        public bool CheckCardinalCollision(Vector2 offset)
        {
            if (CurrentChunk != null)
            {
                Rectangle check = Rect;
                check.Offset(offset);
                return CollisionHelper.IsCollidingWithChunk(CurrentChunk, check);
            }
            else
                return false;
        }

        void GetAnimation(float elapsed)
        {
            //Walk animation
            if (walkingTimer < walkFrameSpeed)
            {
                walkingTimer += elapsed;
                if (walkingTimer >= walkFrameSpeed)
                {
                    walkFrame = (walkFrame + 1) % walkFramesTotal;
                    Texture = Graphics.PlayerWalkTextures[walkFrame];
                    walkingTimer = 0f;
                }
            }
            
            //Idle
            else if (Grounded && velocity.X == 0)
            {
                Texture = Graphics.PlayerTextures[0];
            }
        }

        //This is necessary for altering the player's hitbox. This method lops off the bottom pixel from the hitbox.
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                if (LockToPixelGrid)
                {
                    Rectangle drawRect = new Rectangle(
                        (int)Math.Round(Position.X / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE,
                        (int)Math.Round((Position.Y + Game1.PIXEL_SCALE) / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE,
                        Texture.Width * Game1.PIXEL_SCALE,
                        Texture.Height * Game1.PIXEL_SCALE
                        );

                    spriteBatch.Draw(Texture, destinationRectangle: drawRect, color: Color.White, effects: facing);
                }
                else
                    spriteBatch.Draw(Texture, destinationRectangle: Rect, color: Color.White, effects: facing);
            }

        }

        public void KnockBack(Vector2 boom)
        {
            velocity.X = boom.X;
            velocity.Y = boom.Y;
            Game1.Camera.ScreenShake(4f, 0.3f);
        }

        //This will do something eventually
        public void Kill()
        {
            //Death animation

            Active = false;
        }
    }
}
