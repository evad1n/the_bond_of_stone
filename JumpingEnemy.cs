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
    /// A hostile entity that acts just like the ground enemy, but hops (koopa troopa) instead of walking (WIP).
    /// 
    /// By Will Dickinson
    /// </summary>
    class JumpingEnemy : Enemy
    {
        float jumpHeight = 500f;
        int yOffset;

        //Animation?
        SpriteEffects facing = SpriteEffects.None;

        public bool Grounded;

        public new Rectangle Rect
        {
            get
            {
                yOffset = (Game1.TILE_PIXEL_SIZE - Graphics.EnemySlugTextures[0].Height) * 3;

                return new Rectangle(
                    (int)Math.Round(Position.X / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE,
                    (int)Math.Round((Position.Y + Game1.PIXEL_SCALE) / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE,
                    Graphics.EnemyJumperTextures[0].Width * Game1.PIXEL_SCALE,
                    Graphics.EnemyJumperTextures[0].Height * Game1.PIXEL_SCALE
                    );
            }
        }

        public JumpingEnemy(Vector2 position) : base(Graphics.EnemyJumperTextures[0], position)
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
        public override void Update(GameTime gameTime)
        {

            //Check collision directions
            Grounded = CheckCardinalCollision(new Vector2(0, 3));
            
            if (Grounded)
                velocity.Y = -jumpHeight;

            base.Update(gameTime);

            //Apply the physics
            ApplyPhysics(gameTime);

            GetAnimation();
            
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

            //Set the X and Y components of the velocity separately.
            velocity.Y = velocity.Y + Game1.GRAVITY.Y * elapsed;

            if (velocity.X > 0)
            {
                facing = SpriteEffects.FlipHorizontally;
            }
            else
            {
                facing = SpriteEffects.None;
            }

            //Move the player and correct for collisions
            Position += velocity * elapsed;
        }

        void GetAnimation()
        {
            if (Grounded)
            {
                Texture = Graphics.EnemyJumperTextures[1];
            }
            else
                Texture = Graphics.EnemyJumperTextures[0];
        }

        //This is necessary for altering the player's hitbox. This method lops off the bottom pixel from the hitbox.
        public override void Draw(SpriteBatch spriteBatch, Color color, int depth = 0)
        {
            //debug
            //spriteBatch.Draw(Graphics.Tiles_gold[0], gapRect, Color.White);
            //spriteBatch.Draw(Graphics.Tiles_gold[0], wallRect, Color.White);

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
