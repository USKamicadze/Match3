using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Match3
{
    abstract class Clickable : DrawableGameComponent
    {
        public Rectangle rectangle;

        public event EventHandler onMouseIn;
        public event EventHandler onMouseOut;
        public event EventHandler onMouseDown;
        public event EventHandler onMouseUp;
        protected bool clicked = false;
        protected bool over = false;
        public Clickable(Game game, Rectangle rectangle)
            : base(game)
        {
            this.rectangle = rectangle;
        }
        public override void Update(GameTime gameTime)
        {
            if (IsMouseOver()) {
                if (!over && onMouseIn != null)
                    onMouseIn(this, new EventArgs());
                over = true;
                if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
                    if (onMouseDown != null)
                        onMouseDown(this, new EventArgs());
                    clicked = true;
                } else if (clicked) {
                    if (onMouseUp != null)
                        onMouseUp(this, new EventArgs());
                    clicked = false;
                } else {
                    clicked = false;
                }
            } else {
                if (over && onMouseOut != null)
                    onMouseOut(this, new EventArgs());
                clicked = false;
                over = false;
            }
            base.Update(gameTime);
        }

        protected bool IsMouseOver()
        {
            return this.rectangle.Contains(Mouse.GetState().Position);
        }
    }
}
