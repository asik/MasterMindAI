using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterMindAI {


    /// <summary>
    /// A set of parameters to customize a game
    /// </summary>
    class Rules {
        /// <summary>
        /// A string representation of the different "color pegs" that can be used to form a combination.
        /// </summary>
        public string[] CodePegs { get; set; }
        /// <summary>
        /// The number of CodePegs available
        /// </summary>
        public int NumCodePegs { get { return CodePegs.Length; } }
        /// <summary>
        /// The number of available guesses to find the solution. Typically "Rows" on a Mastermind board.
        /// </summary>
        public int NumRows { get; set; }
        /// <summary>
        /// The number of CodePegs in any given combination
        /// </summary>
        public int RowWidth { get; set; }
        /// <summary>
        /// Whether two or more CodePegs can be of the same "color" in a combination
        /// </summary>
        public bool RepetitionAllowed { get; set; }

        /// <summary>
        /// This is from Neverwinter Nights 2 : Mask of the Betrayer, Act 2, Slumbering Coven. You meet a bard and he wagers you can't
        /// beat him at a game called "Hells". Turns out it's a modified version of Mastermind. Not having to solve it manually each
        /// time was the reason for writing this program in the first place.
        /// </summary>
        public static Rules Hells = new Rules { CodePegs = new string[] { "Avernus", "Dis", "Minauros", "Phlegethos", "Stygia", "Malbolge", "Maladomini", "Cania", "Nessus" }, NumRows = 0, RepetitionAllowed = false, RowWidth = 4 };
        /// <summary>
        /// These are the classic Mastermind rules.
        /// </summary>
        public static Rules MasterMind = new Rules { CodePegs = new string[] { "yellow", "green", "red", "blue", "purple", "orange" }, NumRows = 0, RepetitionAllowed = true, RowWidth = 4 };
        /// <summary>
        /// These rules are designed to make the computer suffer. Used to benchmark performance.
        /// </summary>
        public static Rules ReallyHard = new Rules { CodePegs = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n" }, NumRows = 0, RepetitionAllowed = true, RowWidth = 6 };

    }
}
