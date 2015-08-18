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
    class ActivatedBonusBase : DrawableGameComponent
    {
        public float scale = 1;
        public bool ended = false;
        public ActivatedBonusBase(Game game) : base(game) { }

    }

    class LineBonus : ActivatedBonusBase
    {
        float shift;
        Vector2 start;
        Vector2 position;
        Vector2 end;
        float duration = 1;
        float elapsed = 0;
        AnimatedTexture texture;

        public delegate void LineBonusEventHandler(LineBonus lineBonus, LineBonusEventAgrs e);
        public event LineBonusEventHandler onEnd;
        public event LineBonusEventHandler onUpdate;

        public class LineBonusEventAgrs : EventArgs
        {
            public Vector2 prevPosition;
            public Vector2 newPosition;
            public LineBonusEventAgrs(Vector2 prevPosition, Vector2 newPosition)
            {
                this.prevPosition = prevPosition;
                this.newPosition = newPosition;
            }
        }
        public LineBonus(Game game, Vector2 start, Vector2 end, Vector2 dir, float shift)
            : base(game)
        {
            this.texture = new GlowingBallTexture(game.GraphicsDevice, 16, 16);
            this.shift = shift;
            this.position = start;
            this.end = end + dir * Config.tile.Size.ToVector2() / 2;
            this.start = start;
            Vector2 d = (this.start - this.end) / shift;
            this.duration = Math.Abs(d.X - d.Y);
            this.position = start;
        }

        public override void Update(GameTime gameTime)
        {
            float elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (!ended) {
                elapsed += elapsedGameTime;
                Vector2 prevPos = position;
                position = start + (end - start) * elapsed / duration;
                texture.Update(gameTime);
                if (onUpdate != null)
                    onUpdate(this, new LineBonusEventAgrs(prevPos, position));
                if (duration < elapsed) {
                    ended = true;
                    position = end;
                    if (onEnd != null)
                        onEnd(this, new LineBonusEventAgrs(prevPos, position));
                }
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!ended) {
                SpriteBatch sb = Game.Services.GetService<SpriteBatch>();
                SpriteRender sr = Game.Services.GetService<SpriteRender>();
                sb.Begin();
                texture.Draw(gameTime, sr, position, scale);
                sb.End();
                base.Draw(gameTime);
            }
        }

    }

    class BombBonus : ActivatedBonusBase
    {


        public Element el { get; private set; }
        AnimatedTexture explosionTexture;
        double elapsed = 0;
        bool exploded = false;
        public delegate void BombBonusEventHandler(BombBonus bombBonus);
        public event BombBonusEventHandler onExplosion;
        public BombBonus(Game game, Element el)
            : base(game)
        {
            this.explosionTexture = new ExplosionTexture(GraphicsDevice, Config.tile.Width * 3, Config.tile.Height * 3);
            this.el = el;
            this.scale = 5;
        }

        public override void Update(GameTime gameTime)
        {
            elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (elapsed < Config.bombBonusDuration) {
                float v = 255 - (float)(255 * elapsed / Config.bombBonusDuration);
                el.color = new Color(255, v, v);
            } else if (!exploded) {
                exploded = true;
                el.color = Color.Red;
                if (onExplosion != null)
                    onExplosion(this);
            }
            if (exploded)
                explosionTexture.Update(gameTime);
            if (explosionTexture.ended) ended = true;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!ended) {
                if (!exploded) {
                    if (el.dying)
                        el.Draw(gameTime);
                } else {
                    SpriteBatch sb = Game.Services.GetService<SpriteBatch>();
                    SpriteRender sr = Game.Services.GetService<SpriteRender>();
                    sb.Begin();
                    explosionTexture.Draw(gameTime, sr, el.rectangle.Center.ToVector2(), scale);
                    sb.End();
                }
            }
            base.Draw(gameTime);
        }
    }

}
