using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Sudoku {
    class Exec {
        static void Main(string[] args) {
            if (args.Length == 0) throw new ArgumentException("Please supply a search algorithm on execution.");

            // maak een nieuwe sudoku en sudoku-oplosser
            Sudoku sudoku = new Sudoku();
            SudokuSolver solver;
            
            switch (args[0]) {
                case "CBT":
                    solver = new NaiveBacktracking(sudoku);
                    break;
                case "FCO":
                    solver = new Ordered(sudoku);
                    break;
                case "FCH":
                    solver = new Heuristic(sudoku);
                    break;
                default:
                    Console.WriteLine("Unkown search algorithm.");
                    return;
            }

            // los de sukodu op en toon deze, met de besteedde tijd, in de console
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            solver.Solve();
            stopwatch.Stop();

            Console.WriteLine("Sudoku solved in {0} ticks ({1} milliseconds).\n\n{2}", stopwatch.ElapsedTicks, stopwatch.ElapsedMilliseconds, sudoku);
        }
    }

    class Sudoku {
        public int N, sN;
        public int[] values;
        public List<int> free;

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

        #region DOMEIN-FUNCTIE
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

    #region SOLVERS
    abstract class SudokuSolver {
        protected Sudoku sudoku;
        protected int index;

        // geef aan of de sudoku klaar is, of nog ingevuld wordt
        protected abstract bool Done { get; }
        // geef aan of er nog gezocht wordt, of alle waardes al zijn geprobeerd
        protected abstract bool Searching { get; }
        // lever de volgende waarde-zoekende instantie op
        protected abstract SudokuSolver Next { get; }

        // geef de waarde die als volgende geprobeert moet worden
        protected abstract void TryValue();
        // beoordeel de geprobeerde waarde
        protected abstract bool ConstraintCheck(int column, int row);

        // vul de sudoku cel-voor-cel in aan de hand van de bovenstaande hulpmethoden
        public bool Solve() {
            //Console.WriteLine("Filling in space {0}/{1}", index, sudoku.free.Count);

            // stop als de hele sudoku bekeken is
            if (Done) return true;

            Tuple<int, int> coord = sudoku.ConvertCoord(sudoku.free[index]);

            while (Searching) {
                // verander het eerste lege vakje naar de eerste beschikbare waarde
                TryValue();

                // kijk of deze legaal is, en ga in dit geval door
                if (ConstraintCheck(coord.Item1, coord.Item2) && Next.Solve()) return true;
                // haal de invulling anders weg
                Reset();
            }

            return false;
        }
        // zet de sudoku terug als alle mogelijke waardes illegaal zijn
        protected virtual void Reset() {
            sudoku.values[sudoku.free[index]] = 0;
        }
    }

    #region BACKTRACKING
    sealed class NaiveBacktracking : SudokuSolver {
        int value;

        public NaiveBacktracking(Sudoku s, int i = 0) {
            sudoku = s;
            value = 1;
            index = i;
        }

        protected override bool Done =>
            index >= sudoku.free.Count;
        protected override bool Searching =>
            value <= sudoku.N;
        protected override SudokuSolver Next =>
            new NaiveBacktracking(sudoku, index + 1);

        protected override void TryValue() {
            sudoku.values[sudoku.free[index]] = value++;
        }
        protected override bool ConstraintCheck(int column, int row) {
            // kijk of de rij / de kolom / het blok geen duplicaten bevat
            return sudoku.DomainFunc(column, row, (x, y, v) => sudoku.values[sudoku.ConvertCoord(x, y)] == v);
        }
    }
    #endregion

    #region FORWARD CHECKING
    abstract class ForwardChecking : SudokuSolver {
        protected int domain_index;
        protected List<List<int>> domains;

        protected int value;
        protected List<int> domains_changed;

        protected ForwardChecking(Sudoku s, int i = 0, List<List<int>> d = null) {
            sudoku = s;
            domain_index = 0;
            domains = d;
            index = i;

            if (d == null) InitDomains();
            domains_changed = new List<int>();
        }

        // initialiseer alle domeinen
        protected void InitDomains() {
            domains = new List<List<int>>();

            Tuple<int, int> coord;
            List<int> domain;

            for (int i = 0; i < sudoku.free.Count; i++) {
                coord = sudoku.ConvertCoord(sudoku.free[i]);

                domain = new List<int>();
                for (int v = 0; v <= sudoku.N; v++) domain.Add(v);

                sudoku.DomainFunc(coord.Item1, coord.Item2, (x, y, v) => {
                    domain.Remove(sudoku.values[sudoku.ConvertCoord(x, y)]);
                    return false;
                });

                domains.Add(domain);
            }
        }
        // update een enkel domein
        protected void StrengthenDomains(int i) {
            Tuple<int, int> coord = sudoku.ConvertCoord(sudoku.free[i]);
            int free_index;

            sudoku.DomainFunc(coord.Item1, coord.Item2, (x, y, v) => {
                free_index = sudoku.free.IndexOf(sudoku.ConvertCoord(x, y));
                if (free_index == -1) return false;
                if (domains[free_index].Remove(v)) domains_changed.Add(free_index);

                return domains[free_index].Count == 0;
            });
        }
        protected void RollbackDomains() {
            foreach (int free_index in domains_changed) domains[free_index].Add(value);
            domains_changed = new List<int>();
        }

        protected sealed override bool Searching =>
            domain_index < domains[index].Count;

        protected override void TryValue() {
            value = domains[index][domain_index++];
            sudoku.values[sudoku.free[index]] = value;
            StrengthenDomains(index);
        }
        protected sealed override bool ConstraintCheck(int column, int row) {
            // kijk of de rij / de kolom / het blok geen lege domeinen bevat
            int free_index;

            return sudoku.DomainFunc(column, row, (x, y, v) => {
                free_index = sudoku.free.IndexOf(sudoku.ConvertCoord(x, y));
                if (free_index == -1) return false;

                return domains[free_index].Count == 0;
            });
        }
        protected override void Reset() {
            base.Reset();
            RollbackDomains();
        }
    }
    sealed class Ordered : ForwardChecking {
        public Ordered(Sudoku s, int i = 0, List<List<int>> d = null) : base(s, i, d) { }

        protected override bool Done =>
            index >= sudoku.free.Count;
        protected override SudokuSolver Next =>
            new Ordered(sudoku, index + 1, domains);
    }
    sealed class Heuristic : ForwardChecking {
        List<int> passed;

        public Heuristic(Sudoku s, int i = 0, List<List<int>> d = null, List<int> p = null) : base(s, i, d) {
            if (p == null) passed = new List<int>();
            else passed = p;
        }

        private int NextIndex() {
            passed.Add(index);
            int domain = -1, smallest = int.MaxValue;

            for (int i = 0; i < domains.Count; i++)
                if (domains[i].Count < smallest && !passed.Contains(i)) {
                    domain = i; smallest = domains[i].Count;
                }

            return domain;
        }
        protected override void Reset() {
            base.Reset();
            passed.Remove(index);
        }

        protected override bool Done =>
            passed.Count >= sudoku.free.Count;
        protected override SudokuSolver Next =>
            new Heuristic(sudoku, NextIndex(), domains, passed);
    }
    #endregion
    #endregion
}
