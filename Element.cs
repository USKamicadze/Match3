using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Match3
{

    class Element : Clickable
    {

        public enum Type
        {
            Grey,
            Blue,
            Red,
            Yellow,
            Pink,
            Cyan,
            COUNT,
            Empty
        }

        public enum Bonus
        {
            LineHorizontal,
            LineVertical,
            Bomb
        };

        public int col;
        public int row;
        public Bonus? bonus = null;
        private ElementAnimation bonusAnimation = null;
        public Type type { get; private set; }
        public Texture2D texture;
        public Color color = Color.White;
        public float transparency = 1;
        public float rotation = 0;
        public float scale = 1;
        public Vector2 origin = new Vector2();
        public SpriteEffects effects = SpriteEffects.None;
        public float layerDepth = 1;
        //public List<ElementAnimation> animations = new List<ElementAnimation>();
        private Dictionary<System.Type, ElementAnimation> animations = new Dictionary<System.Type, ElementAnimation>();
        private HashSet<System.Type> animationsToRemove = new HashSet<System.Type>();
        private Dictionary<System.Type, ElementAnimation> animationsToAdd = new Dictionary<System.Type, ElementAnimation>();
        public double lastMoved = -1;
        public bool dying = false;

        public Element(Game game, int col, int row, Type type, Rectangle rect) : base(game, rect)
        {
            this.col = col;
            this.row = row;
            this.type = type;
            if (type == Type.Empty) return;
            this.texture = Textures.elements[type];
            origin.X = this.texture.Width / 2;
            origin.Y = this.texture.Height / 2;
        }

        public void AddAnimation(System.Type type, ElementAnimation animation)
        {
            if (!animationsToAdd.ContainsKey(type)) {
                animationsToAdd.Add(type, animation);
            }
        }

        public void AddAnimation<T>(T animation) where T : ElementAnimation
        {
            AddAnimation(typeof(T), animation);
        }

        public void RemoveAnimation(System.Type type)
        {
            if (animations.ContainsKey(type)) {
                animationsToRemove.Add(type);
            }
        }

        public void RemoveAnimation<T>() where T : ElementAnimation
        {
            RemoveAnimation(typeof(T));
        }

        public void EndAndRemoveAllAnimations()
        {
            foreach (ElementAnimation a in animations.Values) {
                if (!a.ended) a.End();
            }
            animations.Clear();
        }

        private void RemoveNotNeededAnimations()
        {
            foreach (System.Type shouldBeRemoved in animationsToRemove) {
                animations[shouldBeRemoved].looping = false;
                if (animations[shouldBeRemoved].ended) {
                    animations.Remove(shouldBeRemoved);
                }
            }
            animationsToRemove.RemoveWhere(s => !animations.ContainsKey(s));
        }

        private void AddNeededAnimations()
        {
            List<System.Type> added = new List<System.Type>();
            foreach (KeyValuePair<System.Type, ElementAnimation> pair in animationsToAdd) {
                bool contains = animations.ContainsKey(pair.Key);
                if (contains && animations[pair.Key].ended) {
                    animations[pair.Key] = pair.Value;
                    pair.Value.Start();
                    added.Add(pair.Key);
                } else if (!contains) {
                    animations.Add(pair.Key, pair.Value);
                    pair.Value.Start();
                    added.Add(pair.Key);
                }
            }
            foreach (System.Type a in added) {
                animationsToAdd.Remove(a);
            }
        }

        public void UpdateAnimations(GameTime gameTime)
        {
            foreach (KeyValuePair<System.Type, ElementAnimation> animation in animations) {
                animation.Value.Update(gameTime);
            }
            AddNeededAnimations();
            RemoveNotNeededAnimations();
        }
        public override void Update(GameTime gameTime)
        {
            if (type == Type.Empty) return;
            UpdateAnimations(gameTime);
            if (bonusAnimation != null) bonusAnimation.Update(gameTime);
            base.Update(gameTime);
        }

        
        public override void Draw(GameTime gameTime)
        {
            if (type == Type.Empty) return;
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();
            spriteBatch.Begin();
            foreach (KeyValuePair<System.Type, ElementAnimation> animation in animations) {
                animation.Value.Draw(gameTime);
            }
            spriteBatch.Draw(
                texture, 
                new Rectangle(rectangle.Center.X, rectangle.Center.Y, (int) (rectangle.Width * scale) , (int) (rectangle.Height * scale)), 
                texture.Bounds,
                color * transparency, 
                rotation, 
                origin, 
                effects, 
                layerDepth
            );
            if (bonusAnimation != null) bonusAnimation.Draw(gameTime);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }

        public void SetBonus(Bonus bonus)
        {
            switch (bonus){
                case Bonus.LineHorizontal : bonusAnimation = new HorizontalLineBonusElementAnimation(Game, this); break;
                case Bonus.LineVertical : bonusAnimation = new VerticalLineBonusElementAnimation(Game, this); break;
                case Bonus.Bomb : bonusAnimation = new BombElementAnimation(Game, this); break;
            }
            this.bonus = bonus;
        }

        public void RemoveBonus()
        {
            this.bonus = null;
            this.bonusAnimation = null;
        }
    }
}
