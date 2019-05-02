using System;

namespace sudoku {
    class Program {
        static void Main(string[] args) {
            if (args.Length > 0) new Sudoku(args[0]);
            else Console.WriteLine("Enter a search algorithm");
        }
    }

    class Sudoku {
        // structuur
        int[,] blocks = new int[9, 9];
        int mask = 0;

        // initialiseer de sudoku en pas een algoritme toe
        public Sudoku(string alg) {
            Parse();

            if (alg == "ILS") IteratedLocalSearch();
            else if (alg == "SAS") SimulatedAnnealingSearch();
            else Console.WriteLine("Unkown search algorithm");
        }

        // lees de sudoku uit een file
        // todo meerdere sudokus
        public void Parse() {
            Console.ReadLine();
            // loop door de individuele elementen van de puzzel
            for (int y = 0; y < 9; y++) {
                for (int x = 0; x < 9; x++) {
                    // pak de waarde van dit element
                    int c = Console.Read() - 48;
                    // als deze niet leeg is ...
                    if (c != 0) {
                        // ... sla het dan op
                        blocks[x / 3 + y / 3, x % 3 + y % 3 * 3] = c;
                        mask |= (1 << x + y * 9);
                    }
                }
                // ga naar de volgende regel
                Console.ReadLine();
            }
        }

        private void Swap(int x1, int y1, int x2, int y2) {

        }
        private void Value() {

        }

        private void IteratedLocalSearch() {
            // todo implementeren
        }
        private void SimulatedAnnealingSearch() {
            // todo implementeren
        }
    }
}