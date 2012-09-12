using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

//BUG: EvaluateTruth function is incorrect for regular Mastermind rules, ok for Hells though.
//TODO: ComputeGuess could relatively easily be multithreaded by splitting the possibilities evenly
//      across all cores.

namespace MasterMindAI {

    /// <summary>
    /// Represents the evaluation of a guess: number of blacks and whites. The guess is true if 
    /// NumWhites == 0 and NumBlacks == the number of pegs in the guess.
    /// </summary>
    struct Truth {
        /// <summary>
        /// A white identifies that one peg in the guess is also in the answer, but at another place.
        /// </summary>
        public int NumWhites;
        /// <summary>
        /// A black identifies that one peg in the guess is also in the answer, at the same place.
        /// </summary>
        public int NumBlacks;

        public bool IsEqualTo(Truth other) {
            return NumWhites == other.NumWhites && NumBlacks == other.NumBlacks;
        }

        public override string ToString() {
            return string.Format("{0} whites, {1} blacks", NumWhites, NumBlacks);
        }
    }

    class AI {
        int[] m_secret;
        Rules m_rules;
        Dictionary<int[], int> m_guessScores;
        List<int[]> m_possibles;
        List<int[]> m_tries;

        public AI(Rules rules) {
            m_rules = rules;
            InitGame();

            //while (m_possibles.Count > 0) {
            //    PlayMove();
            //}
            //Console.WriteLine("Found combination in {0} tries!", m_tries.Count);
            //DisplayCombination(m_tries[m_tries.Count -1]);
            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();
        }

        void DisplayCombination(int[] combination) {
            foreach (var value in combination) {
                Console.Write("{0} ", m_rules.CodePegs[value]);
            }
            Console.Write("\n");
        }



        public void CommitGuess(int[] guess, Truth evaluation) {
            m_tries.Add(guess);

            m_possibles.Remove(guess);
            m_possibles.RemoveAll(possibility => !evaluation.IsEqualTo(EvaluateTruth(guess, possibility)));
        }

        void PlayMove() {
            Console.WriteLine("Currently {0} possibilities. Thinking...", m_possibles.Count);

            InitGuessScores();
            var guess = GenerateGuess();
            m_tries.Add(guess);
            var guessTruth = EvaluateTruth(guess, m_secret);

            m_possibles.Remove(guess);
            m_possibles.RemoveAll(possibility => !guessTruth.IsEqualTo(EvaluateTruth(guess, possibility)));

            Console.WriteLine("Reduced the set to only {0} possibilities with the following combination :", m_possibles.Count);
            DisplayCombination(guess);
            Console.WriteLine("Evaluation : {0}", guessTruth);
            Console.WriteLine("===================================\n");
        }

        public int[] GenerateGuess() {
            // If there are too many possibilities, like typically on the first
            // few moves (1 and often 2 for Hells and Mastermind), use a generic
            // "always good" move. This is still optimal on the first move, but not
            // on any subsequent move; it's a trade-off.
            // Right now we use a constant but it could be generated based on computer performance.
            InitGuessScores();
            if (m_possibles.Count > 300) {
                return Enumerable.Range(m_tries.Count, m_rules.RowWidth).ToArray();
            }
            else {
                return ComputeGuess();
            }
        }

        int[] ComputeGuess() {
            var highScore = -1;
            var bestGuess = new int[m_rules.RowWidth];
            //int count = 0;

            // For each possible secret, evalute how all guesses would score. This is described in EvaluteScore.
            // The highest scoring guess overall for all possible secrets is our best guess.
            foreach (var possibleSecret in m_possibles) {

                //var stopWatch = new Stopwatch();
                //stopWatch.Start();

                EvaluatePossibility(ref highScore, ref bestGuess, possibleSecret);

                //stopWatch.Stop();
                //Console.WriteLine("Evaluated a possibility in {0} milliseconds", stopWatch.ElapsedMilliseconds);
                //Console.WriteLine("{0} remaining.", m_possibles.Count - ++count);
            }

            return bestGuess;
        }

        void EvaluatePossibility(ref int highScore, ref int[] bestGuess, int[] possibleSecret) {
            var keys = new List<int[]>(m_guessScores.Keys);
            // Minimax algorithm: return the guess with the highest minimum score.
            foreach (var guess in keys) {
                var oldScore = m_guessScores[guess];
                var newScore = EvaluateScore(guess, possibleSecret);
                if (newScore < oldScore) {
                    m_guessScores[guess] = newScore;
                    oldScore = newScore;
                }
                if (oldScore > highScore) {
                    highScore = oldScore;
                    bestGuess = guess;
                }
            }
        }

