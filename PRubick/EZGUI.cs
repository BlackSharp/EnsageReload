using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace PRubick
{
    #region Help classes
    #region Drawer
    internal class Drawer
    {
        #region Fields
        private static Line _line;
        private static Font _font;
        private static EzGui _owner;
        #endregion

        public static void Init(EzGui owner)
        {
            _owner = owner;
            _line = new Line(Drawing.Direct3DDevice9);
            _font = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 15,
                    OutputPrecision = FontPrecision.Outline,
                    Quality = FontQuality.Proof
                });
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        #region Drawing

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Game.IsInGame) return;
            if (Game.IsKeyDown(0x2E)) { _owner.IsMoving = true; }
            else _owner.IsMoving = false;
            if (_owner.IsVisible) _owner.Draw();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            GetFont().OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            GetFont().OnLostDevice();
        }

        #endregion

        #region Methods
        public static void DrawLine(float x1, float y1, float x2, float y2, float w, ColorBGRA color)
        {
            Vector2[] vLine = { new Vector2(x1, y1), new Vector2(x2, y2) };

            _line.GLLines = true;
            _line.Antialias = true;
            _line.Width = w;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();

        }

        public static void DrawFilledBox(float x, float y, float w, float h, ColorBGRA color)
        {
            Vector2[] vLine = new Vector2[2];

            _line.GLLines = true;
            _line.Antialias = false;
            _line.Width = w;

            vLine[0].X = x + w / 2;
            vLine[0].Y = y;
            vLine[1].X = x + w / 2;
            vLine[1].Y = y + h;

            _line.Begin();
            _line.Draw(vLine, color);
            _line.End();
        }

        public static void DrawBox(float x, float y, float w, float h, float px, ColorBGRA color)
        {
            DrawLine(x, y, x + w, y, 1, new ColorBGRA(27, 31, 28, 166));
            DrawLine(x, y, x, y + h, 1, new ColorBGRA(27, 31, 28, 166));
            DrawLine(x, y + h, x + w, y + h, 1, new ColorBGRA(27, 31, 28, 166));
            DrawLine(x + w, y, x + w, y + h, 1, new ColorBGRA(27, 31, 28, 166));
            DrawFilledBox(x, y, w, h, color);
            color.A = 166;
            DrawFilledBox(x, y + h, w, px, color);
            DrawFilledBox(x - px, y, px, h, color);
            DrawFilledBox(x + w, y, px, h, color);
            color.A = 177;
            DrawFilledBox(x, y - px - 10, w, px + 10, color);
        }

        #region DrawText
        public static void DrawShadowText(string text, float x, float y, ColorBGRA color)
        {
            _font.DrawText(null, text, (int)x, (int)y, color);
        }
        #endregion

        public static Font GetFont()
        {
            return _font;
        }
        #endregion
    }
    #endregion
    #region Other
    public enum ElementType
    {
        Checkbox, Text, Category
    }
    public class EzElement
    {
        public ElementType Type;
        private List<EzElement> _inside = new List<EzElement>();
        public string Content;
        public bool IsActive;
        public Entity Attached = null;
        public string Data = null;
        public float[] Position = { 0, 0, 0, 0 };
        public List<EzElement> GetElements()
        {
            return _inside;
        }
        public void AddElement(EzElement element)
        {
            _inside.Add(element);
        }
        public EzElement(ElementType type, string content, bool active)
        {
            Type = type;
            Content = content;
            IsActive = active;
        }
    }
    #endregion
    #endregion
    public class EzGui
    {
        #region Fields
        private float _x;
        private float _y;
        private float _w = 300;
        private float _h = 250;
        private readonly string _title = "EzGUI";

        public bool IsMoving;
        public bool IsVisible = true;

        private int _cachedCount;

        public EzElement Main;
        #endregion

        public EzGui(float x, float y, string title)
        {
            Main = new EzElement(ElementType.Category, "MAIN_CAT", true);
            _x = x;
            _y = y;
            _title = title;
            Drawer.Init(this);
            Game.OnWndProc += Game_OnWndProc;
        }

        #region GameAPI
        void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsInGame)
            {
                switch (args.Msg)
                {
                    case (uint)Utils.WindowsMessages.WM_KEYDOWN:
                        switch (args.WParam)
                        {
                            case 0x24:
                                IsVisible = !IsVisible;
                                break;
                        }
                        break;
                    case (uint)Utils.WindowsMessages.WM_LBUTTONDOWN:
                        MouseClick(Main);
                        break;
                }
            }
        }
        #endregion

        #region Drawing
        public void Draw()
        {
            if (IsMoving)
            {
                Vector2 mPos = Game.MouseScreenPosition;
                _x = mPos.X;
                _y = mPos.Y;
            }
            DrawBase();
            int i = 0;
            int n = 1;
            DrawElements(Main.GetElements(), ref n, ref i);
        }

        public void DrawElements(List<EzElement> category, ref int n, ref int i)
        {
            foreach (EzElement element in category)
            {
                i++;
                DrawElement(element, i, n);
                if (element.Type == ElementType.Category)
                {
                    if (element.IsActive)
                    {
                        int n2 = n + 1;
                        DrawElements(element.GetElements(), ref n2, ref i);
                    }
                }
            }
        }

        public void DrawElement(EzElement element, int i, int incat)
        {
            byte alpha = 60;
            if (element.IsActive) alpha = 255;
            //
            int xoffset = 5 * incat;
            int yoffset = 20;
            int width = 15;
            int height = 15;
            int textoffset = 10;
            int menuoffset = 15;
            int twoxoffset = 18;
            //
            ColorBGRA color = new ColorBGRA(120, 199, 170, alpha);
            //
            element.Position = new[] { _x + xoffset, _x + xoffset + width, _y + yoffset * i - menuoffset, _y + yoffset * i };
            //
            if (MouseIn(element.Position)) color.R = 10;
            //
            switch (element.Type)
            {
                case ElementType.Category:
                    Drawer.DrawFilledBox(element.Position[0], element.Position[2], width, height, color);
                    Drawer.DrawShadowText("> " + element.Content, _x + xoffset + twoxoffset, element.Position[2], new ColorBGRA(199, 199, 199, 255));
                    break;
                case ElementType.Checkbox:
                    Drawer.DrawFilledBox(element.Position[0], element.Position[2], width, height, color);
                    Drawer.DrawShadowText(element.Content, _x + xoffset + twoxoffset, element.Position[2], new ColorBGRA(199, 199, 199, 255));
                    break;
                case ElementType.Text:
                    Drawer.DrawShadowText(element.Content, element.Position[0] + textoffset, element.Position[2], new ColorBGRA(199, 199, 199, 255));
                    break;
            }
        }

        public void DrawBase()
        {
            _h = 5 + (Length() * 20);
            Drawer.DrawBox(_x, _y, _w, _h, 10, new ColorBGRA(32, 32, 32, 125));
            Drawer.DrawShadowText(_title, _x + 3, _y - 15, new ColorBGRA(199, 199, 199, 255));
            Drawer.DrawShadowText("EzGUI • KiKRee", _x + _w - 85, _y + _h - 15, new ColorBGRA(40, 48, 51, 255));
        }
        #endregion

        #region Methods

        public void SetPos(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public void AddMainElement(EzElement en)
        {
            Main.GetElements().Add(en);
        }

        public void Count(EzElement cat, ref int i)
        {
            foreach (EzElement element in cat.GetElements())
            {
                i++;
                if (element.Type == ElementType.Category && element.IsActive) Count(element, ref i);
            }
        }

        public int Length()
        {
            if (Utils.SleepCheck("ezmenu_count"))
            {
                int i = 0;
                Count(Main, ref i);
                _cachedCount = i;
                Utils.Sleep(125, "ezmenu_count");
                return _cachedCount;
            }
            else return _cachedCount;
        }
        #endregion

        #region Events
        public bool MouseIn(float[] pos)
        {
            if (Game.MouseScreenPosition.X >= pos[0] && Game.MouseScreenPosition.X <= pos[1] && Game.MouseScreenPosition.Y >= pos[2] && Game.MouseScreenPosition.Y <= pos[3]) { return true; }
            else return false;
        }

        public void MouseClick(EzElement cat)
        {
            foreach (EzElement element in cat.GetElements())
            {
                bool mouseIn = MouseIn(element.Position);
                if (mouseIn) { element.IsActive = !element.IsActive; return; }
                if (element.Type == ElementType.Category)
                    if (element.IsActive) MouseClick(element);
            }

        }
        #endregion
    }
}