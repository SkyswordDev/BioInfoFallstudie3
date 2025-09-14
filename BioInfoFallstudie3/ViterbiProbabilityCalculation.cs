using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BioInfoFallstudie3;

public delegate TPrecision ViterbiProbabilityCalculation<TPrecision>(TPrecision emissionProbability, TPrecision currentPathProbability, TPrecision transitionProbability) where TPrecision : IFloatingPoint<TPrecision>;
public delegate TPrecision ViterbiProbabilityInitialization<TPrecision>(TPrecision initialStateProbability, TPrecision emissionProbability) where TPrecision : IFloatingPoint<TPrecision>;

public interface IViterbiProbabilityCalculationProvider<TPrecision> where TPrecision : IFloatingPoint<TPrecision>
{
    ViterbiProbabilityCalculation<TPrecision> ViterbiProbabilityCalculation { get; }
    ViterbiProbabilityInitialization<TPrecision> ViterbiProbabilityInitialization { get; }
}


public class NormalViterbiProbabilityCalculationProvider<TPrecision> : IViterbiProbabilityCalculationProvider<TPrecision> where TPrecision : IFloatingPoint<TPrecision>
{
    public ViterbiProbabilityCalculation<TPrecision> ViterbiProbabilityCalculation => (TPrecision emissionProbability, TPrecision currentPathProbability, TPrecision transitionProbability) =>
    {
        return currentPathProbability * transitionProbability * emissionProbability;
    };

    public ViterbiProbabilityInitialization<TPrecision> ViterbiProbabilityInitialization => (TPrecision initialStateProbability, TPrecision emissionProbability) =>
    {
        return initialStateProbability * emissionProbability;
    };
}

public class LogarithmicViterbiProbabilityCalculationProvider<TPrecision> : IViterbiProbabilityCalculationProvider<TPrecision> where TPrecision : IFloatingPoint<TPrecision>
{
    public ViterbiProbabilityCalculation<TPrecision> ViterbiProbabilityCalculation => (TPrecision emissionProbability, TPrecision currentPathProbability, TPrecision transitionProbability) =>
    {
        return currentPathProbability + Log(transitionProbability) + Log(emissionProbability);
    };

    public ViterbiProbabilityInitialization<TPrecision> ViterbiProbabilityInitialization => (TPrecision initialStateProbability, TPrecision emissionProbability) =>
    {
        return Log(initialStateProbability) + Log(emissionProbability);
    };

    private static TPrecision Log(TPrecision value)
    {
        Type precisionType = typeof(TPrecision);
        if (precisionType == typeof(float))
            return TPrecision.CreateTruncating(Math.Log(float.CreateTruncating(value)));
        if (precisionType == typeof(double))
            return TPrecision.CreateTruncating(Math.Log(double.CreateTruncating(value)));
        //if (precisionType == typeof(decimal))
        //    return TPrecision.CreateTruncating(Math.Log(decimal.CreateTruncating(value)));

        throw new NotSupportedException();
    }
}

public static class ViterbiProbabilityCalculationProviders<TPrecision> where TPrecision : IFloatingPoint<TPrecision>
{
    public static IViterbiProbabilityCalculationProvider<TPrecision> Normal => new NormalViterbiProbabilityCalculationProvider<TPrecision>();
    public static IViterbiProbabilityCalculationProvider<TPrecision> Logarithmic => new LogarithmicViterbiProbabilityCalculationProvider<TPrecision>();
}
