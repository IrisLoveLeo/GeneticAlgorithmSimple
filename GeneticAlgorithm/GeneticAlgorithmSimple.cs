using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KangryBaseLib.Logger;
using System.Collections;
using System.IO;

namespace GeneticAlgorithm
{
//  max f(x1, x2) = 21.5 + x1·sin(4p x1) + x2·sin(20p x2)
//  s.t.  -3.0 le x1  le 12.1
//        4.1 le  x2  le 5.8

    public class GeneticAlgorithmSimple
    {
        private IEventLogger _logger;

        private double _pCrossover;          // the probability of crossover
        private double _pMutation;           // the probability of mutation
        private int _popSize;                // the number of chromosome in one generation
        private int _maxGeneration;
        
        private List<double> _globalMaxValue = new List<double>();      // current maxmium solution
        private List<double> _localMaxValue = new List<double>();            // current maxmium solution in current generation
                
        private int _code1Len;
        private int _code2Len;

        private double _maxValue = double.MinValue;
        private double _optX1 = 0;
        private double _optX2 = 0;

        private const double MIN_X1 = -3.0;
        private const double MAX_X1 = 12.1;
        private const double MIN_X2 = 4.1;
        private const double MAX_X2 = 5.8;
        private const double PRECISION = 0.0001;

        private static Random R = new Random(0);

        public GeneticAlgorithmSimple(IEventLogger logger, double pCrossover, double pMutation, int popSize, int maxGeneration)
        {
            this._logger = logger;
            this._pCrossover = pCrossover;
            this._pMutation = pMutation;
            this._popSize = popSize;
            this._maxGeneration = maxGeneration;
            _code1Len = GetEncodeLen(MIN_X1, MAX_X1, PRECISION);
            _code2Len = GetEncodeLen(MIN_X2, MAX_X2, PRECISION);
        }

        private double ValueFunction(double x1, double x2)
        {
            return 21.5 + x1 * Math.Sin(4 * Math.PI * x1) + x2 * Math.Sin(20 * Math.PI * x2);
        }

        private int GetEncodeLen(double min, double max, double precision)
        {
            double temp = Math.Log((max - min) / precision + 1, 2);
            int result = (int)temp;
            if(result < temp)
            {
                result = result + 1;
            }
            return result;
        }

        private Chromosome Encode(double num1, double num2)
        {
            num1 = (num1 - MIN_X1) * (Math.Pow(2, _code1Len) - 1) / (MAX_X1 - MIN_X1);
            num2 = (num2 - MIN_X2) * (Math.Pow(2, _code2Len) - 1) / (MAX_X2 - MIN_X2);

            var str1 = DecToBinary((int)num1);
            var str2 = DecToBinary((int)num2);
            
            while(str1.Length < _code1Len)
            {
                str1 = '0' + str1;
            }

            while(str2.Length < _code2Len)
            {
                str2 = '0' + str2;
            }

            var str = str1 + str2;

            Chromosome c = new Chromosome(str);

            return c;
        }

        private Tuple<double, double> Decode(Chromosome ch)
        {
            Chromosome ch1 = ch.SubChromosome(0, _code1Len - 1);
            Chromosome ch2 = ch.SubChromosome(_code1Len, _code1Len + _code2Len - 1);

            double num1 = BinaryToDec(ch1.ToString());
            double num2 = BinaryToDec(ch2.ToString());

            num1 = MIN_X1 + num1 * (MAX_X1 - MIN_X1) / (Math.Pow(2, _code1Len) - 1);
            num2 = MIN_X2 + num2 * (MAX_X2 - MIN_X2) / (Math.Pow(2, _code2Len) - 1);

            return Tuple.Create<double, double>(num1, num2);
        }

        private string DecToBinary(int num)
        {
            var str = "";
            while(num != 0)
            {
                str = num % 2 + str;
                num = num / 2;
            }
            return str;
        }

        private int BinaryToDec(string str)
        {
            int num = 0;
            int len = str.Length;
            for(int i = 0; i< len; i++)
            {
                if(str[i] == '1')
                {
                    num += (int)Math.Pow(2, len - i - 1);
                }
            }
            return num;
        }
        
        private double RandomNumber(double min, double max, int? seed = null)
        {
            double rand = R.NextDouble();
            return rand * (max - min) + min;
        }

        public void Run()
        {
            int generation = 0;
            List<Chromosome> current = Initial();
            Fitness(current);

            while(generation < _maxGeneration)
            {
                var crossChildren = Crossover(current);
                var mutationChildren = Mutation(current);
                current.AddRange(crossChildren);
                current.AddRange(mutationChildren);
                Fitness(current);
                current = Selection(current);
                generation++;
            }
        }

