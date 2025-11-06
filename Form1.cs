using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;
using static System.Math;

namespace NonlinearSolver
{
    public partial class Form1 : Form
    {
        private readonly Solver solver;
        private readonly ComplexityCalculator complexityCalculator;
        private readonly EquationValidator validator;
        private ComplexityMetrics lastResult;
        private const int MAX_LINES = 10;
        private const int MIN_LINES = 2;
        private readonly float baseFontSize = 9.0f;
        private bool isUserTyping = false;
        private DateTime lastGenerateTime = DateTime.MinValue;
        private const int COOLDOWN_SECONDS = 5;
        private System.Windows.Forms.Timer countdownTimer;
        private int remainingCooldown;
        private const int MAX_ITERATIONS = 10000;
        private const int MIN_ITERATIONS = 10;

        public Form1()
        {
            InitializeComponent();
            solver = new Solver();
            complexityCalculator = new ComplexityCalculator();
            validator = new EquationValidator(solver, MAX_LINES, MIN_LINES);
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            validator.PrintRules(txtRules);
            btnComplexityCheck.Visible = false;
            btnVisualize.Visible = false;
            validator.UpdateLineCount(txtEquations, lblLineCount);
            txtIterations.Text = "1000";
            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += CountdownTimer_Tick;
            InitializeComboBoxes();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 500);
            this.Resize += Form1_Resize;
            this.FormClosing += Form1_FormClosing;


            txtEquations.Enter += txtEquations_Enter;
            txtEquations.Leave += txtEquations_Leave;
            txtEquations.KeyDown += txtEquations_KeyDown;
            txtEquations.KeyUp += txtEquations_KeyUp;
            txtEquations.TextChanged += txtEquations_TextChanged;
            txtIterations.KeyPress += txtIterations_KeyPress;
            txtIterations.Leave += txtIterations_Leave;
        }

        private void InitializeComboBoxes()
        {
            cmbMethod.Items.Clear();
            cmbEpsilon.Items.Clear();
            cmbMethod.Items.Add("Newton Method");
            cmbMethod.Items.Add("Secant Method");
            cmbMethod.SelectedIndex = 0;
            cmbEpsilon.Items.Add("1e-2 (0.01)");
            cmbEpsilon.Items.Add("1e-3 (0.001)");
            cmbEpsilon.Items.Add("1e-4 (0.0001)");
            cmbEpsilon.Items.Add("1e-5 (0.00001)");
            cmbEpsilon.Items.Add("1e-6 (0.000001)");
            cmbEpsilon.Items.Add("1e-7 (0.0000001)");
            cmbEpsilon.Items.Add("1e-8 (0.00000001)");
            cmbEpsilon.SelectedIndex = 2;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CleanupResources();
        }

