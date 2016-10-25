using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для LoadListUsers.xaml
    /// </summary>
    public partial class LoadListUsers : Window
    {
        private Dictionary<char, string> _translit_To_LAT = new Dictionary<char, string>() 
        { 
            {'а', "a"}, {'А', "A"}, {'б', "b"}, {'Б', "B"},
            {'в', "v"}, {'В', "V"}, {'г', "g"}, {'Г', "G"},
            {'д', "d"}, {'Д', "D"}, {'е', "e"}, {'Е', "E"},
            {'ё', "e"}, {'Ё', "E"}, {'ж', "zh"}, {'Ж', "Zh"},
            {'з', "z"}, {'З', "Z"}, {'и', "i"}, {'И', "I"},
            {'й', "y"}, {'Й', "Y"}, {'к', "k"}, {'К', "K"},
            {'л', "l"}, {'Л', "L"}, {'м', "m"}, {'М', "M"},
            {'н', "n"}, {'Н', "N"}, {'о', "o"}, {'О', "O"},
            {'п', "p"}, {'П', "P"}, {'р', "r"}, {'Р', "R"},
            {'с', "s"}, {'С', "S"}, {'т', "t"}, {'Т', "T"},
            {'у', "u"}, {'У', "U"}, {'ф', "f"}, {'Ф', "F"},
            {'х', "kh"}, {'Х', "Kh"}, {'ц', "ts"}, {'Ц', "Ts"},
            {'ч', "ch"}, {'Ч', "Ch"}, {'ш', "sh"}, {'Ш', "Sh"},
            {'щ', "shch"}, {'Щ', "Shch"}, {'ъ', ""}, {'Ъ', ""},
            {'ы', "y"}, {'Ы', "Y"}, {'ь', ""}, {'Ь', ""},
            {'э', "e"}, {'Э', "E"}, {'ю', "yu"}, {'Ю', "Yu"},
            {'я', "ya"}, {'Я', "Ya"}
        }; // Словарь сопоставлений русских и английских букв
        private DirectoryEntry _sessionAD; //сессия АД
        private PrincipalContext _principalContext; // Контекст соединения с АД

        // Данные для загрузки
        public string dataForLoad { get; set; } 

        // Конструктор
        public LoadListUsers(DirectoryEntry entry, PrincipalContext context)
        {
            InitializeComponent();
            _sessionAD = entry;
            _principalContext = context;
            dataForLoad = "";
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
        // Нажата кнопка транслитерации
        private void btTranslit_Click(object sender, RoutedEventArgs e)
        {
            char[] inChars = inputData.Text.ToCharArray();
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
            inputData.Text = result;
        }
        // Нажата кнопка  проверки пользователей в АД
        private void btCheckInAD_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputData.Text))
            {
                return;
            }
            string[] separator = { Environment.NewLine };
            string[] inputArr = inputData.Text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            string result = "";
            string fieldForSearch = "";
            bool? dwResult = null;
            SelectUser _dwSU;
            string selectedUserDN = "";
            if (checkDefault.IsChecked == true) fieldForSearch = "Default";
            if (checkNameInAD.IsChecked == true) fieldForSearch = "name";
            if (checkSurname.IsChecked == true) fieldForSearch = "sn";
            if (checkLogin.IsChecked == true) fieldForSearch = "sAMAccountName";
            if (checkDisplayName.IsChecked == true) fieldForSearch = "displayName";
            Thread t = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    statusBarText.Content = "Проверка в АД...";
                    checkNameInAD.IsEnabled = false;
                    checkSurname.IsEnabled = false;
                    checkLogin.IsEnabled = false;
                    checkDisplayName.IsEnabled = false;
                    checkDefault.IsEnabled = false;
                    btTranslit.IsEnabled = false;
                    btCheckInAD.IsEnabled = false;
                    btLoad.IsEnabled = false;
                }));

                for (int i = 0; i < inputArr.Length; i++)
                {
                    if (!inputArr[i].StartsWith("CN="))
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            statusBarText.Content = "Проверка в АД... " + i.ToString() + @"/" + inputArr.Length.ToString() + " " + inputArr[i].Trim();
                        }));
                        DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                        dirSearcher.SearchScope = SearchScope.Subtree;
                        if (fieldForSearch == "Default")
                            dirSearcher.Filter = string.Format("(&(objectClass=user)(!(objectClass=computer))(|(cn=*{0}*)(sn=*{0}*)(givenName=*{0}*)(sAMAccountName=*{0}*)))", inputArr[i].Trim());
                        else
                            dirSearcher.Filter = string.Format("(&(objectClass=user)(!(objectClass=computer))({1}=*{0}*))", inputArr[i].Trim(), fieldForSearch);
                        dirSearcher.PropertiesToLoad.Add("distinguishedName");
                        SearchResultCollection searchResults = dirSearcher.FindAll();
                        if (searchResults.Count == 1)
                        {
                            result += (string)searchResults[0].Properties["distinguishedName"][0] + Environment.NewLine;
                        }
                        else if (searchResults.Count > 1)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                 _dwSU = new SelectUser(inputArr[i], fieldForSearch, _sessionAD, _principalContext);
                                 _dwSU.Owner = this;
                                 dwResult = _dwSU.ShowDialog();
                                 selectedUserDN = _dwSU.selectedUserDN;
                            }));
                            while (dwResult == null)
                            {
                                Thread.Sleep(100);
                            }
                            if (dwResult == true)
                                result += selectedUserDN + Environment.NewLine;
                            else
                                result += inputArr[i] + Environment.NewLine;
                            dwResult = null;
                        }
                        else
                        {
                            result += inputArr[i] + Environment.NewLine;
                        }
                    }
                    else
                    {
                        result += inputArr[i] + Environment.NewLine;
                    }
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    statusBarText.Content = "";
                    checkNameInAD.IsEnabled = true;
                    checkSurname.IsEnabled = true;
                    checkLogin.IsEnabled = true;
                    checkDisplayName.IsEnabled = true;
                    checkDefault.IsEnabled = true;
                    btTranslit.IsEnabled = true;
                    btCheckInAD.IsEnabled = true;
                    btLoad.IsEnabled = true;
                    inputData.Text = result;
                }));
            });
            t.IsBackground = true;
            t.Start();
        }
        // Нажата кнопка загрузки данных
        private void btLoad_Click(object sender, RoutedEventArgs e)
        {
            dataForLoad = inputData.Text;
            Thread t = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    statusBarText.Content = "Проверка данных перед выгрузкой...";
                    checkNameInAD.IsEnabled = false;
                    checkSurname.IsEnabled = false;
                    checkLogin.IsEnabled = false;
                    checkDisplayName.IsEnabled = false;
                    checkDefault.IsEnabled = false;
                    btTranslit.IsEnabled = false;
                    btCheckInAD.IsEnabled = false;
                    btLoad.IsEnabled = false;
                }));
                string[] separator = { Environment.NewLine };
                string[] loadArr = dataForLoad.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < loadArr.Length; j++)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        statusBarText.Content = "Проверка данных перед выгрузкой... " + j.ToString() + @"/" + loadArr.Length.ToString() + " " + loadArr[j].Trim();
                    }));
                    DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                    dirSearcher.SearchScope = SearchScope.Subtree;
                    dirSearcher.Filter = string.Format("(&(objectClass=user)(!(objectClass=computer))(distinguishedName={0}))", loadArr[j].Trim());
                    dirSearcher.PropertiesToLoad.Add("distinguishedName");
                    SearchResult searchResults = dirSearcher.FindOne();
                    if (searchResults == null)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("Запись:\r\n" + loadArr[j].Trim() + Environment.NewLine + "Отсутствует в АД !!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            statusBarText.Content = "";
                            checkNameInAD.IsEnabled = true;
                            checkSurname.IsEnabled = true;
                            checkLogin.IsEnabled = true;
                            checkDisplayName.IsEnabled = true;
                            checkDefault.IsEnabled = true;
                            btTranslit.IsEnabled = true;
                            btCheckInAD.IsEnabled = true;
                            btLoad.IsEnabled = true;
                        }));
                        return;
                    }
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    statusBarText.Content = "";
                    checkNameInAD.IsEnabled = true;
                    checkSurname.IsEnabled = true;
                    checkLogin.IsEnabled = true;
                    checkDisplayName.IsEnabled = true;
                    checkDefault.IsEnabled = true;
                    btTranslit.IsEnabled = true;
                    btCheckInAD.IsEnabled = true;
                    btLoad.IsEnabled = true;
                    DialogResult = true;
                    Close();
                }));
            });
            t.IsBackground = true;
            t.Start();
        }
        // Нажата кнопка закрытия
        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
