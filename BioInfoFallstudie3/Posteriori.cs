using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioInfoFallstudie3;



public class Posteriori
{
    public static string[] observations = ["normal", "cold", "dizzy", "normal", "cold"];
    public static string[] observationsReversed = observations.Reverse().ToArray();

    public static List<Dictionary<string, double>> RunTest()
    {
        string[] states = ["Healthy", "Fever"];
        string end_state = "E";

        Dictionary<string, double> start_probability = new(){ { "Healthy", 0.6 }, { "Fever", 0.4} };

        Tricktionary<string, string, double> transition_probability = new(){
            { "Healthy", new() { { "Healthy", 0.69 }, { "Fever", 0.3 }, { "E", 0.01 } } },
            { "Fever", new() { { "Healthy", 0.4 }, { "Fever", 0.59 }, { "E", 0.01 } } },
        };

        Tricktionary<string, string, double> emission_probability = new(){
            { "Healthy", new() { { "normal", 0.5 }, { "cold", 0.4 }, { "dizzy", 0.1 } } },
            { "Fever", new() { { "normal", 0.1 }, { "cold", 0.3 }, { "dizzy", 0.6 } } },
        };

        return Run(observations, states, start_probability, transition_probability, emission_probability, end_state);
    }

    public static List<Dictionary<string, double>> RunTest2()
    {
        string[] states = ["Healthy", "Fever"];
        string end_state = "E";

        Dictionary<string, double> start_probability = new(){ { "Healthy", 0.6 }, { "Fever", 0.4} };

        Tricktionary<string, string, double> transition_probability = new(){
            { "Healthy", new() { { "Healthy", 0.69 }, { "Fever", 0.3 }, { "E", 0.01 } } },
            { "Fever", new() { { "Healthy", 0.4 }, { "Fever", 0.59 }, { "E", 0.01 } } },
        };

        Tricktionary<string, string, double> emission_probability = new(){
            { "Healthy", new() { { "normal", 0.5 }, { "cold", 0.4 }, { "dizzy", 0.1 } } },
            { "Fever", new() { { "normal", 0.1 }, { "cold", 0.3 }, { "dizzy", 0.6 } } },
        };

        return Run(observationsReversed, states, start_probability, transition_probability, emission_probability, end_state);
    }

    public static List<Dictionary<string, double>> Run(string[] observations, string[] states, Dictionary<string, double> start_probabilities, Tricktionary<string, string, double> transition_probabilities, Tricktionary<string, string, double> emmission_probabilities, string end_st)
    {
        //# Forward part of the algorithm
        List<Dictionary<string, double>> forwardProbabilities = [];
        double previousForwardProbabilitiesSum;
        Dictionary<string, double> currentForwardStateProbabilities = new();
        Dictionary<string, double> previousForwardStateProbabilities = new();

        for (int i = 0; i < observations.Length; i++)
        {
            currentForwardStateProbabilities = new();
            string observation_i = observations[i];

            double probabilitySum = 0;

            foreach (string st in states)
            {
                if (i == 0)
                {
                    // base case for the forward part
                    previousForwardProbabilitiesSum = start_probabilities[st];
                }
                else
                {
                    previousForwardProbabilitiesSum = 0;
                    foreach (var k in states)
                    {
                        previousForwardProbabilitiesSum += previousForwardStateProbabilities[k] * transition_probabilities[k][st];
                    }
                }

                double nextValue = emmission_probabilities[st][observation_i] * previousForwardProbabilitiesSum;
                currentForwardStateProbabilities[st] = nextValue;
                probabilitySum += nextValue;
            }

            double scaling = 1 / probabilitySum;

            foreach (string key in states)
            {
                currentForwardStateProbabilities[key] = currentForwardStateProbabilities[key] * scaling;
            }

            forwardProbabilities.Add(currentForwardStateProbabilities);
            previousForwardStateProbabilities = currentForwardStateProbabilities;
        }

        double p_fwd = 0;
        foreach (var k in states)
        {
            p_fwd += currentForwardStateProbabilities[k] * transition_probabilities[k][end_st];
        }

        // Backward part of the algorithm
        List<Dictionary<string, double>> backwardProbabilities = [];
        Dictionary<string, double> currentBackwardStateProbabilities = new();
        Dictionary<string, double> previousBackwardStateProbabilties = new();
        string[] reversedObservations = observations[1..].Append("None").Reverse().ToArray();
        for (int i = 0; i < reversedObservations.Length; i++)
        {
            string observation_i_plus = reversedObservations[i];
            currentBackwardStateProbabilities = new();
            double probabilitySum = 0;
            foreach (string st in states)
            {
                if (i == 0)
                {
                    // base case for backward part
                    currentBackwardStateProbabilities[st] = transition_probabilities[st][end_st];
                }
                else
                {
                    double sum = 0;
                    foreach (string l in states)
                    {
                        double transitionProbability = transition_probabilities[st][l];
                        double emmissionProbability = emmission_probabilities[l][observation_i_plus];
                        double previousBackwardStateProb = previousBackwardStateProbabilties[l];
                        sum += transitionProbability * emmissionProbability * previousBackwardStateProb;
                    }
                    currentBackwardStateProbabilities[st] = sum;
                }
                probabilitySum += currentBackwardStateProbabilities[st];
            }

            double scaling = 1 / probabilitySum;

            foreach (string key in states)
            {
                currentBackwardStateProbabilities[key] = currentBackwardStateProbabilities[key] * scaling;
            }

            backwardProbabilities.Add(currentBackwardStateProbabilities);
            previousBackwardStateProbabilties = currentBackwardStateProbabilities;
        }
        backwardProbabilities.Reverse();

        //p_bkw = sum(start_prob[l] * emm_prob[l][observations[0]] * b_curr[l] for l in states)
        double p_bkw = 0;
        foreach (string l in states)
        {
            p_bkw = start_probabilities[l] * emmission_probabilities[l][observations[0]] * currentBackwardStateProbabilities[l];
        }

        //# Merging the two parts
        List<Dictionary<string, double>> posterior = [];
        for (int i = 0; i < observations.Length; i++)
        {
            Dictionary<string, double> tmp = new();
            foreach (string st in states)
            {
                tmp.Add(st, forwardProbabilities[i][st] * backwardProbabilities[i][st] / p_fwd);
            }

            posterior.Add(tmp);
        }

        if (Math.Abs(p_fwd - p_bkw) < 0.00000001)
        {
            // these should be equal
        }

        //return fwd, bkw, posterior
        return posterior;
    }
}
