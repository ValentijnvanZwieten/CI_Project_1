using System;
using System.Diagnostics;

using SudokuProblem;

class Program {
    static void Main(string[] args) {
        string alg;
        if (args.Length == 0) throw new ArgumentException("Please supply a search algorithm on execution.");
        else alg = args[0];

        
        Stopwatch stopwatch = new Stopwatch();
        int solved_sudokus = 0, total_sudokus = 0;
        long total_ticks = 0, total_milliseconds = 0;
        bool succes;

        Sudoku sudoku;
        SudokuSolver solver;

        // los de sukodu op en toon deze, met de besteedde tijd, in de console
        do {
            sudoku = new Sudoku();
            total_sudokus++;

            // todo remove switch
            switch (alg) {
                case "CBT":
                    solver = new BacktrackingChronological(sudoku);
                    break;
                case "CFC":
                    solver = new ForwardCheckingChronological(sudoku);
                    break;
                case "HFC":
                    solver = new ForwardCheckingHeuristic(sudoku);
                    break;
                default:
                    Console.WriteLine("Unkown search algorithm.");
                    return;
            }

            // hou bij hoe lang het oplossen duurt
            stopwatch.Start();
            succes = solver.Solve();
            stopwatch.Stop();

            // geef de resultaten weer en ga verder
            if (succes) {
                Console.WriteLine("Solved sudoku {0} in {1} ticks ({2} milliseconds):\n{3}", total_sudokus, stopwatch.ElapsedTicks, stopwatch.ElapsedMilliseconds, sudoku);
                solved_sudokus++;
                total_ticks += stopwatch.ElapsedTicks; total_milliseconds += stopwatch.ElapsedMilliseconds;
            } else {
                Console.WriteLine("Failed to solve sudoku {0}.\n{1}", total_sudokus, sudoku);
            }
            
            stopwatch.Reset();
        } while (Console.ReadLine() == "+");

        if (solved_sudokus > 0) Console.WriteLine("Solved {0}/{1} sudoku's in {2} ticks ({3} milliseconds). Avarage solve time is {4} ticks ({5} milliseconds).", solved_sudokus, total_sudokus, total_ticks, total_milliseconds, total_ticks / solved_sudokus, total_milliseconds / solved_sudokus);
        else Console.WriteLine("Solved 0/{0} sudoku's.", total_sudokus);
    }
}
