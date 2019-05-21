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
        int N, sN;
        int[] values;
        bool[] mask;
        int score;

        Random rnd = new Random();

        ///
        /// INITIALISATIE 
        ///

        // initialiseer de sudoku en pas een algoritme toe
        public Sudoku(string alg)
        {
            // todo variabele N
            N = 9; sN = (int)Math.Sqrt(N);
            values = new int[N * N];
            mask = new bool[N * N];
            Parse();
            score = Score();

            if (alg == "ILS") IteratedLocalSearch();
            else if (alg == "SAS") SimulatedAnnealingSearch(2);
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
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
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

            List<int> present;
            int oldvalue, newvalue;

            // vul de lege plekken in
            for (int b = 0; b < N; b++)
            {
                present = new List<int>();
                newvalue = 0;

                // kijk welke getallen al gebruikt zijn per block
                for (int y = 0; y < sN; y++)
                {
                    for (int x = 0; x < sN; x++)
                    {
                        oldvalue = GetValue(b, x, y, values);
                        if (oldvalue != 0) present.Add(oldvalue);
                    }
                }

                // voeg de overige getallen toe
                for (int y = 0; y < sN; y++)
                {
                    for (int x = 0; x < sN; x++)
                    {
                        if (GetValue(b, x, y, values) == 0)
                        {
                            // verhoog het getal totdat deze niet voorkomt in dit block
                            while (present.Contains(newvalue)) newvalue++;
                            SetValue(b, x, y, values, newvalue);
                        }
                    }
                }
            }
        }

        ///
        /// HULPFUNCTIES 
        ///

        // lees de waarde van een coordinaat
        private T GetValue<T>(int block, int x, int y, T[] a)
        {
            x += block / sN * sN;
            y += block % sN * sN;
            return GetValue(x, y, a);
        }
        private T GetValue<T>(int x, int y, T[] a)
        {
            return a[x + y * N];
        }
        // schrijf de waarde van een coordinaat
        private void SetValue<T>(int block, int x, int y, T[] a, T value)
        {
            x += block / sN * sN;
            y += block % sN * sN;
            SetValue(x, y, a, value);
        }
        private void SetValue<T>(int x, int y, T[] a, T value)
        {
            a[x + y * N] = value;
        }
        // evalueer de sudoku
        private int Score()
        {
            int eval = 0;

            for (int i = 0; i < N; i++)
            {
                eval += Score(i, i);
            }

            return eval;
        }
        private int Score(int row, int column)
        {
            int flag = 0;
            int eval = 0;

            // check de rij
            for (int x = 0; x < N; x++)
            {
                flag |= 1 << GetValue(x, column, values);
            }
            for (int x = 0; x < N; x++)
            {
                eval += flag |= 1 << x;
            }

            flag = 0;

            // check de kolom
            for (int y = 0; y < N; y++)
            {
                flag |= 1 << GetValue(row, y, values);
            }
            for (int y = 0; y < N; y++)
            {
                eval += flag |= 1 << y;
            }

            return eval;
        }
        // lever de coordinaten op die verwisseld kunnen worden binnen een blok
        private Queue<Tuple<int, int>> Swappable(int block)
        {
            Queue<Tuple<int, int>> swappable = new Queue<Tuple<int, int>>();
            
            for (int y = 0; y < sN; y++)
            {
                for (int x = 0; x < sN; x++)
                {
                    if (!GetValue(block, x, y, mask)) swappable.Enqueue(new Tuple<int, int>(x, y));
                }
            }

            return swappable;
        }
        // wissel de waardes van twee coordinaten
        private int Swap(int x1, int y1, int x2, int y2)
        {
            // voorkom dat vaste waardes gewisseld worden
            if (GetValue(x1, y1, mask) || GetValue(x2, y2, mask)) throw new ArgumentException("Swapped value(s) is/are static");

            // bewaar de originele score en waarde
            int oldscore = Score(x1, y1) + Score(x2, y2);
            int oldvalue = GetValue(x1, y1, values);

            // voer de wisseling uit
            SetValue(x1, y1, values, GetValue(x2, y2, values));
            SetValue(x2, y2, values, oldvalue);

            // geef het verschil in scores terug
            return (Score(x1, y1) + Score(x2, y2)) - oldscore;
        }

        ///
        /// ZOEKALGORITMES 
        ///

        private void IteratedLocalSearch()
        {
            Queue<Tuple<int, int>> swappable = Swappable(0); // rnd.Next(9);

            // verwissel deze coordinaten en bepaal welke keuze het beste is
            Tuple<int, int> current1, best1 = new Tuple<int, int>(-1, -1), best2 = new Tuple<int, int>(-1, -1);
            int currentscore, bestscore = 81;
            while (swappable.Count != 0)
            {
                current1 = swappable.Dequeue();

                foreach (Tuple<int, int> current2 in swappable)
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

        private void SimulatedAnnealingSearch(float c)
        {
            Queue<Tuple<int, int>> swappable = Swappable(0); // rnd.Next(9);

            // kies random 2 verwisselbare coordinaten
            Tuple<int, int> coordinate1 = new Tuple<int, int>(-1, -1), coordinate2 = new Tuple<int, int>(-1, -1);
            int rndnr1 = rnd.Next(swappable.Count);
            int rndnr2 = rnd.Next(swappable.Count - 1);
            for (int i = 0; i <= rndnr1; i++)
            {
                coordinate1 = swappable.Dequeue();
                if (i < rndnr1) swappable.Enqueue(coordinate1);
            }
            for (int i = 0; i <= rndnr2; i++)
            {
                coordinate2 = swappable.Dequeue();
            }

            // bepaal de nieuwe heuristische waarde
            int currentscore;
            currentscore = Swap(coordinate1.Item1, coordinate1.Item2, coordinate2.Item1, coordinate2.Item2) + score;

            // bepaal de kans dat een hogere waarde toch wisselt, wissel met 1 - kans terug
            if (currentscore > score)
            {
                double chance = Math.Exp((score - currentscore) / c);
                chance *= 100;
                int rndchance = rnd.Next(100);
                if (chance < rndchance) Swap(coordinate1.Item1, coordinate1.Item2, coordinate2.Item1, coordinate2.Item2);
                else score = currentscore;
            }
            else score = currentscore;

            float a = 0.95f;
            if (score > 0) SimulatedAnnealingSearch(a * c);
        }
    }
}
