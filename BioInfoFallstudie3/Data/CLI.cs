using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioInfoFallstudie3.Data;

public static class CLI
{

    public static void Run()
    {
        while(true)
        {
            RunHmm();
        }

    }

    private static void RunHmm()
    {
        Console.WriteLine("Enter path to file containing hidden states (1 per Line, whitespace lines are ignored):");
        string hiddenStatePath = Console.ReadLine()!;
        if (hiddenStatePath.Length == 0)
            hiddenStatePath = "C:\\Users\\Dev\\source\\repos\\BioInfoFallstudie3\\BioInfoFallstudie3\\ExampleInput\\states.txt";
        string[] hiddenStates = File.ReadAllLines(hiddenStatePath).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        Console.WriteLine($"Found {hiddenStates.Length} hidden states:");
        foreach (string hiddenState in hiddenStates)
        {
            Console.WriteLine(hiddenState);
        }
        Console.WriteLine();

        Console.WriteLine("Enter path to file containing emissions (1 per Line, whitespace lines are ignored):");
        string emitsPath = Console.ReadLine()!;
        if (emitsPath.Length == 0)
            emitsPath = "C:\\Users\\Dev\\source\\repos\\BioInfoFallstudie3\\BioInfoFallstudie3\\ExampleInput\\emissions.txt";
        string[] emits = File.ReadAllLines(emitsPath).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        Console.WriteLine($"Found {emits.Length} emissions:");
        foreach (string emit in emits)
        {
            Console.WriteLine(emit);
        }
        Console.WriteLine();

        Console.WriteLine("Enter path to file containing initial hidden state probabilities (1 Line per hidden state, whitespace lines are ignored):");
        string initialProbabilitiesPath = Console.ReadLine()!;
        if (initialProbabilitiesPath.Length == 0)
            initialProbabilitiesPath = "C:\\Users\\Dev\\source\\repos\\BioInfoFallstudie3\\BioInfoFallstudie3\\ExampleInput\\initialStateProbabilities.txt";
        double[] initialProbabilities = File.ReadAllLines(initialProbabilitiesPath).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();

        if (initialProbabilities.Length != hiddenStates.Length)
        {
            Console.WriteLine($"The file contained {initialProbabilities.Length} non-whitespace lines, but was expected to contain {hiddenStates.Length}.");
            return;
        }

        Console.WriteLine($"Initial probabilities:");
        for (int i = 0; i < hiddenStates.Length; i++)
        {
            Console.WriteLine($"{hiddenStates[i]}: {initialProbabilities[i]}");
        }
        Console.WriteLine();


        Console.WriteLine("Enter path to file containing hidden state transition probabilities (one row per state, one column per state):");
        string stateTransitionsPath = Console.ReadLine()!;
        if (stateTransitionsPath.Length == 0)
            stateTransitionsPath = "C:\\Users\\Dev\\source\\repos\\BioInfoFallstudie3\\BioInfoFallstudie3\\ExampleInput\\stateTransitionProbabilities.txt";
        if (!TryReadMatrixFromFile(stateTransitionsPath, hiddenStates.Length, hiddenStates.Length, out double[,] stateTransitionProbabilities))
        {
            return;
        }

        Console.WriteLine($"State Transmission Probabilities:");
        for (int i = 0; i < stateTransitionProbabilities.GetLength(0); i++)
        {
            for(int j = 0; j < stateTransitionProbabilities.GetLength(1); j++)
            {
                Console.Write("{0:N2} ", stateTransitionProbabilities[i, j]);
            }
            Console.WriteLine();
        }
        Console.WriteLine();


        Console.WriteLine("Enter path to file containing hidden state emission probabilities (one row per state, one column per emission):");
        string stateEmissionsPath = Console.ReadLine()!;
        if (stateEmissionsPath.Length == 0)
            stateEmissionsPath = "C:\\Users\\Dev\\source\\repos\\BioInfoFallstudie3\\BioInfoFallstudie3\\ExampleInput\\emissionProbabilities.txt";
        if (!TryReadMatrixFromFile(stateEmissionsPath, hiddenStates.Length, emits.Length, out double[,] stateEmissionProbabilities))
        {
            return;
        }

        Console.WriteLine($"State Emission Probabilities:");
        for (int i = 0; i < stateEmissionProbabilities.GetLength(0); i++)
        {
            for (int j = 0; j < stateEmissionProbabilities.GetLength(1); j++)
            {
                Console.Write("{0:N2} ", stateEmissionProbabilities[i, j]);
            }
            Console.WriteLine();
        }
        Console.WriteLine();

        Console.WriteLine("Enter path to file containing actual observations (1 per Line, whitespace lines are ignored):");
        string observationsPath = Console.ReadLine()!;
        if (observationsPath.Length == 0)
            observationsPath = "C:\\Users\\Dev\\source\\repos\\BioInfoFallstudie3\\BioInfoFallstudie3\\ExampleInput\\observations.txt";
        string[] observations = File.ReadAllLines(observationsPath).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        Console.WriteLine($"Found {observations.Length} observations.");
        Console.WriteLine();

        string logarithmicConfirm = null;
        while (string.IsNullOrEmpty(logarithmicConfirm))
        {
            Console.WriteLine("Do you want to use the logarithmic transformation? (y/n)");
            logarithmicConfirm = Console.ReadLine().ToLower();
        }

        IViterbiProbabilityCalculationProvider<double> viterbiProbabilityCalculationProvider = ViterbiProbabilityCalculationProviders<double>.Normal;
        if (logarithmicConfirm.Equals("y"))
        {
            viterbiProbabilityCalculationProvider = ViterbiProbabilityCalculationProviders<double>.Logarithmic;
        }

        string[] viterbiResult = Viterbi.Run<string>(viterbiProbabilityCalculationProvider, hiddenStates, initialProbabilities, stateTransitionProbabilities, stateEmissionProbabilities, GetObservationIndexes(emits.ToList(), observations).ToArray());
        Console.WriteLine("Found the following Viterbi-Path:");

        string  result = string.Join(string.Empty, viterbiResult);
        Console.WriteLine(result);
    }

    private static bool TryReadMatrixFromFile(string path, int expectedLineCount, int expectedValueCount, out double[,] result)
    {
        string[] lines = File.ReadAllLines(path).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (lines.Length != expectedLineCount)
        {
            Console.WriteLine($"The file contained {lines.Length} non-whitespace lines but was expected to contain {expectedLineCount} non whitepsace lines.");
            result = default;
            return false;
        }

        result = new double[expectedLineCount, expectedValueCount];

        for(int i = 0; i < expectedLineCount; i++)
        {
            string line = lines[i];
            double[] entries = line.Trim().Split().Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            if (entries.Length != expectedValueCount)
            {
                Console.WriteLine($"Not enough values in line {i}. Expected {expectedValueCount} but got {entries.Length}.");
                result = default;
                return false;
            }

            for(int j = 0; j < expectedValueCount; j++)
            {
                result[i, j] = entries[j];
            }
        }

        return true;
    }

    public static ImmutableArray<int> GetObservationIndexes(List<string> observationStates, IReadOnlyList<string> observationsList)
    {
        ImmutableArray<int>.Builder resultBuilder = ImmutableArray.CreateBuilder<int>(observationsList.Count);
        for (int i = 0; i < observationsList.Count; i++)
        {
            resultBuilder.Add(observationStates.IndexOf(observationsList[i]));
        }
        return resultBuilder.ToImmutable();
    }

}

