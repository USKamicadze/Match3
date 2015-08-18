using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using TexturePackerLoader;
namespace Match3
{
    abstract public class AnimatedTexture : Texture2D
    {
        private List<string> frames;
        private SpriteSheet spriteSheet;
        private double milisecondsPerFrame;
        private double timeSinceLastFrame = 0;
        private int curFrame = 0;
        public bool loop = true;
        public EventHandler onEnd;
        public bool ended { get; private set; }
        public AnimatedTexture(GraphicsDevice graphicsDevice, int width, int height, double milisecondsPerFrame, SpriteSheet spriteSheet, List<string> frames) 
            : base(graphicsDevice, width, height)
        {
            this.spriteSheet = spriteSheet;
            this.frames = frames;
            this.milisecondsPerFrame = milisecondsPerFrame;
            ended = false;
        }

        private void NextFrame()
        {
            ++curFrame;
            timeSinceLastFrame = 0;
            if (loop && curFrame == frames.Count)
                curFrame = 0;
            if (!ended && !loop & curFrame == frames.Count) {
                ended = true;
                if (onEnd != null) 
                    onEnd(this, new EventArgs());
            }
        }

        public void Update(GameTime gameTime)
        {
            timeSinceLastFrame += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeSinceLastFrame > milisecondsPerFrame)
                NextFrame();
        }

        public void Draw(GameTime gameTime, SpriteRender spriteRender, Vector2 position, float scale)
        {
            if (!ended) {
                spriteRender.Draw(
                    spriteSheet.Sprite(frames[curFrame]),
                    position,
                    Color.White,
                    0,
                    scale,
                    SpriteEffects.None
                );
            }
        }

    }

    public class GlowingBallTexture : AnimatedTexture
    {

        public GlowingBallTexture(GraphicsDevice graphicsDevice, int width, int height) :
            base(graphicsDevice, width, height, Config.glowingBallAnimationFrameDuration, Textures.glowingBall, new List<string> {
                SpriteNames.GlowingBall.Glowing_ball_1,
                SpriteNames.GlowingBall.Glowing_ball_2,
                SpriteNames.GlowingBall.Glowing_ball_3,
                SpriteNames.GlowingBall.Glowing_ball_4,
                SpriteNames.GlowingBall.Glowing_ball_5,
        })
        {

        }
        
    }

    public class BombBonusTexture : AnimatedTexture
    {

        public BombBonusTexture(GraphicsDevice graphicsDevice, int width, int height) :
            base(graphicsDevice, width, height, Config.glowingBallAnimationFrameDuration, Textures.bombBonus, new List<string> {
                SpriteNames.BombBonus.Bomb_bonus_1,
                SpriteNames.BombBonus.Bomb_bonus_2,
                SpriteNames.BombBonus.Bomb_bonus_3,
                SpriteNames.BombBonus.Bomb_bonus_4,
                SpriteNames.BombBonus.Bomb_bonus_5,
        })
        {

        }

    }

    public class ExplosionTexture : AnimatedTexture
    {
        public ExplosionTexture(GraphicsDevice graphicsDevice, int width, int height) :
            base(graphicsDevice, width, height, Config.explosionAnimationFrameDuration, Textures.explosion, new List<string> { 
                SpriteNames.Explosion.Explosion_1,
                SpriteNames.Explosion.Explosion_2,
                SpriteNames.Explosion.Explosion_3,
                SpriteNames.Explosion.Explosion_4,
                SpriteNames.Explosion.Explosion_5,
                SpriteNames.Explosion.Explosion_6,
                SpriteNames.Explosion.Explosion_7,
                SpriteNames.Explosion.Explosion_8,
                SpriteNames.Explosion.Explosion_9,
                SpriteNames.Explosion.Explosion_10,
                SpriteNames.Explosion.Explosion_11,
                SpriteNames.Explosion.Explosion_12,
                SpriteNames.Explosion.Explosion_13,
                SpriteNames.Explosion.Explosion_14,
                SpriteNames.Explosion.Explosion_15,
                SpriteNames.Explosion.Explosion_16,
                SpriteNames.Explosion.Explosion_17,
                SpriteNames.Explosion.Explosion_18,
                SpriteNames.Explosion.Explosion_19,
                SpriteNames.Explosion.Explosion_20,
                SpriteNames.Explosion.Explosion_21,
                SpriteNames.Explosion.Explosion_22,
                SpriteNames.Explosion.Explosion_23,
                SpriteNames.Explosion.Explosion_24,
                SpriteNames.Explosion.Explosion_25,
                SpriteNames.Explosion.Explosion_26,
                SpriteNames.Explosion.Explosion_27,
            }) 
        {
            loop = false;    
        }
    }
}
