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
                    new Backtracking(s).Solve();
                    break;
                case "FCO":
                    //InitConstraints();
                    new Ordered(s).Solve();
                    break;
                case "FCH":
                    //InitConstraints();
                    new Heuristic(s).Solve();
                    break;
                default:
                    Console.WriteLine("Unkown search algorithm.");
                    return;
            }

            // toon de opgeloste sudoku
            Console.WriteLine(s);
        }
    }

    class Sudoku {
        public int N, sN;
        public int[] values;
        public List<int> free;
        public List<List<int>> domains;

        #region INITIALISERING
        // lees de sudoku uit een file
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
        #endregion

        #region PREDIKAAT-CHECKER
        // bepaal of het gegeven coordinaat de oplossing illegaal maakt volgens het bijgeleverde predikaat
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
        #endregion
    }

    #region SOLVERS
    abstract class SudokuSolver {
        protected Sudoku sudoku;
        protected int index;

        // geef de waarde die als volgende geprobeert moet worden
        abstract protected int Value();
        // geef aan of er nog gezocht wordt, of alle waardes al zijn geprobeerd
        abstract protected bool Searching();
        // geef aan of de sudoku klaar is, of nog ingevuld wordt
        abstract protected bool Done();
        // beoordeel de geprobeerde waarde
        abstract protected bool ConstraintCheck(int column, int row);
        // lever de volgende waarde-zoekende instantie op
        abstract protected SudokuSolver Next();

        // vul de sudoku cel-voor-cel in aan de hand van de bovenstaande hulpmethoden
        public bool Solve() {
            //Console.WriteLine("Filling in space {0}", i);

            // stop als de hele sudoku bekeken is
            if (Done())
                return true;

            Tuple<int, int> coord = sudoku.ConvertCoord(sudoku.free[index]);

            while (Searching()) {
                // verander het eerste lege vakje naar de eerste beschikbare waarde
                sudoku.values[sudoku.free[index]] = Value();

                // kijk of deze legaal is, en ga in dit geval door
                if (ConstraintCheck(coord.Item1, coord.Item2) && Next().Solve()) return true;
            }

            // maak het vakje weer leeg als er geen correcte invulling is gevonden
            sudoku.values[sudoku.free[index]] = 0;
            return false;
        }
    }

    #region BACKTRACKING
    class Backtracking : SudokuSolver {
        int value;

        public Backtracking(Sudoku s, int i = 0) {
            sudoku = s;
            value = 1;
            index = i;
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
        override protected bool ConstraintCheck(int column, int row) {
            // kijk of de rij / de kolom / het blok geen duplicaten bevat
            return sudoku.CellPredicate(column, row, (x, y, v) => sudoku.values[sudoku.ConvertCoord(x, y)] == v);
        }
        override protected SudokuSolver Next() {
            return new Backtracking(sudoku, index + 1);
        }
    }
    #endregion BACKTRACKING

    #region FORWARD CHECKING
    abstract class ForwardChecking : SudokuSolver {
        protected int domain_index;

        sealed override protected int Value() {
            return sudoku.domains[index][domain_index];
        }
        sealed override protected bool Searching() {
            return domain_index <= sudoku.domains[index].Count;
        }
        sealed override protected bool ConstraintCheck(int column, int row) {
            // kijk of de rij / de kolom / het blok geen lege domeinen bevat
            return sudoku.CellPredicate(column, row, (x, y, v) => sudoku.values[sudoku.ConvertCoord(x, y)] == v);
        }
    }
    class Ordered : ForwardChecking {
        public Ordered(Sudoku s, int i = 0) {
            sudoku = s;
            domain_index = 0;
            index = i;
        }
        
        override protected bool Done() {
            return index >= sudoku.free.Count;
        }
        override protected SudokuSolver Next() {
            return new Ordered(sudoku, index + 1);
        }
    }
    // todo
    class Heuristic : ForwardChecking {
        public Heuristic(Sudoku s) {
            sudoku = s;
            domain_index = 0;
            index = 0; // todo
        }
        
        override protected bool Done() {
            return true; // todo
        }
        override protected SudokuSolver Next() {
            return new Heuristic(sudoku);
        }
    }
    #endregion FORWARD CHECKING
    #endregion SOLVERS
}
