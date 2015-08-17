using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Match3
{
    class GameplayScreen : BaseScreen
    {
        private Field field;
        private double totalMilliseconds = 0;
        private Message messageGameOver;

        private class Message : DrawableGameComponent
        {
            public string text;
            private Button okButton;
            private Texture2D background;
            private Vector2 position;
            public Message(Game game, string text, EventHandler onOk, Vector2 position, Texture2D background) : base(game)
            {
                this.text = text;
                this.position = position;
                this.background = background;
                this.okButton = new Button(game, new Rectangle(), Textures.okButton);
                if (onOk != null)
                    this.okButton.onMouseUp += onOk;
            }

            public override void Draw(GameTime gameTime)
            {
                Vector2 textSize = Textures.font.MeasureString(text);
                Vector2 textPos = new Vector2(position.X - textSize.X / 2, position.Y - textSize.Y / 2);
                okButton.rectangle = new Rectangle(
                    (int)position.X - okButton.texture.Width / 2,
                    (int) position.Y - okButton.texture.Height / 2 + background.Height / 3,
                    okButton.texture.Width, okButton.texture.Height
                );
                SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();
                spriteBatch.Begin();
                spriteBatch.Draw(background, position - background.Bounds.Center.ToVector2(), Color.White);
                spriteBatch.DrawString(Textures.font, text, textPos, Color.White);
                spriteBatch.End();
                okButton.Draw(gameTime);       
                base.Draw(gameTime);
            }

            public override void Update(GameTime gameTime)
            {
                okButton.Update(gameTime);
                base.Update(gameTime);
            }
        }
        public GameplayScreen(Game game) : base(game) 
        {
            field = new Field(game);
            messageGameOver = new Message(game, "Game Over", onOkMessage,
                GraphicsDevice.Viewport.Bounds.Center.ToVector2(), Textures.dialogBackground
            );
        }

        public void onOkMessage(Object Sender, EventArgs e)
        {
            ScreenManager screenManager = Game.Services.GetService<ScreenManager>();
            screenManager.RemoveScreen();
            screenManager.GetTop().hidden = false;
        }
        public override void Draw(GameTime gameTime)
        {
            int offset = 10;
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();
            string scoreLabel = "Score";
            string score = field.GetScore().ToString();
            string timeLabel = "Time";
            string time = GetTimeLeft().ToString();
            Vector2 timeLabelSize = Textures.font.MeasureString(timeLabel);
            Vector2 scoreLabelSize = Textures.font.MeasureString(scoreLabel);
            Vector2 timeSize = Textures.font.MeasureString(time);
            Vector2 scoreSize = Textures.font.MeasureString(score);
            spriteBatch.Begin();
            spriteBatch.DrawString(Textures.font, scoreLabel, new Vector2(offset, offset), Config.textColor);
            spriteBatch.DrawString(Textures.font, timeLabel, 
                new Vector2(GraphicsDevice.Viewport.Width - timeLabelSize.X - offset, offset), Config.textColor
            );
            spriteBatch.DrawString(Textures.font, score, new Vector2(offset, 2 * offset + scoreLabelSize.Y), Config.textColor);
            spriteBatch.DrawString(Textures.font, time,
                new Vector2(GraphicsDevice.Viewport.Width - timeSize.X - offset, 2 * offset + timeLabelSize.Y),
                Config.textColor);
            spriteBatch.End();
            field.Draw(gameTime);
            if (isGameOver()) {
                messageGameOver.Draw(gameTime);
            }
            base.Draw(gameTime);
        }

        private bool isGameOver()
        {
            return GetTimeLeft() == 0;
        }
        private int GetTimeLeft()
        {
            int time = Config.gameTime - (int)(totalMilliseconds / 1000);
            return time < 0 ? 0 : time;
        }
        public override void Update(GameTime gameTime)
        {
            if (!isGameOver()) {
                totalMilliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;
                field.Update(gameTime);
            } else {
                messageGameOver.Update(gameTime);
            }
            base.Update(gameTime);
        }
    }
}
