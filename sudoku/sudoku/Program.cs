using System;
using System.Collections.Generic;

namespace sudoku {
    class Program {
        static void Main(string[] args) {
            if (args.Length > 0) new Sudoku(args[0]);
            else { Console.WriteLine("Enter a search algorithm"); new Sudoku(Console.ReadLine()); }
        }
    }

    class Sudoku {
        // algemene members
        int N, sN;
        int[] values;
        bool[] mask;
        int score;
        int iteration;

        // algoritme-specifieke members
        int[] topvalues;
        int topscore;

        Random rnd = new Random();

        ///
        /// INITIALISATIE 
        ///

        // initialiseer de sudoku en pas een algoritme toe
        public Sudoku(string alg) {
            // todo efficientere mask
            Parse();
            score = Score();
            topscore = int.MaxValue;
            iteration = 0;

            if (alg == "ILS") IteratedLocalSearch(9, 9, 25);
            else if (alg == "SAS") SimulatedAnnealingSearch(0.5f, 0.999f);
            else { Console.WriteLine("Unkown search algorithm"); return; }

            printSudoku();
        }
        // lees de sudoku uit een file
        private void Parse() {
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
            mask = new bool[N * N];

            for (int y = 0; y < N; y++) {
                for (int x = 0; x < N; x++) {
                    // pak de waarde van dit element
                    int c = int.Parse(line[x]);
                    // als deze niet leeg is ...
                    if (c != 0) {
                        // ... sla het dan op
                        SetValue(x, y, values, c);
                        SetValue(x, y, mask, true);
                    }
                }
                // ga naar de volgende regel
                if (y < N - 1) {
                    if (N > 9) line = Console.ReadLine().Split(' ');
                    else line = CharsToString(Console.ReadLine());
                }
            }

            List<int> present;
            int oldvalue, newvalue;

            // vul de lege plekken in
            for (int b = 0; b < N; b++) {
                present = new List<int>();
                newvalue = 1;

                // kijk welke getallen al gebruikt zijn per block
                for (int y = 0; y < sN; y++) {
                    for (int x = 0; x < sN; x++) {
                        oldvalue = GetValue(b, x, y, values);
                        if (oldvalue != 0) present.Add(oldvalue);
                    }
                }

                // voeg de overige getallen toe
                for (int y = 0; y < sN; y++) {
                    for (int x = 0; x < sN; x++) {
                        if (GetValue(b, x, y, values) == 0) {
                            // verhoog het getal totdat deze niet voorkomt in dit block
                            while (present.Contains(newvalue)) newvalue++;
                            SetValue(b, x, y, values, newvalue++);
                        }
                    }
                }
            }
        }

        ///
        /// HULPFUNCTIES 
        ///

