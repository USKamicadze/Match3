using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TexturePackerLoader;
namespace Match3
{
    static class Textures
    {
        static public Texture2D background;
        static public Texture2D dialogBackground;

        static public Texture2D playButton;
        static public Texture2D okButton;

        static public SpriteSheet glowingBall;
        static public SpriteSheet bombBonus;
        static public SpriteSheet explosion;
        static public SpriteFont font;

        static public Texture2D test;
        static public Dictionary<Element.Type, Texture2D> elements = new Dictionary<Element.Type, Texture2D>();

        static public void Load(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            background = Content.Load<Texture2D>("Images/background");
            dialogBackground = Content.Load<Texture2D>("Images/dialogBackground");
            playButton = Content.Load<Texture2D>("Images/playButton");
            okButton = Content.Load<Texture2D>("Images/okButton");
            elements[Element.Type.Blue] = Content.Load<Texture2D>("Images/blue");
            elements[Element.Type.Cyan] = Content.Load<Texture2D>("Images/cyan");
            elements[Element.Type.Grey] = Content.Load<Texture2D>("Images/grey");
            elements[Element.Type.Pink] = Content.Load<Texture2D>("Images/pink");
            elements[Element.Type.Red] = Content.Load<Texture2D>("Images/red");
            elements[Element.Type.Yellow] = Content.Load<Texture2D>("Images/yellow");
            font = Content.Load<SpriteFont>("font");
            var spriteSheetLoader = new SpriteSheetLoader(Content);
            glowingBall = spriteSheetLoader.Load("Images/glowingBall");
            explosion = spriteSheetLoader.Load("Images/Explosion");
            bombBonus = spriteSheetLoader.Load("Images/bombBonus");
        }
    }
}
