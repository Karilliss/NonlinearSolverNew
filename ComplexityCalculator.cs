using System;
using System.Linq;
using System.Text;
namespace NonlinearSolver
{
    public class ComplexityMetricsDetailed
    {
        public int SystemSize { get; set; }
        public int Iterations { get; set; }
        public string Method { get; set; }
        public double TimeComplexity { get; set; }
        public string TimeComplexityNotation { get; set; }
        public double SpaceComplexity { get; set; }
        public string SpaceComplexityNotation { get; set; }
        public long EstimatedOperations { get; set; }
        public long EstimatedMemoryBytes { get; set; }
        public int UpdatePeriod { get; set; }
    }
    public class ComplexityCalculator
    {
        private readonly ComplexityProfileManager _profileManager;
        public ComplexityCalculator()
        {
            _profileManager = new ComplexityProfileManager();
        }
        public ComplexityMetricsDetailed ComputeTimeComplexity(string[] equations, int iterations, string method, int updatePeriod = 5)
        {
            int n = equations.Length;
            var metrics = new ComplexityMetricsDetailed
            {
                SystemSize = n,
                Iterations = iterations,
                Method = method,
                UpdatePeriod = updatePeriod
            };
            CalculateTimeComplexity(metrics, n, iterations, method, updatePeriod);
            CalculateSpaceComplexity(metrics, n, method);
            return metrics;
        }
        private void CalculateTimeComplexity(ComplexityMetricsDetailed metrics, int n, int iterations, string method, int updatePeriod)
        {
            var profile = _profileManager.GetProfile(method);
            var timeAnalysis = profile.CalculateTimeComplexity(n, iterations, updatePeriod);
            metrics.TimeComplexity = timeAnalysis.ComplexityValue;
            metrics.TimeComplexityNotation = timeAnalysis.Notation;
            metrics.EstimatedOperations = timeAnalysis.Operations;
            // Fix: Set effective p for report
            if (method.ToLower().Contains("newton"))
            {
                metrics.UpdatePeriod = 1;
            }
        }
        private void CalculateSpaceComplexity(ComplexityMetricsDetailed metrics, int n, string method)
        {
            var profile = _profileManager.GetProfile(method);
            var spaceAnalysis = profile.CalculateSpaceComplexity(n);
            metrics.SpaceComplexity = spaceAnalysis.ComplexityValue;
            metrics.SpaceComplexityNotation = spaceAnalysis.Notation;
            metrics.EstimatedMemoryBytes = spaceAnalysis.MemoryBytes;
        }
        public string GenerateComplexityReport(ComplexityMetricsDetailed metrics)
        {
            var reportGenerator = new ComplexityReportGenerator();
            return reportGenerator.GenerateReport(metrics);
        }
        public ComplexityMetricsDetailed GetComplexityMetrics(string[] equations, int iterations, string method, int updatePeriod = 5)
        {
            return ComputeTimeComplexity(equations, iterations, method, updatePeriod);
        }
    }
    internal class ComplexityProfileManager
    {
        public IComplexityProfile GetProfile(string method)
        {
            return method.ToLower().Contains("newton")
                ? new NewtonComplexityProfile()
                : new SecantComplexityProfile();
        }
    }
    internal interface IComplexityProfile
    {
        TimeComplexityResult CalculateTimeComplexity(int n, int iterations, int updatePeriod);
        SpaceComplexityResult CalculateSpaceComplexity(int n);
    }
    internal struct TimeComplexityResult
    {
        public double ComplexityValue { get; set; }
        public string Notation { get; set; }
        public long Operations { get; set; }
    }
    internal struct SpaceComplexityResult
    {
        public double ComplexityValue { get; set; }
        public string Notation { get; set; }
        public long MemoryBytes { get; set; }
    }
    internal abstract class BaseComplexityProfile : IComplexityProfile
    {
        public abstract TimeComplexityResult CalculateTimeComplexity(int n, int iterations, int updatePeriod);
        public abstract SpaceComplexityResult CalculateSpaceComplexity(int n);
        protected virtual long CalculateBaseOperations(int n, int iterations, int updatePeriod)
        {
            long numUpdates = (long)Math.Ceiling((double)iterations / updatePeriod);
            long baseN3 = numUpdates * (long)Math.Pow(n, 3);
            long lineSearchCost = iterations * n * LineSearchEvals;  
            return baseN3 + lineSearchCost;
        }
        protected virtual long CalculateBaseMemory(int n)
        {
            return (n * n + 4 * n) * 8L;
        }

        protected const int LineSearchEvals = 6; 
        protected const double BroydenOverhead = 1.5;  
    }

