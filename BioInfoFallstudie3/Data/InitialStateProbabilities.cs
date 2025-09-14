using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BioInfoFallstudie3.Data;

public record InitialStateProbabilities<TState, TPrecision>
    where TState : notnull
    where TPrecision : IFloatingPoint<TPrecision>
{
    public readonly ImmutableDictionary<TState, TPrecision> Dictionary;
    public readonly ImmutableArray<TPrecision> Array;

    private InitialStateProbabilities(ImmutableDictionary<TState, TPrecision> dictionary, ImmutableArray<TPrecision> array)
    {
        this.Dictionary = dictionary;
        this.Array = array;
    }

    public static ImmutableArray<TPrecision> BuildMatrixFromDictionary(IReadOnlyList<TState> states, ImmutableDictionary<TState, TPrecision> initialStateProbabilityDict)
    {
        int stateCount = states.Count;
        ImmutableArray<TPrecision>.Builder resultDictBuilder = ImmutableArray.CreateBuilder<TPrecision>(stateCount);

        for (int stateIndex = 0; stateIndex < stateCount; stateIndex++)
        {
            TState state = states[stateIndex];
            if (!initialStateProbabilityDict.TryGetValue(state, out TPrecision? initialStateProbability))
            {
                //TODO: Value not in dictionary could alternatively be considered to be zero
                throw new Exception($"No initial state probability provided for state {state}.");
            }
            resultDictBuilder.Add(initialStateProbability);
        }

        return resultDictBuilder.ToImmutable();
    }

    public static ImmutableDictionary<TState, TPrecision> BuildDictionaryFromArray(IReadOnlyList<TState> states, IReadOnlyList<TPrecision> initialStateProbabilities)
    {
        int stateCount = states.Count;
        if (initialStateProbabilities.Count != stateCount)
            throw new ArgumentException($"The amount of states ({states.Count}) did not match the count of the provided initial state probability list ({initialStateProbabilities.Count}).");

        ImmutableDictionary<TState, TPrecision>.Builder resultDictBuilder = ImmutableDictionary.CreateBuilder<TState, TPrecision>();
        for (int stateindex = 0; stateindex < stateCount; stateindex++)
        {
            TState state = states[stateindex];
            resultDictBuilder.Add(state, initialStateProbabilities[stateindex]);
        }

        return resultDictBuilder.ToImmutable();
    }

    public static InitialStateProbabilities<TState, TPrecision> FromDictionary(IReadOnlyList<TState> states, ImmutableDictionary<TState, TPrecision> initialStateProbabilityDict)
    {
        ImmutableArray<TPrecision> transitionMatrix = BuildMatrixFromDictionary(states, initialStateProbabilityDict);
        return new(initialStateProbabilityDict, transitionMatrix);
    }

    public static InitialStateProbabilities<TState, TPrecision> FromArray(IReadOnlyList<TState> states, ImmutableArray<TPrecision> initialStateProbabilities)
    {
        ImmutableDictionary<TState, TPrecision> transitionProbabilityDictionary = BuildDictionaryFromArray(states, initialStateProbabilities);
        return new(transitionProbabilityDictionary, initialStateProbabilities);
    }

}
