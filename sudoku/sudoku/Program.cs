using System;

namespace sudoku
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0) new Sudoku(args[0]);
            else Console.WriteLine("Enter a search algorithm");
        }
    }

    class Sudoku
    {
        // structuur
        int[] blocks = new int[9 * 9];
        int mask = 0;
        int score;

        // initialiseer de sudoku en pas een algoritme toe
        public Sudoku(string alg)
        {
            Parse();

            if (alg == "ILS") IteratedLocalSearch();
            else if (alg == "SAS") SimulatedAnnealingSearch();
            else Console.WriteLine("Unkown search algorithm");
        }

        // lees de sudoku uit een file
        // todo meerdere sudokus
        private void Parse()
        {
            Console.ReadLine();
            // loop door de individuele elementen van de puzzel
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    // pak de waarde van dit element
                    int c = Console.Read() - 48;
                    // als deze niet leeg is ...
                    if (c != 0)
                    {
                        // ... sla het dan op
                        blocks[x + y * 9] = c;
                        mask |= (1 << x + y * 9);
                    }
                }
                // ga naar de volgende regel
                Console.ReadLine();
            }
        }

        private void EvalAll()
        {
            score = 0;

            for (int x = 0; x < 9; x++)
            {
                score += Eval(x, x);
            }
        }
        private int Eval(int row, int column)
        {
            int flag = 0;
            int eval = 0;

            // check de rij
            for (int x = 0; x < 9; x++)
            {
                flag |= 1 << blocks[x + column * 9];
            }
            for (int x = 0; x < 9; x++)
            {
                eval += flag |= 1 << x;
            }

            flag = 0;

            // check de kolom
            for (int y = 0; y < 9; y++)
            {
                flag |= 1 << blocks[y * 9 + row];
            }
            for (int y = 0; y < 9; y++)
            {
                eval += flag |= 1 << y;
            }

            return eval;
        }

        // wissel de waardes van twee coordinaten
        private void Swap(int x1, int y1, int x2, int y2)
        {
            // bewaar de originele score
            int oldscore = Eval(x1, y1) + Eval(x2, y2);

            // voer de wisseling uit
            int index1 = x1 + y1 * 9, index2 = x2 + y2 * 9, value1 = blocks[index1];
            blocks[index1] = blocks[index2];
            blocks[index2] = value1;

            // update de score
            score += oldscore - (Eval(x1, y1) + Eval(x2, y2));
        }
        // lees de waarde van een coordinaat
        private int Value(int x, int y)
        {
            return blocks[x + y * 9];
        }

        private void IteratedLocalSearch()
        {
            // todo implementeren
        }
        private void SimulatedAnnealingSearch()
        {
            // todo implementeren
        }
    }
}
