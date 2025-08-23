using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VisualProgramingProject
{
    public partial class GameForm : Form
    {
        private readonly System.Windows.Forms.Timer timer;
        private readonly List<RectangleF> stack = new();
        private readonly List<Color> colors = new();

        private float moveX;
        private float moveWidth;
        private float moveY;
        private float speed;
        private int dir; // +1 right, -1 left
        private int score;
        private int highScore;
        private bool gameOver;
        private readonly Random rng = new();

        private const float perfectTolerance = 4f;
        private string statusText = "";
        private int statusTimer = 0;

        private bool showInstructions = true;

        public GameForm()
        {
            DoubleBuffered = true;
            Text = "Stack (WinForms)";
            ClientSize = new Size(420, 720);
            BackColor = Color.Black;

            timer = new System.Windows.Forms.Timer { Interval = 16 };
            timer.Tick += (_, __) => TickGame();

            StartNewGame();

            KeyDown += (_, e) =>
            {
                if (e.KeyCode is Keys.Space or Keys.Up or Keys.Return)
                {
                    if (gameOver) { StartNewGame(); return; }
                    PlaceBlock();
                }
                if (e.KeyCode == Keys.R) StartNewGame();
            };

            MouseDown += (_, __) =>
            {
                if (gameOver) { StartNewGame(); return; }
                PlaceBlock();
            };

            Resize += GameForm_Resize;
        }

        private void StartNewGame()
        {
            if (score > highScore) highScore = score;

            stack.Clear();
            colors.Clear();

            float baseHeight = 24f;
            float baseWidth = ClientSize.Width * 0.8f;
            float startX = (ClientSize.Width - baseWidth) / 2f;
            float startY = ClientSize.Height - baseHeight;

            stack.Add(new RectangleF(startX, startY, baseWidth, baseHeight));
            colors.Add(NextColor());

            moveWidth = baseWidth;
            moveY = startY - baseHeight;
            moveX = 0;
            speed = 3.2f;
            dir = +1;
            score = 0;
            gameOver = false;
            statusText = "";
            statusTimer = 0;
            showInstructions = true;

            timer.Start();
            Invalidate();
        }

        private Color NextColor()
        {
            Color[] palette = new Color[]
            {
                Color.Crimson,
                Color.OrangeRed,
                Color.Gold,
                Color.LimeGreen,
                Color.DeepSkyBlue,
                Color.MediumSlateBlue,
                Color.Orchid,
                Color.Coral,
                Color.Chartreuse,
                Color.Aqua,
                Color.DodgerBlue,
                Color.SlateBlue,
                Color.HotPink,
                Color.MediumVioletRed,
                Color.Tomato,
                Color.SpringGreen,
                Color.Turquoise,
                Color.CadetBlue,
                Color.Plum
            };

            return palette[rng.Next(palette.Length)];
        }



        private void TickGame()
        {
            if (gameOver) return;

            moveX += speed * dir;
            if (moveX <= 0)
            {
                moveX = 0;
                dir = +1;
            }
            else if (moveX + moveWidth >= ClientSize.Width)
            {
                moveX = ClientSize.Width - moveWidth;
                dir = -1;
            }

            if (statusTimer > 0)
            {
                statusTimer--;
                if (statusTimer == 0) statusText = "";
            }

            Invalidate();
        }

        private void PlaceBlock()
        {
            if (gameOver) return;

            if (showInstructions) showInstructions = false;

            RectangleF prev = stack.Last();

            float overlapLeft = Math.Max(prev.Left, moveX);
            float overlapRight = Math.Min(prev.Right, moveX + moveWidth);
            float overlapWidth = overlapRight - overlapLeft;

            if (overlapWidth <= 0f)
            {
                gameOver = true;
                timer.Stop();
                Invalidate();
                return;
            }

            float blockHeight = prev.Height;
            var placed = new RectangleF(overlapLeft, moveY, overlapWidth, blockHeight);

            bool isPerfect = Math.Abs(moveX - prev.X) <= perfectTolerance;

            stack.Add(placed);
            colors.Add(NextColor());

            if (isPerfect)
            {
                score += 2;
                statusText = "PERFECT!";
                statusTimer = 25;
            }
            else
            {
                score++;
            }

            moveWidth = overlapWidth;
            moveY -= blockHeight;

            float topY = moveY;
            if (topY < ClientSize.Height * 0.25f)
            {
                float shift = (ClientSize.Height * 0.25f) - topY;
                for (int i = 0; i < stack.Count; i++)
                {
                    var r = stack[i];
                    r.Y += shift;
                    stack[i] = r;
                }
                moveY += shift;
            }

            dir = -dir;
            speed = Math.Min(speed + 0.12f, 12f);
            moveX = dir > 0 ? 0 : Math.Max(0, ClientSize.Width - moveWidth);

            Invalidate();
        }

        private void GameForm_Resize(object? sender, EventArgs e)
        {
            if (stack.Count == 0) return;

            float totalWidth = stack.Last().Right - stack[0].Left;
            float centerX = (ClientSize.Width - totalWidth) / 2f;
            float shiftX = centerX - stack[0].X;

            for (int i = 0; i < stack.Count; i++)
            {
                var r = stack[i];
                r.X += shiftX;
                stack[i] = r;
            }

            moveX += shiftX;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawBackground(g);

            for (int i = 0; i < stack.Count; i++)
            {
                using var b = new SolidBrush(colors[i]);
                g.FillRoundedRect(b, stack[i], 8f);
            }

            if (!gameOver)
            {
                using var b = new SolidBrush(Color.White);
                var movingRect = new RectangleF(moveX, moveY, moveWidth, stack[0].Height);
                g.FillRoundedRect(b, movingRect, 8f);
            }

            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
            using var scoreFont = new Font(FontFamily.GenericSansSerif, 24, FontStyle.Bold);
            using var smallFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
            using var shadow = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
            using var white = new SolidBrush(Color.White);

            g.DrawString(score.ToString(), scoreFont, shadow, new PointF(ClientSize.Width / 2f + 1, 13), sf);
            g.DrawString(score.ToString(), scoreFont, white, new PointF(ClientSize.Width / 2f, 12), sf);

            g.DrawString("Highest Score: " + highScore, smallFont, white, new PointF(ClientSize.Width / 2f, 75), sf);

            // Perfect text
            if (!string.IsNullOrEmpty(statusText))
            {
                using var perfectFont = new Font(FontFamily.GenericSansSerif, 18, FontStyle.Bold);
                g.DrawString(statusText, perfectFont, Brushes.Gold, new PointF(ClientSize.Width / 2f, 100), sf);
            }

            if (gameOver)
            {
                string over = "Game Over";
                string hint = "Press Enter / Click to Restart";
                using var overFont = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold);

                var center = new PointF(ClientSize.Width / 2f, ClientSize.Height / 2f - 20);
                g.DrawString(over, overFont, shadow, new PointF(center.X + 2, center.Y + 2), sf);
                g.DrawString(over, overFont, white, center, sf);

                var center2 = new PointF(ClientSize.Width / 2f, ClientSize.Height / 2f + 18);
                g.DrawString(hint, smallFont, shadow, new PointF(center2.X + 1, center2.Y + 1), sf);
                g.DrawString(hint, smallFont, white, center2, sf);
            }

            // Controls / instructions (only if first move not done)
            if (showInstructions)
            {
                string controls = "Space/Click: Place · R: Restart";
                using var hintBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
                g.DrawString(controls, smallFont, hintBrush, new PointF(10, ClientSize.Height - 400));
            }
        }

        private void DrawBackground(Graphics g)
        {
            var top = Color.FromArgb(255, 25, 28, 45);
            var bottom = Color.FromArgb(255, 58, 66, 86);
            using var lg = new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle, top, bottom, 90f);
            g.FillRectangle(lg, ClientRectangle);
        }

        private void GameForm_Load(object sender, EventArgs e)
        {

        }
    }

    internal static class GraphicsExtensions
    {
        public static void FillRoundedRect(this Graphics g, Brush brush, RectangleF r, float radius)
        {
            using var gp = RoundedRect(r, radius);
            g.FillPath(brush, gp);
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(RectangleF b, float r)
        {
            var gp = new System.Drawing.Drawing2D.GraphicsPath();
            float d = r * 2f;
            gp.AddArc(b.Left, b.Top, d, d, 180, 90);
            gp.AddArc(b.Right - d, b.Top, d, d, 270, 90);
            gp.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
            gp.AddArc(b.Left, b.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}
