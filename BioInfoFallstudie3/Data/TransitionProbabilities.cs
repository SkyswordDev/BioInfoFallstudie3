//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;


//namespace BioInfoFallstudie3.Data;

//public record TransitionProbabilities<TState> where TState : notnull
//{
//    public ImmutableDictionary<TState, ImmutableDictionary<TState, double>> Dictionary;
//    public Matrix2D<double> Matrix;

//    private TransitionProbabilities(
//        ImmutableDictionary<TState, ImmutableDictionary<TState, double>> dictionary,
//        Matrix2D<double> matrix
//    )
//    {
//        this.Dictionary = dictionary;
//        this.Matrix = matrix;
//    }

//    public static Matrix2D<double> BuildMatrixFromDictionary(IReadOnlyList<TState> states, ImmutableDictionary<TState, ImmutableDictionary<TState, double>> transitionProbabilityDictionary)
//    {
//        int stateCount = states.Count;
//        ImmutableArray<ImmutableArray<double>>.Builder transitionMatrixBuilder = ImmutableArray.CreateBuilder<ImmutableArray<double>>(stateCount);

//        for (int state1Index = 0; state1Index < stateCount; state1Index++)
//        {
//            ImmutableArray<double>.Builder transitionMatrixColumn = ImmutableArray.CreateBuilder<double>(stateCount);

//            TState stateFrom = states[state1Index];
//            IDictionary<TState, double> stateTransitionProbabilities = transitionProbabilityDictionary[stateFrom];
//            for (int state2Index = 0; state2Index < stateCount; state2Index++)
//            {
//                TState state2 = states[state2Index];
//                double transitionProbability = stateTransitionProbabilities[state2];
//                transitionMatrixColumn.Add(transitionProbability);
//            }

//            transitionMatrixBuilder.Add(transitionMatrixColumn.ToImmutable());
//        }

//        return new(transitionMatrixBuilder.ToImmutable());
//    }

//    public static ImmutableDictionary<TState, ImmutableDictionary<TState, double>> BuildDictionaryFromMatrix(IReadOnlyList<TState> states, Matrix2D<double> matrix)
//    {
//        int stateCount = states.Count;
//        if (matrix.ColumnCount != stateCount)
//            throw new ArgumentException($"The amount of states ({states.Count}) did not match the column count of the matrix ({matrix.ColumnCount}).");
//        if (matrix.RowCount != stateCount)
//            throw new ArgumentException($"The amount of states ({states.Count}) did not match the row count of the matrix ({matrix.RowCount}).");




//    }

//    public static TransitionProbabilities<TState> FromDictionary(IReadOnlyList<TState> states, ImmutableDictionary<TState, ImmutableDictionary<TState, double>> transitionProbabilityDictionary)
//    {
//        Matrix2D<double> transitionMatrix = BuildMatrixFromDictionary(states, transitionProbabilityDictionary);
//        return new(transitionProbabilityDictionary, transitionMatrix);
//    }

//    public static TransitionProbabilities<TState> FromMatrix(IReadOnlyList<TState> states, Matrix2D<double> transitionMatrix)
//    {
//        ImmutableDictionary<TState,ImmutableDictionary<TState, double>> transitionProbabilityDictionary = BuildDictionaryFromMatrix(states, transitionMatrix);
//        return new(transitionProbabilityDictionary, transitionMatrix);
//    }



//}
