﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Bond_of_Stone
{
    /// <summary>
    /// A hostile entity that walks along a flat surface, and switches direction when it hits a wall or gap. (WIP)
    /// 
    /// By Will Dickinson
    /// </summary>
    class GroundEnemy : Enemy
    {
        float speed = 50f;
        int direction = 1;

        Chunk nextChunk;
        Rectangle gapRect;
        Rectangle wallRect;
        int yOffset = (Game1.TILE_PIXEL_SIZE - Graphics.EnemySlugTextures[0].Height) * Game1.PIXEL_SCALE;

        //Animation?
        SpriteEffects facing = SpriteEffects.None;
        float walkingTimer = 0;
        float walkFrameSpeed = 0.075f;
        int walkFrame = 0;
        int walkFramesTotal = 5;

        public bool Grounded;

        public new Rectangle Rect
        {
            get
            {

                return new Rectangle(
                    (int)Math.Round(Position.X / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE,
                    (int)Math.Round(Position.Y / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE,
                    Graphics.EnemySlugTextures[0].Width * Game1.PIXEL_SCALE,
                    Graphics.EnemySlugTextures[0].Height * Game1.PIXEL_SCALE
                    );
            }
        }

        public GroundEnemy(Vector2 position) : base(Graphics.EnemySlugTextures[0], position)
        {
            Texture = texture;
            Position = position;
            Position = new Vector2(Position.X, Position.Y + 5);
        }

        /// <summary>
        /// Updates player collision and input states.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //Update pathfinding colliders
            gapRect = new Rectangle(Rect.X + (Game1.TILE_SIZE * direction), Rect.Y - (yOffset) + Game1.TILE_SIZE, Game1.TILE_SIZE, Game1.TILE_SIZE);
            wallRect = new Rectangle(Rect.X + (Game1.TILE_SIZE * direction / 5), Rect.Y - yOffset, Game1.TILE_SIZE, Game1.TILE_SIZE);
            nextChunk = Game1.Generator.GetEntityChunkID(gapRect);

            //Check for pathfinding (gaps and walls)
            if ((!CollisionHelper.IsCollidingWithChunk(nextChunk, gapRect) || CollisionHelper.IsCollidingWithChunk(nextChunk, wallRect)))
            {
                direction *= -1;
            }

            velocity.X = speed * direction;

            base.Update(gameTime);

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

            //Save the previous position
            Vector2 previousPosition = Position;

            if(velocity.X > 0)
            {
                facing = SpriteEffects.FlipHorizontally;
            }
            else
            {
                facing = SpriteEffects.None;
            }

            //Move the player and correct for collisions
            Position += velocity * elapsed;


            GetAnimation(elapsed);
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
                    Texture = Graphics.EnemySlugTextures[walkFrame];
                    walkingTimer = 0f;
                }
            }
        }

        //This is necessary for altering the player's hitbox. This method lops off the bottom pixel from the hitbox.
        public override void Draw(SpriteBatch spriteBatch, Color color, int depth = 0)
        {
            //debug
            //spriteBatch.Draw(Graphics.Tiles_gold[0], gapRect, Color.Red);
            //spriteBatch.Draw(Graphics.Tiles_gold[0], wallRect, Color.DarkRed);

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

                    spriteBatch.Draw(Texture, destinationRectangle: drawRect, color: color, effects: facing);
                }
                else
                    spriteBatch.Draw(Texture, destinationRectangle: Rect, color: color, effects: facing);
            }
        }
    }
}
