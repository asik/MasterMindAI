using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterMindAI {
    class UI {

        AI m_ai;
        Rules m_rules;

        public void StartGame() {
            m_rules = Rules.Hells;
            m_ai = new AI(m_rules);

            Console.WriteLine("Let's play a game of Hells!");
            Console.WriteLine("I will provide you with guesses and you will tell me their evaluation.");
            Console.WriteLine("Give me just a few seconds to initialize everything...");

            while (true) {
                var guess = m_ai.GenerateGuess();
                PrintGuess(guess);
                var evaluation = GetEvaluation();
                m_ai.CommitGuess(guess, evaluation);
            }
        }

        Truth GetEvaluation() {
            Console.WriteLine("Please enter the evaluation. Example: \"011\" for one \"white\" and two \"blacks\"");
            var input = Console.ReadLine();
            while (!IsValid(input)) {
                Console.WriteLine("Invalid Input.");
                Console.WriteLine("Please enter the evaluation. Example: \"011\" for one \"white\" and two \"blacks\"");
                input = Console.ReadLine();
            }

            return new Truth { 
                NumBlacks = input.Count(c => c == '1'), 
                NumWhites = input.Count(c => c == '0') 
            };
        }

        bool IsValid(string input) {
            return
                input.Length <= m_rules.RowWidth &&
                input.All(i => i == '0' || i == '1');
        }

        void PrintGuess(int[] guess) {
            Console.WriteLine("I suggest you try:");
            foreach (var value in guess) {
                Console.Write("{0} ", m_rules.CodePegs[value]);
            }
            Console.Write("\n");
        }
    }
}
