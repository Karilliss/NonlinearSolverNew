using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using static NonlinearSolver.Solver;
using static System.Math;

namespace NonlinearSolver
{
    public interface ISolverMethod
    {
        ComplexityMetrics Solve(FunctionEval[] functions, double[] initialGuess, double epsilon, int maxIterations);
    }

    public abstract class BaseSolverMethod : ISolverMethod
    {
        protected readonly Solver solver;
        protected BaseSolverMethod(Solver solver)
        {
            this.solver = solver;
        }

        public virtual double CalculateNorm(double[] vector)
        {
            double sum = 0;
            for (int i = 0; i < vector.Length; i++)
                sum += vector[i] * vector[i];
            return Math.Sqrt(sum);
        }

        protected virtual bool SolveLinearSystem(double[,] A, double[] b, double[] x)
        {
            int n = A.GetLength(0);
            double[,] aug = new double[n, n + 1];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    aug[i, j] = A[i, j];
                aug[i, n] = b[i];
            }
            if (!GaussianEliminationWithPivoting(aug))
                return false;
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = aug[i, n];
                for (int j = i + 1; j < n; j++)
                {
                    sum -= aug[i, j] * x[j];
                }
                x[i] = sum / aug[i, i];
            }
            return true;
        }

        protected virtual bool GaussianEliminationWithPivoting(double[,] A)
        {
            int n = A.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                int maxRow = i;
                double maxVal = Math.Abs(A[i, i]);
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(A[k, i]) > maxVal)
                    {
                        maxVal = Math.Abs(A[k, i]);
                        maxRow = k;
                    }
                }
                if (maxRow != i)
                {
                    for (int k = 0; k < n + 1; k++)
                    {
                        double temp = A[i, k];
                        A[i, k] = A[maxRow, k];
                        A[maxRow, k] = temp;
                    }
                }
                if (Math.Abs(A[i, i]) < 1e-15)
                    return false;
                for (int k = i + 1; k < n; k++)
                {
                    double factor = A[k, i] / A[i, i];
                    for (int j = i; j < n + 1; j++)
                    {
                        A[k, j] -= factor * A[i, j];
                    }
                }
            }
            return true;
        }

        public abstract ComplexityMetrics Solve(FunctionEval[] functions, double[] initialGuess, double epsilon, int maxIterations);

        protected ComplexityMetrics CreateMetrics(double[] solution, int iterations, double finalError,
                                               bool converged, int funcEvals, int jacobianEvals, string method)
        {
            return new ComplexityMetrics
            {
                Solution = solution,
                Iterations = iterations,
                FinalError = finalError,
                Converged = converged,
                FunctionEvaluations = funcEvals,
                JacobianEvaluations = jacobianEvals,
                MethodUsed = method
            };
        }

        protected double[] Negate(double[] v)
        {
            double[] result = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
                result[i] = -v[i];
            return result;
        }

        protected void ApplyGradientDescentStep(double[] fx, double[] dx, double epsilon)
        {
            double gradientNorm = CalculateNorm(fx);
            double stepSize = 0.01 / (1 + gradientNorm);
            for (int i = 0; i < dx.Length; i++)
                dx[i] = -stepSize * fx[i];
        }

        protected bool ShouldStop(double error, double[] dx, double epsilon, int iteration)
        {
            return error < epsilon || (iteration > 5 && CalculateNorm(dx) < epsilon * 1e-4);
        }

        protected double ChooseOptimalStepSize(double x)
        {
            double h = Math.Pow(2.2e-16, 1.0 / 3.0) * Math.Max(Math.Abs(x), 1.0);
            return Math.Max(h, 1e-8);
        }

        protected double CalculateNorm(double[] v1, double[] v2)
        {
            double sum = 0;
            for (int i = 0; i < v1.Length; i++)
                sum += (v1[i] - v2[i]) * (v1[i] - v2[i]);
            return Math.Sqrt(sum);
        }
    }

    public class NewtonMethod : BaseSolverMethod
    {
        public NewtonMethod(Solver solver) : base(solver) { }

        public override ComplexityMetrics Solve(FunctionEval[] f, double[] x0, double epsilon, int maxIterations)
        {
            int n = x0.Length;
            double[] x = (double[])x0.Clone();
            double[] fx = new double[n];
            double[] dx = new double[n];
            double finalError = double.MaxValue;
            int funcEvals = 0;
            int jacobianEvals = 0;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                for (int i = 0; i < n; i++)
                {
                    fx[i] = f[i](x);
                    funcEvals++;
                }
                finalError = CalculateNorm(fx);
                if (ShouldStop(finalError, dx, epsilon, iter))
                {
                    return CreateMetrics(x, iter + 1, finalError, true, funcEvals, jacobianEvals, "Newton");
                }
                double[,] Jmat = ComputeNumericalJacobian(f, x, fx, n, ref funcEvals);
                jacobianEvals += n * n;
                if (!SolveLinearSystem(Jmat, Negate(fx), dx))
                {
                    ApplyGradientDescentStep(fx, dx, epsilon);
                }
                double alpha = PerformLineSearch(f, x, dx, fx, n, ref funcEvals);
                for (int i = 0; i < n; i++)
                    x[i] += alpha * dx[i];
                if (iter > 10 && CalculateNorm(dx) < epsilon * 1e-3)
                {
                    return CreateMetrics(x, iter + 1, finalError, finalError < epsilon * 10,
                                       funcEvals, jacobianEvals, "Newton");
                }
            }
            return CreateMetrics(x, maxIterations, finalError, finalError < epsilon,
                               funcEvals, jacobianEvals, "Newton");
        }

        private double[,] ComputeNumericalJacobian(FunctionEval[] f, double[] x, double[] fx0, int n, ref int funcEvals)
        {
            double[,] J = new double[n, n];
            double[] xPlus = (double[])x.Clone();
            for (int j = 0; j < n; j++)
            {
                double h = ChooseOptimalStepSize(x[j]);
                xPlus[j] = x[j] + h;
                double[] fPlus = new double[n];
                for (int i = 0; i < n; i++)
                {
                    fPlus[i] = f[i](xPlus);
                    funcEvals++;
                }
                for (int i = 0; i < n; i++)
                {
                    J[i, j] = (fPlus[i] - fx0[i]) / h;
                }
                xPlus[j] = x[j];
            }
            return J;
        }

        private double PerformLineSearch(FunctionEval[] f, double[] x, double[] dx, double[] fx, int n, ref int funcEvals)
        {
            double currentError = CalculateNorm(fx);
            double c1 = 1e-4;
            double[] alphaSequence = { 1.0, 0.5, 0.25, 0.125, 0.0625, 0.03125, 0.015625, 0.0078125 };
            foreach (double testAlpha in alphaSequence)
            {
                double[] xNew = new double[n];
                for (int i = 0; i < n; i++)
                    xNew[i] = x[i] + testAlpha * dx[i];
                double[] fNew = new double[n];
                double newError = 0;
                for (int i = 0; i < n; i++)
                {
                    fNew[i] = f[i](xNew);
                    funcEvals++;
                    newError += fNew[i] * fNew[i];
                }
                newError = Math.Sqrt(newError);
                if (newError < currentError * (1 - c1 * testAlpha))
                    return testAlpha;
            }
            return alphaSequence[alphaSequence.Length - 1];
        }
    }

    public class SecantMethod : BaseSolverMethod
    {
        public SecantMethod(Solver solver) : base(solver) { }

        public override ComplexityMetrics Solve(FunctionEval[] f, double[] x0, double epsilon, int maxIterations)
        {
            int n = x0.Length;
            double[] x = (double[])x0.Clone();
            double finalError = double.MaxValue;
            int funcEvals = 0;
            double[] xPrev = new double[n];
            for (int i = 0; i < n; i++)
            {
                xPrev[i] = x0[i] + 0.001 * (i + 1);
            }
            double[] fx = new double[n];
            double[] fxPrev = new double[n];
            for (int i = 0; i < n; i++)
            {
                fx[i] = f[i](x);
                fxPrev[i] = f[i](xPrev);
                funcEvals += 2;
            }
            double[,] B = InitializeIdentityMatrix(n);
            UpdateBroyden(B, x, xPrev, fx, fxPrev, n);
            double prevError = CalculateNorm(fx);
            int noProgressCount = 0;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                finalError = CalculateNorm(fx);
                if (finalError < epsilon)
                {
                    return CreateMetrics(x, iter + 1, finalError, true, funcEvals, 0, "Secant");
                }
                if (Math.Abs(finalError - prevError) < epsilon * 1e-3)
                {
                    noProgressCount++;
                    if (noProgressCount > 10)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            x[i] = (x[i] + xPrev[i]) / 2.0;
                        }
                        for (int i = 0; i < n; i++)
                        {
                            fx[i] = f[i](x);
                        }
                        funcEvals += n;
                        noProgressCount = 0;
                    }
                }
                else
                {
                    noProgressCount = 0;
                }
                prevError = finalError;
                double[] dx = new double[n];
                bool systemSolved = SolveLinearSystem(B, Negate(fx), dx);
                if (!systemSolved)
                {
                    ApplyGradientDescentStep(fx, dx, epsilon);
                    B = InitializeIdentityMatrix(n);
                }
                double stepNorm = CalculateNorm(dx);
                if (stepNorm > 1.0)
                {
                    double scale = 1.0 / stepNorm;
                    for (int i = 0; i < n; i++)
                    {
                        dx[i] *= scale;
                    }
                }
                double alpha = PerformSecantLineSearch(f, x, dx, fx, n, ref funcEvals);
                Array.Copy(x, xPrev, n);
                Array.Copy(fx, fxPrev, n);
                for (int i = 0; i < n; i++)
                {
                    x[i] += alpha * dx[i];
                }
                for (int i = 0; i < n; i++)
                {
                    fx[i] = f[i](x);
                    funcEvals++;
                }
                double xChange = CalculateNorm(x, xPrev);
                if (xChange > epsilon * 1e-2)
                {
                    UpdateBroyden(B, x, xPrev, fx, fxPrev, n);
                }
                if (iter > 5 && xChange < epsilon * 1e-4)
                {
                    return CreateMetrics(x, iter + 1, finalError, finalError < epsilon * 10,
                                       funcEvals, 0, "Secant");
                }
            }
            return CreateMetrics(x, maxIterations, finalError, finalError < epsilon,
                               funcEvals, 0, "Secant");
        }

        private double[,] InitializeIdentityMatrix(int n)
        {
            double[,] matrix = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = (i == j) ? 1.0 : 0.0;
                }
            }
            return matrix;
        }

        private void UpdateBroyden(double[,] B, double[] x, double[] xPrev, double[] fx, double[] fxPrev, int n)
        {
            double[] s = new double[n];
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                s[i] = x[i] - xPrev[i];
                y[i] = fx[i] - fxPrev[i];
            }
            double sNormSq = 0;
            for (int i = 0; i < n; i++)
            {
                sNormSq += s[i] * s[i];
            }
            if (sNormSq < 1e-16) return;
            double[] Bs = new double[n];
            for (int i = 0; i < n; i++)
            {
                Bs[i] = 0.0;
                for (int j = 0; j < n; j++)
                {
                    Bs[i] += B[i, j] * s[j];
                }
            }
            double[] yMinusBs = new double[n];
            for (int i = 0; i < n; i++)
            {
                yMinusBs[i] = y[i] - Bs[i];
            }
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    B[i, j] += yMinusBs[i] * s[j] / sNormSq;
                }
            }
        }

        private double PerformSecantLineSearch(FunctionEval[] f, double[] x, double[] dx, double[] fx, int n, ref int funcEvals)
        {
            double alpha = 1.0;
            double currentError = CalculateNorm(fx);
            for (int lsIter = 0; lsIter < 5; lsIter++)
            {
                double[] xNew = new double[n];
                for (int i = 0; i < n; i++)
                {
                    xNew[i] = x[i] + alpha * dx[i];
                }
                double[] fNew = new double[n];
                double newError = 0.0;
                for (int i = 0; i < n; i++)
                {
                    fNew[i] = f[i](xNew);
                    funcEvals++;
                    newError += fNew[i] * fNew[i];
                }
                newError = Math.Sqrt(newError);
                if (newError < currentError * (1.0 - 1e-4 * alpha) || lsIter == 4)
                {
                    return alpha;
                }
                alpha *= 0.5;
            }
            return alpha;
        }
    }
}