using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Globalization;
using System.Drawing.Drawing2D;
namespace NonlinearSolver
{
    public class VisualizationForm : Form
    {
        private string[] equations;
        private double[] solution;
        private PictureBox pictureBox;
        private Button btnClose;
        private Button btnFullscreen;
        private Label lblInfo;
        private bool isFullscreen = false;
        private FormWindowState previousWindowState;
        private double epsilon;
        private readonly Size fixedSize = new Size(800, 600);
        private System.Windows.Forms.Timer animationTimer; 
        private double animationProgress = 0.0;
        private const int ANIMATION_DURATION = 1000;
        private DateTime animationStartTime;
        public VisualizationForm(string[] equations, double[] solution, double epsilon)
        {
            this.equations = equations;
            this.solution = solution;
            this.epsilon = epsilon;
            InitializeComponent();
            this.Text = "Solution Visualization";
            this.Size = fixedSize;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;
            this.KeyDown += VisualizationForm_KeyDown;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            InitializeAnimation();
            StartAnimation();
        }
        private void InitializeAnimation()
        {
            animationTimer = new System.Windows.Forms.Timer(); 
            animationTimer.Interval = 50; 
            animationTimer.Tick += AnimationTimer_Tick;
        }
        private void StartAnimation()
        {
            animationProgress = 0.0;
            animationStartTime = DateTime.Now;
            animationTimer.Start();
            pictureBox.Invalidate();
        }
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            double elapsed = (DateTime.Now - animationStartTime).TotalMilliseconds;
            animationProgress = Math.Min(1.0, elapsed / ANIMATION_DURATION);
            pictureBox.Invalidate();
            if (animationProgress >= 1.0)
            {
                animationTimer.Stop();
            }
        }
        private void InitializeComponent()
        {
            lblInfo = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(760, 40),
                Text = $"Solution Visualization for {solution.Length}-variable system",
                Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pictureBox = new PictureBox
            {
                Location = new Point(10, 60),
                Size = new Size(760, 450),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pictureBox.Paint += PictureBox_Paint;
            btnClose = new Button
            {
                Location = new Point(300, 520),
                Size = new Size(100, 30),
                Text = "Close",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular)
            };
            btnClose.Click += (s, e) => this.Close();
            btnFullscreen = new Button
            {
                Location = new Point(410, 520),
                Size = new Size(120, 30),
                Text = "Fullscreen (F11)",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular)
            };
            btnFullscreen.Click += BtnFullscreen_Click;
            this.Controls.Add(lblInfo);
            this.Controls.Add(pictureBox);
            this.Controls.Add(btnClose);
            this.Controls.Add(btnFullscreen);
        }
        private string FormatNumber(double value)
        {
            int digits = Math.Max(2, (int)(-Math.Log10(epsilon)));
            digits = Math.Min(10, digits);
            return value.ToString($"F{digits}", CultureInfo.InvariantCulture);
        }
        private void VisualizationForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullscreen();
            }
            else if (e.KeyCode == Keys.Escape && isFullscreen)
            {
                ToggleFullscreen();
            }
        }
        private void BtnFullscreen_Click(object sender, EventArgs e)
        {
            ToggleFullscreen();
        }
        private void ToggleFullscreen()
        {
            if (!isFullscreen)
            {
                previousWindowState = this.WindowState;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                btnClose.Visible = false;
                btnFullscreen.Visible = false;
                lblInfo.Visible = false;
                isFullscreen = true;
                pictureBox.Location = new Point(0, 0);
                pictureBox.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);
                pictureBox.BorderStyle = BorderStyle.None;
                StartAnimation(); 
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;
                this.Size = fixedSize;
                this.StartPosition = FormStartPosition.CenterScreen;
                btnClose.Visible = true;
                btnFullscreen.Visible = true;
                lblInfo.Visible = true;
                isFullscreen = false;
                pictureBox.Location = new Point(10, 60);
                pictureBox.Size = new Size(760, 450);
                pictureBox.BorderStyle = BorderStyle.FixedSingle;
                StartAnimation();
            }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (isFullscreen)
            {
                pictureBox.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);
                pictureBox.Invalidate();
            }
            else if (this.WindowState == FormWindowState.Normal && this.Size != fixedSize)
            {
                this.Size = fixedSize;
            }
        }
        protected override void OnResizeBegin(EventArgs e)
        {
            if (!isFullscreen)
            {
                e = EventArgs.Empty;
                return;
            }
            base.OnResizeBegin(e);
        }
        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int width = pictureBox.Width;
            int height = pictureBox.Height;
            int margin = isFullscreen ? 80 : 50;
            if (!isFullscreen)
            {
                using (Font titleFont = new Font("Arial", 12, FontStyle.Bold))
                using (Brush titleBrush = new SolidBrush(Color.Blue))
                {
                    g.DrawString("Solution Values Visualization", titleFont, titleBrush, width / 2 - 120, 10);
                }
            }
            DrawStatistics(g, width, height);
            DrawMultiDVisualization(g, width, height, margin);
            if (isFullscreen)
            {
                using (Font hintFont = new Font("Arial", 10, FontStyle.Italic))
                using (Brush hintBrush = new SolidBrush(Color.Gray))
                {
                    string hintText = "Press ESC to exit fullscreen";
                    SizeF textSize = g.MeasureString(hintText, hintFont);
                    g.DrawString(hintText, hintFont, hintBrush, width - textSize.Width - 20, 20);
                }
            }
        }
        private void DrawStatistics(Graphics g, int width, int height)
        {
            int topMargin = isFullscreen ? 50 : 40;
            int lineHeight = isFullscreen ? 25 : 20;
            Font statsFont = isFullscreen ? new Font("Arial", 12, FontStyle.Bold) : new Font("Arial", 10, FontStyle.Bold);
            using (Brush statsBrush = new SolidBrush(Color.Black))
            {
                int positiveCount = solution.Count(x => x >= 0);
                int negativeCount = solution.Count(x => x < 0);
                double maxPositive = solution.Where(x => x >= 0).DefaultIfEmpty(0).Max();
                double minNegative = solution.Where(x => x < 0).DefaultIfEmpty(0).Min();
                string statsText = $"Variables: {solution.Length} | Positive: {positiveCount} | Negative: {negativeCount}";
                g.DrawString(statsText, statsFont, statsBrush, 20, topMargin);
                string valuesText = "";
                if (positiveCount > 0 && negativeCount > 0)
                {
                    valuesText = $"Max positive: {FormatNumber(maxPositive)} | Min negative: {FormatNumber(minNegative)}";
                }
                else if (positiveCount > 0)
                {
                    valuesText = $"Max positive: {FormatNumber(maxPositive)}";
                }
                else if (negativeCount > 0)
                {
                    valuesText = $"Min negative: {FormatNumber(minNegative)}";
                }
                if (!string.IsNullOrEmpty(valuesText))
                {
                    g.DrawString(valuesText, statsFont, statsBrush, 20, topMargin + lineHeight);
                }
            }
            if (isFullscreen)
            {
                statsFont.Dispose();
            }
        }
        private void DrawMultiDVisualization(Graphics g, int width, int height, int margin)
        {
            int adjustedMargin = margin + (isFullscreen ? 50 : 30);
            int barWidth = Math.Max(isFullscreen ? 30 : 20, (width - 2 * adjustedMargin) / (solution.Length * 2));
            int maxBarHeight = (height - 2 * adjustedMargin - (isFullscreen ? 100 : 80)) / 2;
            int zeroLineY = height - adjustedMargin - maxBarHeight;
            double maxPositive = solution.Where(x => x >= 0).DefaultIfEmpty(0).Max();
            double maxNegative = Math.Abs(solution.Where(x => x < 0).DefaultIfEmpty(0).Min());
            double maxVal = Math.Max(maxPositive, maxNegative);
            if (maxVal == 0) maxVal = 1;
            using (Pen zeroPen = new Pen(Color.Black, isFullscreen ? 3 : 2))
            {
                g.DrawLine(zeroPen, adjustedMargin, zeroLineY, width - adjustedMargin, zeroLineY);
                using (Font zeroFont = new Font("Arial", isFullscreen ? 11 : 9))
                using (Brush zeroBrush = new SolidBrush(Color.Black))
                {
                    g.DrawString("0", zeroFont, zeroBrush, adjustedMargin - 20, zeroLineY - 10);
                }
            }
            for (int i = 0; i < solution.Length; i++)
            {
                int x = adjustedMargin + (i * 2 + 1) * barWidth;
                double value = solution[i];
                string varName = $"x{i + 1}";
                double animatedHeightFactor = Math.Abs(value) / maxVal * maxBarHeight * animationProgress;
                int barHeight = (int)animatedHeightFactor;
                if (value >= 0)
                {
                    int y = zeroLineY - barHeight;
                    using (Brush barBrush = new SolidBrush(Color.Green))
                    {
                        g.FillRectangle(barBrush, x, y, barWidth, barHeight);
                    }
                    using (Font valueFont = new Font("Arial", isFullscreen ? 10 : 8))
                    using (Brush valueBrush = new SolidBrush(Color.DarkGreen))
                    {
                        string valueStr = $"{varName}={FormatNumber(value)}";
                        SizeF textSize = g.MeasureString(valueStr, valueFont);
                        g.DrawString(valueStr, valueFont, valueBrush,
                            x + (barWidth - textSize.Width) / 2, y - (isFullscreen ? 35 : 25));
                    }
                }
                else
                {
                    int y = zeroLineY;
                    using (Brush barBrush = new SolidBrush(Color.Red))
                    {
                        g.FillRectangle(barBrush, x, y, barWidth, barHeight);
                    }
                    using (Font valueFont = new Font("Arial", isFullscreen ? 10 : 8))
                    using (Brush valueBrush = new SolidBrush(Color.DarkRed))
                    {
                        string valueStr = $"{varName}={FormatNumber(value)}";
                        SizeF textSize = g.MeasureString(valueStr, valueFont);
                        g.DrawString(valueStr, valueFont, valueBrush,
                            x + (barWidth - textSize.Width) / 2, y + barHeight + (isFullscreen ? 10 : 5));
                    }
                }
                using (Font varFont = new Font("Arial", isFullscreen ? 11 : 9, FontStyle.Bold))
                using (Brush varBrush = new SolidBrush(Color.Blue))
                {
                    SizeF textSize = g.MeasureString(varName, varFont);
                    g.DrawString(varName, varFont, varBrush,
                        x + (barWidth - textSize.Width) / 2, zeroLineY + maxBarHeight + (isFullscreen ? 25 : 15));
                }
            }
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            animationTimer?.Stop();
            animationTimer?.Dispose();
            pictureBox?.Dispose();
            btnClose?.Dispose();
            btnFullscreen?.Dispose();
            lblInfo?.Dispose();
        }
    }
}