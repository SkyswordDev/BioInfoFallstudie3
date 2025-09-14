using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BioInfoFallstudie3.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace BioInfoFallstudie3;
class ViterbiV2
{

    public static string[] Run(
        IViterbiProbabilityCalculationProvider<double> viterbiProbabilityCalculationProvider,
        HiddenMarkovModel<string, string, double> hmm, string[] observations)
    {
        return Run<string>(viterbiProbabilityCalculationProvider,
            hmm.States, hmm.InitialStateProbabilities.Array.ToArray(), hmm.TransitionProbabilities.To2DArray(), hmm.EmissionProbabilities.To2DArray(),
            hmm.GetObservationIndexes(observations).ToArray());
    }

    public static TState[] Run<TState>(IViterbiProbabilityCalculationProvider<double> viterbiProbabilityCalculationProvider,
        IReadOnlyList<TState> states, double[] initialStateProbabilities, double[,] transitionMatrix, double[,] emissionMatrix, int[] observations)
    {
        int[] path = Run(viterbiProbabilityCalculationProvider, states.Count, initialStateProbabilities, transitionMatrix, emissionMatrix, observations);
        return path.Select(p => states[p]).ToArray();
    }

    public static int[] Run(IViterbiProbabilityCalculationProvider<double> viterbiProbabilityCalculationProvider,
        int stateCount, double[] initialStateProbabilities, double[,] transitionMatrix, double[,] emissionMatrix, int[] observationIndexes)
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
        for (int i = 0; i < observationIndexes.Length; i++)
            for (int j = 0; j < stateCount; j++)
                prob[i, j] = double.MinValue;
        //    prev ← empty T × S matrix
        int[,] prev = new int[observationIndexes.Length, stateCount];


        //for each state s in states do
        //prob[0][s] = init[s] * emit[s][obs[0]]
        for (int state = 0; state < stateCount; state++)
        {
            //prob[0, state] = initialStateProbabilities[state] * emissionMatrix[state, observationIndexes[0]];
            prob[0, state] = viterbiProbabilityCalculationProvider.ViterbiProbabilityInitialization(initialStateProbabilities[state], emissionMatrix[state, observationIndexes[0]]);
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
                    //newProb = prob[observationNumber-1, state2] * transitionMatrix[state2, state] * emissionMatrix[state, observationIndex];
                    newProb = viterbiProbabilityCalculationProvider.ViterbiProbabilityCalculation(emissionMatrix[state, observationIndex], prob[observationNumber - 1, state2], transitionMatrix[state2, state]);
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

        //double[,] prob2 = new double[observationIndexes.Length, stateCount];
        //for (int i = 0; i < observationIndexes.Length; i++)
        //    for (int j = 0; j < stateCount; j++)
        //        prob2[i, j] = Math.Exp(prob[i, j]);

        //prob = prob2;

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
