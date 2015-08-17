using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Match3
{
    public static class Config
    {
        public static Rectangle tile = new Rectangle(0,0, 50, 50);
        public static double animationSpeed = 1.5;
        public static double shift = 1;
        public static double shiftTime = 1e-2;
        public static double moveAnimationSpeed = 1;
        public static double overAnimationSpeed = 1;
        public static float overAnimationAmplitude = 0.3f;
        public static Color selectAnimationColor = Color.Lime;
        public static double destroyAnimationSpeed = 1;
        public static int scorePerElement = 10;
        public static Color textColor = Color.MediumSeaGreen;
        public static int gameTime = 5; //seconds
    }
}
