using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Match3
{
    abstract class ElementAnimation : DrawableGameComponent
    {
        protected double duration;
        protected double elapsed;
        public Element element {get; private set;} 
        public bool looping = false;
        public event EventHandler onEnd;
        public bool started { get; private set; }
        public bool ended { get; private set; }
        public ElementAnimation(Game game, double duration, Element el) : base(game) 
        {
            this.duration = duration / Config.animationSpeed;
            this.element = el;
        }

        public override void Draw(GameTime gameTime)
        {
            if (started && !ended) {
                Animate(gameTime);
            }
            base.Draw(gameTime);
        }

        protected virtual void Animate(GameTime gameTime){}

        public virtual void Start()
        {
            elapsed = 0;
            started = true;
            ended = false;
        }

        public virtual void End()
        {
            ended = true;
            if (ended && onEnd != null)
                onEnd(this, new EventArgs());
        }

        public override void Update(GameTime gameTime)
        {
            if (started && !ended) {
                float elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                elapsed += elapsedGameTime;
            }
            if (!looping && elapsed >= duration) End();
            base.Update(gameTime);
        }
    }

    class TiltElementAnimation : ElementAnimation
    {
        
        protected bool toright = true;
        protected float amplitude;
        protected double cur;
        public TiltElementAnimation(Game game, double duration, Element el, float amplitude)
            : base(game, duration, el)
        {
            this.amplitude = amplitude;
        }

        protected override void Animate(GameTime gameTime)
        {
            element.rotation = (float)cur;
        }

        public override void Update(GameTime gameTime)
        {
            double elapsedGameTime = gameTime.ElapsedGameTime.TotalSeconds;
            double r = 4 * amplitude * elapsedGameTime / duration;
            if (toright)
                cur += r;
            else
                cur -= r;
            if (toright && cur > amplitude) toright = false;
            else if (!toright && cur < -amplitude) toright = true;
            base.Update(gameTime);
        }

        public override void End()
        {
            element.rotation = 0;
            base.End();
        }
         
    }

    class HighlightElementAnimation : ElementAnimation
    {
        Color color;
        Color original;
        public HighlightElementAnimation(Game game, double duration, Element el, Color color)
            : base(game, duration, el)
        {
            this.color = color;
        }
        protected override void Animate(GameTime gameTime)
        {
            element.color = color;
        }

        public override void Start()
        {
            this.original = element.color;
            base.Start();
        }
        public override void End()
        {
            element.color = original;
            base.End();
        }
    }

    class TransparencyElementAnimation : ElementAnimation
    {
        float transparencySource;
        float transparencyFinal;
        float cur;
        public TransparencyElementAnimation(Game game, double duration, Element el, float transparency)
            : base(game, duration, el) 
        {
            this.transparencyFinal = transparency;
        }

        protected override void Animate(GameTime gameTime)
        {
            this.element.transparency = cur;
        }

        public override void Update(GameTime gameTime)
        {
            float elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            cur += (transparencyFinal - transparencySource) * (float)elapsedGameTime / (float)duration;
            base.Update(gameTime);
        }

        public override void Start()
        {
            this.transparencySource = cur = element.transparency;
            base.Start();
        }
        public override void End()
        {
            element.transparency = transparencyFinal;
            base.End();
        }
    }
    class OverElementAnimation : TiltElementAnimation
    {
        public OverElementAnimation(Game game, Element el) 
            : base(game, Config.overAnimationSpeed, el, Config.overAnimationAmplitude) { this.looping = true; }
        
    }


    class SelectElementAnimation : HighlightElementAnimation
    {
        public SelectElementAnimation(Game game, Element el) : base(game, 0.1, el, Color.Lime) { this.looping = true; }
        
    }

    class MoveElementAnimation : ElementAnimation
    {
        
        private Rectangle destination;
        private Rectangle source;
        private Vector2 amount;
        public MoveElementAnimation(Game game, Element el, Rectangle rectangle, double duration)
            : base(game, duration / Config.moveAnimationSpeed, el)
        {
            this.destination = rectangle;
        }

        protected override void Animate(GameTime gameTime)
        {
            element.rectangle.Location = source.Location + amount.ToPoint();
            base.Animate(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            amount = (destination.Center.ToVector2() - source.Center.ToVector2())
                * (float)elapsed
                / (float)duration;
            base.Update(gameTime);
        }
        public override void Start()
        {
            this.source = element.rectangle;
            base.Start();
        }
        public override void End()
        {
            element.rectangle = destination;
            element.RemoveAnimation<MoveElementAnimation>();
            base.End();
        }
        
    }

    class DestroyElementAnimation : TransparencyElementAnimation
    {
        public DestroyElementAnimation(Game game, Element el) : base(game, Config.destroyAnimationSpeed, el, 0) 
        {

        }
    }

}
