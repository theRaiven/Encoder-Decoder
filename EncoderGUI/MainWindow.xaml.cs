using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace EncoderGUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, double>? alphabet;
        private List<string>? sequence;
        private StringBuilder? encoded;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoadAlphabet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog? ofd = new OpenFileDialog();
                ofd.Filter = "Text files (*.txt)|*.txt";
                if (ofd.ShowDialog() == true)
                {
                    alphabet = ReadAlphabet(ofd.FileName);
                    txtAlphabetFile.Text = ofd.FileName;
                    MessageBox.Show("Алфавит загружен!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private void btnLoadSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog? ofd = new OpenFileDialog();
                ofd.Filter = "Text files (*.txt)|*.txt";
                if (ofd.ShowDialog() == true)
                {
                    sequence = ReadSequence(ofd.FileName);
                    txtSequenceFile.Text = ofd.FileName;
                    MessageBox.Show("Последовательность загружена!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void btnEncode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (alphabet == null || sequence == null)
                {
                    MessageBox.Show("Сначала загрузите алфавит и последовательность.");
                    return;
                }

                var (encodedStr, codes, avgLength, redundancy, kraft) = Encoding(alphabet, sequence);

                txtOutput.Text = encodedStr.ToString();

                // Выводим данные
                StringBuilder info = new();
                info.AppendLine("Кодовые слова:");
                int idx = 0;
                foreach (var symbol in alphabet.Keys)
                {
                    info.AppendLine($"{symbol}: {codes[idx++]}");
                }
                info.AppendLine($"\nСредняя длина кодового слова: {avgLength:F4} бит");
                info.AppendLine($"Избыточность: {redundancy:F4} бит");
                info.AppendLine($"Проверка неравенства Крафта: {kraft:F4} {(kraft <= 1 ? "✓" : "✗")}");

                MessageBox.Show(info.ToString(), "Результаты кодирования");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка кодирования:\n" + ex.Message);
            }
        }


        private void btnDecode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (alphabet == null || sequence == null)
                {
                    MessageBox.Show("Сначала загрузите алфавит и последовательность.");
                    return;
                }

                var (decodedStr, codes, avgLength, redundancy, kraft) = Decoding(alphabet, sequence);

                txtOutput.Text = decodedStr.ToString();

                StringBuilder info = new();
                info.AppendLine("Кодовые слова:");
                int idx = 0;
                foreach (var symbol in alphabet.Keys)
                {
                    info.AppendLine($"{symbol}: {codes[idx++]}");
                }
                info.AppendLine($"\nСредняя длина кодового слова: {avgLength:F4} бит");
                info.AppendLine($"Избыточность: {redundancy:F4} бит");
                info.AppendLine($"Проверка неравенства Крафта: {kraft:F4} {(kraft <= 1 ? "✓" : "✗")}");

                MessageBox.Show(info.ToString(), "Результаты декодирования");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка декодирования:\n" + ex.Message);
            }
        }

    }
}
