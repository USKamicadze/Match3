using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Match3
{
    public class ScreenManager: DrawableGameComponent
    {
        private IList<BaseScreen> screenStack;



        public ScreenManager(Game game) : base(game)
        {
            screenStack = new List<BaseScreen>();
        }

        public ScreenManager AddScreen(BaseScreen screen)
        {
            screenStack.Add(screen);
            return this;
        }

        public ScreenManager RemoveScreen()
        {
            screenStack.RemoveAt(screenStack.Count - 1);
            return this;
        }

        public BaseScreen GetTop()
        {
            return screenStack.Last();
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (BaseScreen screen in screenStack)
            {
                if (!screen.hidden)
                    screen.Draw(gameTime);
            }
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (BaseScreen screen in Enumerable.Reverse(screenStack))
            {
                if (!screen.hidden) {
                    screen.Update(gameTime);
                    break;
                }
            }
            base.Update(gameTime);
        }


    }
}