        /// <summary>
        /// Returns how good "guess" would be if "possibleSecret" was the secret.
        /// The score is just a count of all possibilities that could be crossed out.
        /// </summary>
        int EvaluateScore(int[] guess, int[] possibleSecret) {
            var guessTruth = EvaluateTruth(guess, possibleSecret);
            int score = 0;
            // Increment score for each possibility that would not give the same evaluation,
            // i.e. each one that is not in fact a possibility.
            foreach (var possibility in m_possibles) {
                if (!guessTruth.IsEqualTo(EvaluateTruth(possibility, possibleSecret))) {
                    ++score;
                }
            }
            return score;
        }

        void InitGame() {
            //InitSecret();
            InitPossibles();
            m_tries = new List<int[]>();
        }

        void InitSecret() {
            var valueSet = Enumerable.Range(0, m_rules.NumCodePegs).ToList();
            var randomGen = new Random();
            m_secret = new int[m_rules.RowWidth];
            for (int i = 0; i < m_rules.RowWidth; ++i) {
                int randomIndex = randomGen.Next(valueSet.Count);
                m_secret[i] = valueSet[randomIndex];
                if (!m_rules.RepetitionAllowed) {
                    valueSet.RemoveAt(randomIndex);
                }
            }

            Console.WriteLine("Secret combination generated!");
            DisplayCombination(m_secret);
        }

        void InitPossibles() {
            m_possibles = new List<int[]>();
            if (m_rules.RepetitionAllowed) {
                GeneratePossiblesWithRepetition();
            }
            else {
                GeneratePossiblesWithoutRepetition();
            }
            if (m_rules.CodePegs == Rules.Hells.CodePegs) {
                m_possibles.RemoveAll(p => p.Contains(Array.IndexOf(m_rules.CodePegs, "Avernus")));
                m_possibles.RemoveAll(p => m_rules.CodePegs[p[0]] != "Nessus");
            }
        }

        void InitGuessScores() {
            m_guessScores = new Dictionary<int[], int>(m_possibles.Count);
            m_possibles.ForEach(i => m_guessScores[i] = Int32.MaxValue);
        }

        void GeneratePossiblesWithoutRepetition() {
            var currentPossible = new int[m_rules.RowWidth];
            GeneratePossiblesWithoutRepetitionImpl(0, ref currentPossible, new List<int>(Enumerable.Range(0, m_rules.NumCodePegs)));
        }

        void GeneratePossiblesWithoutRepetitionImpl(int column, ref int[] currentPossible, List<int> valueSet) {
            if (column == m_rules.RowWidth) {
                m_possibles.Add(currentPossible);
                var nextPossible = new int[currentPossible.Length];
                Array.Copy(currentPossible, nextPossible, column);
                currentPossible = nextPossible;
                return;
            }

            for (var i = 0; i < valueSet.Count; ++i) {
                var currentValue = valueSet[i];
                valueSet.RemoveAt(i);
                currentPossible[column] = currentValue;
                GeneratePossiblesWithoutRepetitionImpl(column + 1, ref currentPossible, valueSet);
                valueSet.Insert(i, currentValue);
            }
        }

        void GeneratePossiblesWithRepetition(int column = 0) {
            if (column == m_rules.RowWidth) {
                return;
            }

            if (m_possibles.Count == 0) {
                m_possibles.Add(new int[m_rules.RowWidth]);
            }

            for (var i = 1; i < m_rules.NumCodePegs; ++i) {
                GeneratePossiblesWithRepetition(column + 1);
                var lastPossible = m_possibles[m_possibles.Count - 1];
                var newPossible = new int[m_rules.RowWidth];
                Array.Copy(lastPossible, newPossible, column);
                newPossible[column] = i;
                m_possibles.Add(newPossible);
            }
            GeneratePossiblesWithRepetition(column + 1);
        }

        /// <summary>
        /// Evaluates the truth of the guess compared to the supplied secret
        /// </summary>
        Truth EvaluateTruth(int[] guess, int[] secret) {
            var evaluation = new Truth();
            var rowWidth = m_rules.RowWidth;
            for (var i = 0; i < rowWidth; ++i) {
                if (guess[i] == secret[i]) {
                    ++evaluation.NumBlacks;
                }
                else if (secret.Contains(guess[i])) {
                    ++evaluation.NumWhites;
                }
            }

            return evaluation;
        }
    }

   
}
