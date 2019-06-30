using System;
using System.Collections.Generic;

namespace SudokuProblem {
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
            if (Done)
                return true;

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
    sealed class BacktrackingChronological : SudokuSolver {
        int value;

        public BacktrackingChronological(Sudoku s, int i = 0) {
            sudoku = s;
            value = 1;
            index = i;
        }

        protected override bool Done =>
            index >= sudoku.free.Count;
        protected override bool Searching =>
            value <= sudoku.N;
        protected override SudokuSolver Next =>
            new BacktrackingChronological(sudoku, index + 1);

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
    sealed class ForwardCheckingChronological : ForwardChecking {
        public ForwardCheckingChronological(Sudoku s, int i = 0, List<List<int>> d = null) : base(s, i, d) { }

        protected override bool Done =>
            index >= sudoku.free.Count;
        protected override SudokuSolver Next =>
            new ForwardCheckingChronological(sudoku, index + 1, domains);
    }
    sealed class ForwardCheckingHeuristic : ForwardChecking {
        List<int> passed;

        public ForwardCheckingHeuristic(Sudoku s, int i = 0, List<List<int>> d = null, List<int> p = null) : base(s, i, d) {
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
            new ForwardCheckingHeuristic(sudoku, NextIndex(), domains, passed);
    }
    #endregion
}
