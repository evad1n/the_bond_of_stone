﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace The_Bond_of_Stone {
    /// <summary>
    /// The player class. Allows the player to move throughout the world and interprets collisions and entity interactions.
    /// 
    /// By Dom Liotti and Chip Butler
    /// </summary>
    public class Player : Entity {

        //PHYSICS
        float speedJump = -1500f; //Speed of the player's initial jump
        float acceleration = 13000f; //how fast the player picks up speed from rest
        float maxFallSpeed = 1500f; //max effect of gravity
        float maxSpeed = 1200f; //maximum speed

        float drag = .48f; //speed reduction (need this)

        float goombaForce = -1000;

        //Particle production
        float particleFrequency = 0.065f;
		float particleLifetime = 4.5f;
        float particleTimer;
        List<Particle> particles = new List<Particle>();

        float dynamicParticleFrequency = 0.2f;
        float dynamicParticleTimer;

        //Projectile effects
        List<Bullet> stickies = new List<Bullet>();

        //Animation?
        SpriteEffects facing = SpriteEffects.None;
        SpriteEffects prevFacing = SpriteEffects.None;
        float walkingTimer = 0;
        float walkFrameSpeed = 0.05f;
        int walkFrame = 0;
        int walkFramesTotal = 4;

        //Jumping
        public bool isJumping;
        bool wasJumping;
        float jumpTime; //The length of the jump
        float maxJumpTime = 0.5f; //how long can the player "sustain" a jump?
        float jumpControlPower = 0.14f;

		bool WalljumpGivesXVelocity = false;
        float wallJumpXVelocity = 750;

        float airTime;

        bool wallJumped;
        public bool canStartJump;

        SoundEffectInstance wallSlideSound;
        SoundEffectInstance walkSound;

        public bool Alive;
        public bool Grounded;
        public bool Walled;
        bool walledRight;
        bool walledLeft;

        public Vector2 velocity;
        Vector2 previousVelocity;

        Chunk nextChunk;
        Chunk previousChunk;

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

        //Returns the center of the drawn sprite
        public Vector2 Center
        {
            get
            {
                int x = (int)Math.Round(Position.X / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE;
                int y = (int)Math.Round((Position.Y + Game1.PIXEL_SCALE) / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE;
                return new Vector2(x + (Texture.Width * Game1.PIXEL_SCALE / 2), y + (Texture.Height * Game1.PIXEL_SCALE / 2));
            }
        }

        public Player(Texture2D texture, Vector2 position) : base(texture, position) {
            Texture = texture;
            Position = position;

            particleTimer = 0;

            wallSlideSound = Sound.PlayerWallSlide.CreateInstance();
            walkSound = Sound.PlayerWalk.CreateInstance();
        }

        /// <summary>
        /// Updates player collision and input states.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="keyboardState">Provides a snapshot of inputs.</param>
        /// <param name="prevKeyboardState">Provides a snapshot of the previous frame's inputs.</param>
        public void Update(GameTime gameTime, KeyboardState keyboardState, KeyboardState prevKeyboardState) {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Alive = Game1.PlayerStats.IsAlive;
            previousChunk = Game1.Generator.GetEntityChunkID(new Vector2(Position.X - 3 * Game1.TILE_SIZE, Position.Y));
            nextChunk = Game1.Generator.GetEntityChunkID(new Vector2(Position.X + 3 * Game1.TILE_SIZE, Position.Y));

            //Check collision booleans
            Grounded = CheckCardinalCollision(new Vector2(0, 3), CurrentChunk);

            if(nextChunk != null && nextChunk != CurrentChunk && Rect.X + Rect.Width > nextChunk.Rect.Left)
                walledLeft = CheckCardinalCollision(new Vector2(-6, 0), CurrentChunk) || CheckCardinalCollision(new Vector2(-6, 0), nextChunk);
            else
                walledLeft = CheckCardinalCollision(new Vector2(-6, 0), CurrentChunk);

            if (previousChunk != null && previousChunk != CurrentChunk && Rect.X < previousChunk.Rect.Right)
                walledRight = CheckCardinalCollision(new Vector2(6, 0), CurrentChunk) || CheckCardinalCollision(new Vector2(6, 0), previousChunk);
            else
                walledRight = CheckCardinalCollision(new Vector2(6, 0), CurrentChunk);

            Walled = walledLeft || walledRight;


            //Determine canStartJump states (Yes, this is necessary)
            isJumping = (keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.W) ||
                keyboardState.IsKeyDown(Keys.Up));

            canStartJump = (keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.W) ||
                keyboardState.IsKeyDown(Keys.Up)) &&
                !(prevKeyboardState.IsKeyDown(Keys.Space) ||
                prevKeyboardState.IsKeyDown(Keys.W) ||
                prevKeyboardState.IsKeyDown(Keys.Up)) &&
                !wallJumped;

            if (canStartJump && Grounded) {
                Sound.PlayerJump.Play();
                MakeJumpParticles(new Vector2(0, 1));
            } else if (canStartJump && Walled) {
                Sound.PlayerWallJump.Play();
            }
            

            if (!Alive) {
                    Walled = false;
                    canStartJump = false;
            }

            //Stuff that happens when you hit the ground
            if (!Grounded && !Walled)
                airTime += elapsed;
            else if (Grounded && velocity.Y >= maxFallSpeed / 4)
            {
                Game1.Camera.ScreenShake(airTime * 7.5f, airTime * 1.5f);
                airTime = 0;

                if (velocity.Y > 1000)
                {
                    MakeJumpParticles(new Vector2(0, 2), 10);
                    Sound.PlayerLandHard.Play();
                }
                else
                {
                    MakeJumpParticles(new Vector2(0, 1), 2);
                    Sound.PlayerLandSoft.Play();
                }
            } else if (Grounded && !Walled)
                airTime = 0;
            else
            {
                airTime = 0;
            }

            //At the apex
            if (previousVelocity.Y < 0 && (velocity.Y > 0 || velocity.Y == 0))
            {
                airTime = 0;
            }

            if (Walled && !wallJumped)
                maxFallSpeed = 125;
            else
                maxFallSpeed = 1500;

            //Apply the physics
            ApplyPhysics(gameTime, keyboardState);

            //Clear the jumping state
            isJumping = false;

            //Create particles if necessary, also do some sound stuff
            dynamicParticleTimer += elapsed;
            

            particleTimer += elapsed;
            if (particleTimer >= particleFrequency) {
                bool canSpawnBottom = 
                    CollisionHelper.IsCollidingWithChunk(CurrentChunk, new Rectangle(Rect.X, Rect.Center.Y, 1, Rect.Height/2 + 1)) &&
                    CollisionHelper.IsCollidingWithChunk(CurrentChunk, new Rectangle(Rect.Right, Rect.Center.Y, 1, Rect.Height/2 + 1));
                bool canSpawnLeft = 
                    CollisionHelper.IsCollidingWithChunk(CurrentChunk, new Rectangle(Rect.Center.X - (Rect.Width/2 + 1), Rect.Top, Rect.Width/2 + 1, 1)) &&
                    CollisionHelper.IsCollidingWithChunk(CurrentChunk, new Rectangle(Rect.Center.X - (Rect.Width/2 + 1), Rect.Bottom, Rect.Width/2 + 1, 1));
                bool canSpawnRight = 
                    CollisionHelper.IsCollidingWithChunk(CurrentChunk, new Rectangle(Rect.Center.X, Rect.Top, Rect.Width/2 + 1, 1)) &&
                    CollisionHelper.IsCollidingWithChunk(CurrentChunk, new Rectangle(Rect.Center.X, Rect.Bottom, Rect.Width/2 + 1, 1));

                if ((canSpawnLeft || canSpawnRight) && velocity.Y != 0) {
                    if (wallSlideSound.State == SoundState.Stopped)
                        wallSlideSound.Play();

                    Game1.Camera.ScreenShake(0.5f, 0.25f);

                } else
                    wallSlideSound.Stop();

                if(canSpawnBottom && velocity.X != 0) {
                    if (walkSound.State == SoundState.Stopped)
                        walkSound.Play();
                } else
                    walkSound.Stop();

                if (Grounded && canSpawnBottom && velocity.X != 0)
                {
                    if (!IsTouchingTile(18))
                        particles.Add(new Particle(Graphics.Effect_PlayerParticlesBottom[Game1.RandomObject.Next(0, Graphics.Effect_PlayerParticlesBottom.Length)], new Vector2(Position.X, Position.Y + Game1.PIXEL_SCALE * 7), particleLifetime + (float)Game1.RandomObject.NextDouble() * particleLifetime));

                    if (dynamicParticleTimer >= dynamicParticleFrequency)
                        MakeJumpParticles(new Vector2(0, 1), 2);
                }
                else if (walledLeft && canSpawnLeft && velocity.Y != 0)
                {
                    if (!IsTouchingTile(18))
                        particles.Add(new Particle(Graphics.Effect_PlayerParticlesLeft[Game1.RandomObject.Next(0, Graphics.Effect_PlayerParticlesLeft.Length)], new Vector2(Position.X - Game1.PIXEL_SCALE * 2, Position.Y), particleLifetime + (float)Game1.RandomObject.NextDouble() * particleLifetime));

                    if (dynamicParticleTimer >= dynamicParticleFrequency)
                        MakeJumpParticles(new Vector2(1, 0), 2);
                }
                else if (walledRight && canSpawnRight && velocity.Y != 0)
                {
                    if(!IsTouchingTile(18))
                        particles.Add(new Particle(Graphics.Effect_PlayerParticlesRight[Game1.RandomObject.Next(0, Graphics.Effect_PlayerParticlesRight.Length)], new Vector2(Position.X + Game1.PIXEL_SCALE * 4, Position.Y), particleLifetime + (float)Game1.RandomObject.NextDouble() * particleLifetime));

                    if (dynamicParticleTimer >= dynamicParticleFrequency)
                        MakeJumpParticles(new Vector2(-1, 0), 2);
                }
                particleTimer = 0;
            }
            
            //Update positions of sticky projectiles
            foreach(Bullet b in stickies)
            {
                if(prevFacing != facing)
                {
                    b.Flip();
                }
                b.Position = (Center + b.relativePosition) + new Vector2((b.Position.X - b.Origin.X), 0);
            }


            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update(gameTime);

                if (!particles[i].Active)
                    particles.Remove(particles[i]);
            }

            //Collect coins if necessary
            if (CurrentChunk != null) {
                foreach (Entity e in CurrentChunk.Entities)
                {
                    if (e is CoinPickup)
                    {
                        CoinPickup c = (CoinPickup)e;

                        if (c != null && Rect.Intersects(c.Rect))
                            c.Collect();
                    }
                    else if (e is HealthPickup)
                    {
                        HealthPickup hp = (HealthPickup)e;

                        if (hp != null && Rect.Intersects(hp.Rect))
                            hp.Collect();
                    }
                }
                foreach (Entity e in CurrentChunk.Traps)
                {
                    if (e is SpearTrap)
                    {
                        SpearTrap s = (SpearTrap)e;

                        //If the player is touching this spike...
                        if (s != null && Rect.Intersects(s.Rect))
                        {
                            //Take damage 
                            Game1.PlayerStats.TakeDamage(1, s);
                        }
                    }
                    else if (e is Spike)
                    {
                        Spike s = (Spike)e;

                        //If the player is touching this spike...
                        if (s != null && Rect.Intersects(s.HitRectangle))
                        {
                            //Take damage 
                            Game1.PlayerStats.TakeDamage(1, s);
                        }
                    }
                }
            }

            ResolveDynamicEntityCollisions();
        }

        public void MakeJumpParticles(Vector2 dir, int maxNum = 4)
        {
            int particlesToMake = Game1.RandomObject.Next(1, maxNum);
            Vector2 position, direction;

            if (dir.X < 0)
                position = new Vector2(Rect.Right, Rect.Center.Y);
            else if (dir.X > 0)
                position = new Vector2(Rect.Left, Rect.Center.Y);
            else
                position = new Vector2(Rect.Center.X, Rect.Bottom - 5 * Game1.PIXEL_SCALE);

            for (int i = 0; i < particlesToMake; i++)
            {
                if (dir.Y != 0)
                    direction = new Vector2(25, Game1.RandomObject.Next(0, 50));
                else if (dir.Y == 1)
                    direction = new Vector2(50, Game1.RandomObject.Next(0, 50));
                else
                    direction = new Vector2(50, Game1.RandomObject.Next(0, 75));

                Game1.Entities.particles.Add(
                        new DynamicParticle(
                            Graphics.PlayerJumpParticles[0],
                            Graphics.PlayerJumpParticles,
                            position,
                            0.75f,
                            direction, -50, true));
            }
        }

        public void ResolveDynamicEntityCollisions()
        {
            foreach (Enemy e in Game1.Entities.enemies)
            {
                if (e != null && Rect.Intersects(e.Rect))
                {
                    if (e.Active)
                    {
                        if (Position.Y < e.Position.Y && velocity.Y > 0)
                        {
                            e.Kill();
                            KnockBack(new Vector2(0, goombaForce));
                        }
                        else
                            Game1.PlayerStats.TakeDamage(1, e);
                    }
                }
            }

            foreach(Bullet b in Game1.Entities.projectiles)
            {

                if (b != null && Rect.Intersects(b.Rect))
                {
                    if (b.Active && !b.stuck) 
                    {
                        if(!b.bounce)
                        {
                            //Save rotation and relative position for stuck projectiles
                            b.sticky = true;
                            b.stuck = true;
                            b.relativePosition = (b.Position - new Vector2(b.Rect.X, b.Rect.Y));
                            b.stuckRotation = b.rotation;
                            stickies.Add(b);
                        }

                        Game1.PlayerStats.TakeDamage(1, b);
                    }
                }
            }
        }

        /// <summary>
        /// Applies all the physics (collisions, etc.) for the player.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="keyboardState">Provides a snapshot of inputs.</param>
        public void ApplyPhysics(GameTime gameTime, KeyboardState keyboardState) {
            //Save the elapsed time
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float motion = 0f;

            //Save the previous velocity
            previousVelocity = velocity;

            //Save the previous position
            Vector2 previousPosition = Position;    
            //Save the horizontal motion
            if(Alive)
                motion = GetXMotionFromInput(keyboardState);

            //Set the X and Y components of the velocity separately.
            velocity.X += motion * acceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + Game1.GRAVITY.Y * elapsed, -maxFallSpeed, maxFallSpeed);

            //Apply tertiary forces
            velocity.Y = DoJump(velocity.Y, gameTime);
            velocity.X *= drag;

            //Clamp the velocity
            velocity.X = MathHelper.Clamp(velocity.X, -maxSpeed, maxSpeed);

            //Move the player and correct for collisions
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            if (CurrentChunk != null && Game1.PlayerStats.IsAlive)
            {
                Position = CollisionHelper.DetailedCollisionCorrection(previousPosition, Position, Rect, CurrentChunk);

                if (walledLeft)
                    Position = CollisionHelper.DetailedCollisionCorrection(previousPosition, Position, Rect, previousChunk);

                if (walledRight)
                    Position = CollisionHelper.DetailedCollisionCorrection(previousPosition, Position, Rect, nextChunk);
            }

            //Reset the velocity vector.
            if (Position.X == previousPosition.X)
                velocity.X = 0;
            if (Position.Y == previousPosition.Y) {
                velocity.Y = 0;
                jumpTime = 0.0f;
            }

            GetAnimation(elapsed);

            //set the grounded-walled state
            if (Grounded || !Walled)
                wallJumped = false;
        }

        public bool IsTouchingTile(int ID)
        {
            if (Grounded)
            {
                Rectangle floorRect = Rect;

                floorRect.Offset(new Point(0, Rect.Height + 6));

                if (CollisionHelper.TileIDAtPosition(CurrentChunk, floorRect) == ID)
                    return true;

            }
            else if (Walled)
            {
                Rectangle wallRectLeft = Rect;
                Rectangle wallRectRight = Rect;

                wallRectRight.Offset(new Point(Rect.Width + 6, 0));
                wallRectLeft.Offset(new Point(-(Rect.Width) + 6, 0));

                if (walledLeft && CollisionHelper.TileIDAtPosition(CurrentChunk, wallRectLeft) == ID)
                    return true;
                else if(walledRight && CollisionHelper.TileIDAtPosition(CurrentChunk, wallRectRight) == ID)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Return a float for the velocity of a jumping/falling character.
        /// </summary>
        /// <param name="velocityY">The Y velocity to modify.</param>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <returns></returns>
        public float DoJump(float velocityY, GameTime gameTime) {
            //Do the following is we're jumping
            if (isJumping) {
                //If we're in the middle of a jump...
                if ((!wasJumping && Grounded) || (Walled && !wallJumped) || jumpTime > 0f) {
                    if (jumpTime == 0f && !wallJumped && !Grounded)
                    {
                        if (WalljumpGivesXVelocity)
                        {
                            if (walledLeft)
                                velocity.X = wallJumpXVelocity;
                            else if (walledRight)
                                velocity.X = -wallJumpXVelocity;
                        }
                        wallJumped = true;
                    }

                    //If we're just starting a jump or we're midair... 
                    if (jumpTime != 0f || canStartJump) {
                        jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }

                //If we're in the ascent of the jump...
                if (0f < jumpTime && jumpTime <= maxJumpTime) {
                    velocityY = speedJump * (1.0f - (float)Math.Pow(jumpTime / maxJumpTime, jumpControlPower));

                } else
                    jumpTime = 0f;
            } else
                jumpTime = 0f;

            wasJumping = isJumping;

            return velocityY;
        }

        //Returns -1, 0, or 1 depending on the X input state.
        public float GetXMotionFromInput(KeyboardState keyboardState) {
            float motion = 0f;
            prevFacing = facing;

            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left)) {
                motion += -1f;

                if(!Walled)
                {
                    facing = SpriteEffects.FlipHorizontally;
                }
            }

            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            {
                motion += 1f;

                if (!Walled)
                {
                    facing = SpriteEffects.None;
                }
            }

            return motion;
        }
        
        Vector2 Move(Vector2 motion) {
            return Position + motion;
        }

        /// <summary>
        /// checks whether the player is "next to" a collidable surface.
        /// </summary>
        /// <param name="offset">The direction of the check.</param>
        /// <returns>Boolean is true when the player's rect offset by "offset" is colliding with the level.</returns>
        public bool CheckCardinalCollision(Vector2 offset, Chunk chunk) {
            if (CurrentChunk != null) {
                Rectangle check = Rect;
                check.Offset(offset);
                return CollisionHelper.IsCollidingWithChunk(CurrentChunk, check);
            } else
                return false;
        }

        void GetAnimation(float elapsed) {
            if(!Alive)
                Texture = Graphics.PlayerTextures[6];
            //Jump animation
            else if (!Grounded && !Walled) {
                if (velocity.Y > -450 && velocity.Y < -100)
                    Texture = Graphics.PlayerTextures[2];
                else if (velocity.Y > -100 && velocity.Y < -50)
                    Texture = Graphics.PlayerTextures[3];
                else if (velocity.Y > -50 && velocity.Y < 50)
                    Texture = Graphics.PlayerTextures[4];
                else if (velocity.Y > 50 && velocity.Y < 100)
                    Texture = Graphics.PlayerTextures[5];
                else if (velocity.Y > -100 && velocity.Y < 450)
                    Texture = Graphics.PlayerTextures[6];
            }
            //Walk animation
            else if (Grounded && !Walled && velocity.X != 0) {
                if(walkingTimer < walkFrameSpeed)
                {
                    walkingTimer += elapsed;
                    if(walkingTimer >= walkFrameSpeed)
                    {
                        walkFrame = (walkFrame + 1) % walkFramesTotal;
                        Texture = Graphics.PlayerWalkTextures[walkFrame];
                        walkingTimer = 0f;
                    }
                }
            }
            //Idle
            else if(Grounded && velocity.X == 0) {
                Texture = Graphics.PlayerTextures[0];
            }
            //Walled texture
            else if (!Grounded && Walled) {
                    Texture = Graphics.PlayerTextures[1];
            }
        }

        //This is necessary for altering the player's hitbox. This method lops off the bottom pixel from the hitbox.
        public override void Draw(SpriteBatch spriteBatch, Color color, int depth = 0) {
            if (Active) {
                if (LockToPixelGrid) {
                    Rectangle drawRect = new Rectangle(
                        (int)Math.Round(Position.X / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE,
                        (int)Math.Round((Position.Y + Game1.PIXEL_SCALE) / Game1.PIXEL_SCALE) * Game1.PIXEL_SCALE,
                        Texture.Width * Game1.PIXEL_SCALE,
                        Texture.Height * Game1.PIXEL_SCALE
                        );
        
                    spriteBatch.Draw(Texture, destinationRectangle: drawRect, color: color, effects: facing);
                } else
                    spriteBatch.Draw(Texture, destinationRectangle: Rect, color: color, effects: facing);
            }

            foreach (Particle p in particles)
                p.Draw(spriteBatch, Color.White);
        }

        public void KnockBack(Vector2 boom)
        {
            velocity.X = 2 * boom.X;
            velocity.Y = MathHelper.Clamp(boom.Y, -700, 700);
            Game1.Camera.ScreenShake(4f, 0.3f);
        }
    }
}