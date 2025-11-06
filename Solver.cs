using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using static System.Math;

namespace NonlinearSolver
{
    public class Solver
    {
        public delegate double FunctionEval(double[] x);
        private readonly Random rand;

        public Solver()
        {
            rand = new Random();
        }

        public Solver(int seed)
        {
            rand = new Random(seed);
        }

        private ISolverMethod CreateSolverMethod(string method)
        {
            return method.ToLower() switch
            {
                "newton" => new NewtonMethod(this),
                "secant" => new SecantMethod(this),
                _ => throw new ArgumentException("Method must be 'newton' or 'secant'")
            };
        }

        public ComplexityMetrics Solve(string[] equations, double[] x0, double epsilon,
                                      string method = "newton", int maxIterations = 1000)
        {
            ValidateInputs(equations, x0, epsilon, maxIterations);
            ValidateNoTrigonometricExpressions(equations);
            var functionEvaluators = new FunctionEvaluators(equations, this);
            ISolverMethod solverMethod = CreateSolverMethod(method);
            return solverMethod.Solve(functionEvaluators.Functions, x0, epsilon, maxIterations);
        }

        public string[] GenerateRandomSystem(int n)
        {
            ValidateEquationCount(n);
            var equations = new string[n];
            var trueSolution = GenerateTrueSolution(n);
            for (int i = 0; i < n; i++)
            {
                equations[i] = GenerateSingleEquation(i, n, trueSolution);
            }
            return equations;
        }

        private void ValidateEquationCount(int n)
        {
            if (n < 2 || n > 10)
                throw new ArgumentException("Number of equations must be between 2 and 10.");
        }

        private double[] GenerateTrueSolution(int n)
        {
            var trueSolution = new double[n];
            for (int i = 0; i < n; i++)
                trueSolution[i] = (rand.NextDouble() * 4) - 2;
            return trueSolution;
        }

        private string GenerateSingleEquation(int eqIndex, int n, double[] trueSolution)
        {
            var equationBuilder = new EquationBuilder();
            double constant = equationBuilder.BuildEquation(eqIndex, n, trueSolution, rand);
            return equationBuilder.GetEquation() + " = 0";
        }

        private class EquationBuilder
        {
            private readonly StringBuilder equation = new StringBuilder();
            private double constant = 0;

            public double BuildEquation(int eqIndex, int n, double[] trueSolution, Random rand)
            {
                constant = 0;
                equation.Clear();
                constant += AddLinearTerm(eqIndex, trueSolution, rand);
                constant += AddCrossTerm(eqIndex, n, trueSolution, rand);
                constant += AddQuadraticTerm(eqIndex, trueSolution, rand);
                AddConstantTerm(rand);
                return constant;
            }

            private double AddLinearTerm(int eqIndex, double[] trueSolution, Random rand)
            {
                double a = (rand.NextDouble() * 3 + 0.5) * (rand.Next(2) == 0 ? 1 : -1);
                equation.Append($"{FormatNumberForEquation(a)}*{GetVarName(eqIndex)}");
                return a * trueSolution[eqIndex];
            }

            private double AddCrossTerm(int eqIndex, int n, double[] trueSolution, Random rand)
            {
                int j = GetRandomDifferentIndex(eqIndex, n, rand);
                double b = (rand.NextDouble() * 2 - 1) * 1.5;
                equation.Append($" + {FormatNumberForEquation(b)}*{GetVarName(eqIndex)}*{GetVarName(j)}");
                return b * trueSolution[eqIndex] * trueSolution[j];
            }

            private int GetRandomDifferentIndex(int currentIndex, int n, Random rand)
            {
                int j;
                do
                {
                    j = rand.Next(n);
                } while (j == currentIndex);
                return j;
            }

            private double AddQuadraticTerm(int eqIndex, double[] trueSolution, Random rand)
            {
                double d = rand.NextDouble() * 2;
                equation.Append($" + {FormatNumberForEquation(d)}*{GetVarName(eqIndex)}*{GetVarName(eqIndex)}");
                return d * trueSolution[eqIndex] * trueSolution[eqIndex];
            }

