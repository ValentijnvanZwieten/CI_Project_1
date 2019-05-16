using System;
using System.Collections.Generic;

namespace sudoku
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0) new Sudoku(args[0]);
            else Console.WriteLine("Enter a search algorithm"); new Sudoku(Console.ReadLine());
        }
    }

    class Sudoku
    {
        int[] values = new int[9 * 9];
        bool[] mask = new bool[9 * 9];
        int score;

        Random rnd = new Random();

        ///
        /// INITIALISATIE 
        ///

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
        // todo omgaan met lege regels
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
                        SetValue(x, y, values, c);
                        SetValue(x, y, mask, true);
                    }
                }
                // ga naar de volgende regel
                while (Console.Read() != '\n') ;
            }
        }

        ///
        /// HULPFUNCTIES 
        ///

        // lees de waarde van een coordinaat
        private T GetValue<T>(int x, int y, T[] a)
        {
            return a[x + y * 9];
        }
        private T GetValue<T>(int block, int x, int y, T[] a)
        {
            return a[x + y * 9];
        }
        // schrijf de waarde van een coordinaat
        private void SetValue<T>(int x, int y, T[] a, T value)
        {
            a[x + y * 9] = value;
        }
        // evalueer de hele sudoku
        private void EvalAll()
        {
            score = 0;

            for (int i = 0; i < 9; i++)
            {
                score += Eval(i, i);
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
                flag |= 1 << values[x + column * 9];
            }
            for (int x = 0; x < 9; x++)
            {
                eval += flag |= 1 << x;
            }

            flag = 0;

            // check de kolom
            for (int y = 0; y < 9; y++)
            {
                flag |= 1 << values[y * 9 + row];
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
            if (GetValue(x1, y1, mask) || GetValue(x2, y2, mask)) throw new ArgumentException("Swapped value(s) is/are static");

            // bewaar de originele score en waarde
            int oldscore = Eval(x1, y1) + Eval(x2, y2);
            int oldvalue = GetValue(x1, y1, values);

            // voer de wisseling uit
            SetValue(x1, y1, values, GetValue(x2, y2, values));
            SetValue(x2, y2, values, oldvalue);

            // geef het verschil in scores terug
            return (Eval(x1, y1) + Eval(x2, y2)) - oldscore;
        }

        ///
        /// ZOEKALGORITMES 
        ///

        private void IteratedLocalSearch()
        {
            // bepaal de linkerboven-coordinaat van het block dat je gaat onderzoeken
            int block = 0; // rnd.Next(9);
            int offsety = block / 3 * 3;
            int offsetx = block % 3 * 3;

            Queue<Tuple<int, int>> swap = new Queue<Tuple<int, int>>();

            // verzamel alle coordinaten die verwisseld kunnen worden
            for (int y = offsety; y < offsety + 3; y++)
            {
                for (int x = offsetx; x < offsetx + 3; x++)
                {
                    if (!GetValue(x, y, mask))
                        swap.Enqueue(new Tuple<int, int>(x, y));
                }
            }

            // verwissel deze coordinaten en bepaal welke keuze het beste is
            Tuple<int, int> current1, best1 = new Tuple<int, int>(-1, -1), best2 = new Tuple<int, int>(-1, -1);
            int currentscore, bestscore = 81;
            while (swap.Count != 0)
            {
                current1 = swap.Dequeue();

                foreach (Tuple<int, int> current2 in swap)
                {
                    // bereken de score van de huidige wisseling ...
                    currentscore = Swap(current1.Item1, current1.Item2, current2.Item1, current2.Item2);

                    // ... en onthoud deze als het de beste tot nu toe is
                    if (currentscore < bestscore)
                    {
                        bestscore = currentscore;
                        best1 = current1;
                        best2 = current2;
                    }

                    // zet de puzzel terug naar de beginstand
                    Swap(current2.Item1, current2.Item2, current1.Item1, current1.Item2);
                }
            }

            // als er een goede wisseling is gevonden, voer deze dan uit en ga door
            if (bestscore < 0 && best1.Item1 != -1)
            {
                Swap(best1.Item1, best1.Item2, best2.Item1, best2.Item2);
                score += bestscore;
                IteratedLocalSearch();
            }
            // zo niet, begin dan een random walk
            else
            {
                // todo random walk
            }

        }
        private void SimulatedAnnealingSearch()
        {
            // todo implementeren
        }
    }
}