    internal class NewtonComplexityProfile : BaseComplexityProfile
    {
        public override TimeComplexityResult CalculateTimeComplexity(int n, int iterations, int updatePeriod)
        {
            int effectiveP = 1; 
            long numUpdates = (long)Math.Ceiling((double)iterations / effectiveP);

            long funcEvals = iterations * n;  
            long jacobianCost = iterations * n * n;  
            long gaussianCost = numUpdates * n * n * n;  
            long lineSearchCost = iterations * n * LineSearchEvals; 
            long totalOps = funcEvals + jacobianCost + gaussianCost + lineSearchCost;

            double complexity = iterations * Math.Pow(n, 3) + iterations * n * LineSearchEvals;  
            string notation = $"O(k n³ + k n (ls={LineSearchEvals})) ≈ O({iterations} n³ + {iterations * LineSearchEvals} n)";

            return new TimeComplexityResult
            {
                ComplexityValue = complexity,
                Notation = notation,
                Operations = totalOps
            };
        }
        public override SpaceComplexityResult CalculateSpaceComplexity(int n)
        {
            double complexity = Math.Pow(n, 2);
            string notation = $"O(n²) = O({n}²) = O({n * n})";
            long memoryBytes = CalculateBaseMemory(n);
            return new SpaceComplexityResult
            {
                ComplexityValue = complexity,
                Notation = notation,
                MemoryBytes = memoryBytes
            };
        }
    }
    internal class SecantComplexityProfile : BaseComplexityProfile
    {
        public override TimeComplexityResult CalculateTimeComplexity(int n, int iterations, int updatePeriod)
        {
            long numUpdates = (long)Math.Ceiling((double)iterations / updatePeriod);

            long baseEvals = iterations * n * 2;  
            long broydenSolve = numUpdates * n * n * n;  
            long broydenUpdate = iterations * n * n * (long)BroydenOverhead;  
            long lineSearchCost = iterations * n * LineSearchEvals;
            long totalOps = baseEvals + broydenSolve + broydenUpdate + lineSearchCost;

            double complexity = iterations * n * n + (iterations * Math.Pow(n, 3)) / updatePeriod + iterations * n * LineSearchEvals;
            string notation = $"O(k n² (Broyden) + (k/p) n³ + k n (ls={LineSearchEvals})) ≈ O({iterations} n² + {numUpdates} n³ + {iterations * LineSearchEvals} n)";

            return new TimeComplexityResult
            {
                ComplexityValue = complexity,
                Notation = notation,
                Operations = totalOps
            };
        }
        public override SpaceComplexityResult CalculateSpaceComplexity(int n)
        {
            double complexity = Math.Pow(n, 2);
            string notation = $"O(n²) = O({n}²) = O({n * n})";
            long memoryBytes = CalculateBaseMemory(n) + (2 * n * n) * 8L;     
            return new SpaceComplexityResult
            {
                ComplexityValue = complexity,
                Notation = notation,
                MemoryBytes = memoryBytes
            };
        }
    }
    internal class ComplexityReportGenerator
    {
        public string GenerateReport(ComplexityMetricsDetailed metrics)
        {
            return $@" COMPLEXITY ANALYSIS - {metrics.Method}
 SYSTEM PARAMETERS:
• System size: {metrics.SystemSize} equations
• Number of iterations: {metrics.Iterations}
• Update period (p): {metrics.UpdatePeriod}
• Method: {metrics.Method}

 TIME COMPLEXITY:
• Theoretical: {metrics.TimeComplexityNotation} 
• Estimated operations: {metrics.EstimatedOperations:N0}

 SPACE COMPLEXITY:
• Theoretical: {metrics.SpaceComplexityNotation}
• Estimated memory: {FormatMemorySize(metrics.EstimatedMemoryBytes)}

 COMPARATIVE CHARACTERISTICS:
{GetComplexityComparison(metrics)}";
        }
        private string FormatMemorySize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} bytes";
            if (bytes < 1024 * 1024) return $"{(bytes / 1024.0):F1} KB";
            return $"{(bytes / (1024.0 * 1024.0)):F2} MB";
        }
        private string GetComplexityComparison(ComplexityMetricsDetailed metrics)
        {
            if (metrics.SystemSize <= 3)
                return "• Small system - optimal for both methods";
            if (metrics.SystemSize <= 6)
                return "• Medium system - Newton's method is more appropriate";
            if (metrics.SystemSize <= 10)
                return $"• Large system - Secant method is more appropriate";
            return "• Maximum system size reached - for larger problems, consider external solvers";
        }
    }
}