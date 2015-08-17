using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Match3
{
    public abstract class BaseScreen : DrawableGameComponent
    {
        public BaseScreen(Game game) : base(game) { }
        public bool hidden = false;
    }
}
