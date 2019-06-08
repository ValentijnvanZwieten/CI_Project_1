using System;
using System.Collections.Generic;

namespace sudoku {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0) throw new ArgumentException("Please enter a search algorithm.");
            else new Sudoku(args[0]);
        }
    }

    class Sudoku {
        // algemene members
        int N, sN;
        int[] values;
        List<int> free;

        #region INITIALISERING
        // initialiseer de sudoku en pas een algoritme toe
        public Sudoku(string alg) {
            Parse();

            // voer het algoritme uit
            switch (alg) {
                case "CBT":
                    ChronologicalBacktracking();
                    break;
                case "FCO":
                    ForwardCheckingOrdered();
                    break;
                case "FCH":
                    ForwardCheckingHeuristic();
                    break;
                default:
                    Console.WriteLine("Unkown search algorithm.");
                    return;
            }

            printSudoku();
        }
        // lees de sudoku uit een file
        // todo optimize
        private void Parse() {
            // converteer een char[] naar een string[] waar elke string alleen het originele karakter bevat
            string[] CharsToString(string s) {
                string[] ret = new string[s.Length];
                for (int i = 0; i < s.Length; i++) ret[i] = s[i].ToString();
                return ret;
            }

            string[] line = Console.ReadLine().Split(' ');
            if (line.Length == 1) line = CharsToString(line[0]);
            N = line.Length;
            sN = (int)Math.Sqrt(N);
            values = new int[N * N];
            free = new List<int>();

            for (int y = 0; y < N; y++) {
                for (int x = 0; x < N; x++) {
                    // pak de waarde van dit element
                    int c = int.Parse(line[x]);
                    // als deze niet leeg is, sla het dan op
                    if (c != 0) values[ConvertCoord(x, y)] = c;
                    // geef het anders op als veranderbare waarde
                    else free.Add(ConvertCoord(x, y));
                }
                // ga naar de volgende regel
                if (y < N - 1) {
                    if (N > 9) line = Console.ReadLine().Split(' ');
                    else line = CharsToString(Console.ReadLine());
                }
            }
        }
        #endregion

        #region HULPFUNCTIES
        // print de sudoku
        private void printSudoku() {
            string div;
            if (N > 10) div = " ";
            else div = "";

            for (int y = 0; y < N; y++) {
                for (int x = 0; x < N; x++)
                    Console.Write("{0}{1}", values[ConvertCoord(x, y)], div);
                Console.Write("\n");
            }
        }
        // converteer een coordinaat tussen 2d-1d en terug
        private int ConvertCoord(int x, int y) {
            return x + y * N;
        }
        private Tuple<int, int> ConvertCoord(int i) {
            return new Tuple<int, int>(i % N, i / N);
        }
        // bepaal of het gegeven coordinaat de oplossing illegaal maakt
        // todo fix
        private bool Legal(int row, int column) {
            int value = values[ConvertCoord(row, column)];

            for (int x = 0; x < N; x++) if (x != column && values[ConvertCoord(x, row)] == value) return false;
            for (int y = 0; y < N; y++) if (y != row && values[ConvertCoord(column, y)] == value) return false;
            return true;
        }
        #endregion

        #region ALGORITMES
        private void ChronologicalBacktracking(int i = 0) {
            int value = 1;
            Tuple<int, int> coord = ConvertCoord(free[i]);

            while (value <= N) {
                // verander het eerste lege vakje naar de eerste beschikbare waarde
                values[free[i]] = value;

                // kijk of deze legaal is, en ga in dit geval door
                if (Legal(coord.Item1, coord.Item2)) ChronologicalBacktracking(i + 1);
                // probeer anders andere waardes
                else value++;
            }
        }

        private void ForwardCheckingOrdered() {
            // todo
        }
        private void ForwardCheckingHeuristic() {
            // todo
        }
        #endregion
    }
}