        // print de sudoku
        private void printSudoku() {
            Console.WriteLine("Final score: {0}", score);

            string div;
            if (N > 10) div = " ";
            else div = "";

            for (int y = 0; y < N; y++) {
                for (int x = 0; x < N; x++) {
                    Console.Write("{0}{1}", values[y * N + x], div);
                }
                Console.Write("\n");
            }
        }
        // lees de waarde van een coordinaat
        private T GetValue<T>(int block, int x, int y, T[] a) {
            x += block / sN * sN;
            y += block % sN * sN;
            return GetValue(x, y, a);
        }
        private T GetValue<T>(int x, int y, T[] a) {
            return a[x + y * N];
        }
        // schrijf de waarde van een coordinaat
        private void SetValue<T>(int block, int x, int y, T[] a, T value) {
            x += block / sN * sN;
            y += block % sN * sN;
            SetValue(x, y, a, value);
        }
        private void SetValue<T>(int x, int y, T[] a, T value) {
            a[x + y * N] = value;
        }
        // evalueer de sudoku
        private int Score() {
            int eval = 0;

            for (int i = 0; i < N; i++) {
                eval += Score(i, i);
            }

            return eval;
        }
        private int Score(int row, int column) {
            int flag = 0;
            int eval = 0;

            // check de rij
            for (int x = 0; x < N; x++) {
                flag |= 1 << GetValue(x, column, values) - 1;
            }
            for (int x = 0; x < N; x++) {
                if ((flag & 1 << x) == 0) {
                    eval++;
                }
            }

            flag = 0;

            // check de kolom
            for (int y = 0; y < N; y++) {
                flag |= 1 << GetValue(row, y, values) - 1;
            }
            for (int y = 0; y < N; y++) {
                if ((flag & 1 << y) == 0) {
                    eval++;
                }
            }

            return eval;
        }
        // lever de coordinaten op die verwisseld kunnen worden binnen een blok
        // todo cleanup
        private Queue<Tuple<int, int>> SwappableQ(int block) {
            Queue<Tuple<int, int>> swappable = new Queue<Tuple<int, int>>();

            for (int y = 0; y < sN; y++) {
                for (int x = 0; x < sN; x++) {
                    if (!GetValue(block, x, y, mask)) swappable.Enqueue(new Tuple<int, int>(x, y));
                }
            }

            return swappable;
        }
        private List<Tuple<int, int>> SwappableL(int block) {
            List<Tuple<int, int>> swappable = new List<Tuple<int, int>>();

            for (int y = 0; y < sN; y++) {
                for (int x = 0; x < sN; x++) {
                    if (!GetValue(block, x, y, mask)) swappable.Add(new Tuple<int, int>(x, y));
                }
            }

            return swappable;
        }
        // wissel de waardes van twee coordinaten
        private int Swap(int block, int x1, int y1, int x2, int y2) {
            int offsetx = block / sN * sN;
            int offsety = block % sN * sN;
            x1 += offsetx;
            x2 += offsetx;
            y1 += offsety;
            y2 += offsety;
            return Swap(x1, y1, x2, y2);
        }
        private int Swap(int x1, int y1, int x2, int y2) {
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

        private void IteratedLocalSearch(int timeoutt, int S, int walkcount) {
            IteratedLocalSearch(timeoutt, timeoutt, S, walkcount);
        }
        private void IteratedLocalSearch(int timeout, int timeoutt, int S, int walkcount) {
            // Console.WriteLine("Iteration: {0}\nScore: {1}\n", iteration++, score);

            int block = rnd.Next(N);
            Queue<Tuple<int, int>> swappable = SwappableQ(block);

            // verwissel deze coordinaten en bepaal welke keuze het beste is
            Tuple<int, int> coords1, bestcoords1 = new Tuple<int, int>(-1, -1), bestcoords2 = new Tuple<int, int>(-1, -1);
            int scoredelta, bestscoredelta = int.MaxValue;
            while (swappable.Count != 0) {
                coords1 = swappable.Dequeue();

                foreach (Tuple<int, int> coords2 in swappable) {
                    // bereken de score van de huidige wisseling ...
                    scoredelta = Swap(block, coords1.Item1, coords1.Item2, coords2.Item1, coords2.Item2);

                    // ... en onthoud deze als het de beste tot nu toe is
                    if (scoredelta < bestscoredelta) {
                        bestscoredelta = scoredelta;
                        bestcoords1 = coords1;
                        bestcoords2 = coords2;
                    }

                    // zet de puzzel terug naar de beginstand
                    Swap(block, coords2.Item1, coords2.Item2, coords1.Item1, coords1.Item2);
                }
            }

            // verklein het timeout als we op een plateau zijn
            if (bestscoredelta == 0) --timeout;
            else timeout = timeoutt;

            // als er een positieve wisseling is gevonden, voer deze dan uit en ga door
            if (bestscoredelta <= 0 && timeout > 0 && bestcoords1.Item1 != -1) {
                Swap(block, bestcoords1.Item1, bestcoords1.Item2, bestcoords2.Item1, bestcoords2.Item2);
                score += bestscoredelta;
                IteratedLocalSearch(timeout, timeoutt, S, walkcount);
            }
            // zo niet, begin dan een random walk
            else {
                // bewaar dit maximum als het de beste tot nu toe is
                if (score < topscore) {
                    topvalues = values;
                    topscore = score;
                }
                // stop als het walkbudget op is
                if (walkcount-- == 0) {
                    // zet de sudoku op de beste die is gevonden
                    if (topvalues != null) {
                        values = topvalues;
                        score = topscore;
                    }
                    return;
                }
                // Console.WriteLine("Running random walk... ({0} left)\n", walkcount);

                List<Tuple<int, int>> s;
                int i1, i2;
                // verwissel random waardes in een random blok
                for (int i = 0; i < S; i++) {
                    block = rnd.Next(N);
                    s = SwappableL(block);
                    i1 = rnd.Next(s.Count);
                    i2 = rnd.Next(s.Count);
                    while (i2 == i1) i2 = rnd.Next(s.Count);
                    score += Swap(block, s[i1].Item1, s[i1].Item2, s[i2].Item1, s[i2].Item2);
                }
                IteratedLocalSearch(timeoutt, timeoutt, S, walkcount);
            }
        }

        private void SimulatedAnnealingSearch(float c, float a) {
            while (score > 0 && c > 0.001) {
                int block = rnd.Next(N);
                Queue<Tuple<int, int>> swappable = SwappableQ(block);

                Console.WriteLine("Iteration: {0}\nScore: {1}\n", iteration++, score);

                // kies random 2 verwisselbare coordinaten
                Tuple<int, int> coordinate1 = new Tuple<int, int>(-1, -1), coordinate2 = new Tuple<int, int>(-1, -1);

                int rndnr1 = rnd.Next(swappable.Count);
                for (int i = 0; i <= rndnr1; i++) {
                    coordinate1 = swappable.Dequeue();
                    if (i < rndnr1) swappable.Enqueue(coordinate1);
                }
                int rndnr2 = rnd.Next(swappable.Count);
                for (int i = 0; i <= rndnr2; i++) {
                    coordinate2 = swappable.Dequeue();
                }

                // doe de random swap en bepaal de nieuwe waarde
                int currentscore;
                currentscore = Swap(block, coordinate1.Item1, coordinate1.Item2, coordinate2.Item1, coordinate2.Item2) + score;

                // bepaal de kans dat een hogere waarde toch wisselt
                if (currentscore > score) {
                    double chance = Math.Exp((score - currentscore) / c);
                    chance *= 100;
                    int rndchance = rnd.Next(100);

                    //  wissel met 1 - kans terug
                    if (chance < rndchance) Swap(block, coordinate1.Item1, coordinate1.Item2, coordinate2.Item1, coordinate2.Item2);
                    else score = currentscore;
                } else score = currentscore;

                // verlaag de kans dat een slechtere waarde toch wisselt
                c *= a;
            }
        }
    }
}
