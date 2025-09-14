using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace BioInfoFallstudie3.Data;

public record TransitionProbabilities<TState, TPrecision>
    where TState : notnull
    where TPrecision : IFloatingPoint<TPrecision>
{
    public ImmutableDictionary<TState, ImmutableDictionary<TState, TPrecision>> Dictionary;
    public Matrix2D<TPrecision> Matrix;


    private TransitionProbabilities(
        ImmutableDictionary<TState, ImmutableDictionary<TState, TPrecision>> dictionary,
        Matrix2D<TPrecision> matrix
    )
    {
        this.Dictionary = dictionary;
        this.Matrix = matrix;
    }

    public static Matrix2D<TPrecision> BuildMatrixFromDictionary(IReadOnlyList<TState> states, ImmutableDictionary<TState, ImmutableDictionary<TState, TPrecision>> transitionProbabilityDictionary)
    {
        int stateCount = states.Count;
        ImmutableArray<ImmutableArray<TPrecision>>.Builder transitionMatrixBuilder = ImmutableArray.CreateBuilder<ImmutableArray<TPrecision>>(stateCount);

        for (int state1Index = 0; state1Index < stateCount; state1Index++)
        {
            ImmutableArray<TPrecision>.Builder transitionMatrixColumn = ImmutableArray.CreateBuilder<TPrecision>(stateCount);

            TState stateFrom = states[state1Index];
            IDictionary<TState, TPrecision> stateTransitionProbabilities = transitionProbabilityDictionary[stateFrom];
            for (int state2Index = 0; state2Index < stateCount; state2Index++)
            {
                TState state2 = states[state2Index];
                TPrecision transitionProbability = stateTransitionProbabilities[state2];
                transitionMatrixColumn.Add(transitionProbability);
            }

            transitionMatrixBuilder.Add(transitionMatrixColumn.ToImmutable());
        }

        return new(transitionMatrixBuilder.ToImmutable());
    }

    public static ImmutableDictionary<TState, ImmutableDictionary<TState, TPrecision>> BuildDictionaryFromMatrix(IReadOnlyList<TState> states, Matrix2D<TPrecision> matrix)
    {
        int stateCount = states.Count;
        if (matrix.ColumnCount < stateCount)
            throw new ArgumentException($"The amount of states ({states.Count}) did not match the column count of the matrix ({matrix.ColumnCount}).");
        if (matrix.RowCount < stateCount)
            throw new ArgumentException($"The amount of states ({states.Count}) did not match the row count of the matrix ({matrix.RowCount}).");

        ImmutableDictionary<TState, ImmutableDictionary<TState, TPrecision>>.Builder resultDictBuilder = ImmutableDictionary.CreateBuilder<TState, ImmutableDictionary< TState, TPrecision>>();

        for (int state1index = 0; state1index < stateCount; state1index++)
        {
            ImmutableDictionary<TState, TPrecision>.Builder innerDictBuilder = ImmutableDictionary.CreateBuilder<TState, TPrecision>();
            TState state1 = states[state1index];
            for(int state2index = 0; state2index < stateCount; state2index++)
            {
                TState state2 = states[state2index];
                innerDictBuilder.Add(state2, matrix[state1index, state2index]);
            }
            resultDictBuilder.Add(state1, innerDictBuilder.ToImmutable());
        }

        return resultDictBuilder.ToImmutable();
    }

    public static TransitionProbabilities<TState, TPrecision> FromDictionary(IReadOnlyList<TState> states, ImmutableDictionary<TState, ImmutableDictionary<TState, TPrecision>> transitionProbabilityDictionary)
    {
        Matrix2D<TPrecision> transitionMatrix = BuildMatrixFromDictionary(states, transitionProbabilityDictionary);
        return new(transitionProbabilityDictionary, transitionMatrix);
    }

    public static TransitionProbabilities<TState, TPrecision> FromMatrix(IReadOnlyList<TState> states, Matrix2D<TPrecision> transitionMatrix)
    {
        ImmutableDictionary<TState,ImmutableDictionary<TState, TPrecision>> transitionProbabilityDictionary = BuildDictionaryFromMatrix(states, transitionMatrix);
        return new(transitionProbabilityDictionary, transitionMatrix);
    }

    public TPrecision[,] To2DArray() => Matrix.To2DArray();
}
