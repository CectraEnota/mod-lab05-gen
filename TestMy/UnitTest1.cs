using ProjCharGenerator;

namespace TestMy
{
    public class BigramGeneratorTests
    {
        private readonly string _testBigramsPath = "test_bigrams.txt";

        public BigramGeneratorTests()
        {
            File.WriteAllText(_testBigramsPath, "ab\t10\nbc\t5\ncd\t3\nde\t2\nef\t1");
        }

        [Fact]
        public void BigramGenerator_LoadsFileCorrectly()
        {
            var generator = new BigramGenerator(_testBigramsPath);

            Assert.Equal(5, generator.bigrams.Count);
            Assert.Equal(10, generator.bigrams["ab"]);
            Assert.Equal(1, generator.bigrams["ef"]);
        }

        [Fact]
        public void BigramGenerator_ThrowsFileNotFound()
        {
            Assert.Throws<FileNotFoundException>(() => new BigramGenerator("nonexistent.txt"));
        }

        [Fact]
        public void GetRandomBigram_ReturnsValidBigram()
        {
            var generator = new BigramGenerator(_testBigramsPath);

            var bigram = generator.GetRandomBigram();

            Assert.Equal(2, bigram.Length);
            Assert.Contains(bigram, generator.bigrams.Keys);
        }

        [Fact]
        public void GetNextBigram_ReturnsValidBigram()
        {
            var generator = new BigramGenerator(_testBigramsPath);

            var nextBigram = generator.GetNextBigram('b');

            Assert.Equal('b', nextBigram[0]);
            Assert.Contains(nextBigram, generator.bigrams.Keys);
        }

        [Fact]
        public void GenerateText_ReturnsCorrectLength()
        {
            var generator = new BigramGenerator(_testBigramsPath);

            var text = generator.GenerateText(50);

            Assert.Equal(50, text.Length);
        }

        [Fact]
        public void GenerateText_StartsWithValidBigram()
        {
            var generator = new BigramGenerator(_testBigramsPath);

            var text = generator.GenerateText(10);
            var firstBigram = text.Substring(0, 2);

            Assert.Contains(firstBigram, generator.bigrams.Keys);
        }
    }

    public class WordGeneratorTests
    {
        private readonly string _testWordsPath = "test_words.txt";

        public WordGeneratorTests()
        {
            File.WriteAllText(_testWordsPath, "apple\t10\nbanana\t5\ncherry\t3\ndate\t2\nelderberry\t1");
        }

        [Fact]
        public void WordGenerator_LoadsFileCorrectly()
        {
            var generator = new WordGenerator(_testWordsPath);

            Assert.Equal(5, generator.wordWeights.Count);
            Assert.Equal(10, generator.wordWeights["apple"]);
            Assert.Equal(1, generator.wordWeights["elderberry"]);
        }

        [Fact]
        public void WordGenerator_ThrowsFileNotFound()
        {
            Assert.Throws<FileNotFoundException>(() => new WordGenerator("nonexistent.txt"));
        }

        [Fact]
        public void GetRandomWord_ReturnsValidWord()
        {
            var generator = new WordGenerator(_testWordsPath);

            var word = generator.GetRandomWord();

            Assert.Contains(word, generator.wordWeights.Keys);
        }

        [Fact]
        public void GenerateText_ReturnsCorrectWordCount()
        {
            var generator = new WordGenerator(_testWordsPath);

            var text = generator.GenerateText(20);
            var wordCount = text.Split(' ').Length;

            Assert.Equal(20, wordCount);
        }

        [Fact]
        public void GenerateText_ContainsValidWords()
        {
            var generator = new WordGenerator(_testWordsPath);

            var text = generator.GenerateText(10);
            var words = text.Split(' ');

            foreach (var word in words)
            {
                Assert.Contains(word, generator.wordWeights.Keys);
            }
        }
    }

    public class FileHelperTests
    {
        [Fact]
        public void GetResultsDirectory_ReturnsValidPath()
        {
            var path = FileHelper.GetResultsDirectory();

            Assert.NotNull(path);
            Assert.Contains("Results", path);
            Assert.True(Directory.Exists(path));
        }
    }
}