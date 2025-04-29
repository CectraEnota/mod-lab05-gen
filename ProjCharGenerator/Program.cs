using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ScottPlot;
using System.Drawing;


namespace generator
{
    public class BigramGenerator
    {
        public Dictionary<string, double> bigrams = new Dictionary<string, double>();
        private Random random = new Random();
        private double totalWeight = 0;
        public BigramGenerator(string filePath)
        {
            LoadBigrams(filePath);
            CalculateTotalWeight();
        }
        private void LoadBigrams(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл биграмм не найден: {filePath}");

            var lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('\t');
                if (parts.Length == 2 && double.TryParse(parts[1], out double weight))
                {
                    string bigram = parts[0];

                    if (bigrams.ContainsKey(bigram))
                    {
                        bigrams[bigram] += weight;
                    }
                    else
                    {
                        bigrams.Add(bigram, weight);
                    }
                }
            }

            if (bigrams.Count == 0)
                throw new InvalidDataException("Файл биграмм не содержит допустимых данных");
        }

        private void CalculateTotalWeight()
        {
            totalWeight = bigrams.Values.Sum();
        }

        public string GenerateText(int length)
        {
            if (bigrams.Count == 0)
                throw new InvalidOperationException("Биграммы не загружены");

            string result = "";
            string currentBigram = GetRandomBigram();

            if (currentBigram.Length > 0)
                result += currentBigram[0];

            while (result.Length < length)
            {
                if (currentBigram.Length > 1)
                {
                    result += currentBigram[1];
                }

                char lastChar = currentBigram.Length > 1 ? currentBigram[1] : currentBigram[0];
                currentBigram = GetNextBigram(lastChar);
            }

            return result.Substring(0, Math.Min(length, result.Length));
        }

        public string GetRandomBigram()
        {
            double randomValue = random.NextDouble() * totalWeight;
            double cumulative = 0;

            foreach (var pair in bigrams)
            {
                cumulative += pair.Value;
                if (randomValue <= cumulative)
                {
                    return pair.Key;
                }
            }

            return bigrams.Keys.First();
        }

