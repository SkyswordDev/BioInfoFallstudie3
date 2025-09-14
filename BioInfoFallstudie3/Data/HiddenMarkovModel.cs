using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmissionionProbability = double;
using System.Numerics;

namespace BioInfoFallstudie3.Data;


public record HiddenMarkovModel<TState, TObservation, TPrecision>(
    ImmutableArray<TState> States,
    ImmutableArray<TObservation> Observations,
    InitialStateProbabilities<TState, TPrecision> InitialStateProbabilities,
    TransitionProbabilities<TState, TPrecision> TransitionProbabilities,
    EmissionProbabilities<TState, TObservation, TPrecision> EmissionProbabilities
    )
    where TState : notnull
    where TObservation : notnull
    where TPrecision : IFloatingPoint<TPrecision>
{

    public ImmutableArray<int> GetObservationIndexes(IReadOnlyList<TObservation> observations)
    {
        ImmutableArray<int>.Builder resultBuilder = ImmutableArray.CreateBuilder<int>(observations.Count);
        for(int i = 0; i < observations.Count; i++)
        {
            resultBuilder.Add(this.Observations.IndexOf(observations[i]));
        }
        return resultBuilder.ToImmutable();
    }



    public static HiddenMarkovModel<TState, TObservation, TPrecision> Construct(
        IReadOnlyList<TState> states,
        IReadOnlyList<TObservation> observations,
        TPrecision[] initialStateProbabilities,
        TPrecision[,] transitionProbabilities,
        TPrecision[,] emissionProbabilities
        )
    {
        //LODO: validate
        return new(states.ToImmutableArray(), observations.ToImmutableArray(),
            InitialStateProbabilities<TState, TPrecision>.FromArray(states, initialStateProbabilities.ToImmutableArray()),
            TransitionProbabilities<TState, TPrecision>.FromMatrix(states, new(transitionProbabilities)),
            EmissionProbabilities<TState, TObservation, TPrecision>.FromMatrix(states, observations, new(emissionProbabilities))
            );
    }


}
