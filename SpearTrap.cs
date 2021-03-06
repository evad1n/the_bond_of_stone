﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Bond_of_Stone
{
    class SpearTrap : Entity
    {
        float rotation;

        bool attack = false;
        bool retract = false;
        bool wait = false;
        float attackTimer;
        float retractTimer;
        float waitTimer;

        Vector2 startPosition;
        Vector2 endPosition;
        Vector2 direction;

        public new Rectangle Rect
        {
            get
            {
                int x = (int)(Position.X + (texture.Width * 0.5f * Game1.PIXEL_SCALE)) - Game1.hitBox.Width / 2;
                int y = (int)(Position.Y);
                Rectangle hitRect = new Rectangle(x, y + 15, Game1.hitBox.Width, Game1.hitBox.Height);
                return hitRect.RotateRect(rotation, Origin);
            }
        }

        public Rectangle drawRect
        {
            get
            {
                Rectangle drawRect = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                Texture.Width * Game1.PIXEL_SCALE,
                Texture.Height * Game1.PIXEL_SCALE
                );

                drawRect.X += drawRect.Width / 2;
                drawRect.Y += drawRect.Height / 2;

                return drawRect;
            }
        }

        public Vector2 Origin
        {
            get
            {
                return new Vector2(Position.X + drawRect.Width / 2, Position.Y + drawRect.Height / 2);
            }
        }

        public SpearTrap(Vector2 position, Vector2 direction) : base (Graphics.Spear, position)
        {
            Position = position;

            //Right
            if (direction.X > 0)
            {
                Position = new Vector2(Position.X, Position.Y - (Game1.TILE_SIZE / 2));
                Position = new Vector2(Position.X + 8, Position.Y);
            }
            //Left
            else if (direction.X < 0)
            {
                Position = new Vector2(Position.X, Position.Y - (Game1.TILE_SIZE / 2));
                Position = new Vector2(Position.X + 8, Position.Y);
            }
            //Down
            else if (direction.Y > 0)
            {
                Position = new Vector2(Position.X + (Game1.TILE_SIZE / 2), Position.Y);
                Position = new Vector2(Position.X, Position.Y - 10);
            }
            //Up
            else if (direction.Y < 0)
            {
                Position = new Vector2(Position.X + (Game1.TILE_SIZE / 2), Position.Y);
                Position = new Vector2(Position.X, Position.Y - 10);
            }

            this.direction = direction;
            startPosition = Position;
            endPosition = startPosition + (direction * texture.Height * 2f);

            //Calculate spear rotation
            rotation = (float)Math.Atan2(direction.Y, direction.X);
            rotation += MathHelper.ToRadians(90);
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            bool ready = !attack && !retract && !wait;

            if (attack)
            {
                attackTimer += elapsed;

                if (attackTimer > 0.2f)
                    attackTimer = 0.2f;

                Position = Position.TimedMove(endPosition, attackTimer / 0.2f);
            }
            else if (retract)
            {
                retractTimer += elapsed;

                if (retractTimer > 2f)
                    retractTimer = 2f;

                Position = Position.TimedMove(startPosition, retractTimer / 2f);
            }
            else if (wait)
            {
                waitTimer += elapsed;
            }

            if (attackTimer == 0.2f)
            {
                attack = false;
                wait = true;
                attackTimer = 0;
                waitTimer = 0;
            }

            if(waitTimer > 0.5f)
            {
                wait = false;
                waitTimer = 0;
                retract = true;
                retractTimer = 0;
            }

            if(retractTimer == 2f)
            {
                retract = false;
                retractTimer = 0;
                attackTimer = 0;
            }

            //Shoot timer
            if(Vector2.DistanceSquared(Game1.PlayerStats.Player.Position, Position) < 5000 && ready)
            {
                attack = true;
                NotifyNearby();
                attackTimer = 0;
            }
           
        }

        public void DrawSpear(SpriteBatch spriteBatch, Color color, int depth = 0)
        {
            if (Active)
            {
                spriteBatch.Draw(texture: Texture, destinationRectangle: drawRect, color: color, origin: new Vector2(texture.Width / 2, texture.Height / 2), rotation: rotation);
            }

            //Debug view
            //spriteBatch.Draw(Graphics.DebugTexture, destinationRectangle: Rect, color: Color.Red);
            //spriteBatch.Draw(Graphics.BlackTexture, position: Origin, color: Color.Black);
            //spriteBatch.Draw(Graphics.Tiles_gold[0], position: Position, color: Color.Blue);
            //int x = (int)(Position.X + (texture.Width * 0.5f * Game1.PIXEL_SCALE)) - Game1.hitBox.Width / 2;
            //int y = (int)(Position.Y);
            //spriteBatch.Draw(Graphics.Tiles_gold[0], position: new Vector2(x, y), color: Color.White);
        }

        public void NotifyNearby()
        {
            foreach(Entity e in Game1.Player.CurrentChunk.Traps)
            {
                if(e is SpearTrap && Vector2.DistanceSquared(Position, e.Position) < 5000)
                {
                    ((SpearTrap)e).attack = true;
                    if(!((SpearTrap)e).attack)
                        ((SpearTrap)e).NotifyNearby();
                }
            }
        }
    }
}