        public string GetNextBigram(char firstChar)
        {
            var possibleBigrams = bigrams
                .Where(b => b.Key.Length > 0 && b.Key[0] == firstChar)
                .ToList();

            if (possibleBigrams.Count == 0)
            {
                return GetRandomBigram();
            }

            double subTotal = possibleBigrams.Sum(b => b.Value);
            double randomValue = random.NextDouble() * subTotal;
            double cumulative = 0;

            foreach (var pair in possibleBigrams)
            {
                cumulative += pair.Value;
                if (randomValue <= cumulative)
                {
                    return pair.Key;
                }
            }

            return possibleBigrams.First().Key;
        }
    }
    public class WordGenerator
    {
        public Dictionary<string, double> wordWeights = new Dictionary<string, double>();
        private Random random = new Random();
        private double totalWeight = 0;

        public WordGenerator(string filePath)
        {
            LoadWords(filePath);
            CalculateTotalWeight();

            if (wordWeights.Count == 0)
            {
                throw new InvalidOperationException("Файл слов не содержит допустимых данных");
            }
        }

        private void LoadWords(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл со словами не найден: {filePath}");
            }

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            if (lines.Length == 0)
            {
                throw new InvalidDataException("Файл со словами пуст");
            }

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    string word = parts[0].Trim();

                    string numberStr = new string(parts[1].Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());

                    if (double.TryParse(numberStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double weight))
                    {
                        if (wordWeights.ContainsKey(word))
                        {
                            wordWeights[word] += weight;
                        }
                        else
                        {
                            wordWeights.Add(word, weight);
                        }
                    }
                }
            }
        }

        private void CalculateTotalWeight()
        {
            totalWeight = wordWeights.Values.Sum();
        }

        public string GenerateText(int wordCount)
        {
            if (wordWeights.Count == 0)
            {
                throw new InvalidOperationException("Невозможно сгенерировать текст - слова не загружены");
            }

            List<string> selectedWords = new List<string>();
            for (int i = 0; i < wordCount; i++)
            {
                selectedWords.Add(GetRandomWord());
            }

            return string.Join(" ", selectedWords);
        }

        public string GetRandomWord()
        {
            double randomValue = random.NextDouble() * totalWeight;
            double cumulative = 0;

            foreach (var pair in wordWeights)
            {
                cumulative += pair.Value;
                if (randomValue <= cumulative)
                {
                    return pair.Key;
                }
            }

            return wordWeights.Keys.First();
        }
    }
    public static class PlotGenerator
    {
        public static void CreateFrequencyPlot(Dictionary<string, double> expected,
                                             Dictionary<string, int> actual,
                                             string title,
                                             string fileName)
        {
            var plt = new Plot();
            plt.Width = 1200;
            plt.Height = 800;

            var topData = expected
                .OrderByDescending(x => x.Value)
                .Take(50)
                .Select(x => new {
                    Key = x.Key,
                    Expected = x.Value,
                    Actual = actual.ContainsKey(x.Key) ? actual[x.Key] : 0
                })
                .ToList();

            double[] positions = topData.Select((x, i) => (double)i).ToArray();
            string[] labels = topData.Select(x => x.Key).ToArray();
            double[] expectedValues = topData.Select(x => x.Expected).ToArray();
            double[] actualValues = topData.Select(x => (double)x.Actual).ToArray();

            double maxActual = actualValues.Max();
            double maxExpected = expectedValues.Max();
            double scaleFactor = maxActual / maxExpected;
            double[] scaledExpected = expectedValues.Select(x => x * scaleFactor).ToArray();

            var actualBars = plt.AddBar(actualValues, positions);
            actualBars.Label = "Фактические частоты";
            actualBars.Color = Color.FromArgb(100, 78, 121, 167);

            var expectedBars = plt.AddBar(scaledExpected, positions.Select(x => x + 0.2).ToArray());
            expectedBars.Label = "Ожидаемые частоты (нормализованные)";
            expectedBars.Color = Color.FromArgb(100, 225, 87, 89);

            plt.Title(title);
            plt.YLabel("Частота");
            plt.XLabel("Элементы");
            plt.Legend();

            plt.XAxis.ManualTickPositions(positions.Select(x => x + 0.1).ToArray(), labels);
            plt.XAxis.TickLabelStyle(rotation: 45);
            plt.XAxis.ManualTickSpacing(1);

            string filePath = Path.Combine(FileHelper.GetResultsDirectory(), fileName);
            plt.SaveFig(filePath);
        }
    }
    public static class FileHelper
    {
        public static string GetResultsDirectory()
        {
            string programDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string resultsDir = Path.Combine(Directory.GetParent(programDir).FullName, "Results");

            return resultsDir;
        }
    }
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string resultsDir = FileHelper.GetResultsDirectory();
                var bigramGenerator = new BigramGenerator("bigrams.txt");
                string bigramText = bigramGenerator.GenerateText(1000);
                File.WriteAllText((Path.Combine(resultsDir, "gen-1.txt")), bigramText);
                var actualBigrams = new Dictionary<string, int>();
                for (int i = 0; i < bigramText.Length - 1; i++)
                {
                    string bigram = bigramText.Substring(i, 2).ToLower();
                    actualBigrams[bigram] = actualBigrams.ContainsKey(bigram) ? actualBigrams[bigram] + 1 : 1;
                }

                PlotGenerator.CreateFrequencyPlot(
                    bigramGenerator.bigrams.ToDictionary(x => x.Key, x => x.Value),
                    actualBigrams,
                    "Топ-50 биграмм",
                    "gen-1.png");

                var wordGenerator = new WordGenerator("words.txt");
                string wordText = wordGenerator.GenerateText(1000);
                File.WriteAllText(Path.Combine(resultsDir, "gen-2.txt"), wordText);
                Console.WriteLine("Анализ частот слов...");
                var actualWords = wordText.Split(' ')
                    .GroupBy(x => x.ToLower())
                    .ToDictionary(x => x.Key, x => x.Count());

                PlotGenerator.CreateFrequencyPlot(
                    wordGenerator.wordWeights,
                    actualWords,
                    "Распределение частот слов",
                    "gen-2.png");

                Console.WriteLine("\nГенерация завершена. Результаты:");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}

