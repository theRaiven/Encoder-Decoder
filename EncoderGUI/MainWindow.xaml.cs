using System.Windows.Input;

namespace EncoderGUI
{
    /// <summary>
    /// Логика взаимодействия для графики
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, double>? alphabet;
        private List<string>? sequence;
        private StringBuilder? resultCode;
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Метод работы с масштабированием(как оно пишется) кнопок (приближать и отдалять)
        /// </summary>
        /// <param name="sender"> Источник вызова (окно)</param>
        /// <param name="e"> Объект события колесика мыши </param>
        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                double step = 0.1; 
                double newScale = MainScale.ScaleX;

                if (e.Delta > 0)
                {
                    newScale += step;
                }
                else if (e.Delta < 0 && newScale > 1)
                {
                    newScale -= step;
                }
                
                MainScale.ScaleX = newScale;
                MainScale.ScaleY = newScale;
                
                e.Handled = true;
            }
        }
        /// <summary>
        /// Вспомогательный метод сохранения данных: закодированная/декодированная последовательность,
        /// Средняя длина кодового слова, Избыточность, Проверка неравенства Крафта
        /// </summary>
        /// <param name="codedStr">Последовательность</param>
        /// <param name="info">Длинна, избыточность, неравенство Крафта</param>
        /// <returns></returns>
        private StringBuilder SaveData(StringBuilder codedStr, StringBuilder info)
     => new StringBuilder(codedStr.ToString()).AppendLine().Append(info.ToString());
        /// <summary>
        /// Загружает файл алфавита из текстового файла и отображает путь в окне.
        /// Выполняет проверку формата и содержания файла.
        /// </summary>
        /// <param name="sender">Источник вызова (кнопка).</param>
        /// <param name="e">Аргументы события.</param>
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
        /// <summary>
        /// Загружает последовательности алфавита из текстового файла и отображает путь в окне.
        /// Выполняет проверку формата и содержания файла.
        /// </summary>
        /// <param name="sender">Источник вызова (кнопка).</param>
        /// <param name="e">Аргументы события.</param>
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
        /// <summary>
        /// Выполняет кодирование загруженной последовательности по алфавиту.
        /// Отображает результат и статистику кодирования в окне.
        /// </summary>
        /// <param name="sender">Источник вызова (кнопка).</param>
        /// <param name="e">Аргументы события.</param>
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

                resultCode = SaveData(encodedStr, info);


                MessageBox.Show(info.ToString(), "Результаты кодирования");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка кодирования:\n" + ex.Message);
            }
        }
        /// <summary>
        /// Выполняет декодирование загруженной последовательности по алфавиту.
        /// Отображает результат и статистику декодирования в окне.
        /// </summary>
        /// <param name="sender">Источник вызова (кнопка).</param>
        /// <param name="e">Аргументы события.</param>
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

                resultCode = SaveData(decodedStr, info);

                MessageBox.Show(info.ToString(), "Результаты декодирования");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка декодирования:\n" + ex.Message);
            }
        }
        /// <summary>
        /// Сохраняет результат кодирования/декодирования в выбранный пользователем файл.
        /// </summary>
        /// <param name="sender">Источник вызова (кнопка).</param>
        /// <param name="e">Аргументы события.</param>
        private void btnSaveOutput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (resultCode is null || resultCode.Length == 0)
                {
                    MessageBox.Show("Нет данных для сохранения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt",
                    DefaultExt = ".txt",
                    Title = "Сохранить результат"
                };

                if (sfd.ShowDialog() == true)
                {
                    SaveEncodedToFile(sfd.FileName, resultCode);
                    MessageBox.Show($"Результат сохранён в файл:\n{sfd.FileName}", "Сохранено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
