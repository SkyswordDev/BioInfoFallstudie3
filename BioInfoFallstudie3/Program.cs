using System.Text;
using BioInfoFallstudie3;
using BioInfoFallstudie3.Data;

public class Program
{
    public static void Main()
    {
        Console.WriteLine((1d/6d));

        string diceResults = "5232261464466441435225126522642312556132443456526366163666666322664352516321436566646526566652615164361561666666132326521113334426663663653645145642523254632465354222164246161355365662656664661513544256626622651313614611331631311664535355561552126135644244515532651456556633614666616516666215331431415232261466666441435225126522642312556132443456526366163666666322664352516321436566646526566652615164461561666666132326521113334426663663653645145642523254632465354666164243161355365662656";
        string diceResultsReversed = string.Join("", diceResults.Reverse());

        //string[] observations = diceResults.ToCharArray().Select(c => c.ToString()).ToArray();

        //foreach(string observ in observations)
        //{
        //    Console.WriteLine(observ);
        //}




        //int[] observations = diceResults.ToCharArray().Select(c => int.Parse(c.ToString()) - 1).ToArray();
        //ViterbiV2.Run(ViterbiProbabilityCalculationProviders<double>.Normal, Posteriori.testModel, diceResults.ToCharArray().Select(c => c.ToString()).ToArray());
        return;
        //foreach (Dictionary<int, double> dict in Posteriori.RunTest())
        //{
        //    HiddenMarkovModel<string, string, double> hmm = Posteriori.testModel;
        //    Console.WriteLine("{ " + string.Join(", ", dict.Select((KeyValuePair<int, double> kvp) => $"{hmm.States[kvp.Key]}: {kvp.Value}")) + " }");
        //}
        //Console.WriteLine();
        //foreach (Dictionary<int, double> dict in Posteriori.RunTest2())
        //{
        //    HiddenMarkovModel<string, string, double> hmm = Posteriori.testModel;
        //    Console.WriteLine("{ " + string.Join(", ", dict.Select((KeyValuePair<int, double> kvp) => $"{hmm.States[kvp.Key]}: {kvp.Value}")) + " }");
        //}

        //return;

        double diceSwapProbably = 1d/20d;

        
        ViterbiDicePrintResults(diceResults, diceSwapProbably);
        ViterbiDicePrintResults(diceResultsReversed, diceSwapProbably, true);

        //Console.WriteLine("Observations: " + string.Join(", ", observations));
        //Console.WriteLine("Most likely states: " + string.Join(" -> ", pathNames!));
    }

    private static string ViterbiDicePrintResults(string diceResults, double diceSwapProbably, bool writeReversed = false)
    {
        string[] pathNames = ViterbiDice(diceResults, diceSwapProbably);
        if (writeReversed)
        {
            pathNames = pathNames.Reverse().ToArray();
        }
        string  result = string.Join(string.Empty, pathNames!);
        Console.WriteLine(result);
        return result;
    }

    private static string[] ViterbiDice(string diceResults, double diceSwapProbably)
    {
        // Hidden states: F = fair, L = loaded
        int numStates = 2;
        string[] stateNames = [ "F", "L" ];
        //string[] obsVocab = [ "1", "2", "3", "4", "5", "6" ];

        // P(S0)
        double[] start = { 0.5, 0.5 };

        // P(S_t | S_{t-1})
        double[,] trans =
        {
            { 1 - diceSwapProbably, diceSwapProbably },
            { diceSwapProbably, 1 - diceSwapProbably }
        };

        double oneSixth = 1d/6d;
        // P(O | S)
        double[,] emit =
        {
            // normal dice:
            { oneSixth, oneSixth, oneSixth, oneSixth, oneSixth, oneSixth },
            // loaded dice
            { 0.1, 0.1, 0.1, 0.1, 0.1, 0.5 },
        };

        int[] observations = diceResults.ToCharArray().Select(c => int.Parse(c.ToString()) - 1).ToArray();

        var pathNames = Viterbi.Run(
            ViterbiProbabilityCalculationProviders<double>.Normal,
            numStates,
            start,
            trans,
            emit,
            observations
        );

        return pathNames!.Select((int i) => i switch { 0 => "F", _ => "L" }).ToArray();
    }

    //private static string[] ViterbiDice(string diceResults, double diceSwapProbably)
    //{
    //    // Hidden states: F = fair, L = loaded
    //    int numStates = 2;
    //    string[] stateNames = [ "F", "L" ];
    //    string[] obsVocab = [ "1", "2", "3", "4", "5", "6" ];

    //    // P(S0)
    //    double[] start = { 0.5, 0.5 };

    //    // P(S_t | S_{t-1})
    //    double[,] trans =
    //    {
    //        { 1 - diceSwapProbably, diceSwapProbably },
    //        { diceSwapProbably, 1 - diceSwapProbably }
    //    };

    //    double oneSixth = 1d/6d;
    //    // P(O | S)
    //    double[,] emit =
    //    {
    //        // normal dice:
    //        { oneSixth, oneSixth, oneSixth, oneSixth, oneSixth, oneSixth },
    //        // loaded dice
    //        { 0.1, 0.1, 0.1, 0.1, 0.1, 0.5 },
    //    };

    //    string[] observations = diceResults.ToCharArray().Select(c => c.ToString()).ToArray();

    //    var (_, pathNames) = ViterbiOld2.Run(
    //        observations,
    //        obsVocab,
    //        numStates,
    //        start,
    //        trans,
    //        emit,
    //        stateNames
    //    );

    //    return pathNames!;
    //}

    //public static void Main()
    //{
    //    // Hidden states: 0=Rainy, 1=Sunny
    //    // Observations: 0=walk, 1=shop, 2=clean
    //    int numStates = 2;
    //    var stateNames = new[] { "Rainy", "Sunny" };
    //    var obsVocab = new[] { "walk", "shop", "clean" };

    //    // P(S0)
    //    double[] start = { 0.6, 0.4 };

    //    // P(S_t | S_{t-1})
    //    double[,] trans =
    //    {
    //        { 0.7, 0.3 },
    //        { 0.4, 0.6 }
    //    };

    //    // P(O | S)
    //    double[,] emit =
    //    {
    //        // from Rainy:   walk, shop, clean
    //        { 0.1, 0.4, 0.5 },
    //        // from Sunny:
    //        { 0.6, 0.3, 0.1 }
    //    };

    //    var observations = new[] { "walk", "shop", "clean" };

    //    var (_, pathNames) = Viterbi.Run(
    //        observations,
    //        obsVocab,
    //        numStates,
    //        start,
    //        trans,
    //        emit,
    //        stateNames
    //    );

    //    Console.WriteLine("Observations: " + string.Join(", ", observations));
    //    Console.WriteLine("Most likely states: " + string.Join(" -> ", pathNames!));
    //}
}