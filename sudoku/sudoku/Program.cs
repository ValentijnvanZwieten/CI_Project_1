using System;
using System.Linq;
using System.Collections.Generic;

namespace sudoku {
    class Exec {
        static void Main(string[] args) {
            if (args.Length == 0) throw new ArgumentException("Please supply a search algorithm on execution.");

            // maak een nieuwe sudoku
            Sudoku s = new Sudoku();

            // voer het gevraagde algoritme uit
            switch (args[0]) {
                case "CBT":
                    new Backtracking(s);
                    break;
                case "FCO":
                    //InitConstraints();
                    new ForwardCheckingOrdered(s);
                    break;
                case "FCH":
                    //InitConstraints();
                    new ForwardCheckingHeuristic(s);
                    break;
                default:
                    Console.WriteLine("Unkown search algorithm.");
                    return;
            }

            // toon de opgeloste sudoku
            s.printSudoku();
        }
    }

    class Sudoku {
        public int N, sN;
        public int[] values;
        public List<int> free;
        public List<List<int>> domains;

        #region INITIALISERING
        // lees de sudoku uit een file
        // todo optimize
        public Sudoku() {
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
        // todo fix
        /*
        private void InitConstraints() {
            foreach (int i in free) {
                domains.Add(new List<int>().AddRange(Enumerable.Range(1, N)));
                UpdateConstraints(i);
            }
        }
        */
        #endregion

        #region HELPERS
        // print de sudoku
        public void printSudoku() {
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
        public int ConvertCoord(int x, int y) {
            return x + y * N;
        }
        public Tuple<int, int> ConvertCoord(int i) {
            return new Tuple<int, int>(i % N, i / N);
        }
        #endregion

        #region LEGAL CHECKS
        // bepaal of het gegeven coordinaat de oplossing illegaal maakt
        public bool CellPredicate(int column, int row, Func<int, int, int, bool> predicate) {
            int value = values[ConvertCoord(column, row)];

            // check de rij / kolom
            for (int x = 0; x < N; x++) if (x != column && predicate(x, row, value)) return false;
            for (int y = 0; y < N; y++) if (y != row && predicate(column, y, value)) return false;

            // check het blok
            int by = row - row % sN; int bx = column - column % sN;
            for (int y = by; y < by + sN; y++) for (int x = bx; x < bx + sN; x++) if (x != column && y != row && predicate(x, y, value)) return false;

            return true;
        }
        // kijk of de rij / de kolom / het blok geen duplicaten bevat
        public bool NoDuplicates(int column, int row) {
            return CellPredicate(column, row, (x, y, v) => values[ConvertCoord(x, y)] == v);
        }
        // kijk of de rij / de kolom / het blok geen lege domeinen bevat
        // todo
        public bool NoEmptyDomains(int column, int row) {
            return CellPredicate(column, row, (x, y, v) => values[ConvertCoord(x, y)] == v);
        }
        #endregion
    }

    #region CSPs
    abstract class SudokuSolver {
        protected Sudoku sudoku;
        protected int index;

        abstract protected int Value();
        abstract protected bool Searching();
        abstract protected bool Done();
        abstract protected SudokuSolver Next();

        public bool Solve(Func<int, int, bool> constraint_check) {
            //Console.WriteLine("Filling in space {0}", i);

            // stop als de hele sudoku bekeken is
            if (Done()) return true;

            Tuple<int, int> coord = sudoku.ConvertCoord(sudoku.free[index]);

            while (Searching()) {
                // verander het eerste lege vakje naar de eerste beschikbare waarde
                sudoku.values[sudoku.free[index]] = Value();

                // kijk of deze legaal is, en ga in dit geval door
                if (constraint_check(coord.Item1, coord.Item2) && Next().Solve(constraint_check)) return true;
            }

            // maak het vakje weer leeg als er geen correcte invulling is gevonden
            sudoku.values[sudoku.free[index]] = 0;
            return false;
        }

    }
    // todo generalise domain-based
    class Backtracking : SudokuSolver {
        int value;

        public Backtracking(Sudoku s, int i = 0) {
            sudoku = s;
            value = 1;
            index = i;
            Solve(s.NoDuplicates);
        }

        override protected int Value() {
            return value++;
        }
        override protected bool Searching() {
            return value <= sudoku.N;
        }
        override protected bool Done() {
            return index >= sudoku.free.Count;
        }
        override protected SudokuSolver Next() {
            return new Backtracking(sudoku, index + 1);
        }
    }
    class ForwardCheckingOrdered : SudokuSolver {
        int domain_index;

        public ForwardCheckingOrdered(Sudoku s, int i = 0) {
            sudoku = s;
            domain_index = 0;
            index = i;
            Solve(s.NoEmptyDomains);
        }

        override protected int Value() {
            return sudoku.domains[index][domain_index];
        }
        override protected bool Searching() {
            return domain_index <= sudoku.domains[index].Count;
        }
        override protected bool Done() {
            return index >= sudoku.free.Count;
        }
        override protected SudokuSolver Next() {
            return new ForwardCheckingOrdered(sudoku, index + 1);
        }
    }
    // todo
    class ForwardCheckingHeuristic : SudokuSolver {
        int domain_index;

        public ForwardCheckingHeuristic(Sudoku s) {
            sudoku = s;
            domain_index = 0;
            index = 0; // todo
            Solve(s.NoEmptyDomains);
        }

        override protected int Value() {
            return sudoku.domains[index][domain_index];
        }
        override protected bool Searching() {
            return domain_index <= sudoku.domains[index].Count;
        }
        override protected bool Done() {
            return true; // todo
        }
        override protected SudokuSolver Next() {
            return new ForwardCheckingHeuristic(sudoku);
        }
    }
    #endregion VALUE GENERATORS
}
