using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace BioInfoFallstudie3.Data;

public record EmissionProbabilities<TState, TEmit, TPrecision>
    where TState : notnull
    where TEmit : notnull
    where TPrecision : IFloatingPoint<TPrecision>
{
    public ImmutableDictionary<TState, ImmutableDictionary<TEmit, TPrecision>> Dictionary;
    public Matrix2D<TPrecision> Matrix;


    private EmissionProbabilities(
        ImmutableDictionary<TState, ImmutableDictionary<TEmit, TPrecision>> dictionary,
        Matrix2D<TPrecision> matrix
    )
    {
        this.Dictionary = dictionary;
        this.Matrix = matrix;
    }

    public static Matrix2D<TPrecision> BuildMatrixFromDictionary(IReadOnlyList<TState> states, IReadOnlyList<TEmit> emits, ImmutableDictionary<TState, ImmutableDictionary<TEmit, TPrecision>> transitionProbabilityDictionary)
    {
        int stateCount = states.Count;
        int emitCount = emits.Count;

        ImmutableArray<ImmutableArray<TPrecision>>.Builder transitionMatrixBuilder = ImmutableArray.CreateBuilder<ImmutableArray<TPrecision>>(stateCount);

        for (int stateIndex = 0; stateIndex < stateCount; stateIndex++)
        {
            ImmutableArray<TPrecision>.Builder transitionMatrixColumn = ImmutableArray.CreateBuilder<TPrecision>(stateCount);

            TState stateFrom = states[stateIndex];
            IDictionary<TEmit, TPrecision> stateTransitionProbabilities = transitionProbabilityDictionary[stateFrom];
            for (int emitIndex = 0; emitIndex < emitCount; emitIndex++)
            {
                TEmit emit = emits[emitIndex];
                TPrecision transitionProbability = stateTransitionProbabilities[emit];
                transitionMatrixColumn.Add(transitionProbability);
            }

            transitionMatrixBuilder.Add(transitionMatrixColumn.ToImmutable());
        }

        return new(transitionMatrixBuilder.ToImmutable());
    }

    public static ImmutableDictionary<TState, ImmutableDictionary<TEmit, TPrecision>> BuildDictionaryFromMatrix(IReadOnlyList<TState> states, IReadOnlyList<TEmit> emits, Matrix2D<TPrecision> matrix)
    {
        int stateCount = states.Count;
        int emitCount = emits.Count;

        if (matrix.ColumnCount < stateCount)
            throw new ArgumentException($"The amount of states ({states.Count}) did not match the column count of the matrix ({matrix.ColumnCount}).");
        if (matrix.RowCount < stateCount)
            throw new ArgumentException($"The amount of states ({states.Count}) did not match the row count of the matrix ({matrix.RowCount}).");

        ImmutableDictionary<TState, ImmutableDictionary<TEmit, TPrecision>>.Builder resultDictBuilder = ImmutableDictionary.CreateBuilder<TState, ImmutableDictionary<TEmit, TPrecision>>();

        for (int stateIndex = 0; stateIndex < stateCount; stateIndex++)
        {
            ImmutableDictionary<TEmit, TPrecision>.Builder innerDictBuilder = ImmutableDictionary.CreateBuilder<TEmit, TPrecision>();
            TState state = states[stateIndex];
            for (int emitIndex = 0; emitIndex < emitCount; emitIndex++)
            {
                TEmit emit = emits[emitIndex];
                innerDictBuilder.Add(emit, matrix[stateIndex, emitIndex]);
            }
            resultDictBuilder.Add(state, innerDictBuilder.ToImmutable());
        }

        return resultDictBuilder.ToImmutable();
    }

    public static EmissionProbabilities<TState, TEmit, TPrecision> FromDictionary(IReadOnlyList<TState> states, IReadOnlyList<TEmit> emits, ImmutableDictionary<TState, ImmutableDictionary<TEmit, TPrecision>> transitionProbabilityDictionary)
    {
        Matrix2D<TPrecision> transitionMatrix = BuildMatrixFromDictionary(states, emits, transitionProbabilityDictionary);
        return new(transitionProbabilityDictionary, transitionMatrix);
    }

    public static EmissionProbabilities<TState, TEmit, TPrecision> FromMatrix(IReadOnlyList<TState> states, IReadOnlyList<TEmit> emits, Matrix2D<TPrecision> emissionMatrix)
    {
        ImmutableDictionary<TState, ImmutableDictionary<TEmit, TPrecision>> transitionProbabilityDictionary = BuildDictionaryFromMatrix(states, emits, emissionMatrix);
        return new(transitionProbabilityDictionary, emissionMatrix);
    }

    public TPrecision[,] To2DArray() => Matrix.To2DArray();


}