        public void WriteResult(string resultFile)
        {
            KangryBaseLib.Utils.Functions.ActionTrace(_logger, "WriteResult", () =>
            {
                using (StreamWriter sw = new StreamWriter(resultFile))
                {
                    sw.WriteLine("Z = " + _maxValue + ", X1 = " + _optX1 + ", X2 = " + _optX2);
                    for(int i = 0; i< _globalMaxValue.Count; i++)
                    {
                        sw.WriteLine(i + "\t" + _globalMaxValue[i] + "\t" + _localMaxValue[i]);
                    }
                }
            });
        }

        protected List<Chromosome> Initial()
        {
            List<Chromosome> result = new List<Chromosome>();
            KangryBaseLib.Utils.Functions.ActionTrace(_logger, "Initial", () => 
            {
                for(int i = 0; i<_popSize; i++)
                {
                    double num1 = RandomNumber(MIN_X1, MAX_X2);
                    double num2 = RandomNumber(MIN_X2, MAX_X2);

                    result.Add(Encode(num1, num2));
                }
            });
            return result;
        }

        protected List<Chromosome> Crossover(List<Chromosome> chs)
        {
            List<Chromosome> result = new List<Chromosome>();
            KangryBaseLib.Utils.Functions.ActionTrace(_logger, "Crossover", () =>
            {
                for(int k = 0; k < chs.Count / 2; k++)
                {
                    if(_pCrossover >= RandomNumber(0, 1))
                    {
                        int i = 0, j = 0;
                        while(i == j)
                        {
                            i = (int)RandomNumber(0, chs.Count - 1);
                            j = (int)RandomNumber(0, chs.Count - 1);
                        }
                        int p = (int)RandomNumber(1, _code1Len + _code2Len - 2);
                        var ch1 = new Chromosome(chs[i].SubChromosome(0, p - 1).ToString() + chs[j].SubChromosome(p, _code1Len + _code2Len - 1).ToString());
                        var ch2 = new Chromosome(chs[j].SubChromosome(0, p - 1).ToString() + chs[i].SubChromosome(p, _code1Len + _code2Len - 1).ToString());
                        result.Add(ch1);
                        result.Add(ch2);
                    }
                }
            });
            return result;
        }

        protected List<Chromosome> Mutation(List<Chromosome> chs)
        {
            List<Chromosome> result = new List<Chromosome>();
            KangryBaseLib.Utils.Functions.ActionTrace(_logger, "Mutation", () =>
            {
                foreach(var ch in chs)
                {
                    BitArray genes = new BitArray(ch.Genes);
                    for(int j = 0; j < _code1Len + _code2Len; j++)
                    {
                        if(_pMutation >= RandomNumber(0, 1))
                        {
                            genes.Set(j, !genes.Get(j));
                        }
                    }
                    result.Add(new Chromosome(genes));
                }
            });
            return result;
        }

        protected List<Chromosome> Selection(List<Chromosome> chs)
        {
            List<Chromosome> result = new List<Chromosome>();
            KangryBaseLib.Utils.Functions.ActionTrace(_logger, "Selection", () =>
            {
                List<double> values = new List<double>();
                foreach(var ch in chs)
                {
                    var decode = Decode(ch);
                    values.Add(ValueFunction(decode.Item1, decode.Item2));
                }

                double F = values.Sum();
                List<double> ps = new List<double>();
                foreach(var value in values)
                {
                    ps.Add(value / F);
                }

                List<double> cdf = new List<double>();
                cdf.Add(0);         //在开始填上0，方便后面编程
                for(int i = 0; i < ps.Count; i++)
                {
                    cdf.Add(ps[i] + cdf[i]);
                }

                for(int i = 0; i < _popSize; i++)
                {
                    double r = RandomNumber(0, 1);
                    int j = 0;
                    for(j = 0; j < cdf.Count - 1; j++)
                    {
                        if(r >= cdf[j] && r < cdf[j + 1])
                        {
                            break;
                        }
                    }
                    result.Add(chs[j]);
                }
            });
            return result;
        }

        protected void Fitness(List<Chromosome> chs)
        {
            KangryBaseLib.Utils.Functions.ActionTrace(_logger, "Fitness", () =>
            {
                double temMaxValue = double.MinValue;
                double optimalX1 = 0;
                double optimalX2 = 0;
                foreach(var ch in chs)
                {
                    var decode = Decode(ch);
                    var value = ValueFunction(decode.Item1, decode.Item2);
                    if(value > temMaxValue)
                    {
                        temMaxValue = value;
                        optimalX1 = decode.Item1;
                        optimalX2 = decode.Item2;
                    }
                }
                _localMaxValue.Add(temMaxValue);
                if(temMaxValue > _maxValue)
                {
                    _maxValue = temMaxValue;
                    _optX1 = optimalX1;
                    _optX2 = optimalX2;
                }
                _globalMaxValue.Add(_maxValue);

                _logger.LogInformation(new { MaxValue = _maxValue, Generation = _globalMaxValue.Count - 1 }, "");
            });
        }
    }
}
