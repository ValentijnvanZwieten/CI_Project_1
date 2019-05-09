using System;
using System.Collections.Generic;

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
        // sudoku
        int[] blocks = new int[9 * 9];
        bool[] mask = new bool[9 * 9];
        int score;

        Random rnd = new Random();

        // initialiseer de sudoku en pas een algoritme toe
        public Sudoku(string alg)
        {
            Parse();
            EvalAll();

            if (alg == "ILS") IteratedLocalSearch();
            else if (alg == "SAS") SimulatedAnnealingSearch();
            else Console.WriteLine("Unkown search algorithm");
        }

        // lees de sudoku uit een file
        // todo meerdere sudokus
        // todo verschillende groottes
        private void Parse()
        {
            // todo omgaan met lege regels
            Console.ReadLine();

            int index;

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
                        index = x + y * 9;
                        blocks[index] = c;
                        mask[index] = true;
                    }
                }
                // ga naar de volgende regel
                Console.ReadLine();
            }
        }

        // evalueer de hele sudoku
        private void EvalAll()
        {
            score = 0;

            for (int x = 0; x < 9; x++)
            {
                score += Eval(x, x);
            }
        }
        // evalueer een specifieke rij/kolom combinatie
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
        private int Swap(int x1, int y1, int x2, int y2)
        {
            // voorkom dat vaste waardes gewisseld worden
            if (Value(x1, y1, mask) || Value(x2, y2, mask)) throw new ArgumentException("Swapped value(s) is/are static");

            // bewaar de originele score
            int oldscore = Eval(x1, y1) + Eval(x2, y2);

            // voer de wisseling uit
            int index1 = x1 + y1 * 9, index2 = x2 + y2 * 9, value1 = blocks[index1];
            blocks[index1] = blocks[index2];
            blocks[index2] = value1;

            // geef het verschil in scores terug
            return (Eval(x1, y1) + Eval(x2, y2)) - oldscore;
        }
        // lees de waarde van een coordinaat
        private T Value<T>(int x, int y, T[] a)
        {
            return a[x + y * 9];
        }

        private void IteratedLocalSearch()
        {
            // bepaal de linkerboven-coordinaat van het block dat je gaat onderzoeken
            int block = rnd.Next(9);
            int offsety = (block - 1) / 3 * 3;
            int offsetx = (block - 1) % 3 * 3;
            List<Tuple<int, int>> coordinates = new List<Tuple<int, int>>();

            // verzamel alle coordinaten die verwisseld kunnen worden
            for (int y = offsety; y < offsety + 3; y++)
            {
                for (int x = offsetx; x < offsetx + 3; x++)
                {
                    if (!Value(x, y, mask)) coordinates.Add(new Tuple<int, int>(x, y));
                }
            }

            // verwissel deze coordinaten en bepaal welke keuze het beste is
            Tuple<int, int> currentcoordinate, bestcoordinate1, bestcoordinate2;
            int currentscore, lowestscore = 81;
            while (coordinates.Count > 0)
            {
                currentcoordinate = coordinates[0];
                coordinates.RemoveAt(0);

                foreach (Tuple<int, int> coordinate2 in coordinates)
                {
                    currentscore = Swap(currentcoordinate.Item1, currentcoordinate.Item2, coordinate2.Item1, coordinate2.Item2);

                    if (currentscore < lowestscore)
                    {
                        lowestscore = currentscore;
                        bestcoordinate1 = currentcoordinate;
                        bestcoordinate2 = coordinate2;
                    }

                    Swap(coordinate2.Item1, coordinate2.Item2, currentcoordinate.Item1, currentcoordinate.Item2);
                }
            }

            if (lowestscore < 0) Swap(bestcoordinate1.Item1, bestcoordinate1.Item2, bestcoordinate2.Item1, bestcoordinate2.Item2);

        }
        private void SimulatedAnnealingSearch()
        {
            // todo implementeren
        }
    }
}