            private void AddConstantTerm(Random rand)
            {
                double randomConstant = -constant + (rand.NextDouble() * 0.4 - 0.2);
                string formattedConstant = FormatNumberForEquation(randomConstant);
                if (randomConstant < 0)
                {
                    equation.Append($" - {FormatNumberForEquation(Math.Abs(randomConstant))}");
                }
                else
                {
                    equation.Append($" + {formattedConstant}");
                }
            }

            public string GetEquation() => equation.ToString();
        }

        private static string FormatNumberForEquation(double value)
        {
            Random tempRand = new Random();
            int decimals = tempRand.Next(2, 8);
            string formatted = value.ToString($"F{decimals}", CultureInfo.InvariantCulture);
            return value < 0 ? $"({formatted})" : formatted;
        }

        public double[] GenerateInitialGuess(int n)
        {
            var guess = new double[n];
            for (int i = 0; i < n; i++)
                guess[i] = (rand.NextDouble() * 1.5) - 0.75;
            return guess;
        }

        private class FunctionEvaluators
        {
            public FunctionEval[] Functions { get; }
            private readonly Solver solver;

            public FunctionEvaluators(string[] equations, Solver solver)
            {
                this.solver = solver;
                Functions = ParseEquations(equations);
            }

            private FunctionEval[] ParseEquations(string[] equations)
            {
                int n = equations.Length;
                FunctionEval[] f = new FunctionEval[n];
                for (int i = 0; i < n; i++)
                {
                    string expr = equations[i].Split('=')[0].Replace(" ", "").Replace(",", ".");
                    f[i] = x => solver.FastEvaluate(expr, x);
                }
                return f;
            }
        }

        public double FastEvaluate(string expr, double[] x)
        {
            string remaining = expr.Replace(" ", "");
            double result = 0.0;
            int i = 0;
            double currentSign = 1.0;
            if (i < remaining.Length)
            {
                if (remaining[i] == '-')
                {
                    currentSign = -1.0;
                    i++;
                }
                else if (remaining[i] == '+')
                {
                    i++;
                }
            }
            while (i < remaining.Length)
            {
                double term = ParseTerm(ref i, remaining, x);
                result += currentSign * term;
                if (i < remaining.Length && (remaining[i] == '+' || remaining[i] == '-'))
                {
                    currentSign = (remaining[i] == '+') ? 1.0 : -1.0;
                    i++;
                }
                else
                {
                    break;
                }
            }
            if (i < remaining.Length)
            {
                double term = ParseTerm(ref i, remaining, x);
                result += currentSign * term;
            }
            return result;
        }

        private double ParseTerm(ref int pos, string s, double[] x)
        {
            double coef = 1.0;
            double var1 = 1.0;
            double var2 = 1.0;
            bool hasVar1 = false;
            bool hasVar2 = false;
            bool hasCoef = false;
            if (pos < s.Length && s[pos] == '(')
            {
                pos++;
                if (pos < s.Length && s[pos] == '-')
                {
                    pos++;
                }
                double absVal = ParseNumber(ref pos, s);
                if (pos < s.Length && s[pos] == ')')
                {
                    pos++;
                }
                coef = -absVal;
                hasCoef = true;
            }
            else if (pos < s.Length && (char.IsDigit(s[pos]) || s[pos] == '.'))
            {
                coef = ParseNumber(ref pos, s);
                hasCoef = true;
            }
            else if (pos < s.Length && s[pos] == 'x')
            {
                int idx = ParseVar(ref pos, s);
                if (idx >= 0 && idx < x.Length)
                {
                    var1 = x[idx];
                    hasVar1 = true;
                }
            }
            while (pos < s.Length && s[pos] == '*')
            {
                pos++;
                if (pos < s.Length && (char.IsDigit(s[pos]) || s[pos] == '.'))
                {
                    double num = ParseNumber(ref pos, s);
                    coef *= num;
                }
                else if (pos < s.Length && s[pos] == 'x')
                {
                    int idx = ParseVar(ref pos, s);
                    if (idx >= 0 && idx < x.Length)
                    {
                        if (!hasVar1)
                        {
                            var1 = x[idx];
                            hasVar1 = true;
                        }
                        else if (!hasVar2)
                        {
                            var2 = x[idx];
                            hasVar2 = true;
                        }
                    }
                }
            }
            double termVal = coef;
            if (hasVar1) termVal *= var1;
            if (hasVar2) termVal *= var2;
            return termVal;
        }

