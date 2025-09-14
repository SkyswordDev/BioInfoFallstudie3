//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace BioInfoFallstudie3;
//public static class ViterbiOld
//{

//    public static int[] FindPath(int[] states, int[] init, int[,] trans, int[,] emit, int[] observations)
//    {
//        //input states: S hidden states
//        //input init: initial probabilities of each state
//        //input trans: S × S transition matrix
//        //input emit: S × O emission matrix
//        //input obs: sequence of T observations

//        int[,] prob = new int[states.Length, observations.Length]; // T × S matrix of zeroes
//        int[,] prev = new int[states.Length, observations.Length]; // empty T × S matrix
//        foreach (int state in states)
//        {
//            prob[0, state] = init[state] * emit[state, observations[0]];
//        }

//        for(int t = 1; t < observations.Length; t++)
//        {
//            foreach (int s in states)
//            {
//                int new_prob = 0;
//                foreach (int r in states)
//                {
//                    new_prob = prob[t - 1, r] * trans[r, s] * emit[s, observations[t]];
//                    if (new_prob > prob[t, s])
//                    {
//                        prob[t, s] = new_prob;
//                        prev[t, s] = r;
//                    }
//                }

//            }
//        }

//        int[] path = new int[observations.Length]; //empty array of length T
//        //path[observations.Length - 1] = //the state s with maximum prob[T - 1][s]
//        for(int t = observations.Length - 2; t >= 0; t--)
//        {

//            path[t] = prev[t + 1, path[t + 1]];
//        }

//        return path;
//    }

//}
