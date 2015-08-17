using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Match3.Screen
{
    class BackgroundScreen : BaseScreen
    {
        Texture2D background;

        public BackgroundScreen(Game game)
            : base(game)
        {
            background = Textures.background;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();
            spriteBatch.Begin();
            spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
