using System;

namespace sudokuCBT {

    class Program {

        static void Main(string[] args)
        {
            //todo
        }
    }

    class SudukuCBT {

        int N, sN;
        int[] values;
        bool[] mask;

        public SudukuCBT()
        {
            //todo
        }

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

        private void ChronologicalBacktracking()
        {
            //todo
        }

        private void ForwardCheckingLR()
        {
            //todo
        }

        private void ForwardCheckingMCV()
        {
            //todo
        }
    }
}