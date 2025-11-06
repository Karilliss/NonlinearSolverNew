using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static System.Math;
namespace NonlinearSolver
{
    public class EquationValidator
    {
        private readonly Solver solver;
        private readonly int maxLines;
        private readonly int minLines;
        public EquationValidator(Solver solver, int maxLines, int minLines)
        {
            this.solver = solver;
            this.maxLines = maxLines;
            this.minLines = minLines;
        }
        public void PrintRules(TextBox txtRules)
        {
            txtRules.Text =
@"RULES FOR WRITING EQUATIONS:
1. Use variables: x1, x2, x3, ….
2. EACH EQUATION MUST CONTAIN AT LEAST 2 VARIABLES
3. Each equation must be on a separate line.
4. The program does not support trigonometric expressions.
5. Operators: +, -, *
6. Format: expression = 0
7. Explicit multiplication: x1*x2 (NOT x1x2)
8. 2-10 equations = 2-10 variables
9. Min2 equations, max 10 equations.
10. Iterations: 10-10000
Newton: Needs differentiable functions
Secant: Works with continuous functions";
        }
        public void UpdateLineCount(TextBox txtEquations, Label lblLineCount)
        {
            int lineCount = GetLineCount(txtEquations);
            lblLineCount.Text = $"Lines: {lineCount}/{maxLines}";
            if (lineCount > maxLines)
                lblLineCount.ForeColor = System.Drawing.Color.Red;
            else if (lineCount == maxLines)
                lblLineCount.ForeColor = System.Drawing.Color.Orange;
            else if (lineCount < minLines)
                lblLineCount.ForeColor = System.Drawing.Color.Blue;
            else
                lblLineCount.ForeColor = System.Drawing.Color.Gray;
        }
        private int GetLineCount(TextBox txtEquations)
        {
            return string.IsNullOrEmpty(txtEquations.Text)
                ? 0
                : txtEquations.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
        public void EnsureSeparateLines(TextBox txtEquations, bool isUserTyping)
        {
            if (isUserTyping || string.IsNullOrEmpty(txtEquations.Text))
                return;
            string cleanedText = string.Join(Environment.NewLine,
                txtEquations.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
            if (cleanedText != txtEquations.Text)
            {
                int cursorPosition = txtEquations.SelectionStart;
                txtEquations.Text = cleanedText;
                txtEquations.SelectionStart = Min(cursorPosition, txtEquations.Text.Length);
            }
        }
        public void HandleTxtEquationsLeave(TextBox txtEquations, ref bool isUserTyping)
        {
            isUserTyping = false;
            EnsureSeparateLines(txtEquations, isUserTyping);
        }
        public void HandleTxtEquationsKeyDown(TextBox txtEquations, KeyEventArgs e, ref bool isUserTyping)
        {
            isUserTyping = true;
            if (e.KeyCode == Keys.Enter && GetLineCount(txtEquations) >= maxLines)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ShowWarningMessage($"Maximum {maxLines} equations allowed. Cannot add more lines.");
            }
        }
        public void HandleTxtEquationsKeyUp(TextBox txtEquations, KeyEventArgs e, ref bool isUserTyping)
        {
            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Enter)
            {
                isUserTyping = false;
                EnsureSeparateLines(txtEquations, isUserTyping);
            }
            else
            {
                isUserTyping = false;
            }
        }
        public void HandleTxtEquationsTextChanged(TextBox txtEquations, ref bool isUserTyping, Label lblLineCount)
        {
            UpdateLineCount(txtEquations, lblLineCount);
            if (!isUserTyping)
            {
                int lineCount = GetLineCount(txtEquations);
                if (lineCount > maxLines)
                {
                    TrimExcessLines(txtEquations);
                }
            }
        }
        private void TrimExcessLines(TextBox txtEquations)
        {
            if (txtEquations.Text.Length == 0) return;
            int cursorPosition = txtEquations.SelectionStart;
            string[] lines = txtEquations.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > maxLines)
            {
                txtEquations.Text = string.Join(Environment.NewLine, lines.Take(maxLines));
                txtEquations.SelectionStart = Min(cursorPosition, txtEquations.Text.Length);
                ShowWarningMessage($"Maximum {maxLines} equations allowed. Extra equations have been removed.");
            }
        }
        public string[] GetValidatedEquations(TextBox txtEquations)
        {
            return txtEquations.Text
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line.Trim()))
                .ToArray();
        }
        public bool ValidateEquationSystem(string[] equations)
        {
            if (equations.Length < minLines)
            {
                ShowWarningMessage($"Minimum {minLines} equations required.");
                return false;
            }
            if (equations.Length > maxLines)
            {
                ShowWarningMessage($"Maximum {maxLines} equations allowed.");
                return false;
            }
            return ValidateEquationsContainVariables(equations) &&
                   ValidateEquationsAreValid(equations);
        }
        private bool ValidateEquationsContainVariables(string[] equations)
        {
            foreach (string line in equations)
            {
                string trimmedLine = line.Trim().ToLower();
                HashSet<string> foundVars = new HashSet<string>();
                for (int i = 1; i <= 10; i++)
                {
                    if (trimmedLine.Contains($"x{i}"))
                    {
                        foundVars.Add($"x{i}");
                    }
                }
                if (foundVars.Count < 2)
                {
                    ShowWarningMessage("Each equation must contain at least two variables");
                    return false;
                }
            }
            return true;
        }
        private bool ValidateEquationsAreValid(string[] equations)
        {
            foreach (string line in equations)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;
                if (!trimmedLine.EndsWith("=0") && !trimmedLine.EndsWith("= 0"))
                {
                    ShowWarningMessage($"Equation '{trimmedLine}' must end with '= 0'.\nFormat: expression = 0");
                    return false;
                }
                if (!TestEquationSyntax(trimmedLine, equations.Length))
                {
                    return false;
                }
            }
            return true;
        }
        private bool TestEquationSyntax(string equation, int nVars)
        {
            try
            {
                double[] testSolution = new double[nVars];
                for (int i = 0; i < nVars; i++)
                    testSolution[i] = 1.0;
                string expr = equation.Split('=')[0].Replace(" ", "").Replace(",", ".");
                solver.FastEvaluate(expr, testSolution);
                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error in equation: '{equation}'\n{ex.Message}");
                return false;
            }
        }
        public void PerformComplexityCheck(TextBox txtEquations, int maxIterations, string method, ComplexityCalculator calculator)
        {
            try
            {
                string[] equations = GetValidatedEquations(txtEquations);
                if (equations.Length == 0)
                {
                    ShowWarningMessage("Please enter equations first.");
                    return;
                }
                if (equations.Length < 2)
                {
                    ShowWarningMessage("Minimum 2 equations required for analysis.");
                    return;
                }
                var complexityMetrics = calculator.ComputeTimeComplexity(equations, maxIterations, method);
                string report = calculator.GenerateComplexityReport(complexityMetrics);
                MessageBox.Show(report, "Time and Space Complexity Analysis",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Complexity analysis error: {ex.Message}");
            }
        }
        public void HandleSolvingError(Exception ex, TextBox txtResult, Form1 form)
        {
            ShowErrorMessage($"Solving failed: {ex.Message}");
            txtResult.Text = $"ERROR: {ex.Message}";
            form.HideComplexityButtons(); 
        }
        public void SaveResults(TextBox txtResult)
        {
            if (string.IsNullOrWhiteSpace(txtResult.Text))
            {
                ShowInfoMessage("No results to save.");
                return;
            }
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text files (*.txt)|*.txt";
                sfd.Title = "Save Results";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, txtResult.Text);
                        ShowInfoMessage("Results saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage("Error saving file: " + ex.Message);
                    }
                }
            }
        }
        public void ShowVisualization(TextBox txtEquations, double[] solution, double epsilon)
        {
            try
            {
                string[] equations = GetValidatedEquations(txtEquations);
                using (var visualizationForm = new VisualizationForm(equations, solution, epsilon))
                {
                    visualizationForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Visualization failed: {ex.Message}");
            }
        }
        public void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}