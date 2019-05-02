using System;

namespace sudoku {
    class Program {
        static void Main(string[] args) {
            if (args.Length > 0) {
                Sudoku s = new Sudoku(args[0]);
            }

            Console.WriteLine("Enter a search algorithm");
        }
    }

    class Sudoku {
        // sudoku structuur
        int[,] block = new int[9, 9];
        int mask = 0;

        public Sudoku(string alg) {
            Parse();

            if (alg == "ILS") IteratedLocalSearch();
            else if (alg == "SAS") SimulatedAnnealingSearch();
            else Console.WriteLine("Unkown search algorithm");
        }

        public void Parse() {
            Console.ReadLine();
            // loop door de individuele elementen van de puzzel
            for (int y = 0; y < 9; y++) {
                for (int x = 0; x < 9; x++) {
                    // pak de waarde van dit element
                    int c = Console.Read() - 48;
                    // als deze niet leeg is ...
                    if (c != 0) {
                        // sla het dan op
                        block[x / 3 + y / 3, c - 1] |= (1 << x % 3 + y % 3);
                        mask |= (1 << x + y * 9);
                    }
                }
                // ga naar de volgende regel
                Console.ReadLine();
            }
        }

        static void IteratedLocalSearch() {
            // todo implement
        }
        static void SimulatedAnnealingSearch() {
            // todo implement
        }
    }
}