        private double ParseNumber(ref int pos, string s)
        {
            if (pos >= s.Length || (!char.IsDigit(s[pos]) && s[pos] != '.'))
                throw new FormatException("Invalid number format.");
            int start = pos;
            bool startsWithDot = (s[pos] == '.');
            bool hasDot = startsWithDot;
            pos++;
            while (pos < s.Length && (char.IsDigit(s[pos]) || (s[pos] == '.' && !hasDot)))
            {
                if (s[pos] == '.') hasDot = true;
                pos++;
            }
            string numStr = s.Substring(start, pos - start);
            if (startsWithDot) numStr = "0" + numStr;
            return double.Parse(numStr, CultureInfo.InvariantCulture);
        }

        private int ParseVar(ref int pos, string s)
        {
            if (pos >= s.Length || s[pos] != 'x') return -1;
            pos++;
            int varNum = 0;
            bool hasDigit = false;
            while (pos < s.Length && char.IsDigit(s[pos]))
            {
                varNum = varNum * 10 + (s[pos] - '0');
                pos++;
                hasDigit = true;
            }
            if (!hasDigit || varNum < 1 || varNum > 10) return -1;
            return varNum - 1;
        }

        public void ValidateInputs(string[] equations, double[] x0, double epsilon, int maxIterations)
        {
            if (equations == null || equations.Length == 0)
                throw new ArgumentNullException(nameof(equations));
            if (x0 == null || x0.Length == 0)
                throw new ArgumentNullException(nameof(x0));
            if (equations.Length != x0.Length)
                throw new ArgumentException("Number of equations must match number of variables.");
            if (equations.Length > 10)
                throw new ArgumentException("Maximum number of variables/equations is 10.");
            if (epsilon < 1e-8 || epsilon > 1e-2)
                throw new ArgumentException("Epsilon must be between 1e-8 and 1e-2.");
            if (maxIterations < 10 || maxIterations > 10000)
                throw new ArgumentException("Maximum iterations must be between 10 and 10000.");
        }

        private void ValidateNoTrigonometricExpressions(string[] equations)
        {
            string[] trigonometricFunctions = { "sin", "cos", "tan", "cot", "sec", "csc", "asin", "acos", "atan", "sinh", "cosh", "tanh" };
            foreach (string equation in equations)
            {
                string normalizedEquation = equation.ToLower();
                foreach (string trigFunc in trigonometricFunctions)
                {
                    if (normalizedEquation.Contains(trigFunc))
                    {
                        throw new ArgumentException("Sorry program doesn't support trigonometric expressions");
                    }
                }
            }
        }

        private static string GetVarName(int index)
        {
            string[] vars = { "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8", "x9", "x10" };
            if (index >= 0 && index < vars.Length)
                return vars[index];
            else
                return $"x{index + 1}";
        }
    }

    public class ComplexityMetrics : IDisposable
    {
        public double[] Solution { get; set; }
        public int Iterations { get; set; }
        public double FinalError { get; set; }
        public bool Converged { get; set; }
        public int FunctionEvaluations { get; set; }
        public int JacobianEvaluations { get; set; }
        public string MethodUsed { get; set; }

        public ComplexityMetrics()
        {
            FunctionEvaluations = 0;
            JacobianEvaluations = 0;
            MethodUsed = "";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Solution = null;
                Iterations = 0;
                FinalError = 0;
                Converged = false;
                FunctionEvaluations = 0;
                JacobianEvaluations = 0;
                MethodUsed = null;
            }
        }

        ~ComplexityMetrics()
        {
            Dispose(false);
        }
    }
}