namespace Encoder
{
    /// <summary>
    /// Крутой класс для работы с кодированием, декодированием и обработкой алфавитов(алфамит состоит из симвалов и соответствующими вероятностями).
    /// </summary>
    public static class ReadCode
    {
        /// <summary>
        /// Проверяет, является ли символ допустимым для алфавита.
        /// </summary>
        /// <param name="token">Символ для проверки.</param>
        /// <returns>true, если символ допустим, иначе false.</returns>
        private static bool IsValidSymbol(string token)
        {
            string allowedSymbols = "+-*/=";
            return token.Length == 1 && allowedSymbols.Contains(token);
        }
        /// <summary>
        /// Проверяет, является ли строка допустимым кодом (состоящим только из '0' и '1').
        /// </summary>
        /// <param name="token">Строка для проверки.</param>
        /// <returns>true, если строка является бинарным кодом, иначе false.</returns>
        private static bool IsValidCode(string token)
        {
            return !string.IsNullOrWhiteSpace(token) &&
                   token.All(c => c == '0' || c == '1');
        }
        /// <summary>
        /// Преобразует число в двоичную дробь заданной длины.
        /// </summary>
        /// <param name="value">Число для преобразования.</param>
        /// <param name="length">Количество бит в результате.</param>
        /// <returns>Строка двоичной дроби.</returns>
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
        /// <summary>
        /// Создает код для сивмолов алфавита, вычисляет среднюю длину кода, избыточность и значение неравенства Крафта.
        /// </summary>
        /// <param name="alphabet">Алфавит символов с соответствующими вероятностями каждому символу.</param>
        /// <returns>Кортеж: список кодов, средняя длина, избыточность, значение неравентсва Крафта.</returns>
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

            double totalProb = 0.0;

            foreach (var p in alphabet.Values)
            {
                if (p < 0 || p > 1)
                    throw new Exception($"Ошибка: вероятность символа {p} должна быть в диапазоне [0,1].");

                totalProb += p;
            }

            if (Math.Abs(totalProb - 1.0) > 1e-9)
                throw new Exception($"Ошибка: сумма вероятностей должна быть равна 1 (сейчас {totalProb}).");
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
        /// <summary>
        /// Декодирует последовательность кодовых слов в исходный текст.
        /// </summary>
        /// <param name="alphabet">Алфавит символов с вероятностями.</param>
        /// <param name="sequence">Последовательность кодовых слов для декодирования.</param>
        /// <returns>Кортеж: декодированный текст, список кодов, средняя длина, избыточность, значение Крафта.</returns>
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
        /// <summary>
        /// Кодирует последовательность символов в бинарный кот :).
        /// </summary>
        /// <param name="alphabet">Алфавит символов с вероятностями.</param>
        /// <param name="sequence">Список символов для кодирования.</param>
        /// <returns>Кортеж: закодированная строка, список кодов, средняя длина, избыточность, значение Крафта.</returns>
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
        /// <summary>
        /// Вычисляет длину кода для каждого символа алфавита.
        /// </summary>
        /// <param name="alphabet">Алфавит символов с вероятностями.</param>
        /// <returns>Список длин кодовых слов.</returns>
        public static List<int> CalculateWordLengths(Dictionary<string, double> alphabet)
        {
            List<int> wordLengths = new();
            foreach (var a in alphabet)
            {
                wordLengths.Add((int)Math.Ceiling(-Math.Log(a.Value / 2, 2)));
            }
            return wordLengths;
        }
        /// <summary>
        /// Вычисляет избыточность кода.
        /// </summary>
        /// <param name="alphabet">Алфавит символов с вероятностями.</param>
        /// <param name="wordLengths">Список длин кодовых слов.</param>
        /// <returns>Избыточность.</returns>
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
        /// <summary>
        /// Вычисляет избыточность кода.
        /// </summary>
        /// <param name="alphabet">Алфавит символов с вероятностями.</param>
        /// <param name="wordLengths">Список длин кодовых слов.</param>
        /// <returns>Избыточность.</returns>
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
        /// <summary>
        /// Сохраняет закодированную последовательность в файл.
        /// </summary>
        /// <param name="filename">Имя файла для сохранения.</param>
        /// <param name="encoded">Закодированная строка.</param>
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
        /// <summary>
        /// Считывает алфавит из файла.
        /// </summary>
        /// <param name="filename">Имя файла с алфавитом.</param>
        /// <returns>Словарь с символами и вероятностями.</returns>
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
                if (alphabet.ContainsKey(symbol))
                {
                    throw new Exception($"Ошибка: символ '{symbol}' встречается более одного раза в файле алфавита.");
                }
                
                alphabet[symbol] = probability;
            }
            
            return alphabet;
        }
        /// <summary>
        /// Считывает последовательность/закодированную последоватеьнсть из файла.
        /// </summary>
        /// <param name="filename">Имя файла с алфавитом.</param>
        /// <returns>Список с символами последовательности.</returns>
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
