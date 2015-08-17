using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Match3
{
    class MainScreen : BaseScreen
    {

        private Button playBtn;

        public MainScreen(Game game) : base(game) 
        {
            playBtn = new Button(game, new Rectangle(), Textures.playButton);
            ScreenManager screenManager = game.Services.GetService<ScreenManager>();
            playBtn.onMouseUp += (Object Sender, EventArgs e) =>
            {
                this.hidden = true;
                screenManager.AddScreen(new GameplayScreen(game));
            };
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            int btnCenterX = (GraphicsDevice.Viewport.Width - playBtn.texture.Width) / 2;
            int btnCenterY = (GraphicsDevice.Viewport.Height - playBtn.texture.Height) / 2;
            playBtn.rectangle = new Rectangle(btnCenterX, btnCenterY, playBtn.texture.Width, playBtn.texture.Height);
            playBtn.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            playBtn.Update(gameTime);
            base.Update(gameTime);
        }
    }
}
