using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для TranslitToLAT.xaml
    /// </summary>
    public partial class TranslitToLAT : Window
    {
        private Dictionary<char, string> _translit_To_LAT; // Словарь сопоставлений русских и английских букв

        // Конструктор
        public TranslitToLAT()
        {
            InitializeComponent();
            _translit_To_LAT = new Dictionary<char, string>();
            
            #region Заполнения словаря сопоставлений русских и английских букв
            _translit_To_LAT.Add('а', "a"); _translit_To_LAT.Add('А', "A"); _translit_To_LAT.Add('б', "b"); _translit_To_LAT.Add('Б', "B");
            _translit_To_LAT.Add('в', "v"); _translit_To_LAT.Add('В', "V"); _translit_To_LAT.Add('г', "g"); _translit_To_LAT.Add('Г', "G");
            _translit_To_LAT.Add('д', "d"); _translit_To_LAT.Add('Д', "D"); _translit_To_LAT.Add('е', "e"); _translit_To_LAT.Add('Е', "E");
            _translit_To_LAT.Add('ё', "e"); _translit_To_LAT.Add('Ё', "E"); _translit_To_LAT.Add('ж', "zh"); _translit_To_LAT.Add('Ж', "Zh");
            _translit_To_LAT.Add('з', "z"); _translit_To_LAT.Add('З', "Z"); _translit_To_LAT.Add('и', "i"); _translit_To_LAT.Add('И', "I");
            _translit_To_LAT.Add('й', "y"); _translit_To_LAT.Add('Й', "Y"); _translit_To_LAT.Add('к', "k"); _translit_To_LAT.Add('К', "K");
            _translit_To_LAT.Add('л', "l"); _translit_To_LAT.Add('Л', "L"); _translit_To_LAT.Add('м', "m"); _translit_To_LAT.Add('М', "M");
            _translit_To_LAT.Add('н', "n"); _translit_To_LAT.Add('Н', "N"); _translit_To_LAT.Add('о', "o"); _translit_To_LAT.Add('О', "O");
            _translit_To_LAT.Add('п', "p"); _translit_To_LAT.Add('П', "P"); _translit_To_LAT.Add('р', "r"); _translit_To_LAT.Add('Р', "R");
            _translit_To_LAT.Add('с', "s"); _translit_To_LAT.Add('С', "S"); _translit_To_LAT.Add('т', "t"); _translit_To_LAT.Add('Т', "T");
            _translit_To_LAT.Add('у', "u"); _translit_To_LAT.Add('У', "U"); _translit_To_LAT.Add('ф', "f"); _translit_To_LAT.Add('Ф', "F");
            _translit_To_LAT.Add('х', "kh"); _translit_To_LAT.Add('Х', "Kh"); _translit_To_LAT.Add('ц', "ts"); _translit_To_LAT.Add('Ц', "Ts");
            _translit_To_LAT.Add('ч', "ch"); _translit_To_LAT.Add('Ч', "Ch"); _translit_To_LAT.Add('ш', "sh"); _translit_To_LAT.Add('Ш', "Sh");
            _translit_To_LAT.Add('щ', "shch"); _translit_To_LAT.Add('Щ', "Shch"); _translit_To_LAT.Add('ъ', ""); _translit_To_LAT.Add('Ъ', "");
            _translit_To_LAT.Add('ы', "y"); _translit_To_LAT.Add('Ы', "Y"); _translit_To_LAT.Add('ь', ""); _translit_To_LAT.Add('Ь', "");
            _translit_To_LAT.Add('э', "e"); _translit_To_LAT.Add('Э', "E"); _translit_To_LAT.Add('ю', "yu"); _translit_To_LAT.Add('Ю', "Yu");
            _translit_To_LAT.Add('я', "ya"); _translit_To_LAT.Add('Я', "Ya");
            #endregion
        }

        // Нажата кнопка в основном окне
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
        
        // Нажата кнопка преобразования
        private void btTranslit_Click(object sender, RoutedEventArgs e)
        {
            char [] inChars = inputString.Text.ToCharArray();
            string result = "";
            foreach (char c in inChars) 
            {
                if (_translit_To_LAT.ContainsKey(c))
                {
                    result += _translit_To_LAT[c];
                }
                else 
                {
                    result += c;
                }
            }
            outString.Text = result;
        }
    }
}