        private void CleanupResources()
        {
            countdownTimer?.Stop();
            countdownTimer?.Dispose();
            lastResult?.Dispose();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            remainingCooldown--;
            if (remainingCooldown > 0)
            {
                btnGenerate.Text = $"Wait {remainingCooldown}s";
            }
            else
            {
                btnGenerate.Text = "Generate";
                btnGenerate.Enabled = true;
                countdownTimer.Stop();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            AdjustFontSize();
        }

        private void AdjustFontSize()
        {
            if (this.WindowState == FormWindowState.Minimized)
                return;
            float scaleFactor = Min(Width / 800f, Height / 400f);
            float newFontSize = baseFontSize * Max(1.0f, scaleFactor);
            newFontSize = Max(8.0f, Min(14.0f, newFontSize));
            ApplyFontSizeToControls(newFontSize);
        }

        private void ApplyFontSizeToControls(float fontSize)
        {
            using (Font newFont = new Font("Microsoft Sans Serif", fontSize, FontStyle.Regular))
            {
                SetControlFont(this, newFont);
            }
        }

        private void SetControlFont(Control control, Font font)
        {
            control.Font = font;
            foreach (Control child in control.Controls)
            {
                SetControlFont(child, font);
            }
        }
        private void txtEquations_Enter(object sender, EventArgs e)
        {
            isUserTyping = true;
        }

        private void txtEquations_Leave(object sender, EventArgs e)
        {
            isUserTyping = false;
            validator.HandleTxtEquationsLeave(txtEquations, ref isUserTyping);
        }

        private void txtEquations_KeyDown(object sender, KeyEventArgs e)
        {
            validator.HandleTxtEquationsKeyDown(txtEquations, e, ref isUserTyping);
        }

        private void txtEquations_KeyUp(object sender, KeyEventArgs e)
        {
            validator.HandleTxtEquationsKeyUp(txtEquations, e, ref isUserTyping);
        }

        private void txtEquations_TextChanged(object sender, EventArgs e)
        {
            validator.HandleTxtEquationsTextChanged(txtEquations, ref isUserTyping, lblLineCount);
        }

        private void txtIterations_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtIterations_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtIterations.Text))
            {
                if (int.TryParse(txtIterations.Text, out int iterations))
                {
                    if (iterations < MIN_ITERATIONS || iterations > MAX_ITERATIONS)
                    {
                        validator.ShowWarningMessage($"Iterations must be between {MIN_ITERATIONS} and {MAX_ITERATIONS}.");
                        txtIterations.Text = "1000";
                    }
                }
                else
                {
                    validator.ShowWarningMessage("Please enter a valid integer number for iterations.");
                    txtIterations.Text = "1000";
                }
            }
        }

        private int GetSelectedIterations()
        {
            if (string.IsNullOrWhiteSpace(txtIterations.Text))
                return 1000;
            if (int.TryParse(txtIterations.Text, out int iterations) &&
                iterations >= MIN_ITERATIONS && iterations <= MAX_ITERATIONS)
            {
                return iterations;
            }
            validator.ShowWarningMessage($"Iterations must be an integer between {MIN_ITERATIONS} and {MAX_ITERATIONS}. Using default value of 1000.");
            txtIterations.Text = "1000";
            return 1000;
        }

        private double GetSelectedEpsilon()
        {
            string selectedText = cmbEpsilon.SelectedItem?.ToString() ?? "1e-4 (0.0001)";
            return selectedText switch
            {
                string s when s.Contains("1e-2") => 1e-2,
                string s when s.Contains("1e-3") => 1e-3,
                string s when s.Contains("1e-4") => 1e-4,
                string s when s.Contains("1e-5") => 1e-5,
                string s when s.Contains("1e-6") => 1e-6,
                string s when s.Contains("1e-7") => 1e-7,
                string s when s.Contains("1e-8") => 1e-8,
                _ => 1e-4
            };
        }

        private string FormatNumber(double value, double epsilon)
        {
            int digits = Max(2, (int)(-Log10(epsilon)));
            digits = Min(10, digits);
            return value.ToString($"F{digits}", CultureInfo.InvariantCulture);
        }

        private void DisableAllButtons()
        {
            SetButtonsEnabled(false);
        }

        private void EnableAllButtons()
        {
            SetButtonsEnabled(true);
        }

        private void SetButtonsEnabled(bool enabled)
        {
            btnSolve.Enabled = enabled;
            btnClear.Enabled = enabled;
            btnClearField.Enabled = enabled;
            btnComplexityCheck.Enabled = enabled;
            btnSave.Enabled = enabled;
            btnVisualize.Enabled = enabled;
        }

        public void HideComplexityButtons()
        {
            btnComplexityCheck.Visible = false;
            btnVisualize.Visible = false;
            lastResult = null;
        }

        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            if (IsInCooldownPeriod())
                return;
            await GenerateEquationsAsync();
        }

        private bool IsInCooldownPeriod()
        {
            TimeSpan timeSinceLast = DateTime.Now - lastGenerateTime;
            if (timeSinceLast.TotalSeconds < COOLDOWN_SECONDS)
            {
                remainingCooldown = COOLDOWN_SECONDS - (int)timeSinceLast.TotalSeconds;
                btnGenerate.Text = $"Wait {remainingCooldown}s";
                btnGenerate.Enabled = false;
                countdownTimer.Start();
                return true;
            }
            return false;
        }

        private async Task GenerateEquationsAsync()
        {
            DisableAllButtons();
            btnGenerate.Enabled = false;
            btnGenerate.Text = "Generating...";
            try
            {
                await Task.Run(() =>
                {
                    var equations = solver.GenerateRandomSystem(new Random().Next(MIN_LINES, MAX_LINES + 1));
                    Invoke(new Action(() => UpdateUIWithGeneratedEquations(equations)));
                });
            }
            catch (Exception ex)
            {
                validator.ShowErrorMessage(ex.Message);
            }
            finally
            {
                CompleteGeneration();
            }
        }

        private void UpdateUIWithGeneratedEquations(string[] equations)
        {
            txtEquations.Text = string.Join(Environment.NewLine, equations);
            txtResult.Clear();
            HideComplexityButtons();
            validator.UpdateLineCount(txtEquations, lblLineCount);
        }

        private void CompleteGeneration()
        {
            lastGenerateTime = DateTime.Now;
            EnableAllButtons();
            btnGenerate.Enabled = true;
            btnGenerate.Text = "Generate";
            countdownTimer.Stop();
        }

        private async void btnSolve_Click(object sender, EventArgs e)
        {
            await SolveEquationsAsync();
        }

        private async Task SolveEquationsAsync()
        {
            try
            {
                string[] equations = validator.GetValidatedEquations(txtEquations);
                if (equations.Length == 0) return;
                if (!validator.ValidateEquationSystem(equations))
                    return;
                await SolveEquationSystem(equations);
            }
            catch (Exception ex)
            {
                validator.HandleSolvingError(ex, txtResult, this);
            }
        }

        private async Task SolveEquationSystem(string[] equations)
        {
            int maxIterations = GetSelectedIterations();
            double epsilon = GetSelectedEpsilon();
            double[] x0 = solver.GenerateInitialGuess(equations.Length);
            string method = cmbMethod.SelectedItem?.ToString() ?? "Newton Method";
            txtResult.Text = $"Solving {equations.Length}-equation system using {method}...\r\n";
            txtResult.AppendText("Progress: Initializing...\r\n");
            Application.DoEvents();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            try
            {
                string methodKey = method.ToLower().Contains("newton") ? "newton" : "secant";
                lastResult = await Task.Run(() =>
                {
                    return solver.Solve(equations, x0, epsilon, methodKey, maxIterations);
                }, cts.Token);
                Invoke(new Action(() => DisplaySolution(lastResult, epsilon, method)));
            }
            catch (OperationCanceledException)
            {
                Invoke(new Action(() =>
                {
                    txtResult.AppendText("\r\nTimeout: Computation took too long (>2 min). Suggestions:\r\n");
                    txtResult.AppendText("- Reduce iterations (e.g., 200)\r\n");
                    txtResult.AppendText("- Increase epsilon (e.g., 1e-3)\r\n");
                    txtResult.AppendText("- Use Secant Method (fewer computations)\r\n");
                    txtResult.AppendText("- Try smaller system (n<8)\r\n");
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() => validator.HandleSolvingError(ex, txtResult, this)));
            }
            finally
            {
                cts.Dispose();
            }
        }

        private void DisplaySolution(ComplexityMetrics result, double epsilon, string method)
        {
            txtResult.Clear();
            txtResult.AppendText($"Method: {method}\r\n");
            txtResult.AppendText($"Accuracy (ε): {cmbEpsilon.SelectedItem}\r\n");
            txtResult.AppendText("\r\nSOLUTION:\r\n");
            for (int i = 0; i < result.Solution.Length; i++)
            {
                txtResult.AppendText($"x{i + 1} = {FormatNumber(result.Solution[i], epsilon)}\r\n");
            }
            btnComplexityCheck.Visible = true;
            btnVisualize.Visible = true;
            txtResult.ScrollToCaret();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtEquations.Clear();
            txtResult.Clear();
            cmbMethod.SelectedIndex = 0;
            cmbEpsilon.SelectedIndex = 2;
            txtIterations.Text = "1000";
            HideComplexityButtons();
            validator.UpdateLineCount(txtEquations, lblLineCount);
        }

        private void btnClearField_Click(object sender, EventArgs e)
        {
            txtResult.Clear();
            HideComplexityButtons();
        }

        private void btnComplexityCheck_Click(object sender, EventArgs e)
        {
            int maxIterations = GetSelectedIterations();
            string method = cmbMethod.SelectedItem?.ToString() ?? "Newton Method";
            validator.PerformComplexityCheck(txtEquations, maxIterations, method, complexityCalculator);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            validator.SaveResults(txtResult);
        }

        private void btnVisualize_Click(object sender, EventArgs e)
        {
            if (lastResult == null)
            {
                validator.ShowWarningMessage("No solution available. Solve the system first.");
                return;
            }
            double epsilon = GetSelectedEpsilon();
            validator.ShowVisualization(txtEquations, lastResult.Solution, epsilon);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustLayout();
        }

        private void AdjustLayout()
        {
            if (WindowState == FormWindowState.Minimized || Width <= 800)
                return;
            int leftPanelWidth = (Width - 60) / 2;
            txtEquations.Width = leftPanelWidth;
            txtResult.Width = leftPanelWidth;
            txtRules.Width = leftPanelWidth;
            txtRules.Left = Width - txtRules.Width - 20;
            btnSave.Left = Width - btnSave.Width - 20;
        }
    }
}