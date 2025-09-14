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
        List<Dictionary<string, double>> fwd = [];
        double prev_f_sum;
        Dictionary<string, double> f_curr = new();
        Dictionary<string, double> f_prev = new();

        for (int i = 0; i < observations.Length; i++)
        {
            f_curr = new();
            string observation_i = observations[i];

            foreach (string st in states)
            {
                if (i == 0)
                {
                    // base case for the forward part
                    prev_f_sum = start_probabilities[st];
                }
                else
                {
                    prev_f_sum = 0;
                    foreach (var k in states)
                    {
                        prev_f_sum += f_prev[k] * transition_probabilities[k][st];
                    }
                }

                double nextValue = emmission_probabilities[st][observation_i] * prev_f_sum;
                f_curr[st] = nextValue;
            }

            fwd.Add(f_curr);
            f_prev = f_curr;
        }

        double p_fwd = 0;
        foreach (var k in states)
        {
            p_fwd += f_curr[k] * transition_probabilities[k][end_st];
        }

        //# Backward part of the algorithm
        List<Dictionary<string, double>> bkw = [];
        Dictionary<string, double> b_curr = new();
        Dictionary<string, double> bs_prev = new();
        string[] reversed_observations = observations[1..].Append("None").Reverse().ToArray();
        for (int i = 0; i < reversed_observations.Length; i++)
        {
            string observation_i_plus = reversed_observations[i];
            b_curr = new();

            foreach (string st in states)
            {
                if (i == 0)
                {
                    // base case for backward part
                    b_curr[st] = transition_probabilities[st][end_st];
                }
                else
                {
                    double sum = 0;
                    foreach(string l in states)
                    {
                        double transition_probability = transition_probabilities[st][l];
                        double emmission_probability = emmission_probabilities[l][observation_i_plus];
                        double b_prev = bs_prev[l];
                        sum += transition_probability * emmission_probability * b_prev;
                    }
                    b_curr[st] = sum;
                }
            }

            bkw.Add(b_curr);
            bs_prev = b_curr;
        }
        bkw.Reverse();

        //p_bkw = sum(start_prob[l] * emm_prob[l][observations[0]] * b_curr[l] for l in states)
        double p_sum = 0;
        foreach (string l in states)
        {
            p_sum = start_probabilities[l] * emmission_probabilities[l][observations[0]] * b_curr[l];
        }

        //# Merging the two parts
        List<Dictionary<string, double>> posterior = [];
        for (int i = 0; i < observations.Length; i++)
        {
            Dictionary<string, double> tmp = new();
            foreach(string st in states)
            {
                tmp.Add(st, fwd[i][st] * bkw[i][st] / p_fwd);
            }

            posterior.Add(tmp);
        }

        //assert p_fwd == p_bkw
        //return fwd, bkw, posterior
        return posterior;
    }
}
