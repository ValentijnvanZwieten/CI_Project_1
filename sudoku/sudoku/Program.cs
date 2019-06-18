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
        int N, sN;
        int[] values;
        List<int> free;
        List<List<int>> domains;

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
        private bool Legal(int column, int row) {
            int value = values[ConvertCoord(column, row)];

            // kijk of de rij / kolom alleen unieke getallen heeft
            for (int x = 0; x < N; x++) if (x != column && values[ConvertCoord(x, row)] == value) return false;
            for (int y = 0; y < N; y++) if (y != row && values[ConvertCoord(column, y)] == value) return false;

            // kijk of het blok alleen unieke getallen heeft
            int by = row - row % sN; int bx = column - column % sN;
            for (int y = by; y < by + sN; y++) for (int x = bx; x < bx + sN; x++) if (x != column && y != row && values[ConvertCoord(x, y)] == value) return false;

            return true;
        }
        private bool UpdateConstraints() {
            foreach (int i in free) {
                Tuple<int, int> coord = ConvertCoord(free[i]);
                if (!UpdateConstraints(coord.Item1, coord.Item2)) return false;
            }
            return true;
        }
        private bool UpdateConstraints(int column, int row) {
            return false; // todo
        }
        #endregion

        #region VALUE GENERATORS
        interface ValueGenerator {
            int Index();
            int Value();
            bool Searching();
            bool Done();
            ValueGenerator Next();
        }
        class Chronological : ValueGenerator {
            Sudoku sudoku;
            int value;
            int index;

            public Chronological(Sudoku s, int i) {
                sudoku = s;
                value = 1;
                index = i;
            }
            
            public int Index() {
                return index;
            }
            public int Value() {
                return value++;
            }
            public bool Searching() {
                return value <= sudoku.N;
            }
            public bool Done() {
                return index >= sudoku.free.Count;
            }
            public ValueGenerator Next() {
                return new Chronological(sudoku, index + 1);
            }
        }
        class Domain : ValueGenerator {
            Sudoku sudoku;
            int domain_index;
            int index;

            public Domain(Sudoku s, int i) {
                sudoku = s;
                domain_index = 0;
                index = i;
            }
            
            public int Index() {
                return index;
            }
            public int Value() {
                return sudoku.domains[index][domain_index];
            }
            public bool Searching() {
                return domain_index <= sudoku.domains[index].Count;
            }
            public bool Done() {
                return true; // todo
            }
            public ValueGenerator Next() {
                return new Domain(sudoku, index + 1);
            }
        }
        #endregion VALUE GENERATORS

        #region ALGORITMES
        private bool ChronologicalBacktracking(int i = 0) {
            return CSP(new Chronological(this, i), Legal);
        }
        private bool ForwardCheckingOrdered(int i = 0) {
            return CSP(new Domain(this, i), UpdateConstraints);
        }
        private bool ForwardCheckingHeuristic() {
            return false; // todo
        }

        private bool CSP(ValueGenerator vg, Func<int, int, bool> constraint_check) {
            //Console.WriteLine("Filling in space {0} / {1}", i, free.Count);

            // stop als de hele sudoku bekeken is
            if (vg.Done()) return true;

            int i = vg.Index();
            Tuple<int, int> coord = ConvertCoord(free[i]);

            while (vg.Searching()) {
                // verander het eerste lege vakje naar de eerste beschikbare waarde
                values[free[i]] = vg.Value();

                // kijk of deze legaal is, en ga in dit geval door
                if (constraint_check(coord.Item1, coord.Item2) && CSP(vg.Next(), constraint_check)) return true;
            }

            // maak het vakje weer leeg als geen correcte invulling is gevonden
            values[free[i]] = 0;
            return false;
        }
        #endregion
    }
}
