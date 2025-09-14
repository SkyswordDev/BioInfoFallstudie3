using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace BioInfoFallstudie3;
class Viterbi
{
    private static List<T> ToListThrowIfNotUnique<T>(IEnumerable<T> values, string collectionName = "")
    {
        List<T> itemsList = new();
        foreach (T state in values)
        {
            if (itemsList.Contains(state))
            {
                throw new ArgumentException($"{nameof(collectionName)} must not contain duplicates, but {state} was duplicate.", nameof(collectionName));
            }
            itemsList.Add(state);
        }
        return itemsList;
    }

    public static TState[] Run<TState, TObservation>(IEnumerable<TState> states, IDictionary<TState, double> initialStateProbabilities,
        IDictionary<TState, IDictionary<TState, double>> transitions,
        IDictionary<TState, IDictionary<TObservation, double>> emissions,
        IEnumerable<TObservation> observations)
        where TObservation : notnull
        where TState : notnull
    {
        //build states list; assure states are unique
        List<TState> statesList = ToListThrowIfNotUnique(states, nameof(states));
        
        //build observations list and observationIndexDictionary
        List<TObservation> observationsList = observations.ToList();
        Dictionary<TObservation, int> observationsIndexDictionary = new();
        int[] observationIndexes = new int[observationsList.Count];

        int nextObservationLookupIndex = 0;
        for(int i = 0; i < observationsList.Count; i++)
        {
            TObservation observation = observationsList[i];
            if (observationsIndexDictionary.TryAdd(observation, nextObservationLookupIndex))
            {
                nextObservationLookupIndex++;
                observationIndexes[i] = nextObservationLookupIndex;
            }
            else
            {
                observationIndexes[i] = observationsIndexDictionary[observation];
            }

        }

        int stateCount = statesList.Count;
        int uniqueEmissionCount = nextObservationLookupIndex;

        //build transmission matrix; throws is state1 to state2 transition was not in transitions
        double[,] transitionMatrix = new double[stateCount, stateCount];
        for(int state1Index = 0; state1Index < stateCount; state1Index++)
        {
            TState stateFrom = statesList[state1Index];
            IDictionary<TState, double> stateTransitionProbabilities = transitions[stateFrom];
            for(int state2Index = 0; state2Index < stateCount; state2Index++)
            {
                TState state2 = statesList[state2Index];
                double transitionProbability = stateTransitionProbabilities[state2];
                transitionMatrix[state1Index, state2Index] = transitionProbability;
            }
        }

        //build emission matrix, throws if there is no emission probability for a state
        double[,] emissionMatrix = new double[stateCount, uniqueEmissionCount];
        for(int stateIndex = 0; stateIndex < stateCount; stateIndex++)
        {
            TState stateFrom = statesList[stateIndex];
            IDictionary<TObservation, double> stateEmissionProbabilities = emissions[stateFrom];
            for (int emissionIndex = 0; emissionIndex < uniqueEmissionCount; emissionIndex++)
            {
                TObservation observation = observationsList[emissionIndex];
                double emissionProbability = stateEmissionProbabilities[observation];
                emissionMatrix[stateIndex, emissionIndex] = emissionProbability;
            }
        }

        //build initial state probabilities; throws if there was no initial state probability for a state
        double[] initialStateProbabilitiesArr = new double[stateCount];
        for(int stateIndex = 0; stateIndex < stateCount; stateIndex++)
        {
            TState state = statesList[stateIndex];
            initialStateProbabilitiesArr[stateIndex] = initialStateProbabilities[state];
        }


        return Run<TState>(statesList, initialStateProbabilitiesArr, transitionMatrix, emissionMatrix, observationIndexes);
    }

    public static TState[] Run<TState>(IReadOnlyList<TState> states, double[] initialStateProbabilities, double[,] transitionMatrix, double[,] emissionMatrix, int[] observations)
    {
        int[] path = Run(states.Count, initialStateProbabilities, transitionMatrix, emissionMatrix, observations);
        return path.Select(p => states[p]).ToArray();
    }

    public static int[] Run(int stateCount, double[] initialStateProbabilities, double[,] transitionMatrix, double[,] emissionMatrix, int[] observationIndexes)
    {
        //    input states: S hidden states
        //    input init:
        //        initial probabilities of each state
        //    input trans:
        //        S × S transition matrix
        //    input emit:
        //        S × O emission matrix
        //    input obs:
        //        sequence of T observations

        //    prob ← T × S matrix of zeroes
        double[,] prob = new double[observationIndexes.Length, stateCount];
        //    prev ← empty T × S matrix
        int[,] prev = new int[observationIndexes.Length, stateCount];


        //for each state s in states do
        //prob[0][s] = init[s] * emit[s][obs[0]]
        for (int state = 0; state < stateCount; state++)
        {
            prob[0, state] = initialStateProbabilities[state] * emissionMatrix[state, observationIndexes[0]];
        }

        //for t = 1 to T - 1 inclusive do // t = 0 has been dealt with already
        for (int observationNumber = 1; observationNumber < observationIndexes.Length; observationNumber++)
        {
            if (observationNumber == 419)
            {
                int a = 0;
            }
            int observationIndex = observationIndexes[observationNumber];
            double newProb;
            //for each state s in states do
            for (int state = 0; state < stateCount; state++)
            {
                //for each state r in states do
                for (int state2 = 0; state2 < stateCount; state2++)
                {
                    //new_prob ← prob[t - 1][r] * trans[r][s] * emit[s][obs[t]]
                    newProb = prob[observationNumber-1, state2] * transitionMatrix[state2, state] * emissionMatrix[state, observationIndex];
                    //if new_prob > prob[t][s] then
                    if (newProb > prob[observationNumber, state])
                    {
                        //prob[t][s] ← new_prob
                        prob[observationNumber, state] = newProb;
                        //prev[t][s] ← r
                        prev[observationNumber, state] = state2;
                    }
                }
            }

        }

        //path ← empty array of length T
        int[] path = new int[observationIndexes.Length];

        double curMax = double.MinValue;
        int curMaxState = 0;
        int lastObservationIndex = observationIndexes.Length - 1;
        for (int state = 0; state < stateCount; state++)
        {
            if (curMax < prob[lastObservationIndex, state])
            {
                curMax = prob[lastObservationIndex, state];
                curMaxState = state;
            }
        }

        //path[T - 1] ← the state s with maximum prob[T - 1][s]
        path[lastObservationIndex] = curMaxState;

        //        for t = T - 2 to 0 inclusive do
        for (int observationIndex = lastObservationIndex - 1; observationIndex >= 0; observationIndex--)
        {
            //                path[t] ← prev[t + 1][path[t + 1]]
            path[observationIndex] = prev[observationIndex + 1, path[observationIndex + 1]];
        }

        return path;
        //    return path
        //end
    }
}
