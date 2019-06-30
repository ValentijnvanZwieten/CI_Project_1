using System;
using System.Collections.Generic;

namespace SudokuProblem {
    class Sudoku {
        public int N, sN;
        public int[] values;
        public List<int> free;

        // lees de sudoku uit stdin
        public Sudoku() {
            // converteer een char[] naar een string[] waar elke string alleen het originele karakter bevat
            string[] CharsToString(string s) {
                string[] ret = new string[s.Length];
                for (int i = 0; i < s.Length; i++) ret[i] = s[i].ToString();
                return ret;
            }

            try {
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
            catch(Exception) {
                throw new ArgumentException("Could not parse sudoku; possibly malformed.");
            }
        }

        #region HELPERS
        // print de sudoku
        public override string ToString() {
            string ret = "", div;
            if (N > 10) div = " ";
            else div = "";

            for (int y = 0; y < N; y++) {
                for (int x = 0; x < N; x++)
                    ret += values[ConvertCoord(x, y)] + div;
                ret += "\n";
            }

            return ret;
        }
        // converteer een coordinaat tussen 2d-1d en terug
        public int ConvertCoord(int x, int y) {
            return x + y * N;
        }
        public Tuple<int, int> ConvertCoord(int i) {
            return new Tuple<int, int>(i % N, i / N);
        }
        // voer een geleverde functie uit over (de waarde van) de rij / de kolom / het blok van een bepaald coordinaat
        public bool DomainFunc(int column, int row, Func<int, int, int, bool> function) {
            int value = values[ConvertCoord(column, row)];

            // ga door de rij en de kolom
            for (int x = 0; x < N; x++) if (x != column && function(x, row, value)) return false;
            for (int y = 0; y < N; y++) if (y != row && function(column, y, value)) return false;

            // ga door het blok
            int by = row - row % sN; int bx = column - column % sN;
            for (int y = by; y < by + sN; y++) for (int x = bx; x < bx + sN; x++) if (x != column && y != row && function(x, y, value)) return false;

            return true;
        }
        #endregion
    }
}
