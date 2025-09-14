//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BioInfoFallstudie3.Data;

//namespace BioInfoFallstudie3;

//public static class Posteriori
//{
//    //public static List<Dictionary<int, double>> Run(HiddenMarkovModel<string, string, double> hiddenMarkovModel, string observations)
//    //{

//    //}

//    public static string[] states = ["Healthy", "Fever"];
//    public static string end_state = "E";

//    public static string[] observations = ["normal", "cold", "dizzy", "normal", "cold"];
//    public static string[] observationsReversed = observations.Reverse().ToArray();


//    public static HiddenMarkovModel<string, string, double> testModel = HiddenMarkovModel<string, string, double>.Construct(
//        states: ["Healthy", "Fever"],
//        observations: ["normal", "cold", "dizzy"],
//        initialStateProbabilities: new double[]{ 0.6, 0.4 },
//        transitionProbabilities: (new double[,]
//            {
//            /* Healthy */ { 0.69, 0.3, 0.01 },
//            /* Fever   */ { 0.4, 0.59, 0.01 },
//            }),
//        emissionProbabilities: (new double[,]
//            {
//            /* Healthy */ { 0.5, 0.4, 0.1 },
//            /* Fever   */ { 0.1, 0.3, 0.6 },
//            })
//        );

//    public static List<Dictionary<int, double>> RunTest()
//    {
//        //Dictionary<string, double> start_probability = new(){ { "Healthy", 0.6 }, { "Fever", 0.4} };

//        //Tricktionary<string, string, double> transition_probability = new(){
//        //    { "Healthy", new() { { "Healthy", 0.69 }, { "Fever", 0.3 }, { "E", 0.01 } } },
//        //    { "Fever", new() { { "Healthy", 0.4 }, { "Fever", 0.59 }, { "E", 0.01 } } },
//        //};

//        //Tricktionary<string, string, double> emission_probability = new(){
//        //    { "Healthy", new() { { "normal", 0.5 }, { "cold", 0.4 }, { "dizzy", 0.1 } } },
//        //    { "Fever", new() { { "normal", 0.1 }, { "cold", 0.3 }, { "dizzy", 0.6 } } },
//        //};

//        return Run(testModel.States.Length,
//            testModel.GetObservationIndexes(observations).ToArray(),
//            testModel.InitialStateProbabilities.Array.ToArray(),
//            testModel.TransitionProbabilities.Matrix,
//            testModel.EmissionProbabilities.Matrix, end_state);
//    }

//    public static List<Dictionary<int, double>> RunTest2()
//    {
//        return Run(testModel.States.Length,
//            testModel.GetObservationIndexes(observationsReversed).ToArray(),
//            testModel.InitialStateProbabilities.Array.ToArray(),
//            testModel.TransitionProbabilities.Matrix,
//            testModel.EmissionProbabilities.Matrix, end_state);
//    }

//    public static List<Dictionary<int, double>> Run(int stateCount, int[] observations, double[] initialStateProbabilities, Matrix2D<double> transition_probabilities, Matrix2D<double> emmission_probabilities, string end_st)
//    {
//        //# Forward part of the algorithm
//        double[,] forwardResults = new double[observations.Length, stateCount];
//        double prev_f_sum;

//        double[] f_curr = new double[stateCount];
//        double[] f_prev = new double[stateCount];
//        for (int observationIndex = 0; observationIndex < observations.Length; observationIndex++)
//        {
//            f_curr = new double[stateCount];
//            int observation_i = observations[observationIndex];

//            for(int state1Index = 0; state1Index < stateCount; state1Index++)
//            {
//                if (observationIndex == 0)
//                {
//                    // base case for the forward part
//                    prev_f_sum = initialStateProbabilities[state1Index];
//                }
//                else
//                {
//                    prev_f_sum = 0;
//                    for(int state2Index = 0; state2Index < stateCount; state2Index++)
//                    {
//                        prev_f_sum += f_prev[state2Index] * transition_probabilities[state2Index, state1Index];
//                    }
//                }

//                double nextValue = emmission_probabilities[state1Index, observation_i] * prev_f_sum;
//                f_curr[state1Index] = nextValue;
//            }

//            for(int stateIndex = 0; stateIndex < stateCount; stateIndex++)
//            {
//                forwardResults[observationIndex, stateIndex] = f_curr[stateIndex];
//            }
//            f_prev = f_curr;
//        }

//        double p_fwd = 0;
//        for(int stateIndex = 0; stateIndex < stateCount; stateIndex++)
//        {
//            p_fwd += f_curr[stateIndex] * transition_probabilities[stateIndex, stateCount]; //stateCount was end_st which is for some yet unknown reason in the transition probabilities
//        }

//        //# Backward part of the algorithm
//        double[,] backwardResults = new double[observations.Length, stateCount];
//        double[] b_curr = new double[stateCount];
//        double[] bs_prev = new double[stateCount];
//        int[] reversed_observations = observations[1..].Append(-1).Reverse().ToArray();
//        for (int observationIndex = 0; observationIndex < reversed_observations.Length; observationIndex++)
//        {
//            int observation_i_plus = reversed_observations[observationIndex];
//            b_curr = new double[stateCount];

//            for (int state1Index = 0; state1Index < stateCount; state1Index++)
//            {
//                if (observationIndex == 0)
//                {
//                    // base case for backward part
//                    b_curr[state1Index] = transition_probabilities[state1Index, stateCount];
//                }
//                else
//                {
//                    double sum = 0;
//                    for(int state2Index = 0; state2Index < stateCount; state2Index++)
//                    {
//                        double transition_probability = transition_probabilities[state1Index, state2Index];
//                        double emmission_probability = emmission_probabilities[state2Index, observation_i_plus];
//                        double b_prev = bs_prev[state2Index];
//                        sum += transition_probability * emmission_probability * b_prev;
//                    }
//                    b_curr[state1Index] = sum;
//                }
//            }

//            int observationIndexFromEnd = reversed_observations.Length - 1 - observationIndex;
//            for (int stateIndex = 0; stateIndex < stateCount; stateIndex++)
//            {
//                backwardResults[observationIndexFromEnd, stateIndex] = b_curr[stateIndex];
//            }
//            bs_prev = b_curr;
//        }

//        // the following line is no longer needed, the backwards results are written into the array from the end
//        //backwardResults.Reverse();

//        //p_bkw = sum(start_prob[l] * emm_prob[l][observations[0]] * b_curr[l] for l in states)
//        double p_sum = 0;
//        for(int stateIndex = 0; stateIndex < stateCount; stateCount++)
//        {
//            p_sum = initialStateProbabilities[stateIndex] * emmission_probabilities[stateIndex, observations[0]] * b_curr[stateIndex];
//        }

//        //# Merging the two parts
//        List<Dictionary<int, double>> posterior = [];
//        for (int observationIndex = 0; observationIndex < observations.Length; observationIndex++)
//        {
//            Dictionary<int, double> tmp = new();
//            for(int stateIndex = 0; stateIndex < stateCount; stateIndex++)
//            {
//                tmp.Add(stateIndex, forwardResults[observationIndex, stateIndex] * backwardResults[observationIndex, stateIndex] / p_fwd);
//            }

//            posterior.Add(tmp);
//        }

//        //assert p_fwd == p_bkw
//        //return fwd, bkw, posterior
//        return posterior;
//    }
//}
