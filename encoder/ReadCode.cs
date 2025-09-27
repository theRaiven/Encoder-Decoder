namespace Encoder
{
    public static class ReadCode
    {
        private static bool IsValidSymbol(string token)
        {
            string allowedSymbols = "+-*/=";
            return token.Length == 1 && allowedSymbols.Contains(token);
        }
        private static bool IsValidCode(string token)
        {
            return !string.IsNullOrWhiteSpace(token) &&
                   token.All(c => c == '0' || c == '1');
        }
        
        private static string ToBinaryFraction(double value, int length)
        {
            var bits = new StringBuilder();
            double x = value;

            for (int i = 0; i < length; i++)
            {
                x *= 2;
                if (x >= 1)
                {
                    bits.Append('1');
                    x -= 1;
                }
                else
                {
                    bits.Append('0');
                }
            }

            return bits.ToString();
        }
        public static (List<string> codes, double avgLength, double redundancy, double kraft) GetCodeForSymbol(Dictionary<string, double> alphabet)
        {
            List<double> cumulativeProb = new();
            List<double> midProbability = new();
            List<string> codeWord = new();
            double sum = 0.0;

            foreach (var a in alphabet)
            {
                cumulativeProb.Add(sum);
                sum += a.Value;
            }

            for (int i = 0; i < cumulativeProb.Count; i++)
            {
                midProbability.Add(alphabet.Values.ElementAt(i) / 2 + cumulativeProb[i]);
            }

            List<int> wordLength = CalculateWordLengths(alphabet);

            double kraftValue = ChechKraft(wordLength);
            if (kraftValue > 1)
                throw new Exception($"Неравенство Крафта нарушено: {kraftValue} > 1. Кодировать невозможно.");

            for (int i = 0; i < alphabet.Count; i++)
            {
                string code = ToBinaryFraction(midProbability[i], wordLength[i]);
                codeWord.Add(code);
            }

            double avgLength = wordLength.Select((len, i) => len * alphabet.Values.ElementAt(i)).Sum();
            double redundancy = CalculateRedundancy(alphabet, wordLength);

            return (codeWord, avgLength, redundancy, kraftValue);
        }

        public static (StringBuilder decoded, List<string> codes, double avgLength, double redundancy, double kraft) Decoding(Dictionary<string, double> alphabet, List<string> sequence)
        {
            var (codeWord, avgLength, redundancy, kraft) = GetCodeForSymbol(alphabet);
            List<string> symbols = alphabet.Keys.ToList();

            StringBuilder decoded = new();
            foreach (var code in sequence)
            {
                int index = codeWord.IndexOf(code);
                if (index >= 0)
                {
                    decoded.Append(symbols[index] + " ");
                }
                else
                {
                    throw new Exception($"Ошибка: код '{code}' не найден в алфавите");
                }
            }

            return (decoded, codeWord, avgLength, redundancy, kraft);
        }
        public static (StringBuilder encoded, List<string> codes, double avgLength, double redundancy, double kraft) Encoding(Dictionary<string, double> alphabet, List<string> sequence)
        {
            var (codeWord, avgLength, redundancy, kraft) = GetCodeForSymbol(alphabet);

            StringBuilder encoded = new();
            for (int i = 0; i < sequence.Count; i++)
            {
                string symbol = sequence[i];
                int index = alphabet.Keys.ToList().IndexOf(symbol);

                if (index >= 0)
                {
                    encoded.Append(codeWord[index] + " ");
                }
                else
                {
                    throw new Exception($"Ошибка: символ '{symbol}' не найден в алфавите");
                }
            }

            return (encoded, codeWord, avgLength, redundancy, kraft);
        }

        public static List<int> CalculateWordLengths(Dictionary<string, double> alphabet)
        {
            List<int> wordLengths = new();
            foreach (var a in alphabet)
            {
                wordLengths.Add((int)Math.Ceiling(-Math.Log(a.Value, 2)));
            }
            return wordLengths;
        }
        public static double CalculateRedundancy(Dictionary<string, double> alphabet, List<int> wordLengths)
        {
            double entropy = 0;
            double avgLength = 0;

            int i = 0;
            foreach (var kvp in alphabet)
            {
                double p = kvp.Value;
                entropy += -p * Math.Log(p, 2);
                avgLength += p * wordLengths[i];
                i++;
            }
            //WriteLine();
            //WriteLine($"Энтропия источника: {entropy:F4} бит");
            //WriteLine($"Средняя длина кодового слова: {avgLength:F4} бит");
           
            return avgLength - entropy;
        }
        public static double ChechKraft(List<int> wordLength)
        {
            double kraft = 0.0;
            foreach (var w in wordLength)
            {
                kraft += Math.Pow(2, -w);
            }
            return kraft;
        }


        //================================
        public static void SaveEncodedToFile(string filename, StringBuilder encoded)
        {
            try
            {
                File.WriteAllText(filename, encoded.ToString());
                WriteLine($"Закодированная последовательность сохранена в файл: {filename}");
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка при записи в файл: {ex.Message}");
                throw;
            }
        }
        public static Dictionary<string, double> ReadAlphabet(string filename)
        {
            var alphabet = new Dictionary<string, double>();

            foreach (var line in File.ReadLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                {
                    throw new Exception($"Неверный формат строки: {line}");

                }
                string symbol = parts[0];
                string probStr = parts[1];

                if (!IsValidSymbol(symbol))
                {
                    throw new Exception($"Неверный символ: {symbol}");
                }
                if (!double.TryParse(probStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double probability))
                {
                    throw new Exception($"Неверный формат вероятности: {probStr}");
                }
                alphabet[symbol] = probability;
            }
            return alphabet;
        }
        public static List<string> ReadSequence(string filename)
        {
            var sequence = new List<string>();

            foreach (var line in File.ReadLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var token in parts)
                {
                    if (IsValidSymbol(token) || IsValidCode(token))
                        sequence.Add(token);
                    else
                        throw new Exception($"Недопустимый символ или код: '{token}' в файле {filename}");
                }
            }

            if (sequence.Count == 0)
                throw new Exception($"Файл {filename} пуст или не содержит допустимых символов.");

            return sequence;
        }
    }
}
