using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using TexturePackerLoader;

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
        private GameTime gameTime;
      

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


        private List<ActivatedBonusBase> activatedBonuses = new List<ActivatedBonusBase>(); 

        private Info info = new Info();
        static private Element EMPTY;

        public Field(Game game) : base(game)
        {
            Init();
            EMPTY = new Element(Game, 0, 0, Element.Type.Empty, new Rectangle());
        }

        Element CreateRandomElement(int col, int row, Rectangle rect)
        {
            return CreateElement(col, row, (Element.Type)gen.Next((int)Element.Type.COUNT), rect);
        }

        Element CreateElement(int col, int row,  Element.Type type, Rectangle rect)
        {
            Element el = new Element(Game, col, row, type, rect);
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
                    el.lastMoved = i * size + j;
                }
            }
        }

        private void MouseUpElementEventHandler(Object element)
        {
            Element el = element as Element;
            if (moveAnimations > 0 || destroyAnimations > 0 || activatedBonuses.Count > 0 || info.lastTurn != null || selected == el) return;
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
            if (MatchExists()) {
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


        private void onBombBonusExplosion(BombBonus bombBonus)
        {
            int row = bombBonus.el.row;
            int col = bombBonus.el.col;
            for (int i = row - 1; i < row + 2; ++i) {
                if (OutOfRange(i)) continue;
                for (int j = col - 1; j < col + 2; ++j) {
                    if (OutOfRange(j)) continue;
                    DestroyElement(field[i, j]);
                }
            }
        }

        private void MakeExplosion(Element el)
        {
            BombBonus bombBonus = new BombBonus(Game, el);
            bombBonus.onExplosion += onBombBonusExplosion;
            activatedBonuses.Add(bombBonus);
        }

        private List<Element> GetElementsBetweenPositions(Point pos1, Point pos2)
        {
            List<Element> res = new List<Element>();
            Element el1 = GetElementByPosition(pos1);
            Element el2 = GetElementByPosition(pos2);
            Vector2 d = (pos2 - pos1).ToVector2();
            d.Normalize();
            Point dir = d.ToPoint();
            Point countV = (pos2 - pos1) / Config.tile.Size;
            int count = Math.Abs(countV.X + countV.Y);
            Point curPos = pos1;
            for (Element cur = el1; cur != el2; cur = GetElementByPosition(curPos)) {
                curPos = curPos + dir * Config.tile.Size;
                res.Add(cur);
            }
            res.Add(el2);
            return res;
        }

        private void DestroyElement(Element el)
        {
            if (el.dying || el == EMPTY) return;
            ++destroyAnimations;
            el.dying = true;
            el.onMouseUp -= onMouseUpElement;
            el.onMouseIn -= onMouseInElement;
            el.onMouseOut -= onMouseOutElement;
            DestroyElementAnimation destroyAnimation = new DestroyElementAnimation(Game, el);
            destroyAnimation.onEnd += onDestroyElementEnd;
            el.RemoveAnimation<OverElementAnimation>();
            el.AddAnimation(destroyAnimation);
            ActivateBonus(el);
        }

        private void onLineBonusUpdate(LineBonus linebonus, LineBonus.LineBonusEventAgrs lineEventArgs)
        {
            List<Element> elements = GetElementsBetweenPositions(
                lineEventArgs.prevPosition.ToPoint(), lineEventArgs.newPosition.ToPoint()
            );
            foreach (Element el in elements)
                DestroyElement(el);
        }

        private LineBonus CreateLineBonus(Element el, int colEnd, int rowEnd, Vector2 dir)
        {
            LineBonus res =  new LineBonus(Game,
                el.rectangle.Center.ToVector2(),
                GetRectangleByColAndRow(colEnd, rowEnd).Center.ToVector2(),
                dir,
                Config.lineBonusShift);
            res.onUpdate += onLineBonusUpdate;
            res.onEnd += onLineBonusUpdate;
            return res;
        }
        private void LaunchDestroyers(Element el)
        {
            if (el.bonus == Element.Bonus.LineHorizontal) {
                int i = el.row;
                activatedBonuses.Add(CreateLineBonus(el, size - 1, i, new Vector2(1, 0)));
                activatedBonuses.Add(CreateLineBonus(el, 0, i, new Vector2(-1, 0)));
            } else {
                int i = el.col;
                activatedBonuses.Add(CreateLineBonus(el, i, size - 1, new Vector2(0, 1)));
                activatedBonuses.Add(CreateLineBonus(el, i, 0, new Vector2(0, -1)));
            }
        }
        private void ActivateBonus(Element el){
            switch (el.bonus) {
                case Element.Bonus.Bomb: MakeExplosion(el); break;
                case Element.Bonus.LineVertical:
                case Element.Bonus.LineHorizontal: LaunchDestroyers(el); break;
            }
            el.RemoveBonus();
        }

        private Element GetLastMoved(IEnumerable<Element> e)
        {
            Element res = e.First();
            foreach (Element el in e) {
                if (res.lastMoved < el.lastMoved) {
                    res = el;
                }
            }
            return res;
        }

        private List<Element>[,] FindAllMatches()
        {
            List<Element>[,] matchedElements = new List<Element>[size, size];
            for (int i = 0; i < size; ++i) {
                for (int j = 0; j < size; ++j) {
                    List<Element> matchedForCur = matchedElements[i, j] = new List<Element>();
                    Element el = field[i, j];
                    if (el == EMPTY) continue;
                    Element.Type type = el.type;
                    Point start = new Point(j, i);
                    List<Element> vert = GetSameElementsInLine(start, new Point(0, -1), type);
                    vert.AddRange(GetSameElementsInLine(start, new Point(0, 1), type));
                    List<Element> horiz = GetSameElementsInLine(start, new Point(-1, 0), type);
                    horiz.AddRange(GetSameElementsInLine(start, new Point(1, 0), type));
                    if (horiz.Count >= 3 + 1) //el in list 2 times
                        matchedForCur.AddRange(horiz);
                    if (vert.Count >= 3 + 1)
                        matchedForCur.AddRange(vert);
                    matchedElements[i, j] = matchedForCur.Distinct().ToList();
                }
            }
            return matchedElements;
        }

        //helper function
        private bool isNewBonusElement(Element el, List<Element>[,] matchedElements)
        {
            List<Element> curMatched = matchedElements[el.row, el.col];
            return curMatched.Count > 3 && (
                curMatched.TrueForAll(e => matchedElements[e.row, e.col].Count < curMatched.Count)
                || (
                    GetLastMoved(curMatched) == el
                &&  
                    curMatched.TrueForAll(e => matchedElements[e.row, e.col].Count <= curMatched.Count)
                )
            );
        }
        private void FindMatchesAndDestroy()
        {
            List<Element>[,] matchedElements = FindAllMatches();
            for (int i = 0; i < size; ++i) {
                for (int j = 0; j < size; ++j) {
                    Element el = field[i, j];
                    List<Element> curMatched = matchedElements[i,j];
                    if (el == EMPTY || curMatched.Count < 3) continue;
                    if (isNewBonusElement(el, matchedElements)){
                        ActivateBonus(el);
                        if (curMatched.Count == 4){
                            Element el2 = curMatched.Find(e => e != el);
                            el.SetBonus(el.col - el2.col == 0 ?
                                Element.Bonus.LineVertical : Element.Bonus.LineHorizontal);
                        } else {
                            el.SetBonus(Element.Bonus.Bomb);
                        }
                    } else {
                        DestroyElement(el);
                    }
                }
            }
        }

        private bool MatchExists()
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
        
        private int CalcLine(Point start, Point shift, Element.Type type)
        {
            int count = 0;
            for (Point cur = start; 
                !OutOfRange(cur) && GetElementByPoint(cur).type == type; 
                cur += shift
            )
                ++count;
            return count;
        }

        private List<Element> GetSameElementsInLine(Point start, Point shift, Element.Type type)
        {
            List<Element> line = new List<Element>();
            for (Point cur = start; 
                !OutOfRange(cur) && GetElementByPoint(cur).type == type; 
                cur += shift
            ) 
                line.Add(GetElementByPoint(cur));
            return line;
        }

        private Element GetElementByPosition(Point position)
        {
            return GetElementByPoint((position - leftTop.ToPoint()) / Config.tile.Size);
        }
        private bool OutOfRange(int i)
        {
            return i < 0 || size - 1 < i;
        }

        private bool OutOfRange(Point p)
        {
            return OutOfRange(p.X) || OutOfRange(p.Y);
        }
        private Element GetElementByPoint(Point p)
        {
            if (OutOfRange(p)) return EMPTY;
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
            el.lastMoved = gameTime.TotalGameTime.TotalMilliseconds + moveAnimations * 1.0 / 1000 ;
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
                        if (k >= 0) { //non empty cell found
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
            for (int i = 0; i < events.Count; ++i)
                events[i].Invoke();
            events.Clear();
        }

        private void UpdateBonuses(GameTime gameTime)
        {
            activatedBonuses = activatedBonuses.FindAll((ActivatedBonusBase b) => !b.ended);
            activatedBonuses.ForEach((ActivatedBonusBase b) => b.Update(gameTime));
        }
        public override void Update(GameTime gameTime)
        {
            this.gameTime = gameTime; 
            if (activatedBonuses.Count == 0 && destroyAnimations == 0 && moveAnimations == 0) {
                FindMatchesAndDestroy();
                MakeElementFall();
            }
            if (events.Count == 0)
                UpdateElements(gameTime);
            UpdateBonuses(gameTime);
            UpdateEvents(gameTime);
        }

        private void DrawElements(GameTime gameTime)
        {
            for (int i = 0; i < size; ++i)
                for (int j = 0; j < size; ++j)
                    if (field[i, j] != EMPTY)
                        field[i, j].Draw(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            DrawElements(gameTime);
            activatedBonuses.ForEach((ActivatedBonusBase b) => b.Draw(gameTime));
        }

        public int GetScore()
        {
            return info.score;
        }
        
    }
}
