using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    public class Chromosome
    {
        private readonly BitArray _genes;

        public BitArray Genes { get { return _genes; } }

        public Chromosome(string binaryStr)
        {
            bool[] bArray = new bool[binaryStr.Length];
            for(int i = 0; i < binaryStr.Length; i++)
            {
                bArray[i] = binaryStr[i] == '1';
            }
            _genes = new BitArray(bArray);
        }

        /// <summary>
        /// 截取start到end的基因，包括两端
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Chromosome SubChromosome(int start, int end)
        {
            bool[] bArray = new bool[end - start + 1];
            for(int i = start; i<= end; i++)
            {
                bArray[i - start] = _genes.Get(i);
            }
            return new Chromosome(bArray);
        }

        public Chromosome(bool[] bArray)
        {
            this._genes = new BitArray(bArray);
        }

        public Chromosome(BitArray bitArray)
        {
            this._genes = new BitArray(bitArray);
        }

        public override string ToString()
        {
            char[] chars = new char[_genes.Length];
            for(int i = 0; i<_genes.Length; i++)
            {
                chars[i] = _genes.Get(i) ? '1' : '0';
            }
            return string.Join("", chars);
        }
    }
}
