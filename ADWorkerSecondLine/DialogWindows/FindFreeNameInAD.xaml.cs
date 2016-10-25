using System;
using System.DirectoryServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для FindFreeNameInAD.xaml
    /// </summary>
    public partial class FindFreeNameInAD : Window
    {
        private DirectoryEntry _sessionAD; //сессия АД
        private long counter; // текущее значение счетчика
        private bool _searchIsRun; // текущее состояние поиска
        private string _nameForFind; // текущее имя для поиска

        // Конструктор
        public FindFreeNameInAD(DirectoryEntry entry)
        {
            InitializeComponent();
            _sessionAD = entry;
            _searchIsRun = false;
            _nameForFind = "";
            startName.Focus();
        }
        // Событие потери фокуса начального значения
        private void startFind_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.Parse(startFind.Text) > int.Parse(endFind.Text))
            {
                MessageBox.Show("Начальное значение не может быть больше конечного!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                startFind.Text = "1";
            }
        }
        // Событие потери фокуса конечного значения
        private void endFind_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.Parse(startFind.Text) > int.Parse(endFind.Text))
            {
                MessageBox.Show("Конечное значение не может быть меньше начального!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                endFind.Text = (int.Parse(startFind.Text)+1).ToString();
            }
        }
        // Нажата кнопка начала поиска
        private void btFind_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(startName.Text))
            {
                MessageBox.Show("Не указано начало имени!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int start = int.Parse(startFind.Text);
            int end = int.Parse(endFind.Text);
            string name = startName.Text;
            _nameForFind = startName.Text;
            counter = start;
            _searchIsRun = true;
            btFind.IsEnabled = false;
            btStopFind.IsEnabled = true;
            btNext.IsEnabled = false;
            resultName.Foreground = new SolidColorBrush(Colors.Red);
            Thread t = new Thread(() =>
            {
                for (; counter <= end; counter++)
                {
                    if (_searchIsRun == false) 
                    {
                        _searchIsRun = false;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            btFind.IsEnabled = true;
                            btStopFind.IsEnabled = false;
                            btNext.IsEnabled = true;
                        }));
                        break;
                    };
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        resultName.Text = name + counter.ToString();
                    }));
                    Thread.Sleep(20);
                    DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                    dirSearcher.SearchScope = SearchScope.Subtree;
                    dirSearcher.Filter = string.Format("(&(objectClass=computer)(sAMAccountName=" + name + counter.ToString() + "$" + "))");
                    SearchResult searchResults = dirSearcher.FindOne();
                    if (searchResults == null)
                    {
                        _searchIsRun = false;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            resultName.Foreground = new SolidColorBrush(Colors.Green);
                            btFind.IsEnabled = true;
                            btStopFind.IsEnabled = false;
                            btNext.IsEnabled = true;
                        }));
                        break;
                    }
                }
                if (counter > end)
                {
                    _searchIsRun = false;
                    
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        resultName.Text = "";
                        resultName.Foreground = new SolidColorBrush(Colors.Black);
                        btFind.IsEnabled = true;
                        btStopFind.IsEnabled = false;
                        btNext.IsEnabled = false;
                        MessageBox.Show("Нет свободных имён в заданном диапазоне!!!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }));
                }
            });
            t.IsBackground = true;
            t.Start();
        }
        // Нажата кнопка остановки поиска
        private void btStopFind_Click(object sender, RoutedEventArgs e)
        {
            _searchIsRun = false;
            btFind.IsEnabled = true;
            btStopFind.IsEnabled = false;
            btNext.IsEnabled = true;
        }
        // Нажата кнопка продолжения поиска
        private void btNext_Click(object sender, RoutedEventArgs e)
        {
            if (_nameForFind != startName.Text)
            {
                MessageBox.Show("Изменилось имя для поиска. Начните поиск сначала!!!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int end = int.Parse(endFind.Text);
            string name = startName.Text;
            _searchIsRun = true;
            btFind.IsEnabled = false;
            btStopFind.IsEnabled = true;
            btNext.IsEnabled = false;
            resultName.Foreground = new SolidColorBrush(Colors.Red);
            counter++;
            Thread t = new Thread(() =>
            {
                for (; counter <= end; counter++)
                {
                    if (_searchIsRun == false)
                    {
                        _searchIsRun = false;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //resultName.Foreground = new SolidColorBrush(Colors.Black);
                            //resultName.Text = "";
                            btFind.IsEnabled = true;
                            btStopFind.IsEnabled = false;
                            btNext.IsEnabled = true;
                        }));
                        break;
                    };
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        resultName.Text = name + counter.ToString();
                    }));
                    Thread.Sleep(20);
                    DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                    dirSearcher.SearchScope = SearchScope.Subtree;
                    dirSearcher.Filter = string.Format("(&(objectClass=computer)(sAMAccountName=" + name + counter.ToString() + "$" + "))");
                    SearchResult searchResults = dirSearcher.FindOne();
                    if (searchResults == null)
                    {
                        _searchIsRun = false;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            resultName.Foreground = new SolidColorBrush(Colors.Green);
                            btFind.IsEnabled = true;
                            btStopFind.IsEnabled = false;
                            btNext.IsEnabled = true;
                        }));
                        break;
                    }
                }
                if (counter > end)
                {
                    _searchIsRun = false;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        resultName.Text = "";
                        resultName.Foreground = new SolidColorBrush(Colors.Black);
                        btFind.IsEnabled = true;
                        btStopFind.IsEnabled = false;
                        btNext.IsEnabled = false;
                        MessageBox.Show("Диапазон для поиска исчерпан, расширьте диапазон или начните поиск сначала!!!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }));
                }
            });
            t.IsBackground = true;
            t.Start();
        }
        // Событие нажатия кнопки Enter в поле имени для поиска
        private void startName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btFind_Click(sender, new RoutedEventArgs());
            }
        }
        // Нажата кнопка в основном окне
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
