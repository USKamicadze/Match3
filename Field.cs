using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Match3
{
    class Field : DrawableGameComponent
    {
        public const int size = 8;
        private Element[,] field;

        private static Random gen = new Random();
        private Element selected = null;
        private Rectangle tile;
        private Vector2 leftTop;

        private int destroyAnimations = 0;
        private int moveAnimations = 0;

        private class GameEvent
        {
            public delegate void Handler(Object gameObj);
            private Object gameObj;
            private Handler handler;
            public GameEvent(Object gameObj, Handler handler)
            {
                this.gameObj = gameObj;
                this.handler = handler;
            }

            public void Invoke()
            {
                handler(gameObj);
            }
        }

        private IList<GameEvent> events = new List<GameEvent>();

        private class Info
        {
            public int score = 0;
            public Turn lastTurn = null;
            public int scoreMultiplier = 1;

            public class Turn {
                public Point from;
                public Point to;
                public Turn(Point from, Point to)
                {
                    this.from = from;
                    this.to = to;
                }

            }
                        
        }

        private Info info = new Info();
        static private Element EMPTY;

        public Field(Game game) : base(game)
        {
            Init();
            EMPTY = new Element(Game, 0, 0, Element.Type.Empty, new Rectangle());
        }

        Element CreateRandomElement(int col, int row, Rectangle rect)
        {
            Element el =  new Element(Game,  col, row, (Element.Type)gen.Next((int)Element.Type.COUNT - 1), rect);
            el.onMouseUp += onMouseUpElement;
            el.onMouseIn += onMouseInElement;
            el.onMouseOut += onMouseOutElement;
            return el;
        }
        void Init()
        {
            field = new Element[size, size];
            Vector2 center = GraphicsDevice.Viewport.Bounds.Center.ToVector2();
            tile = Config.tile;
            leftTop = center - new Vector2(tile.Width * size / 2, tile.Height * size / 2);
            for (int i = 0; i < size; ++i) {
                for (int j = 0; j < size; ++j) {
                    Element el = CreateRandomElement(j , i, GetRectangleByColAndRow(j, i));
                    field[i, j] = el;
                }
            }
        }

        private void MouseUpElementEventHandler(Object element)
        {
            Element el = element as Element;
            if (moveAnimations > 0 || destroyAnimations > 0 || info.lastTurn != null || selected == el) return;
            if (selected != null) {
                if (CellsIsNear(selected, el)) {
                    AddTurn(el);
                    return;
                } else {
                    selected.RemoveAnimation<SelectElementAnimation>();
                }
            }
            selected = el;
            el.AddAnimation(new SelectElementAnimation(Game, el));
        }

        private void MouseInElementEventHandler(Object element)
        {
            Element el = element as Element;
            el.AddAnimation(new OverElementAnimation(Game, el));
        }
        
        private void MouseOutElementEventHandler(Object element)
        {
            Element el = element as Element;
            el.RemoveAnimation<OverElementAnimation>();
        }

        private void DestroyElementEventHandler(Object element)
        {
            Element el = element as Element;
            --destroyAnimations;
            field[el.row, el.col] = EMPTY;
            info.score += Config.scorePerElement * info.scoreMultiplier;
            el.EndAndRemoveAllAnimations();
        }

        private void TurnEndEventHandler(Object element)
        {
            SwapElements(info.lastTurn.from, info.lastTurn.to);
            if (ChainExists()) {
                info.lastTurn = null;
            } else {
                AddSwapAnimation(info.lastTurn, onBadTurnEnd);
                SwapElements(info.lastTurn.from, info.lastTurn.to);
            }
            selected.RemoveAnimation<SelectElementAnimation>();
            selected = null;
        }

        private void BadTurnEndEventHandler(Object element)
        {
            info.lastTurn = null;
        }

        private void MoveEndEventHandler(Object element)
        {
            --moveAnimations;
        }

        private void onMouseUpElement(Object Sender, EventArgs e)
        {
            events.Add(new GameEvent(Sender, MouseUpElementEventHandler));
        }

        private void onMouseInElement(Object Sender, EventArgs e)
        {
            events.Add(new GameEvent(Sender, MouseInElementEventHandler));
        }

        private void onMouseOutElement(Object Sender, EventArgs e)
        {
            events.Add(new GameEvent(Sender, MouseOutElementEventHandler));
        }

        private void onDestroyElementEnd(Object Sender, EventArgs e)
        {
            ElementAnimation a = Sender as ElementAnimation;
            events.Add(new GameEvent(a.element, DestroyElementEventHandler));
        }
        private void onTurnEnd(Object Sender, EventArgs e)
        {
            events.Add(new GameEvent(Sender, TurnEndEventHandler));
        }

        private void onBadTurnEnd(Object Sender, EventArgs e)
        {
            events.Add(new GameEvent(Sender, BadTurnEndEventHandler));
        }

        private void MoveAnimationEnd(Object Sender, EventArgs e)
        {
            events.Add(new GameEvent(Sender, MoveEndEventHandler));
        }
        private void AddTurn(Element el)
        {
            if (info.lastTurn != null) return;
            info.lastTurn = new Info.Turn(new Point(selected.col, selected.row), new Point(el.col, el.row));
            AddSwapAnimation(info.lastTurn, onTurnEnd);
        }

        private void FindChainsAndDestroy()
        {
            HashSet<Element> chains = FindChains();
            if (chains.Count > 0) {
                //toDestroy.UnionWith(chains);
                foreach (Element mustDie in chains) {
                    ++destroyAnimations;
                    mustDie.onMouseUp -= onMouseUpElement;
                    mustDie.onMouseIn -= onMouseInElement;
                    mustDie.onMouseOut -= onMouseOutElement;
                    DestroyElementAnimation destroyAnimation = new DestroyElementAnimation(Game, mustDie);
                    destroyAnimation.onEnd += onDestroyElementEnd;
                    mustDie.RemoveAnimation<OverElementAnimation>();
                    mustDie.AddAnimation(destroyAnimation);
                }
            }
        }

        private bool ChainExists()
        {
            for (int i = 0; i < size; ++i) {
                for (int j = 0; j < size; ++j) {
                    if (field[i, j] == EMPTY) continue;
                    Element.Type type = field[i, j].type;
                    if (CalcLine(new Point(j, i), new Point(1, 0), type) >= 3 ||
                        CalcLine(new Point(j, i), new Point(0, 1), type) >= 3
                    ) { return true; }
                }
            }
            return false;
        }
        private HashSet<Element> FindChains()
        {
            HashSet<Element> chains = new HashSet<Element>();
            for (int i = 0; i < size; ++i) {
                for (int j = 0; j < size; ++j) {
                    if (field[i, j] == EMPTY) continue;
                    Element.Type type = field[i, j].type;
                    int countX = CalcLine(new Point(j, i), new Point(1, 0), type);
                    int countY = CalcLine(new Point(j, i), new Point(0, 1), type);
                    chains.UnionWith(GetLineOf3plusElements(new Point(j, i), new Point(1, 0), countX));
                    chains.UnionWith(GetLineOf3plusElements(new Point(j, i), new Point(0, 1), countY));
                }
            }
            return chains;
        }
        
        private int CalcLine(Point start, Point shift, Element.Type type)
        {
            int count = 0;
            for (Point cur = start; cur.X < size && cur.Y < size && GetElementByPoint(cur).type == type; cur += shift)
                ++count;
            return count;
        }

        private IList<Element> GetLineOf3plusElements(Point start, Point shift, int count)
        {
            IList<Element> line = new List<Element>();
            if (count < 3) return line;
            Point cur = start;
            for ( int v = 0; v < count; ++v, cur += shift) {
                line.Add(GetElementByPoint(cur));
            }
            return line;
        }

        private Element GetElementByPoint(Point p)
        {
            return field[p.Y, p.X];
        }
        private void SwapElements(Point a, Point b)
        {
            SwapElements(GetElementByPoint(a), GetElementByPoint(b));
        }
        private void SwapElements(Element a, Element b)
        {
            field[a.row, a.col] = b;
            field[b.row, b.col] = a;
            Swap(ref a.row, ref b.row);
            Swap(ref a.col, ref b.col);
        }

        static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        private Rectangle GetRectangleByColAndRow(int col, int row)
        {
            return new Rectangle(
                (int)leftTop.X + col * tile.Width, 
                (int)leftTop.Y + row * tile.Height, 
                tile.Width, 
                tile.Height
            );
        }

        private bool CellsIsNear(int col1, int row1, int col2, int row2)
        {
            return Math.Abs(col1 - col2) + Math.Abs(row1 - row2) == 1;
        }

        private bool CellsIsNear(Element e1, Element e2)
        {
            return CellsIsNear(e1.col, e1.row, e2.col, e2.row);
        }

        private bool WasInLastTurn(Element el)
        {
            Info.Turn lastTurn = info.lastTurn;
            return lastTurn != null && (
                GetElementByPoint(lastTurn.from) == el ||
                GetElementByPoint(lastTurn.to) == el
            );
        }

        private void AddSwapAnimation(Info.Turn turn, EventHandler onEnd = null)
        {
            Element from = GetElementByPoint(turn.from);
            Element to = GetElementByPoint(turn.to);
            AddMoveAnimation(to, GetRectangleByColAndRow(turn.from.X, turn.from.Y));
            MoveElementAnimation moveSelectedAnimation = AddMoveAnimation(from, GetRectangleByColAndRow(turn.to.X, turn.to.Y));
            if (onEnd != null)
                moveSelectedAnimation.onEnd += onEnd;
        }

        private MoveElementAnimation AddMoveAnimation(Element el, Rectangle dest)
        {
            double duration = Math.Abs(dest.Center.Y - el.rectangle.Center.Y + dest.Center.X - el.rectangle.Center.X) 
                / Config.shift * Config.shiftTime;
            MoveElementAnimation res = new MoveElementAnimation(Game, el, dest, duration);
            res.onEnd += MoveAnimationEnd;
            el.AddAnimation(res);
            ++moveAnimations;
            return res;
        }
        private void MakeElementFall()
        {
            for (int j = 0; j < size; ++j) {
                for (int i = size - 1; i >= 0; --i) {
                    if (field[i, j] == EMPTY) {
                        int k = i;
                        while (k >= 0 && field[k, j] == EMPTY) --k;
                        Element el;
                        if (k >= 0) { //non empty cell finded
                            el = field[k, j];
                            AddMoveAnimation(el, GetRectangleByColAndRow(j, i));
                            field[k, j] = EMPTY;
                            el.row = i;
                            field[i, j] = el;
                        } else {
                            for (k = 0; k <= i; ++k) {
                                el = CreateRandomElement(j, i - k, GetRectangleByColAndRow(j, -1-k));
                                field[i - k, j] = el;
                                AddMoveAnimation(el, GetRectangleByColAndRow(j, i - k));
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateElements(GameTime gameTime)
        {
            for (int i = 0; i < size; ++i) {
                for (int j = 0; j < size; ++j) {
                    Element el = field[i, j];
                    if (el == EMPTY) continue;
                    el.Update(gameTime);
                }
            }
        }

        private void UpdateEvents(GameTime gameTime)
        {
            for (int i = 0; i < events.Count; ++i) {
                events[i].Invoke();
            }
            events.Clear();
        }
        public override void Update(GameTime gameTime)
        {

            if (destroyAnimations == 0 && moveAnimations == 0) {
                FindChainsAndDestroy();
                MakeElementFall();
            }
            if (events.Count == 0) {
                UpdateElements(gameTime);
            }
            UpdateEvents(gameTime);
        }

        private void DrawElements(GameTime gameTime)
        {
            for (int i = 0; i < size; ++i) {
                for (int j = 0; j < size; ++j) {
                    if (field[i, j] != EMPTY)
                        field[i, j].Draw(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            DrawElements(gameTime);
        }

        public int GetScore()
        {
            return info.score;
        }
        
    }
}
