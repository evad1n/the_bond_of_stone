﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Bond_of_Stone {
    /// <summary>
    /// Holds meta-information about the player not directly related to its motor functions.
    /// 
    /// By Noah Bock and Dom Liotti
    /// </summary>
    public class PlayerStats {
        public Player Player;

        public int Health;
        public int MaxHealth;
        //How long invulnerability lasts after getting hit
        public float graceTime = 1f;
        public float invulnerabilityTimer;
        public bool invulnerable;

        bool invulnIsFlashed = false;
        float invulnFlashRate = 0.075f;
        float flash;
        public Color invulnColor = Color.White;

        public int Score = 0;
		public int ScoreMultiTicks = 0;
		public int ScoreMultiplier = 1;

		float distance;

        float healthRegenTimer = 0.5f;
        float regenTime;

        public float Distance {
            get { return distance / Game1.TILE_SIZE; }
        }
        float lastDistance;

        public float Time = 0f;

        bool isAlive = true;
        public bool IsAlive { get { return isAlive; } }

        public PlayerStats(Player player, int maxHealth) {
            Player = player;

            //set the max health (it has to be a multiple of 2 for drawing).
            if (maxHealth % 2 == 1)
                MaxHealth = maxHealth + 1;
            else
                MaxHealth = maxHealth;

            Health = MaxHealth;
        }

        public void Update(GameTime gameTime) {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!invulnIsFlashed)
                invulnColor = Color.White;
            else
                invulnColor = new Color(1, 1, 1, 0f);

            

            //Calculate scoring, time, and distance
            if (Player.Rect.Right > distance) {
				Score += (int)((Math.Round((distance - lastDistance) * elapsed, 1)) * ScoreMultiplier * 100);
				
                lastDistance = distance;
                distance = Player.Rect.Right;
            }

            //Regenerate health when standing on a health tile.
            if(Player.IsTouchingTile(18) && Player.Walled && Player.Grounded)
            {
                //Adjust the regen timer, add health if applicable
                if (regenTime < healthRegenTimer)
                {
                    regenTime += elapsed;
                    if (regenTime >= healthRegenTimer)
                    {
                        regenTime = 0;
                        TakeDamage(-1);
                    }
                }
            }

            Time += elapsed;

            if (invulnerable) {

                if (flash < invulnFlashRate) {
                    flash += elapsed;

                    if (flash >= invulnFlashRate) {
                        flash = 0f;
                        invulnIsFlashed = !invulnIsFlashed;
                    }
                }

                invulnerabilityTimer += elapsed;

                if (invulnerabilityTimer > graceTime) {
                    invulnerable = false;
                    invulnerabilityTimer = 0f;
                }
            } else
                invulnIsFlashed = false;

            //Calculate whether the player died this update
            //if the player is alive
            //and the player has a chunk and is a certain distance below that chunk
            //or if the player is off the screen on the left
            //kill the player.
            if (isAlive &&
                (Player.CurrentChunk != null && Player.Position.Y > Player.CurrentChunk.Bottom + Game1.CHUNK_LOWER_BOUND) ||
                Player.Position.X <= Game1.Camera.Rect.Left) {
                Die();
            }
                
        }

        

        //removes health from the player. If the health is 0, kills the player.
        public void TakeDamage(int damage) {
            if(damage > 0 && !invulnerable)
            {
                MakeParticles();
                Health = MathHelper.Clamp(Health - damage, 0, MaxHealth);
                invulnerable = true;
                invulnerabilityTimer = 0f;
                Sound.PlayerTakeDamage.Play();
            } else if(damage < 0) {
                Health = MathHelper.Clamp(Health - damage, 0, MaxHealth);
                Sound.PickupGem.Play();
            }

            if (Health == 0)
                Die();
        }

        //Take damage from a source and get knockbacked away from the source
        public void TakeDamage(int damage, Entity e)
        {
            Vector2 knockback;
            if (!invulnerable)
            {
                MakeParticles();

                Health = MathHelper.Clamp(Health - damage, 0, MaxHealth);
                knockback = Player.Position - e.Position;
                knockback.Normalize();
                knockback *= 1000;
                Player.KnockBack(knockback);
                invulnerable = true;
                invulnerabilityTimer = 0f;
                Sound.PlayerTakeDamage.Play();
            }

            if (Health == 0)
                Die();
        }

        void MakeParticles()
        {
            int particlesToMake = Game1.RandomObject.Next(1, 4);

            for (int i = 0; i < particlesToMake; i++)
            {
                Game1.Entities.particles.Add(new DynamicParticle(Graphics.PlayerHitParticles[0], Graphics.PlayerHitParticles, new Vector2(Player.Rect.Center.X, Player.Rect.Center.Y), 5f, new Vector2(500, Game1.RandomObject.Next(-500, -200))));
            }
        }

        public void TickScore() {
			ScoreMultiTicks++;

            if (ScoreMultiTicks >= 4 && ScoreMultiplier < 8)
            {
                ScoreMultiTicks = 0;
                if (ScoreMultiplier == 1)
                    ScoreMultiplier = 2;
                else if (ScoreMultiplier == 2)
                    ScoreMultiplier = 4;
                else if (ScoreMultiplier == 4)
                    ScoreMultiplier = 8;

                Sound.MultiplierIncrease.Play();
            }else {
                Sound.PickupCoin.Play();
            }
        }

		public void ResetMultiplier() {
			ScoreMultiplier = 1;
			ScoreMultiTicks = 0;
		}

		//Just sets isAlive to false.
		public void Die() {

			if (isAlive) {
				Game1.Score.mostRecentScore = Score;
				Game1.Score.AddScore(Score);
                Sound.PlayerDeath.Play();
            }
            invulnColor = Color.White;
            isAlive = false;
			
        }

        //Resets this object to its default values.
        public void Reset() {
            Score = 0;
            ScoreMultiplier = 1;
			ScoreMultiTicks = 0;
            distance = 0;
            lastDistance = distance;
            Time = 0;
            invulnerable = false;

            Health = MaxHealth;
            isAlive = true;


        }
    }
}