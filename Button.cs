using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Match3
{
    class Button : Clickable
    {
        public Texture2D texture;
        
        public Button(Game game, Rectangle rectangle, Texture2D texture)
            : base(game, rectangle)
        {
            this.rectangle = rectangle;
            this.texture = texture;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();
            spriteBatch.Begin();
            spriteBatch.Draw(texture, rectangle, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        
    }
}
