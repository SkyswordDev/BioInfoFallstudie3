public static class ViterbiOld2
{
    /// <summary>
    /// Runs the Viterbi algorithm on a discrete HMM using log probabilities.
    /// All probability inputs should be regular (0..1); they are converted to log-space internally.
    /// </summary>
    /// <param name="observations">Sequence of observations as integer IDs [0..numObsSymbols-1]</param>
    /// <param name="numStates">Number of hidden states</param>
    /// <param name="startProb">Start probabilities P(state) of length numStates</param>
    /// <param name="transProb">Transition matrix P(s_t | s_{t-1}) of size [numStates, numStates]</param>
    /// <param name="emitProb">Emission matrix P(obs | state) of size [numStates, numObsSymbols]</param>
    /// <returns>Most likely state path as an array of state IDs</returns>
    public static int[] Run(
        int[] observations,
        int numStates,
        double[] startProb,
        double[,] transProb,
        double[,] emitProb)
    {
        if (observations == null || observations.Length == 0)
            throw new ArgumentException("observations empty");
        if (startProb.Length != numStates)
            throw new ArgumentException("startProb length mismatch");
        if (transProb.GetLength(0) != numStates || transProb.GetLength(1) != numStates)
            throw new ArgumentException("transProb shape mismatch");
        if (emitProb.GetLength(0) != numStates)
            throw new ArgumentException("emitProb state dimension mismatch");

        int T = observations.Length;
        int M = emitProb.GetLength(1); // number of observation symbols
        foreach (var o in observations)
            if (o < 0 || o >= M)
                throw new ArgumentOutOfRangeException($"observation {o} outside 0..{M - 1}");

        // Convert to log space (log(0) = -Infinity)
        double[] logStart = startProb.Select(LogSafe).ToArray();
        double[,] logTrans = Map2D(transProb, LogSafe);
        double[,] logEmit  = Map2D(emitProb,  LogSafe);

        // DP tables
        double[,] dp = new double[T, numStates]; // dp[t, s] = best log prob ending in state s at time t
        int[,] back = new int[T, numStates];     // back pointers

        // Initialization (t = 0)
        int o0 = observations[0];
        for (int s = 0; s < numStates; s++)
        {
            dp[0, s] = logStart[s] + logEmit[s, o0];
            back[0, s] = -1; // start marker
        }

        // Recurrence (t = 1..T-1)
        for (int t = 1; t < T; t++)
        {
            int ot = observations[t];
            for (int s = 0; s < numStates; s++)
            {
                double best = double.NegativeInfinity;
                int bestPrev = -1;
                for (int sp = 0; sp < numStates; sp++)
                {
                    double cand = dp[t - 1, sp] + logTrans[sp, s];
                    if (cand > best)
                    {
                        best = cand;
                        bestPrev = sp;
                    }
                }
                dp[t, s] = best + logEmit[s, ot];
                back[t, s] = bestPrev;
            }
        }

        // Termination: pick best final state
        double bestFinal = double.NegativeInfinity;
        int lastState = -1;
        for (int s = 0; s < numStates; s++)
        {
            if (dp[T - 1, s] > bestFinal)
            {
                bestFinal = dp[T - 1, s];
                lastState = s;
            }
        }

        // Path backtrace
        int[] path = new int[T];
        int cur = lastState;
        for (int t = T - 1; t >= 0; t--)
        {
            path[t] = cur;
            cur = back[t, cur];
        }

        return path;
    }

    /// <summary>
    /// Convenience overload that accepts observations as strings and maps them to indices.
    /// Provide stateNames to map state IDs back to names (optional).
    /// </summary>
    public static (int[] statePath, string[]? stateNamesPath) Run(
        IList<string> observations,
        IList<string> obsVocabulary,
        int numStates,
        double[] startProb,
        double[,] transProb,
        double[,] emitProb,
        IList<string>? stateNames = null)
    {
        var obsToId = new Dictionary<string, int>(obsVocabulary.Count);
        for (int i = 0; i < obsVocabulary.Count; i++)
            obsToId[obsVocabulary[i]] = i;

        int[] obsIds = observations.Select(o =>
            obsToId.TryGetValue(o, out int id) ? id
            : throw new ArgumentException($"Observation '{o}' not in vocabulary")).ToArray();

        var path = Run(obsIds, numStates, startProb, transProb, emitProb);

        string[]? namesPath = null;
        if (stateNames != null && stateNames.Count == numStates)
        {
            namesPath = path.Select(i => stateNames[i]).ToArray();
        }

        return (path, namesPath);
    }

    // ---------- helpers ----------
    private static double LogSafe(double p) => p <= 0.0 ? double.NegativeInfinity : Math.Log(p);

    private static double[,] Map2D(double[,] a, Func<double, double> f)
    {
        int r = a.GetLength(0), c = a.GetLength(1);
        var b = new double[r, c];
        for (int i = 0; i < r; i++)
            for (int j = 0; j < c; j++)
                b[i, j] = f(a[i, j]);
        return b;
    }
}
