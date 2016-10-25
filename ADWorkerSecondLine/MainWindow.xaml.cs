using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.DirectoryServices;
using ADWorkerSecondLine.UISearchTextBox;
using ADWorkerSecondLine.Model;
using ADWorkerSecondLine.DataProvider;
using ADWorkerSecondLine.CustomEventArgs;
using System.Threading;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.ComponentModel;
using ADWorkerSecondLine.Converters;
using System.IO;
using System.Reflection;

namespace ADWorkerSecondLine
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Поля
        private long _maxPwdAgeDay; // Максимальное время действия пароля в днях.
        private bool _isEnableFormUser; // Доступность вкладки Пользователи
        private bool _isEnableFormPC; // Доступность вкладки Компьютеры
        private bool _isEnableFormGroup; // Доступность вкладки Группы
        private bool _isEnableEditFormUser; // Доступность редактирования формы найденного пользователя
        private bool _isEnableEditFormPC; // Доступность редактирования формы найденного компьютера
        private bool _isEnableEditFormGroup; // Доступность редактирования формы найденной группы
        private bool _connectedIsSet; // Флаг соединения с АД
        private DirectoryEntry _connectedSession; // Сессия успешного подключения к АД
        private PrincipalContext _principalContext; // Контекст соединения с АД
        private Dictionary<string, string> fieldsInAD; // Сопоставление групп поиска и полей в АД для вкладки Пользователи
        private Dictionary<string, string> fieldsPCInAD; // Сопоставление групп поиска и полей в АД для вкладки Компьютеры
        private Dictionary<string, string> fieldsGroupInAD; // Сопоставление групп поиска и полей в АД для вкладки Группы
        private static bool _stopSearchProcess = false; // Флаг остановки обратотки результатов поиска
        private static bool _searchIsRun = false; // Флаг состояния поиска
        private ListSortDirection _sortDirection; // Передыдущий режим сортировки вкладки Пользователи
        private GridViewColumnHeader _sortColumn; // Предыдущая колонка сортировки вкладки Пользователи
        private ListSortDirection _sortDirectionPC; // Передыдущий режим сортировки вкладки Компьютеры
        private GridViewColumnHeader _sortColumnPC; // Предыдущая колонка сортировки вкладки Компьютеры
        private ListSortDirection _sortDirectionGroup; // Передыдущий режим сортировки вкладки Группы
        private GridViewColumnHeader _sortColumnGroup; // Предыдущая колонка сортировки вкладки Группы
        #endregion

        #region Свойства
        // Доступность вкладки Пользователи
        public bool IsEnableFormUser
        {
            get { return _isEnableFormUser; }
            set
            {
                _isEnableFormUser = value;
                userTab.IsEnabled = value;
                search.IsEnabled = value;
                findOnlyLockUser.IsEnabled = value;
                findUserInOU.IsEnabled = value;
                btUpdateFind.IsEnabled = value;
                filterFindUser.IsEnabled = value;
                listFindedUsers.IsEnabled = value;
                listUserComps.IsEnabled = value;
                btTranslitToLAT.IsEnabled = value;
                if (value)
                {
                    if (listFindedUsers.SelectedItem != null)
                        IsEnableEditFormUser = value;
                }
                else
                {
                    IsEnableEditFormUser = value;
                }
            }
        }
        // Доступность вкладки Компьютеры
        public bool IsEnableFormPC
        {
            get { return _isEnableFormPC; }
            set
            {
                _isEnableFormPC = value;
                pcTab.IsEnabled = value;
                searchPC.IsEnabled = value;
                findPCInOU.IsEnabled = value;
                btUpdatePCFind.IsEnabled = value;
                filterFindPC.IsEnabled = value;
                listFindedPC.IsEnabled = value;
                btFindFreeNameInAD.IsEnabled = value;
                if (value)
                {
                    if (listFindedPC.SelectedItem != null)
                        IsEnableEditFormPC = value;
                }
                else
                {
                    IsEnableEditFormPC = value;
                }
            }
        }
        // Доступность вкладки Компьютеры
        public bool IsEnableFormGroup
        {
            get { return _isEnableFormPC; }
            set
            {
                _isEnableFormGroup = value;
                groupTab.IsEnabled = value;
                searchGroup.IsEnabled = value;
                findGroupInOU.IsEnabled = value;
                btUpdateGroupFind.IsEnabled = value;
                filterFindGroup.IsEnabled = value;
                listFindedGroup.IsEnabled = value;
                if (value)
                {
                    if (listFindedGroup.SelectedItem != null)
                        IsEnableEditFormGroup = value;
                }
                else
                {
                    IsEnableEditFormGroup = value;
                }
            }
        }
        // Доступность редактирования формы найденного пользователя
        public bool IsEnableEditFormUser
        {
            get { return _isEnableEditFormUser; }
            set
            {
                _isEnableEditFormUser = value;
                btChangeUserName.IsEnabled = value;
                btChangeUserSurname.IsEnabled = value;
                btChangeUserLogin.IsEnabled = value;
                btChangeUserNameInAD.IsEnabled = value;
                btChangeUserPost.IsEnabled = value;
                btChangeUserDepartment.IsEnabled = value;
                btChangeUserCity.IsEnabled = value;
                btChangeUserIntPhone.IsEnabled = value;
                btChangeUserMobPhone.IsEnabled = value;
                btChangeUserExpireDate.IsEnabled = value;
                btResetPassToDefault.IsEnabled = value;
                btUnlockUser.IsEnabled = value;
                btUserGroups.IsEnabled = value;
                btUpdateUserData.IsEnabled = value;
                btDisableEnableUser.IsEnabled = value;
                btChangeUserDisplayName.IsEnabled = value;
                btChangeUserOrganization.IsEnabled = value;
                btChangeUserAdress.IsEnabled = value;
                userPassIsDefault.IsEnabled = value;
                btMoveUserInAD.IsEnabled = value;
            }
        }
        // Доступность редактирования формы найденного Компьютера
        public bool IsEnableEditFormPC
        {
            get { return _isEnableEditFormPC; }
            set
            {
                _isEnableEditFormPC = value;
                btChangePCLocation.IsEnabled = value;
                btChangePCDescription.IsEnabled = value;
                btMovePCInAD.IsEnabled = value;
                btPCGroups.IsEnabled = value;
                btUpdatePCData.IsEnabled = value;
                btDisableEnablePC.IsEnabled = value;
            }
        }
        // Доступность редактирования формы найденного Компьютера
        public bool IsEnableEditFormGroup
        {
            get { return _isEnableEditFormGroup; }
            set
            {
                _isEnableEditFormGroup = value;
                btMoveGroupInAD.IsEnabled = value;
                btEditGroupNameInAD.IsEnabled = value;
                btEditGroupDescription.IsEnabled = value;
                btEditGroupDisplayName.IsEnabled = value;
                btEditGroupNameWin.IsEnabled = value;
                btEditGroupDescription.IsEnabled = value;
            }
        }
        // Возвращает сессию с АД
        public DirectoryEntry ConnectedSession
        {
            get { return _connectedSession; }
        }
        // Возвращает Флаг остановки обработки результатов поиска
        public static bool StopSearchProcess
        {
            get { return _stopSearchProcess; }
            private set { _stopSearchProcess = value; }
        }
        // Возвращает Флаг состояния поиска
        public static bool SearchIsRun
        {
            get { return _searchIsRun; }
            private set { _searchIsRun = value; }
        }
        #endregion

        #region Конструктор
        public MainWindow()
        {
            // Инициализация основного окна
            InitializeComponent();
            // Отключаем все вкладки
            rootTabControl.IsEnabled = false;
            // Отключаем форму пользователя т.к. не установлено соединение с АД
            IsEnableFormUser = false;
            // Отключаем форму Компьютера т.к. не установлено соединение с АД
            IsEnableFormPC = false;
            // Отключаем форму Группы т.к. не установлено соединение с АД
            IsEnableFormGroup = false;
            // Устанавливаем домен и логин для подключения к АД тами же как у пользователя запустившего программу
            if (!string.IsNullOrWhiteSpace(Environment.UserDomainName))
            {
                domainForConnect.Text = Environment.UserDomainName;
                loginForConnect.Focus();
            }
            // Установка фокуса в поле Домен
            domainForConnect.Focus();
            #region Сопоставление групп поиска и полей в АД для вкладки Пользователи
            fieldsInAD = new Dictionary<string, string>();
            fieldsInAD.Add("По умолчанию","Default");
            fieldsInAD.Add("Имя пользователя в АД", "name");
            fieldsInAD.Add("Отображаемое имя", "displayName");
            fieldsInAD.Add("Организация", "company");
            fieldsInAD.Add("Имя", "givenName");
            fieldsInAD.Add("Фамилия", "sn");
            fieldsInAD.Add("Логин", "sAMAccountName");
            fieldsInAD.Add("Должность", "title");
            fieldsInAD.Add("Отдел", "department");
            fieldsInAD.Add("Адрес", "streetAddress");
            fieldsInAD.Add("Город", "l");
            fieldsInAD.Add("Эл. Почта", "mail");
            fieldsInAD.Add("Телефон внутренний", "telephoneNumber");
            fieldsInAD.Add("Телефон мобильный", "mobile");
            #endregion
            // Устанавливаем настройки поиска пользователя в домене, создаём группы поиска и выбираем стиль отображения групп
            List<string> sections = new List<string> { 
                "По умолчанию", 
                "Имя пользователя в АД", 
                "Отображаемое имя", 
                "Организация",
                "Имя", 
                "Фамилия", 
                "Логин", 
                "Должность", 
                "Отдел", 
                "Адрес",
                "Город", 
                "Эл. Почта", 
                "Телефон внутренний", 
                "Телефон мобильный" };
            search.SectionsList = sections;
            search.SectionsStyle = SearchTextBox.SectionsStyles.RadioBoxStyle;
            // Определяем обработчик поиска для вкладки Пользователи
            search.OnSearch += new RoutedEventHandler(search_OnSearch);

            #region Сопоставление групп поиска и полей в АД для вкладки Компьютеры
            fieldsPCInAD = new Dictionary<string, string>();
            fieldsPCInAD.Add("По умолчанию", "Default");
            fieldsPCInAD.Add("Имя", "name");
            fieldsPCInAD.Add("DNS имя", "dNSHostName");
            fieldsPCInAD.Add("Размещение", "location");
            fieldsPCInAD.Add("Описание", "description");
            #endregion
            // Устанавливаем настройки поиска компьютера в домене, создаём группы поиска и выбираем стиль отображения групп
            List<string> sectionsPC = new List<string> { 
                "По умолчанию", 
                "Имя", 
                "DNS имя", 
                "Размещение",
                "Описание" };
            searchPC.SectionsList = sectionsPC;
            searchPC.SectionsStyle = SearchTextBox.SectionsStyles.RadioBoxStyle;
            // Определяем обработчик поиска для вкладки Компьютеры
            searchPC.OnSearch += new RoutedEventHandler(searchPC_OnSearch);

            #region Сопоставление групп поиска и полей в АД для вкладки Группы
            fieldsGroupInAD = new Dictionary<string, string>();
            fieldsGroupInAD.Add("По умолчанию", "Default");
            fieldsGroupInAD.Add("Имя в АД", "name");
            fieldsGroupInAD.Add("Отображаемое имя", "displayName");
            fieldsGroupInAD.Add("Имя группы Win", "sAMAccountName");
            fieldsGroupInAD.Add("Описание", "description");
            fieldsGroupInAD.Add("Эл. Адресс", "mail");
            #endregion
            // Устанавливаем настройки поиска группы в домене, создаём группы поиска и выбираем стиль отображения групп
            List<string> sectionsGroup = new List<string> { 
                "По умолчанию", 
                "Имя в АД", 
                "Отображаемое имя", 
                "Имя группы Win",
                "Описание",
                "Эл. Адресс" };
            searchGroup.SectionsList = sectionsGroup;
            searchGroup.SectionsStyle = SearchTextBox.SectionsStyles.RadioBoxStyle;
            // Определяем обработчик поиска для вкладки группы
            searchGroup.OnSearch += new RoutedEventHandler(searchGroup_OnSearch);
        }
        #endregion

        #region События
        // Нажата клавиша клавиатуры в основном окне
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (SearchIsRun & e.Key == Key.Escape)
            {
                StopSearchProcess = true;
            }
        }
        // Сгенерировано событие отображения информации процесса поиска
        private void updateInfoSearchProcess(object sender, MessageEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                statusBarText.Content = e.message;
            }));
        }
        // Нажата кнопка подключения к АД. Выполняется проверка логина и пароля, а так же устанавливаеться соединение с АД
        private void btConnect_Click(object sender, RoutedEventArgs e)
        {
            //Если соединение не установленно, установить новое, иначе разорвать текущее.
            if (!_connectedIsSet)
            {
                #region Проверка наличия всех необходиммых данных для установки соединения
                if (string.IsNullOrWhiteSpace(domainForConnect.Text))
                {
                    MessageBox.Show("Не указан домен для подключения!!!");
                    return;
                }
                if (string.IsNullOrWhiteSpace(loginForConnect.Text))
                {
                    MessageBox.Show("Не указан логин для подключения!!!");
                    return;
                }
                if (passForConnect.SecurePassword.Length <= 0)
                {
                    MessageBox.Show("Не указан пароль для подключения!!!");
                    return;
                }
                #endregion
                // Объединяем домен и логин, для подключения к АД
                string domainAndUsername = domainForConnect.Text + @"\" + loginForConnect.Text;
                // Сохраняем логин для использования во вторичном потоке
                string login = loginForConnect.Text;
                // Сохраняем домен для использования во вторичном потоке
                string domain = domainForConnect.Text;
                // Сохраняем пароль для использования во вторичном потоке
                string pass = passForConnect.Password;
                // Создаем класс подключения к АД
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domainForConnect.Text, domainAndUsername, passForConnect.Password);
                #region Создание нового потока для подключения к АД, ошибки выводятся сообщением.
                new Thread(() =>
                {
                    // Выводим информацию о начале установки связи с АД
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        statusBarText.Content = "Установка связи с АД...";
                        btConnect.IsEnabled = false;
                        textbtConnect.Text = "Подключение...";
                        domainForConnect.IsEnabled = false;
                        loginForConnect.IsEnabled = false;
                        passForConnect.IsEnabled = false;
                    }));

                    try
                    {
                        // Пробуем подключиться к АД
                        object obj = entry.NativeObject;
                        DirectorySearcher searchAD = new DirectorySearcher(entry);
                        searchAD.Filter = "maxPwdAge=*";
                        SearchResultCollection results = searchAD.FindAll();
                        if (results.Count >= 1)
                        {
                            Int64 maxPwdAge = (Int64)results[0].Properties["maxPwdAge"][0];
                            _maxPwdAgeDay = maxPwdAge / -864000000000;
                        }
                        else
                        {
                            _maxPwdAgeDay = 0;
                        }
                        // Подключение прошло успешно
                        _connectedIsSet = true;
                        _connectedSession = entry;
                        _principalContext = new PrincipalContext(ContextType.Domain, domain, login, pass);
                        // Загрузка плагинов
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            statusBarText.Content = "Загрузка плагинов...";
                        }));
                        string run = Assembly.GetExecutingAssembly().Location;
                        int index = run.LastIndexOf('\\');
                        string pluginDir = run.Substring(0, index) + "\\Plugins";
                        if (Directory.Exists(pluginDir))
                        {
                            bool pluginLoadFinished = false;
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                pluginLoadFinished = AddPluginToForm(rootTabControl,
                                                                     LoadPlugins(pluginDir, domainAndUsername, pass, _connectedSession, _principalContext)
                                                     );
                            }));
                            while (!pluginLoadFinished)
                            {
                                Thread.Sleep(200);
                            }
                        }

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            domainForConnect.IsEnabled = false;
                            loginForConnect.IsEnabled = false;
                            passForConnect.IsEnabled = false;
                            rootTabControl.IsEnabled = true;
                            IsEnableFormUser = true;
                            IsEnableFormPC = true;
                            IsEnableFormGroup = true;
                            imagebtConnect.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/connect_established.ico", UriKind.RelativeOrAbsolute));
                            textbtConnect.Text = "Отключить";
                            btConnect.IsEnabled = true;
                            statusBarText.Content = "";
                            this.search.SetFocus();
                        }));
                    }
                    catch (Exception ex)
                    {
                        // Не удалось подключиться к АД, выводим сообщение с ошибкой.
                        _connectedIsSet = false;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            textbtConnect.Text = "Подключиться";
                            imagebtConnect.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/connect_no.ico", UriKind.RelativeOrAbsolute));
                            btConnect.IsEnabled = true;
                            domainForConnect.IsEnabled = true;
                            loginForConnect.IsEnabled = true;
                            passForConnect.IsEnabled = true;
                            statusBarText.Content = "";
                            _maxPwdAgeDay = 0;
                            ShowErrorMessage(ex.Message);
                        }));
                    }
                }).Start();
                #endregion
            }
            else
            {
                domainForConnect.IsEnabled = true;
                loginForConnect.IsEnabled = true;
                passForConnect.IsEnabled = true;
                IsEnableFormUser = false;
                _connectedIsSet = false;
                _connectedSession = null;
                _maxPwdAgeDay = 0;
                textbtConnect.Text = "Подключиться";
                imagebtConnect.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/connect_no.ico", UriKind.RelativeOrAbsolute));
                listFindedUsers.ItemsSource = AsyncDataProvider.GetItems();
                search.Text = "";
            }
        }
        // Нажата клавиша Enter в поле ввода пароля
        private void passForConnect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btConnect_Click(this, new RoutedEventArgs());
            }
        }

        #region События вкладки Пользователи
        // Выполнено нажатие на заголовок столбца с найденными пользователями
        private void listFindedUsers_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
            if (column == null)
            {
                return;
            }

            if (_sortColumn == column)
            {
                // Выбираем режим сортировки в зависимости от того какая сортировка была до этого
                _sortDirection = _sortDirection == ListSortDirection.Ascending ?
                                                   ListSortDirection.Descending :
                                                   ListSortDirection.Ascending;
            }
            else
            {
                // Убираем стрелку сортировки из предыдущей колонки
                if (_sortColumn != null)
                {
                    _sortColumn.Column.HeaderTemplate = null;
                    _sortColumn.Column.Width = _sortColumn.ActualWidth - 20;
                }

                _sortColumn = column;
                _sortDirection = ListSortDirection.Ascending;
                column.Column.Width = column.ActualWidth + 20;
            }

            if (_sortDirection == ListSortDirection.Ascending)
            {
                column.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
            }
            else
            {
                column.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;
            }

            string header = string.Empty;

            // если используется привязка и имя свойства не совпадает с содержанием заголовка
            Binding b = _sortColumn.Column.DisplayMemberBinding as Binding;
            if (b != null)
            {
                header = b.Path.Path;
            }

            ICollectionView resultDataView = CollectionViewSource.GetDefaultView(listFindedUsers.ItemsSource);
            resultDataView.SortDescriptions.Clear();
            resultDataView.SortDescriptions.Add(new SortDescription(header, _sortDirection));
        }
        // Запущен процесс поиска пользователя в АД
        private void search_OnSearch(object sender, RoutedEventArgs e)
        {
            SearchEventArgs searchArgs = e as SearchEventArgs;
            ReadOnlyCollection<User> items;
            string errorMsg="";
            string ouForFindText = "";
            bool onlyLockUser = false;
            bool userInOU = false;
            if (findOnlyLockUser.IsChecked == true)
                onlyLockUser = true;
            if (findUserInOU.IsChecked == true)
            {
                userInOU = true;
                ouForFindText = OUForFind.Text;
            }
            listFindedUsers.ItemsSource = null;
            pcTab.IsEnabled = false;
            groupTab.IsEnabled = false;
            btUpdateFind.IsEnabled = false;
            search.IsEnabled = false;
            findOnlyLockUser.IsEnabled = false;
            findUserInOU.IsEnabled = false;
            OUForFind.IsEnabled = false;
            btSelectOUForFind.IsEnabled = false;
            filterFindUser.IsEnabled = false;
            filterFindUser.Text = "";
            // Убираем стрелку сортировки из предыдущей колонки
            if (_sortColumn != null)
            {
                _sortColumn.Column.HeaderTemplate = null;
                _sortColumn.Column.Width = _sortColumn.ActualWidth - 20;
                _sortColumn = null;
            }
            new Thread(() => 
            {
                AsyncDataProvider.messageEvent += updateInfoSearchProcess;
                SearchIsRun = true;
                if(onlyLockUser)
                {
                    items = AsyncDataProvider.GetItemsOnlyLock(_maxPwdAgeDay, _principalContext, _connectedSession, ref errorMsg);
                }
                else
                {
                    if (userInOU)
                    {
                        if (!string.IsNullOrWhiteSpace(ouForFindText))
                            items = AsyncDataProvider.GetItemsInOU(_maxPwdAgeDay, ouForFindText, _principalContext, _connectedSession, ref errorMsg);
                        else
                            items = AsyncDataProvider.GetItems();
                    }
                    else
                    {
                        items = AsyncDataProvider.GetItems(_maxPwdAgeDay, fieldsInAD[searchArgs.Sections[0]], searchArgs.Keyword, _principalContext, _connectedSession, ref errorMsg);
                    }
                }
                AsyncDataProvider.messageEvent -= updateInfoSearchProcess;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    listFindedUsers.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listFindedUsers.ItemsSource);
                    view.Filter = FindedUsers_Filter;
                    if (view.Count > 0)
                    {
                        listFindedUsers.SelectedIndex = 0;
                    }
                    btUpdateFind.IsEnabled = true;
                    findOnlyLockUser.IsEnabled = true;
                    pcTab.IsEnabled = true;
                    groupTab.IsEnabled = true;
                    filterFindUser.IsEnabled = true;
                    if (findOnlyLockUser.IsChecked != true)
                    {
                        search.IsEnabled = true;
                        findUserInOU.IsEnabled = true;
                        if (findUserInOU.IsChecked == true)
                        {
                            search.IsEnabled = false;
                            findOnlyLockUser.IsEnabled = false;
                            OUForFind.IsEnabled = true;
                            btSelectOUForFind.IsEnabled = true;
                        }
                    }
                    SearchIsRun = false;
                    StopSearchProcess = false;
                    statusBarText.Content = "";
                    if(!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    search.Focus();
                }));
            }).Start();
        }
        // Изменено содержимое поля фильтрации найденных пользователей
        private void filterFindUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listFindedUsers.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                if (view.Count > 0)
                {
                    listFindedUsers.SelectedIndex = 0;
                }
            }
        }
        // Фильтр найденных пользователей
        private bool FindedUsers_Filter(object item)
        {
            if (String.IsNullOrEmpty(filterFindUser.Text))
                return true;

            var user = (User)item;

            return (user.NameInAD.ToUpper().Contains(filterFindUser.Text.ToUpper()));
        }
        // Найти только заблокированных пользователей вкл/выкл
        private void findOnlyLockUser_Click(object sender, RoutedEventArgs e)
        {
            if (findOnlyLockUser.IsChecked == true)
            {
                search.IsEnabled = false;
                findUserInOU.IsEnabled = false;
                OUForFind.IsEnabled = false;
                btSelectOUForFind.IsEnabled = false;
            }
            else
            {
                search.IsEnabled = true;
                findUserInOU.IsEnabled = true;
                if (findUserInOU.IsChecked == true)
                {
                    search.IsEnabled = false;
                    findOnlyLockUser.IsEnabled = false;
                    OUForFind.IsEnabled = true;
                    btSelectOUForFind.IsEnabled = true;
                }
            }
        }
        // Найти пользователей из указанной OU
        private void findUserInOU_Click(object sender, RoutedEventArgs e)
        {
            if (findUserInOU.IsChecked == true)
            {
                search.IsEnabled = false;
                findOnlyLockUser.IsEnabled = false;
                OUForFind.IsEnabled = true;
                btSelectOUForFind.IsEnabled = true;
            }
            else
            {
                search.IsEnabled = true;
                findOnlyLockUser.IsEnabled = true;
                OUForFind.IsEnabled = false;
                btSelectOUForFind.IsEnabled = false;
                OUForFind.Text = "";
            }
        }
        // Нажата кнопка обновления найденных пользователей
        private void btUpdateFind_Click(object sender, RoutedEventArgs e)
        {
            string selectedSection = search.SelectedSections[0];
            string filterStr = search.Text;
            string errorMsg = "";
            string ouForFindText = "";
            ReadOnlyCollection<User> items;
            bool onlyLockUser = false;
            bool userInOU = false;
            if (findOnlyLockUser.IsChecked == true)
                onlyLockUser = true;
            if (findUserInOU.IsChecked == true)
            {
                userInOU = true;
                ouForFindText = OUForFind.Text;
            }
            listFindedUsers.ItemsSource = null;
            pcTab.IsEnabled = false;
            groupTab.IsEnabled = false;
            btUpdateFind.IsEnabled = false;
            search.IsEnabled = false;
            findOnlyLockUser.IsEnabled = false;
            findUserInOU.IsEnabled = false;
            OUForFind.IsEnabled = false;
            btSelectOUForFind.IsEnabled = false;
            filterFindUser.IsEnabled = false;
            filterFindUser.Text = "";
            // Убираем стрелку сортировки из предыдущей колонки
            if (_sortColumn != null)
            {
                _sortColumn.Column.HeaderTemplate = null;
                _sortColumn.Column.Width = _sortColumn.ActualWidth - 20;
                _sortColumn = null;
            }
            new Thread(() =>
            {
                AsyncDataProvider.messageEvent += updateInfoSearchProcess;
                SearchIsRun = true;
                if (onlyLockUser)
                    items = AsyncDataProvider.GetItemsOnlyLock(_maxPwdAgeDay, _principalContext, _connectedSession, ref errorMsg);
                else
                {
                    if (userInOU)
                    {
                        if (!string.IsNullOrWhiteSpace(ouForFindText))
                            items = AsyncDataProvider.GetItemsInOU(_maxPwdAgeDay, ouForFindText, _principalContext, _connectedSession, ref errorMsg);
                        else
                            items = AsyncDataProvider.GetItems();
                    }
                    else
                    {
                        items = AsyncDataProvider.GetItems(_maxPwdAgeDay, fieldsInAD[selectedSection], filterStr, _principalContext, _connectedSession, ref errorMsg);
                    }
                }
                AsyncDataProvider.messageEvent -= updateInfoSearchProcess;    
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    listFindedUsers.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listFindedUsers.ItemsSource);
                    view.Filter = FindedUsers_Filter;
                    if (view.Count > 0)
                    {
                        listFindedUsers.SelectedIndex = 0;
                    }
                    pcTab.IsEnabled = true;
                    groupTab.IsEnabled = true;
                    btUpdateFind.IsEnabled = true;
                    findOnlyLockUser.IsEnabled = true;
                    filterFindUser.IsEnabled = true;
                    if (findOnlyLockUser.IsChecked != true)
                    {
                        search.IsEnabled = true;
                        findUserInOU.IsEnabled = true;
                        if (findUserInOU.IsChecked == true)
                        {
                            search.IsEnabled = false;
                            findOnlyLockUser.IsEnabled = false;
                            OUForFind.IsEnabled = true;
                            btSelectOUForFind.IsEnabled = true;
                        }
                    }
                    SearchIsRun = false;
                    StopSearchProcess = false;
                    statusBarText.Content = "";
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    search.Focus();
                }));
            }).Start();
        }
        // Изменился выбранный элемент в списке найденных пользователей
        private void listFindedUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listFindedUsers.SelectedItem != null)
            {
                btUpdateUserData.IsEnabled = true;
                IsEnableEditFormUser = true;
            }
            else
            {
                IsEnableEditFormUser = false;
                btUpdateUserData.IsEnabled = false;
                btDisableEnableUser_text.Text = "Отключить пользователя";
                btDisableEnableUser_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/disable_user.ico", UriKind.RelativeOrAbsolute));
            }
        }
        // Нажата кнопка обновления информации о текущем пользователе
        private void btUpdateUserData_Click(object sender, RoutedEventArgs e)
        {
            string login;
            if (sender is Button)
            {
                if (((Button)sender).Name == "btChangeUserLogin")
                {
                    DirectorySearcher dirSearcher = new DirectorySearcher(_connectedSession);
                    dirSearcher.SearchScope = SearchScope.Subtree;
                    dirSearcher.Filter = string.Format("(&(objectClass=user)(distinguishedName={0}))", ((User)listFindedUsers.SelectedItem).PlaceInAD);
                    dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                    SearchResult searchResults = dirSearcher.FindOne();
                    login = (string)searchResults.Properties["sAMAccountName"][0];
                }
                else if (((Button)sender).Name == "btChangeUserNameInAD")
                {
                    string domainAndUsername = domainForConnect.Text + @"\" + loginForConnect.Text;
                    _connectedSession.Close();
                    DirectoryEntry newEntry = new DirectoryEntry("LDAP://" + domainForConnect.Text, domainAndUsername, passForConnect.Password);
                    _connectedSession = newEntry;
                    _principalContext = new PrincipalContext(ContextType.Domain, domainForConnect.Text, loginForConnect.Text, passForConnect.Password);
                    login = ((User)listFindedUsers.SelectedItem).Login;
                }
                else
                {
                    login = ((User)listFindedUsers.SelectedItem).Login;
                }

            }
            else
            {
                login = ((User)listFindedUsers.SelectedItem).Login;
            }
            statusBarText.Content = "Обновление информации о текущем пользователе...";
            pcTab.IsEnabled = false;
            groupTab.IsEnabled = false;
            btUpdateFind.IsEnabled = false;
            search.IsEnabled = false;
            findOnlyLockUser.IsEnabled = false;
            findUserInOU.IsEnabled = false;
            OUForFind.IsEnabled = false;
            btSelectOUForFind.IsEnabled = false;
            filterFindUser.IsEnabled = false;
            IsEnableEditFormUser = false;
            btUpdateUserData.IsEnabled = false;
            new Thread(() =>
            {
                User userData = GetUserInfoFromAD(login, _principalContext, _connectedSession);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ((User)listFindedUsers.SelectedItem).copyData(userData);
                    statusBarText.Content = "";
                    btUpdateFind.IsEnabled = true;
                    findOnlyLockUser.IsEnabled = true;
                    pcTab.IsEnabled = true;
                    groupTab.IsEnabled = true;
                    filterFindUser.IsEnabled = true;
                    if (findOnlyLockUser.IsChecked != true)
                    {
                        search.IsEnabled = true;
                        findUserInOU.IsEnabled = true;
                        if (findUserInOU.IsChecked == true)
                        {
                            search.IsEnabled = false;
                            findOnlyLockUser.IsEnabled = false;
                            OUForFind.IsEnabled = true;
                            btSelectOUForFind.IsEnabled = true;
                        }
                    }
                    btUpdateUserData.IsEnabled = true;
                    IsEnableEditFormUser = true;
                }));
            }).Start();
        }
        // Нажата кнопка просмотра и редактирования групп пользователя
        private void btUserGroups_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.ViewAndEditUserGroups _dwVEUG = new DialogWindows.ViewAndEditUserGroups(((User)listFindedUsers.SelectedItem).PlaceInAD, ConnectedSession, _principalContext);
            _dwVEUG.Owner = Application.Current.MainWindow;
            _dwVEUG.ShowDialog();
        }
        // Выбран пункт меню копирования имени компьютера
        private void CopyName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetDataObject(((Computer)listUserComps.SelectedItem).Name);
            }
            catch (ExternalException)
            {
                MessageBox.Show("Не удалось скопировать данные в буфер обмена.\r\nПопробуйте ещё раз.");
            }
        }
        // Выбран пункт меню копирования размещения компьютера
        private void CopyPlace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetDataObject(((Computer)listUserComps.SelectedItem).Place);
            }
            catch (ExternalException)
            {
                MessageBox.Show("Не удалось скопировать данные в буфер обмена.\r\nПопробуйте ещё раз.");
            }
        }
        // Выбран пункт меню копирования расположения в ад компьютера
        private void CopyPlaceInAD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetDataObject(((Computer)listUserComps.SelectedItem).PlaceInAD);
            }
            catch (ExternalException)
            {
                MessageBox.Show("Не удалось скопировать данные в буфер обмена.\r\nПопробуйте ещё раз.");
            }
        }
        // Нажата кнопка разблокировки пользователя
        private void btUnlockUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(_principalContext, IdentityType.SamAccountName, ((User)listFindedUsers.SelectedItem).Login))
                {
                    oUserPrincipal.UnlockAccount();
                    oUserPrincipal.Save();
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            btUpdateUserData_Click(sender, e);
            MessageBox.Show("Учетная запись разблокирована.");
        }
        // Нажата кнопка сброса пароля на стандартный
        private void btResetPassToDefault_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены что хотите сбросить пароль на стандартный для этого пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                return;
            }
            try
            {
                using (UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(_principalContext, IdentityType.SamAccountName, ((User)listFindedUsers.SelectedItem).Login))
                {
                    oUserPrincipal.SetPassword("vl*12345");
                    oUserPrincipal.Save();
                    oUserPrincipal.ExpirePasswordNow();
                    oUserPrincipal.Save();
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.InnerException.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            btUpdateUserData_Click(sender, e);
            MessageBox.Show("Пароль сброшен на стандартный:\r\nvl*12345");
        }
        // Начата кнопка отключения включения учетной записи пользователя
        private void btDisableEnableUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result;
            if (userIsDisable.Text == "Да")
                result = MessageBox.Show("Вы уверены что хотите включить учетную запись этого пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            else
                result = MessageBox.Show("Вы уверены что хотите отключить учетную запись этого пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                return;
            }
            try
            {
                using (UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(_principalContext, IdentityType.SamAccountName, ((User)listFindedUsers.SelectedItem).Login))
                {
                    if (userIsDisable.Text == "Да")
                    {
                        oUserPrincipal.Enabled = true;
                        oUserPrincipal.Save();
                        btDisableEnableUser_text.Text = "Отключить пользователя";
                        btDisableEnableUser_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/disable_user.ico", UriKind.RelativeOrAbsolute));
                    }
                    else
                    {
                        oUserPrincipal.Enabled = false;
                        oUserPrincipal.Save();
                        btDisableEnableUser_text.Text = "Включить пользователя";
                        btDisableEnableUser_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/enable_user.ico", UriKind.RelativeOrAbsolute));
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            btUpdateUserData_Click(sender, e);
            
            if (userIsDisable.Text == "Да")
                MessageBox.Show("Пользователь включен.");
            else
                MessageBox.Show("Пользователь отключен.");
        }
        // Изменение надписи кнопки включения/отключения пользователя
        private void userIsDisable_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (userIsDisable.Text == "Да")
            {
                btDisableEnableUser_text.Text = "Включить пользователя";
                btDisableEnableUser_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/enable_user.ico", UriKind.RelativeOrAbsolute));
            }
            else
            {
                btDisableEnableUser_text.Text = "Отключить пользователя";
                btDisableEnableUser_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/disable_user.ico", UriKind.RelativeOrAbsolute));
            }
        }
        // Нажата кнопка выбора OU для поиска
        private void btSelectOUForFind_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.MoveUserInAD _dwMUIAD = new DialogWindows.MoveUserInAD("", _connectedSession, "select");
            _dwMUIAD.Owner = Application.Current.MainWindow;
            bool? result = _dwMUIAD.ShowDialog();
            if (result == true)
                OUForFind.Text = _dwMUIAD.SelectedOU;
        }
        // Выполненно двойное нажатие левой кнопки мышки на елементе списка компьютеров пользователя
        private void ListUserCompsItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            pcTab.Focus();
            if (findPCInOU.IsChecked == true)
            {
                findPCInOU.IsChecked = false;
                findPCInOU_Click(sender, new RoutedEventArgs());
            }
            searchPC.Text = ((Computer)listUserComps.SelectedItem).Name;
            btUpdatePCFind_Click("ListUserComps", new RoutedEventArgs());
        }
        // Нажата кнопка получения дополнительной информации компьютера пользователя
        private void btGetNetInfoUserPC_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.GetCompInfoFromNet _dwGCIFN = new DialogWindows.GetCompInfoFromNet(
                ((Computer)listUserComps.SelectedItem).Name,
                domainForConnect.Text,
                loginForConnect.Text,
                passForConnect.Password);
            _dwGCIFN.Owner = Application.Current.MainWindow;
            _dwGCIFN.ShowDialog();
        }
        // Нажата кнопка формы транслитерации
        private void btTranslitToLAT_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.TranslitToLAT _dwTTLAT = new DialogWindows.TranslitToLAT();
            _dwTTLAT.Owner = Application.Current.MainWindow;
            _dwTTLAT.Show();
        }
        #endregion

        #region События вкладки Компьютеры
        // Запущен процесс поиска компьютера в АД
        private void searchPC_OnSearch(object sender, RoutedEventArgs e)
        {
            SearchEventArgs searchArgs = e as SearchEventArgs;
            ReadOnlyCollection<Computer> items;
            string errorMsg = "";
            string ouForFindText = "";
            bool compInOU = false;
            if (findPCInOU.IsChecked == true)
            {
                compInOU = true;
                ouForFindText = OUForFindPC.Text;
            }
            listFindedPC.ItemsSource = null;
            userTab.IsEnabled = false;
            groupTab.IsEnabled = false;
            btUpdatePCFind.IsEnabled = false;
            searchPC.IsEnabled = false;
            findPCInOU.IsEnabled = false;
            OUForFindPC.IsEnabled = false;
            btSelectOUForFindPC.IsEnabled = false;
            filterFindPC.IsEnabled = false;
            filterFindPC.Text = "";
            // Убираем стрелку сортировки из предыдущей колонки
            if (_sortColumnPC != null)
            {
                _sortColumnPC.Column.HeaderTemplate = null;
                _sortColumnPC.Column.Width = _sortColumnPC.ActualWidth - 20;
                _sortColumnPC = null;
            }
            new Thread(() =>
            {
                AsyncDataProvider.messageEvent += updateInfoSearchProcess;
                SearchIsRun = true;

                if (compInOU)
                {
                    if (!string.IsNullOrWhiteSpace(ouForFindText))
                        items = AsyncDataProvider.GetPCItemsInOU(ouForFindText, _principalContext, _connectedSession, ref errorMsg);
                    else
                        items = AsyncDataProvider.GetPCItems();
                }
                else
                {
                    items = AsyncDataProvider.GetPCItems(fieldsPCInAD[searchArgs.Sections[0]], searchArgs.Keyword, _principalContext, _connectedSession, ref errorMsg);
                }
                AsyncDataProvider.messageEvent -= updateInfoSearchProcess;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    listFindedPC.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listFindedPC.ItemsSource);
                    view.Filter = FindedPC_Filter;
                    if (view.Count > 0)
                    {
                        listFindedPC.SelectedIndex = 0;
                    }
                    userTab.IsEnabled = true;
                    groupTab.IsEnabled = true;
                    btUpdatePCFind.IsEnabled = true;
                    filterFindPC.IsEnabled = true;
                    searchPC.IsEnabled = true;
                    findPCInOU.IsEnabled = true;
                    if (findPCInOU.IsChecked == true)
                    {
                        searchPC.IsEnabled = false;
                        OUForFindPC.IsEnabled = true;
                        btSelectOUForFindPC.IsEnabled = true;
                    }
                    SearchIsRun = false;
                    StopSearchProcess = false;
                    statusBarText.Content = "";
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    searchPC.Focus();
                }));
            }).Start();
        }
        // Нажата кнопка обновления найденных компьютеров
        private void btUpdatePCFind_Click(object sender, RoutedEventArgs e)
        {
            string selectedSection = "";
            if (sender is string)
            {
                if ((string)sender == "ListUserComps")
                {
                    selectedSection = searchPC.SectionsList[0];
                }
            }else
                selectedSection = searchPC.SelectedSections[0];
            string filterStr = searchPC.Text;
            ReadOnlyCollection<Computer> items;
            string errorMsg = "";
            string ouForFindText = "";
            bool compInOU = false;
            if (findPCInOU.IsChecked == true)
            {
                compInOU = true;
                ouForFindText = OUForFindPC.Text;
            }
            listFindedPC.ItemsSource = null;
            btUpdatePCFind.IsEnabled = false;
            userTab.IsEnabled = false;
            groupTab.IsEnabled = false;
            searchPC.IsEnabled = false;
            findPCInOU.IsEnabled = false;
            OUForFindPC.IsEnabled = false;
            btSelectOUForFindPC.IsEnabled = false;
            filterFindPC.IsEnabled = false;
            filterFindPC.Text = "";
            // Убираем стрелку сортировки из предыдущей колонки
            if (_sortColumnPC != null)
            {
                _sortColumnPC.Column.HeaderTemplate = null;
                _sortColumnPC.Column.Width = _sortColumnPC.ActualWidth - 20;
                _sortColumnPC = null;
            }
            new Thread(() =>
            {
                AsyncDataProvider.messageEvent += updateInfoSearchProcess;
                SearchIsRun = true;

                if (compInOU)
                {
                    if (!string.IsNullOrWhiteSpace(ouForFindText))
                        items = AsyncDataProvider.GetPCItemsInOU(ouForFindText, _principalContext, _connectedSession, ref errorMsg);
                    else
                        items = AsyncDataProvider.GetPCItems();
                }
                else
                {
                    items = AsyncDataProvider.GetPCItems(fieldsPCInAD[selectedSection], filterStr, _principalContext, _connectedSession, ref errorMsg);
                }
                AsyncDataProvider.messageEvent -= updateInfoSearchProcess;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    listFindedPC.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listFindedPC.ItemsSource);
                    view.Filter = FindedPC_Filter;
                    if (view.Count > 0)
                    {
                        listFindedPC.SelectedIndex = 0;
                    }
                    userTab.IsEnabled = true;
                    groupTab.IsEnabled = true;
                    btUpdatePCFind.IsEnabled = true;
                    filterFindPC.IsEnabled = true;
                    searchPC.IsEnabled = true;
                    findPCInOU.IsEnabled = true;
                    if (findPCInOU.IsChecked == true)
                    {
                        searchPC.IsEnabled = false;
                        OUForFindPC.IsEnabled = true;
                        btSelectOUForFindPC.IsEnabled = true;
                    }
                    SearchIsRun = false;
                    StopSearchProcess = false;
                    statusBarText.Content = "";
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    searchPC.Focus();
                }));
            }).Start();
        }
        // Изменено содержимое поля фильтрации найденных компьютеров
        private void filterFindPC_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listFindedPC.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                if (view.Count > 0)
                {
                    listFindedPC.SelectedIndex = 0;
                }
            }
        }
        // Фильтр найденных компьютеров
        private bool FindedPC_Filter(object item)
        {
            if (String.IsNullOrEmpty(filterFindPC.Text))
                return true;
            //var comp = ((Computer)item);
            return (((Computer)item).Name.ToUpper().Contains(filterFindPC.Text.ToUpper()));
        }
        // Выполнено нажатие на заголовок столбца с найденными компьютерами
        private void listFindedPC_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
            if (column == null)
            {
                return;
            }

            if (_sortColumnPC == column)
            {
                // Выбираем режим сортировки в зависимости от того какая сортировка была до этого
                _sortDirectionPC = _sortDirectionPC == ListSortDirection.Ascending ?
                                                   ListSortDirection.Descending :
                                                   ListSortDirection.Ascending;
            }
            else
            {
                // Убираем стрелку сортировки из предыдущей колонки
                if (_sortColumnPC != null)
                {
                    _sortColumnPC.Column.HeaderTemplate = null;
                    _sortColumnPC.Column.Width = _sortColumnPC.ActualWidth - 20;
                }

                _sortColumnPC = column;
                _sortDirectionPC = ListSortDirection.Ascending;
                column.Column.Width = column.ActualWidth + 20;
            }

            if (_sortDirectionPC == ListSortDirection.Ascending)
            {
                column.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
            }
            else
            {
                column.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;
            }

            string header = string.Empty;

            // если используется привязка и имя свойства не совпадает с содержанием заголовка
            Binding b = _sortColumnPC.Column.DisplayMemberBinding as Binding;
            if (b != null)
            {
                header = b.Path.Path;
            }

            ICollectionView resultDataView = CollectionViewSource.GetDefaultView(listFindedPC.ItemsSource);
            resultDataView.SortDescriptions.Clear();
            resultDataView.SortDescriptions.Add(new SortDescription(header, _sortDirectionPC));
        }
        // Изменился выбранный элемент в списке найденных компьютеров
        private void listFindedPC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listFindedPC.SelectedItem != null)
            {
                btUpdatePCData.IsEnabled = true;
                IsEnableEditFormPC = true;
            }
            else
            {
                IsEnableEditFormPC = false;
                btUpdatePCData.IsEnabled = false;
                btDisableEnablePC_text.Text = "Отключить компьютер";
                btDisableEnablePC_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/disable_computer.ico", UriKind.RelativeOrAbsolute));
            }
        }
        // Изменение надписи кнопки включения/отключения пользователя
        private void pcIsDisable_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (pcIsDisable.Text == "Да")
            {
                btDisableEnablePC_text.Text = "Включить компьютер";
                btDisableEnablePC_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/enable_computer.ico", UriKind.RelativeOrAbsolute));
            }
            else
            {
                btDisableEnablePC_text.Text = "Отключить компьютер";
                btDisableEnablePC_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/disable_computer.ico", UriKind.RelativeOrAbsolute));
            }
        }
        // Начата кнопка отключения включения учетной записи компьютера
        private void btDisableEnablePC_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result;
            if (pcIsDisable.Text == "Да")
                result = MessageBox.Show("Вы уверены что хотите включить учетную запись этого компьютера?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            else
                result = MessageBox.Show("Вы уверены что хотите отключить учетную запись этого компьютера?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                return;
            }
            try
            {
                using (ComputerPrincipal oCompPrincipal = ComputerPrincipal.FindByIdentity(_principalContext, IdentityType.DistinguishedName, ((Computer)listFindedPC.SelectedItem).PlaceInAD))
                {
                    if (pcIsDisable.Text == "Да")
                    {
                        oCompPrincipal.Enabled = true;
                        oCompPrincipal.Save();
                        btDisableEnablePC_text.Text = "Отключить компьютер";
                        btDisableEnablePC_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/disable_computer.ico", UriKind.RelativeOrAbsolute));
                    }
                    else
                    {
                        oCompPrincipal.Enabled = false;
                        oCompPrincipal.Save();
                        btDisableEnablePC_text.Text = "Включить компьютер";
                        btDisableEnablePC_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/enable_computer.ico", UriKind.RelativeOrAbsolute));
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            btUpdatePCData_Click(sender, e);

            if (pcIsDisable.Text == "Да")
                MessageBox.Show("Компьютер включен.");
            else
                MessageBox.Show("Компьютер отключен.");
        }
        // Найти компьютеры из указанной OU
        private void findPCInOU_Click(object sender, RoutedEventArgs e)
        {
            if (findPCInOU.IsChecked == true)
            {
                searchPC.IsEnabled = false;
                OUForFindPC.IsEnabled = true;
                btSelectOUForFindPC.IsEnabled = true;
            }
            else
            {
                searchPC.IsEnabled = true;
                OUForFindPC.IsEnabled = false;
                btSelectOUForFindPC.IsEnabled = false;
                OUForFindPC.Text = "";
            }
        }
        // Нажата кнопка обновления информации о текущем компьютере
        private void btUpdatePCData_Click(object sender, RoutedEventArgs e)
        {
            string distinguishedName;
            if (((Button)sender).Name == "btMovePCInAD")
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(_connectedSession);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=computer)(sAMAccountName={0}))", ((Computer)listFindedPC.SelectedItem).Name + "$");
                dirSearcher.PropertiesToLoad.Add("distinguishedName");
                SearchResult searchResults = dirSearcher.FindOne();
                distinguishedName = (string)searchResults.Properties["distinguishedName"][0];
            }
            else
                distinguishedName = ((Computer)listFindedPC.SelectedItem).PlaceInAD;

            statusBarText.Content = "Обновление информации о текущем компьютере...";

            userTab.IsEnabled = false;
            groupTab.IsEnabled = false;
            btUpdatePCFind.IsEnabled = false;
            searchPC.IsEnabled = false;
            findPCInOU.IsEnabled = false;
            OUForFindPC.IsEnabled = false;
            btSelectOUForFindPC.IsEnabled = false;
            filterFindPC.IsEnabled = false;
            IsEnableEditFormPC = false;
            btUpdatePCData.IsEnabled = false;
            btGetNetInfo.IsEnabled = false;
            btDelCurPC.IsEnabled = false;

            new Thread(() =>
            {
                Computer compData = GetPCInfoFromAD(distinguishedName, _principalContext, _connectedSession);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ((Computer)listFindedPC.SelectedItem).copyData(compData);
                    statusBarText.Content = "";
                    userTab.IsEnabled = true;
                    groupTab.IsEnabled = true;
                    btUpdatePCFind.IsEnabled = true;
                    filterFindPC.IsEnabled = true;
                    searchPC.IsEnabled = true;
                    findPCInOU.IsEnabled = true;
                    if (findPCInOU.IsChecked == true)
                    {
                        searchPC.IsEnabled = false;
                        OUForFindPC.IsEnabled = true;
                        btSelectOUForFindPC.IsEnabled = true;
                    }
                    btUpdatePCData.IsEnabled = true;
                    btGetNetInfo.IsEnabled = true;
                    btDelCurPC.IsEnabled = true;
                    IsEnableEditFormPC = true;
                    statusBarText.Content = "";
                }));
            }).Start();
        }
        // Нажата кнопка выбора OU для поиска
        private void btSelectOUForFindPC_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.MoveUserInAD _dwMUIAD = new DialogWindows.MoveUserInAD("", _connectedSession, "select");
            _dwMUIAD.Owner = Application.Current.MainWindow;
            bool? result = _dwMUIAD.ShowDialog();
            if (result == true)
                OUForFindPC.Text = _dwMUIAD.SelectedOU;
        }
        // Нажата кнопка просмотра и редактирования групп компьютера
        private void btPCGroups_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.ViewAndEditUserGroups _dwVEUG = new DialogWindows.ViewAndEditUserGroups(((Computer)listFindedPC.SelectedItem).PlaceInAD, ConnectedSession, _principalContext, "comp");
            _dwVEUG.Owner = Application.Current.MainWindow;
            _dwVEUG.ShowDialog();
        }
        // Выбран пункт меню перемещения выделенных компьютеров
        private void MoveSelectedPC_Click(object sender, RoutedEventArgs e)
        {
            string ouForMove = "";
            DialogWindows.MoveUserInAD _dwMUIAD = new DialogWindows.MoveUserInAD("", _connectedSession, "moveListPC");
            _dwMUIAD.Owner = Application.Current.MainWindow;
            bool? result = _dwMUIAD.ShowDialog();
            if (result == true)
            {
                ouForMove = _dwMUIAD.SelectedOU;
                if (string.IsNullOrWhiteSpace(ouForMove))
                {
                    MessageBox.Show("Не выбрана OU для перемещения!!!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
                return;
                
            MessageBoxResult selection = MessageBox.Show("Вы уверены что хотите переместить выбранные компьютеры в указанное подразделение?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (selection == MessageBoxResult.No)
            {
                return;
            }

            userTab.IsEnabled = false;
            groupTab.IsEnabled = false;
            btUpdatePCFind.IsEnabled = false;
            searchPC.IsEnabled = false;
            findPCInOU.IsEnabled = false;
            OUForFindPC.IsEnabled = false;
            btSelectOUForFindPC.IsEnabled = false;
            filterFindPC.IsEnabled = false;
            statusBarText.Content = "Перемещение выбранных компьютеров...";
            List<Computer> selectedPC = new List<Computer>();
            for (int i = 0; i < listFindedPC.SelectedItems.Count; i++)
            {
                selectedPC.Add(((Computer)listFindedPC.SelectedItems[i]));
            }
            new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < selectedPC.Count; i++)
                    {
                        DirectorySearcher dirSearcher = new DirectorySearcher(_connectedSession);
                        dirSearcher.SearchScope = SearchScope.Subtree;
                        dirSearcher.Filter = string.Format("(&(objectClass=computer)(distinguishedName=" + selectedPC[i].PlaceInAD + "))");
                        SearchResult searchResults = dirSearcher.FindOne();
                        DirectoryEntry theObjectToMove = searchResults.GetDirectoryEntry();
                        
                        dirSearcher.Filter = string.Format("(&(|(objectClass=organizationalUnit)(objectClass=organization)(cn=Users)(cn=Computers))(distinguishedName=" + ouForMove + "))");
                        searchResults = dirSearcher.FindOne();
                        DirectoryEntry theNewParent = searchResults.GetDirectoryEntry();

                        theObjectToMove.MoveTo(theNewParent);
                    }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        for (int i = 0; i < selectedPC.Count; i++)
                        {
                            Computer compData = GetPCInfoFromAD(selectedPC[i].Name + "$", _principalContext, _connectedSession, "sAMAccountName");
                            ((Computer)listFindedPC.SelectedItems[i]).copyData(compData);
                        }
                        userTab.IsEnabled = true;
                        groupTab.IsEnabled = true;
                        btUpdatePCFind.IsEnabled = true;
                        filterFindPC.IsEnabled = true;
                        searchPC.IsEnabled = true;
                        findPCInOU.IsEnabled = true;
                        if (findPCInOU.IsChecked == true)
                        {
                            searchPC.IsEnabled = false;
                            OUForFindPC.IsEnabled = true;
                            btSelectOUForFindPC.IsEnabled = true;
                        }
                        statusBarText.Content = "";
                    }));
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        userTab.IsEnabled = true;
                        groupTab.IsEnabled = true;
                        btUpdatePCFind.IsEnabled = true;
                        filterFindPC.IsEnabled = true;
                        searchPC.IsEnabled = true;
                        findPCInOU.IsEnabled = true;
                        if (findPCInOU.IsChecked == true)
                        {
                            searchPC.IsEnabled = false;
                            OUForFindPC.IsEnabled = true;
                            btSelectOUForFindPC.IsEnabled = true;
                        }
                        statusBarText.Content = "";
                    }));
                }
            }).Start();
        }
        // Выбран пункт меню удаления выбранных компьютеров
        private void RemoveFromADSelectedPC_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult selection = MessageBox.Show("Вы уверены что хотите УДАЛИТЬ выбранные компьютеры?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (selection == MessageBoxResult.No)
            {
                return;
            }
            userTab.IsEnabled = false;
            groupTab.IsEnabled = false;
            btUpdatePCFind.IsEnabled = false;
            searchPC.IsEnabled = false;
            findPCInOU.IsEnabled = false;
            OUForFindPC.IsEnabled = false;
            btSelectOUForFindPC.IsEnabled = false;
            filterFindPC.IsEnabled = false;
            statusBarText.Content = "Удаление выбранных компьютеров...";
            List<Computer> selectedPC = new List<Computer>();
            for (int i = 0; i < listFindedPC.SelectedItems.Count; i++)
            {
                selectedPC.Add(((Computer)listFindedPC.SelectedItems[i]));
            }
            new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < selectedPC.Count; i++)
                    {
                        DirectorySearcher dirSearcher = new DirectorySearcher(_connectedSession);
                        dirSearcher.SearchScope = SearchScope.Subtree;
                        dirSearcher.Filter = string.Format("(&(objectClass=computer)(distinguishedName=" + selectedPC[i].PlaceInAD + "))");
                        SearchResult searchResults = dirSearcher.FindOne();
                        DirectoryEntry theObjectForRemove = searchResults.GetDirectoryEntry();

                        theObjectForRemove.DeleteTree();
                        theObjectForRemove.CommitChanges();
                    }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        userTab.IsEnabled = true;
                        groupTab.IsEnabled = true;
                        btUpdatePCFind.IsEnabled = true;
                        filterFindPC.IsEnabled = true;
                        searchPC.IsEnabled = true;
                        findPCInOU.IsEnabled = true;
                        if (findPCInOU.IsChecked == true)
                        {
                            searchPC.IsEnabled = false;
                            OUForFindPC.IsEnabled = true;
                            btSelectOUForFindPC.IsEnabled = true;
                        }
                        statusBarText.Content = "";
                        btUpdatePCFind_Click(sender, e);
                    }));
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        userTab.IsEnabled = true;
                        groupTab.IsEnabled = true;
                        btUpdatePCFind.IsEnabled = true;
                        filterFindPC.IsEnabled = true;
                        searchPC.IsEnabled = true;
                        findPCInOU.IsEnabled = true;
                        if (findPCInOU.IsChecked == true)
                        {
                            searchPC.IsEnabled = false;
                            OUForFindPC.IsEnabled = true;
                            btSelectOUForFindPC.IsEnabled = true;
                        }
                        statusBarText.Content = "";
                    }));
                }
            }).Start();
        }
        // Нажата кнопка удаления выбранных компьютеров
        private void btDelCurPC_Click(object sender, RoutedEventArgs e)
        {
            RemoveFromADSelectedPC_Click(sender, e);
        }
        // Нажата кнопка перемещения выделенных компьютеров
        private void btMoveCurPC_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedPC_Click(sender, e);
        }
        // Нажата кнопка получения дополнительной информации по выделенному компьютеру
        private void btGetNetInfo_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.GetCompInfoFromNet _dwGCIFN = new DialogWindows.GetCompInfoFromNet(
                ((Computer)listFindedPC.SelectedItem).Name, 
                domainForConnect.Text, 
                loginForConnect.Text, 
                passForConnect.Password);
            _dwGCIFN.Owner = Application.Current.MainWindow;
            _dwGCIFN.ShowDialog();
        }
        // Нажата кнопка поиска свободного имени в домене
        private void btFindFreeNameInAD_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.FindFreeNameInAD _dwFFNA = new DialogWindows.FindFreeNameInAD(_connectedSession);
            _dwFFNA.Owner = Application.Current.MainWindow;
            _dwFFNA.Show();
        }
        #endregion

        #region События вкладки Группы
        // Запущен процесс поиска группы в АД
        private void searchGroup_OnSearch(object sender, RoutedEventArgs e)
        {
            SearchEventArgs searchArgs = e as SearchEventArgs;
            ReadOnlyCollection<Group> items;
            string errorMsg = "";
            string ouForFindText = "";
            bool groupInOU = false;
            if (findGroupInOU.IsChecked == true)
            {
                groupInOU = true;
                ouForFindText = OUForFindGroup.Text;
            }
            listFindedGroup.ItemsSource = null;
            userTab.IsEnabled = false;
            pcTab.IsEnabled = false;
            btUpdateGroupFind.IsEnabled = false;
            searchGroup.IsEnabled = false;
            findGroupInOU.IsEnabled = false;
            OUForFindGroup.IsEnabled = false;
            btSelectOUForFindGroup.IsEnabled = false;
            filterFindGroup.IsEnabled = false;
            filterFindGroup.Text = "";
            // Убираем стрелку сортировки из предыдущей колонки
            if (_sortColumnGroup != null)
            {
                _sortColumnGroup.Column.HeaderTemplate = null;
                _sortColumnGroup.Column.Width = _sortColumnGroup.ActualWidth - 20;
                _sortColumnGroup = null;
            }
             new Thread(() =>
            {
                AsyncDataProvider.messageEvent += updateInfoSearchProcess;
                SearchIsRun = true;

                if (groupInOU)
                {
                    if (!string.IsNullOrWhiteSpace(ouForFindText))
                        items = AsyncDataProvider.GetGroupsItemsInOU(ouForFindText, _principalContext, _connectedSession, ref errorMsg);
                    else
                        items = AsyncDataProvider.GetGroupsItems();
                }
                else
                {
                    items = AsyncDataProvider.GetGroupsItems(fieldsGroupInAD[searchArgs.Sections[0]], searchArgs.Keyword, _principalContext, _connectedSession, ref errorMsg);
                }
                AsyncDataProvider.messageEvent -= updateInfoSearchProcess;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    listFindedGroup.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listFindedGroup.ItemsSource);
                    if (view != null)
                    {
                        view.Filter = Groups_Filter;
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                        if (view.Count > 0)
                        {
                            listFindedGroup.SelectedIndex = 0;
                            CollectionView view1 = (CollectionView)CollectionViewSource.GetDefaultView(groupInList.ItemsSource);
                            view1.Filter = GroupIn_Filter;
                            view1.SortDescriptions.Clear();
                            view1.SortDescriptions.Add(new SortDescription());
                            CollectionView view2 = (CollectionView)CollectionViewSource.GetDefaultView(userInGroupList.ItemsSource);
                            view2.Filter = UserInGroup_Filter;
                            view2.SortDescriptions.Clear();
                            view2.SortDescriptions.Add(new SortDescription());
                        }
                    }
                    userTab.IsEnabled = true;
                    pcTab.IsEnabled = true;
                    btUpdateGroupFind.IsEnabled = true;
                    searchGroup.IsEnabled = true;
                    filterFindGroup.IsEnabled = true;
                    OUForFindGroup.IsEnabled = false;
                    btSelectOUForFindGroup.IsEnabled = false;
                    findGroupInOU.IsEnabled = true;
                    if (findGroupInOU.IsChecked == true)
                    {
                        searchGroup.IsEnabled = false;
                        OUForFindGroup.IsEnabled = true;
                        btSelectOUForFindGroup.IsEnabled = true;
                    }
                    SearchIsRun = false;
                    StopSearchProcess = false;
                    statusBarText.Content = "";
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    searchGroup.Focus();
                }));
            }).Start();
        }
        // Нажата кнопка обновления информации по текущей группе
        private void btUpdateCurGroupInfo_Click(object sender, RoutedEventArgs e)
        {
            string sAN;
            if (((Button)sender).Name == "btEditGroupNameWin")
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(_connectedSession);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=group)(distinguishedName={0}))", ((Group)listFindedGroup.SelectedItem).PlaceInAD);
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                SearchResult searchResults = dirSearcher.FindOne();
                sAN = (string)searchResults.Properties["sAMAccountName"][0];
            }
            else
                sAN = ((Group)listFindedGroup.SelectedItem).NameWin;

            statusBarText.Content = "Обновление информации о текущей группе...";

            groupInList.SelectedItem = null;
            userInGroupList.SelectedItem = null;

            userTab.IsEnabled = false;
            pcTab.IsEnabled = false;
            btUpdateGroupFind.IsEnabled = false;
            searchGroup.IsEnabled = false;
            findGroupInOU.IsEnabled = false;
            OUForFindGroup.IsEnabled = false;
            btSelectOUForFindGroup.IsEnabled = false;
            filterFindGroup.IsEnabled = false;
            IsEnableEditFormGroup = false;

            btAddUser.IsEnabled = false;
            btAddComputer.IsEnabled = false;
            btUpdateCurGroupInfo.IsEnabled = false;
            userInGroupList.IsEnabled = false;
            groupInList.IsEnabled = false;
            filterFindGroup.IsEnabled = false;
            listFindedGroup.IsEnabled = false;

            new Thread(() =>
            {
                Group groupData = GetGroupInfoFromAD(sAN, _connectedSession);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ((Group)listFindedGroup.SelectedItem).copyData(groupData);
                    
                    CollectionView view1 = (CollectionView)CollectionViewSource.GetDefaultView(groupInList.ItemsSource);
                    if (view1 != null)
                    {
                        view1.Filter = GroupIn_Filter;
                        view1.SortDescriptions.Clear();
                        view1.SortDescriptions.Add(new SortDescription());
                    }

                    CollectionView view2 = (CollectionView)CollectionViewSource.GetDefaultView(userInGroupList.ItemsSource);
                    if (view2 != null)
                    {
                        view2.Filter = UserInGroup_Filter;
                        view2.SortDescriptions.Clear();
                        view2.SortDescriptions.Add(new SortDescription());
                    }

                    switch (groupData.Type)
                    {
                        case -2147483646:
                            gtypeGlobal.IsChecked = true;
                            gtypeSecurity.IsChecked = true;
                            break;
                        case -2147483640:
                            gtypeUnivers.IsChecked = true;
                            gtypeSecurity.IsChecked = true;
                            break;
                        case -2147483644:
                            gtypeLocalInDomain.IsChecked = true;
                            gtypeSecurity.IsChecked = true;
                            break;
                        case 2:
                            gtypeGlobal.IsChecked = true;
                            gtypeDistribution.IsChecked = true;
                            break;
                        case 4:
                            gtypeLocalInDomain.IsChecked = true;
                            gtypeDistribution.IsChecked = true;
                            break;
                        case 8:
                            gtypeUnivers.IsChecked = true;
                            gtypeDistribution.IsChecked = true;
                            break;
                    }

                    Binding binding = new Binding();
                    binding.Source = listFindedGroup; // установить в качестве source object значение ElementName
                    binding.Path = new PropertyPath(ListView.SelectedItemProperty);
                    binding.Mode = BindingMode.OneWay;
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    binding.Converter = new NullToBooleanConverter();

                    btAddUser.IsEnabled = true;
                    btAddComputer.IsEnabled = true;
                    btUpdateCurGroupInfo.IsEnabled = true;
                    userInGroupList.IsEnabled = true;
                    groupInList.IsEnabled = true;
                    filterFindGroup.IsEnabled = true;
                    listFindedGroup.IsEnabled = true;
                    IsEnableEditFormGroup = true;

                    userTab.IsEnabled = true;
                    pcTab.IsEnabled = true;
                    btUpdateGroupFind.IsEnabled = true;
                    searchGroup.IsEnabled = true;
                    filterFindGroup.IsEnabled = true;
                    findGroupInOU.IsEnabled = true;
                    if (findGroupInOU.IsChecked == true)
                    {
                        searchGroup.IsEnabled = false;
                        OUForFindGroup.IsEnabled = true;
                        btSelectOUForFindGroup.IsEnabled = true;
                    }
                    statusBarText.Content = "";

                    BindingOperations.SetBinding(btUpdateCurGroupInfo, Button.IsEnabledProperty, binding);
                    BindingOperations.SetBinding(btAddUser, Button.IsEnabledProperty, binding);
                    BindingOperations.SetBinding(btAddComputer, Button.IsEnabledProperty, binding);
                }));
            }).Start();
        }
        // Нажата кнопка обновления найденных групп
        private void btUpdateGroupFind_Click(object sender, RoutedEventArgs e)
        {
            string selectedSection = searchGroup.SelectedSections[0];
            string filterStr = searchGroup.Text;
            ReadOnlyCollection<Group> items;
            string errorMsg = "";
            string ouForFindText = "";
            bool groupInOU = false;
            if (findGroupInOU.IsChecked == true)
            {
                groupInOU = true;
                ouForFindText = OUForFindGroup.Text;
            }
            listFindedGroup.ItemsSource = null;
            userTab.IsEnabled = false;
            pcTab.IsEnabled = false;
            btUpdateGroupFind.IsEnabled = false;
            searchGroup.IsEnabled = false;
            findGroupInOU.IsEnabled = false;
            OUForFindGroup.IsEnabled = false;
            btSelectOUForFindGroup.IsEnabled = false;
            filterFindGroup.IsEnabled = false;
            filterFindGroup.Text = "";
            // Убираем стрелку сортировки из предыдущей колонки
            if (_sortColumnGroup != null)
            {
                _sortColumnGroup.Column.HeaderTemplate = null;
                _sortColumnGroup.Column.Width = _sortColumnGroup.ActualWidth - 20;
                _sortColumnGroup = null;
            }
            new Thread(() =>
            {
                AsyncDataProvider.messageEvent += updateInfoSearchProcess;
                SearchIsRun = true;

                if (groupInOU)
                {
                    if (!string.IsNullOrWhiteSpace(ouForFindText))
                        items = AsyncDataProvider.GetGroupsItemsInOU(ouForFindText, _principalContext, _connectedSession, ref errorMsg);
                    else
                        items = AsyncDataProvider.GetGroupsItems();
                }
                else
                {
                    items = AsyncDataProvider.GetGroupsItems(fieldsGroupInAD[selectedSection], filterStr, _principalContext, _connectedSession, ref errorMsg);
                }
                AsyncDataProvider.messageEvent -= updateInfoSearchProcess;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    listFindedGroup.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listFindedGroup.ItemsSource);
                    if (view != null)
                    {
                        view.Filter = Groups_Filter;
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                        if (view.Count > 0)
                        {
                            listFindedGroup.SelectedIndex = 0;
                            CollectionView view1 = (CollectionView)CollectionViewSource.GetDefaultView(groupInList.ItemsSource);
                            view1.Filter = GroupIn_Filter;
                            view1.SortDescriptions.Clear();
                            view1.SortDescriptions.Add(new SortDescription());
                            CollectionView view2 = (CollectionView)CollectionViewSource.GetDefaultView(userInGroupList.ItemsSource);
                            view2.Filter = UserInGroup_Filter;
                            view2.SortDescriptions.Clear();
                            view2.SortDescriptions.Add(new SortDescription());
                        }
                    }
                    userTab.IsEnabled = true;
                    pcTab.IsEnabled = true;
                    btUpdateGroupFind.IsEnabled = true;
                    searchGroup.IsEnabled = true;
                    filterFindGroup.IsEnabled = true;
                    OUForFindGroup.IsEnabled = false;
                    btSelectOUForFindGroup.IsEnabled = false;
                    findGroupInOU.IsEnabled = true;
                    if (findGroupInOU.IsChecked == true)
                    {
                        searchGroup.IsEnabled = false;
                        OUForFindGroup.IsEnabled = true;
                        btSelectOUForFindGroup.IsEnabled = true;
                    }
                    SearchIsRun = false;
                    StopSearchProcess = false;
                    statusBarText.Content = "";
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    searchGroup.Focus();
                }));
            }).Start();
        }
        // Фильтр групп
        private bool Groups_Filter(object item)
        {
            if (String.IsNullOrEmpty(filterFindGroup.Text))
                return true;

            var group = (Group)item;

            return (group.Name.ToUpper().Contains(filterFindGroup.Text.ToUpper()));
        }
        // Фильтр участников состоящих в текущей группе
        private bool UserInGroup_Filter(object item)
        {
            if (String.IsNullOrEmpty(memberInGroupFilter.Text))
                return true;

            var user = (string)item;

            return (user.ToUpper().Contains(memberInGroupFilter.Text.ToUpper()));
        }
        // Фильтр групп состоящих в текущей группе
        private bool GroupIn_Filter(object item)
        {
            if (String.IsNullOrEmpty(groupInFilter.Text))
                return true;

            var group = (string)item;

            return (group.ToUpper().Contains(groupInFilter.Text.ToUpper()));
        }
        // Изменился выбранный элемент в списке найденных групп
        private void listFindedGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listFindedGroup.SelectedItem != null)
            {
                IsEnableEditFormGroup = true;
                CollectionView view1 = (CollectionView)CollectionViewSource.GetDefaultView(groupInList.ItemsSource);
                view1.Filter = GroupIn_Filter;
                view1.SortDescriptions.Clear();
                view1.SortDescriptions.Add(new SortDescription());
                CollectionView view2 = (CollectionView)CollectionViewSource.GetDefaultView(userInGroupList.ItemsSource);
                view2.Filter = UserInGroup_Filter;
                view2.SortDescriptions.Clear();
                view2.SortDescriptions.Add(new SortDescription());
                switch (((Group)listFindedGroup.SelectedItem).Type)
                {
                    case -2147483646:
                        gtypeGlobal.IsChecked = true;
                        gtypeSecurity.IsChecked = true;
                        break;
                    case -2147483640:
                        gtypeUnivers.IsChecked = true;
                        gtypeSecurity.IsChecked = true;
                        break;
                    case -2147483644:
                        gtypeLocalInDomain.IsChecked = true;
                        gtypeSecurity.IsChecked = true;
                        break;
                    case 2:
                        gtypeGlobal.IsChecked = true;
                        gtypeDistribution.IsChecked = true;
                        break;
                    case 4:
                        gtypeLocalInDomain.IsChecked = true;
                        gtypeDistribution.IsChecked = true;
                        break;
                    case 8:
                        gtypeUnivers.IsChecked = true;
                        gtypeDistribution.IsChecked = true;
                        break;
                }
            }
            else
            {
                IsEnableEditFormGroup = false;
                gtypeDistribution.IsChecked = false;
                gtypeGlobal.IsChecked = false;
                gtypeLocalInDomain.IsChecked = false;
                gtypeSecurity.IsChecked = false;
                gtypeUnivers.IsChecked = false;
            }
        }
        // Выполнено нажатие на заголовок столбца с найденными группами
        private void listFindedGroup_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
            if (column == null)
            {
                return;
            }

            if (_sortColumnGroup == column)
            {
                // Выбираем режим сортировки в зависимости от того какая сортировка была до этого
                _sortDirectionGroup = _sortDirectionGroup == ListSortDirection.Ascending ?
                                                   ListSortDirection.Descending :
                                                   ListSortDirection.Ascending;
            }
            else
            {
                // Убираем стрелку сортировки из предыдущей колонки
                if (_sortColumnGroup != null)
                {
                    _sortColumnGroup.Column.HeaderTemplate = null;
                    _sortColumnGroup.Column.Width = _sortColumnGroup.ActualWidth - 20;
                }

                _sortColumnGroup = column;
                _sortDirectionGroup = ListSortDirection.Ascending;
                column.Column.Width = column.ActualWidth + 20;
            }

            if (_sortDirectionGroup == ListSortDirection.Ascending)
            {
                column.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
            }
            else
            {
                column.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;
            }

            string header = string.Empty;

            // если используется привязка и имя свойства не совпадает с содержанием заголовка
            Binding b = _sortColumnGroup.Column.DisplayMemberBinding as Binding;
            if (b != null)
            {
                header = b.Path.Path;
            }

            ICollectionView resultDataView = CollectionViewSource.GetDefaultView(listFindedGroup.ItemsSource);
            resultDataView.SortDescriptions.Clear();
            resultDataView.SortDescriptions.Add(new SortDescription(header, _sortDirectionGroup));
        }
        // Изменено содержимое поля фильтрации найденных групп
        private void filterFindGroup_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listFindedGroup.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                if (view.Count > 0)
                {
                    listFindedGroup.SelectedIndex = 0;
                }
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }
        // Изменено содержимое поля фильтрации участников состоящих в текущей группе
        private void memberInGroupFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(userInGroupList.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription());
            }
        }
        // Изменено содержимое поля фильтрации групп в которые входит текущая группа
        private void groupInFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(groupInList.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription());
            }
        }
        // Нажата кнопка добавления пользователей в группу
        private void btAddUser_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.AddUsersToGroup _dwAUG = new DialogWindows.AddUsersToGroup(((Group)listFindedGroup.SelectedItem).PlaceInAD, _connectedSession, _principalContext);
            _dwAUG.Owner = Application.Current.MainWindow;
            bool? result = _dwAUG.ShowDialog();
            if (result == true)
                btUpdateCurGroupInfo_Click(sender, e);
        }
        // Нажата кнопка добавления компьютеров в группу
        private void btAddComputer_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.AddComputersToGroup _dwACG = new DialogWindows.AddComputersToGroup(((Group)listFindedGroup.SelectedItem).PlaceInAD, _connectedSession, _principalContext);
            _dwACG.Owner = Application.Current.MainWindow;
            bool? result = _dwACG.ShowDialog();
            if (result == true)
                btUpdateCurGroupInfo_Click(sender, e);
        }
        // Нажата кнопка удаления выделенных участников из группы
        private void btDelMembers_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены что хотите удалить выбранных участников из текущей группы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                return;
            }
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(_connectedSession);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=group)(distinguishedName={0}))", ((Group)listFindedGroup.SelectedItem).PlaceInAD);
                dirSearcher.PropertiesToLoad.Add("member");
                SearchResult searchResult = dirSearcher.FindOne();
                if (searchResult != null)
                {
                    DirectoryEntry group = searchResult.GetDirectoryEntry();
                    foreach (string item in userInGroupList.SelectedItems)
                    {
                        group.Properties["member"].Remove(item);
                        group.CommitChanges();
                    }
                    btUpdateCurGroupInfo_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("Не удалось получить информацию по группе" + ((Group)listFindedGroup.SelectedItem).PlaceInAD + "./r/nГруппа не найдена в домене!!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Нажата кнопка выбора OU для поиска
        private void btSelectOUForFindGroup_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.MoveUserInAD _dwMUIAD = new DialogWindows.MoveUserInAD("", _connectedSession, "select");
            _dwMUIAD.Owner = Application.Current.MainWindow;
            bool? result = _dwMUIAD.ShowDialog();
            if (result == true)
                OUForFindGroup.Text = _dwMUIAD.SelectedOU;
        }
        // Нажата кнопка перемещения группы в АД
        private void btMoveGroupInAD_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.MoveUserInAD _dwMUIAD = new DialogWindows.MoveUserInAD(((Group)listFindedGroup.SelectedItem).PlaceInAD, _connectedSession, "moveGroup");
            _dwMUIAD.Owner = Application.Current.MainWindow;
            bool? result = _dwMUIAD.ShowDialog();
            if (result == true)
                btUpdateCurGroupInfo_Click(sender, e);
        }
        // Найти компьютеры из указанной OU
        private void findGroupInOU_Click(object sender, RoutedEventArgs e)
        {
            if (findGroupInOU.IsChecked == true)
            {
                searchGroup.IsEnabled = false;
                OUForFindGroup.IsEnabled = true;
                btSelectOUForFindGroup.IsEnabled = true;
            }
            else
            {
                searchGroup.IsEnabled = true;
                OUForFindGroup.IsEnabled = false;
                btSelectOUForFindGroup.IsEnabled = false;
                OUForFindGroup.Text = "";
            }
        }
        // Нажата кнопка создания группы распространения
        private void btCreateDistributionGroup_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region События редактирования вкладки Пользователи
        // Нажата кнопка перемещения пользователя
        private void btMoveUserInAD_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.MoveUserInAD _dwMUIAD = new DialogWindows.MoveUserInAD(((User)listFindedUsers.SelectedItem).PlaceInAD, _connectedSession);
            _dwMUIAD.Owner = Application.Current.MainWindow;
            bool? result = _dwMUIAD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля должность
        private void btChangeUserPost_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData("Редактирование поля должность", "user", ((User)listFindedUsers.SelectedItem).Login, "title", userPost.Text, _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if(result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля имя пользователя
        private void btChangeUserName_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData("Редактирование имени пользователя", "user", ((User)listFindedUsers.SelectedItem).Login, "givenName", userName.Text, _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля фамилии пользователя
        private void btChangeUserSurname_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData("Редактирование фамилии пользователя", "user", ((User)listFindedUsers.SelectedItem).Login, "sn", userSurname.Text, _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля отдел
        private void btChangeUserDepartment_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData("Редактирование поля отдел", "user", ((User)listFindedUsers.SelectedItem).Login, "department", userDepartment.Text, _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля город
        private void btChangeUserCity_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData("Редактирование поля город", "user", ((User)listFindedUsers.SelectedItem).Login, "l", userCity.Text, _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля внутренний номер
        private void btChangeUserIntPhone_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData("Редактирование поля внутренний номер", "user", ((User)listFindedUsers.SelectedItem).Login, "telephoneNumber", userIntPhone.Text, _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля мобильный номер
        private void btChangeUserMobPhone_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData("Редактирование поля мобильный номер", "user", ((User)listFindedUsers.SelectedItem).Login, "mobile", userMobPhone.Text, _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования размещения компьютера
        private void btChangePlacing_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование размещения компьютера " + ((Computer)listUserComps.SelectedItem).Name, 
                "Computer", 
                ((Computer)listUserComps.SelectedItem).Name + "$", 
                "location", ((Computer)listUserComps.SelectedItem).Place, 
                _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования срока действия учетной записи
        private void btChangeUserExpireDate_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditDateTimeData _dwEDTD = new DialogWindows.EditDateTimeData(
                "Редактирование срока действия УЗ",
                ((User)listFindedUsers.SelectedItem).Login,
                ((User)listFindedUsers.SelectedItem).AccountExpireDate,
                _principalContext);
            _dwEDTD.Owner = Application.Current.MainWindow;
            bool? result = _dwEDTD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля отображаемое имя
        private void btChangeUserDisplayName_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование поля Отображаемое имя", 
                "user", 
                ((User)listFindedUsers.SelectedItem).Login,
                "displayName",
                userDisplayName.Text, 
                _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля организация
        private void btChangeUserOrganization_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование поля организация",
                "user",
                ((User)listFindedUsers.SelectedItem).Login,
                "company",
                userOrganization.Text,
                _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля адрес
        private void btChangeUserAdress_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование поля адрес",
                "user",
                ((User)listFindedUsers.SelectedItem).Login,
                "streetAddress",
                userAdress.Text,
                _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования логина
        private void btChangeUserLogin_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование логина пользователя",
                "user",
                ((User)listFindedUsers.SelectedItem).Login,
                "sAMAccountName",
                userLogin.Text,
                _connectedSession,
                20,
                "login");
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажата кнопка редактирования имени пользователя в АД
        private void btChangeUserNameInAD_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование имени пользователя в АД",
                "user",
                ((User)listFindedUsers.SelectedItem).Login,
                "name",
                userNameInAD.Text,
                _connectedSession,
                64,
                "name");
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateUserData_Click(sender, e);
        }
        // Нажат выбор требования смены пароля пользователя при следующем входе в систему
        private void userPassIsDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryEntry entryToUpdate;
                DirectorySearcher dirSearcher = new DirectorySearcher(_connectedSession);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", ((User)listFindedUsers.SelectedItem).Login);
                dirSearcher.PropertiesToLoad.Add("pwdLastSet");
                SearchResult searchResults = dirSearcher.FindOne();
                if (searchResults == null)
                {
                    MessageBox.Show("Не удалось получить информацию по текущему пользователю!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                entryToUpdate = searchResults.GetDirectoryEntry();
                if (searchResults.Properties.Contains("pwdLastSet"))
                {
                    switch (userPassIsDefault.IsChecked)
                    {
                        case true:
                            entryToUpdate.Properties["pwdLastSet"].Value = 0;
                            break;
                        case false:
                            entryToUpdate.Properties["pwdLastSet"].Value = -1;
                            break;
                    }

                }
                else
                {
                    MessageBox.Show("Не удалось получить параметр смены пароля!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                entryToUpdate.CommitChanges();
                btUpdateUserData_Click(sender, e);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region События редактирования вкладки Компьютеры
        // Нажата кнопка перемещения компьютера
        private void btMovePCInAD_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.MoveUserInAD _dwMUIAD = new DialogWindows.MoveUserInAD(((Computer)listFindedPC.SelectedItem).PlaceInAD, _connectedSession, "movePC");
            _dwMUIAD.Owner = Application.Current.MainWindow;
            bool? result = _dwMUIAD.ShowDialog();
            if (result == true)
                btUpdatePCData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля Размещение
        private void btChangePCLocation_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование поля Размещение",
                "Computer",
                ((Computer)listFindedPC.SelectedItem).Name + "$",
                "location", ((Computer)listFindedPC.SelectedItem).Place,
                _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdatePCData_Click(sender, e);
        }
        // Нажата кнопка редактирования поля Описание
        private void btChangePCDescription_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование поля Описание",
                "Computer",
                ((Computer)listFindedPC.SelectedItem).Name + "$",
                "description", ((Computer)listFindedPC.SelectedItem).Description,
                _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdatePCData_Click(sender, e);
        }
        #endregion

        #region События редатрирования вкладки Группы
        // Нажата кнопка редактирования поля Имя в АД
        private void btEditGroupNameInAD_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование имени группы в АД",
                "group",
                ((Group)listFindedGroup.SelectedItem).NameWin,
                "name",
                groupNameInAD.Text,
                _connectedSession,
                64,
                "name");
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateCurGroupInfo_Click(sender, e);
        }
        // Нажата кнопка редактирования поля Отображаемое имя
        private void btEditGroupDisplayName_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование поля Отображаемое имя",
                "group",
                ((Group)listFindedGroup.SelectedItem).NameWin,
                "displayName",
                groupDisplayName.Text,
                _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateCurGroupInfo_Click(sender, e);
        }
        // Нажата кнопка редактирования поля Имя группы Win
        private void btEditGroupNameWin_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование Имени группы Win",
                "group",
                ((Group)listFindedGroup.SelectedItem).NameWin,
                "sAMAccountName",
                groupNameWin.Text,
                _connectedSession,
                20);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateCurGroupInfo_Click(sender, e);
        }
        // Нажата кнопка редактирования поля Описание
        private void btEditGroupDescription_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData(
                "Редактирование описания группы", 
                "group", 
                ((Group)listFindedGroup.SelectedItem).NameWin, 
                "description", 
                groupDescription.Text, 
                _connectedSession);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateCurGroupInfo_Click(sender, e);
        }
        #endregion

        #endregion

        #region Методы
        // Показать сообщение об ошибке с указанным текстом
        private void ShowErrorMessage(string msg)
        {
            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        // Получить информацию о пользователе из домена
        private User GetUserInfoFromAD(string sаmaccountname, PrincipalContext context, DirectoryEntry entry)
        {
            User result = new User();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", sаmaccountname);
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                dirSearcher.PropertiesToLoad.Add("name");
                dirSearcher.PropertiesToLoad.Add("title");
                dirSearcher.PropertiesToLoad.Add("department");
                dirSearcher.PropertiesToLoad.Add("l");
                dirSearcher.PropertiesToLoad.Add("mobile");
                dirSearcher.PropertiesToLoad.Add("company");
                dirSearcher.PropertiesToLoad.Add("streetAddress");
                dirSearcher.PropertiesToLoad.Add("pwdLastSet");
                SearchResult searchResults = dirSearcher.FindOne();
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, sаmaccountname);
                List<Computer> comps = GetCompUserFromAD(sаmaccountname, entry);
                result = new User
                {
                    PlaceInAD = user.DistinguishedName,
                    Name = user.GivenName,
                    Surname = user.Surname,
                    DisplayName = user.DisplayName,
                    Login = (string)searchResults.Properties["sAMAccountName"][0],
                    NameInAD = (string)searchResults.Properties["name"][0],
                    Post = (searchResults.Properties.Contains("title") ? (string)searchResults.Properties["title"][0] : ""),
                    PhoneInt = user.VoiceTelephoneNumber,
                    PhoneMob = (searchResults.Properties.Contains("mobile") ? (string)searchResults.Properties["mobile"][0] : ""),
                    City = (searchResults.Properties.Contains("l") ? (string)searchResults.Properties["l"][0] : ""),
                    Department = (searchResults.Properties.Contains("department") ? (string)searchResults.Properties["department"][0] : ""),
                    Organization = (searchResults.Properties.Contains("company") ? (string)searchResults.Properties["company"][0] : ""),
                    Adress = (searchResults.Properties.Contains("streetAddress") ? (string)searchResults.Properties["streetAddress"][0] : ""),
                    Mail = user.EmailAddress,
                    PassExpireDate = user.PasswordNeverExpires ? new DateTime(1970, 01, 01, 00, 00, 00) : DateTime.FromFileTime((Int64)searchResults.Properties["PwdLastSet"][0]).AddDays(_maxPwdAgeDay),
                    AccountExpireDate = user.AccountExpirationDate,
                    PassMustBeChange = (user.LastPasswordSet == null),
                    AccountIsDisable = (user.Enabled == false ? true : false),
                    AccountIsLock = user.IsAccountLockedOut(),
                    Computers = comps
                };
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }
        // Получить информацию о группе из домена
        private Group GetGroupInfoFromAD(string sаmaccountname, DirectoryEntry entry)
        {
            Group result = new Group();
            DirectorySearcher dirSearcher = new DirectorySearcher(entry);
            dirSearcher.SearchScope = SearchScope.Subtree;
            dirSearcher.Filter = string.Format("(&(objectClass=group)(sAMAccountName={0}))", sаmaccountname);
            dirSearcher.PropertiesToLoad.Add("distinguishedName");
            dirSearcher.PropertiesToLoad.Add("Name");
            dirSearcher.PropertiesToLoad.Add("sAMAccountName");
            dirSearcher.PropertiesToLoad.Add("displayName");
            dirSearcher.PropertiesToLoad.Add("description");
            dirSearcher.PropertiesToLoad.Add("groupType");
            dirSearcher.PropertiesToLoad.Add("mail");
            dirSearcher.PropertiesToLoad.Add("member");
            dirSearcher.PropertiesToLoad.Add("memberOf");
            SearchResult searchResults = dirSearcher.FindOne();
            List<string> member = new List<string>();
            List<string> memberOf = new List<string>();
            if (searchResults.Properties.Contains("member"))
            {
                for (int i = 0; i < searchResults.Properties["member"].Count; i++) member.Add(searchResults.Properties["member"][i].ToString());
            }
            if (searchResults.Properties.Contains("memberOf"))
            {
                for (int i = 0; i < searchResults.Properties["memberOf"].Count; i++) memberOf.Add(searchResults.Properties["memberOf"][i].ToString());
            }
            result = new Group
            {
                PlaceInAD = (string)searchResults.Properties["distinguishedName"][0],
                Name = (string)searchResults.Properties["Name"][0],
                NameWin = (string)searchResults.Properties["sAMAccountName"][0],
                DisplayName = (searchResults.Properties.Contains("displayName") ? (string)searchResults.Properties["displayName"][0] : ""),
                Description = (searchResults.Properties.Contains("description") ? (string)searchResults.Properties["description"][0] : ""),
                Type = (int)searchResults.Properties["groupType"][0],
                Mail = (searchResults.Properties.Contains("mail") ? (string)searchResults.Properties["mail"][0] : ""),
                Users = member,
                InGroups = memberOf
            };
            return result;
        }
        // Получить информацию о компьютере из домена
        private Computer GetPCInfoFromAD(string searchData, PrincipalContext context, DirectoryEntry entry, string searchParam = "distinguishedName")
        {
            Computer result = new Computer();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=computer)({1}={0}))", searchData, searchParam);
                dirSearcher.PropertiesToLoad.Add("distinguishedName");
                dirSearcher.PropertiesToLoad.Add("name");
                dirSearcher.PropertiesToLoad.Add("dNSHostName");
                dirSearcher.PropertiesToLoad.Add("description");
                dirSearcher.PropertiesToLoad.Add("location");
                dirSearcher.PropertiesToLoad.Add("operatingSystem");
                dirSearcher.PropertiesToLoad.Add("operatingSystemServicePack");
                dirSearcher.PropertiesToLoad.Add("operatingSystemVersion");
                SearchResult searchResults = dirSearcher.FindOne();
                ComputerPrincipal comp = ComputerPrincipal.FindByIdentity(context, IdentityType.DistinguishedName, (string)searchResults.Properties["distinguishedName"][0]);
                result = new Computer
                {
                    PlaceInAD = comp.DistinguishedName,
                    Name = (string)searchResults.Properties["name"][0],
                    DnsName = (searchResults.Properties.Contains("dNSHostName") ? (string)searchResults.Properties["dNSHostName"][0] : ""),
                    Description = (searchResults.Properties.Contains("description") ? (string)searchResults.Properties["description"][0] : ""),
                    Place = (searchResults.Properties.Contains("location") ? (string)searchResults.Properties["location"][0] : ""),
                    Os = (searchResults.Properties.Contains("operatingSystem") ? (string)searchResults.Properties["operatingSystem"][0] : "") + " " +
                         (searchResults.Properties.Contains("operatingSystemServicePack") ? (string)searchResults.Properties["operatingSystemServicePack"][0] : "") + " " +
                         (searchResults.Properties.Contains("operatingSystemVersion") ? (string)searchResults.Properties["operatingSystemVersion"][0] : ""),
                    PcIsDisable = (comp.Enabled == false ? true : false)
                };
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }
        // Получить список компьютеров на которые заходил пользователь
        private List<Computer> GetCompUserFromAD(string sаmaccountname, DirectoryEntry entry)
        {
            List<Computer> result = new List<Computer>();
            DirectorySearcher dirSearcher = new DirectorySearcher(entry);
            dirSearcher.SearchScope = SearchScope.Subtree;
            dirSearcher.Filter = string.Format("(&(objectClass=Computer)(|(description={0})))", sаmaccountname);
            dirSearcher.PropertiesToLoad.Add("distinguishedName");
            dirSearcher.PropertiesToLoad.Add("name");
            dirSearcher.PropertiesToLoad.Add("location");
            SearchResultCollection searchResults = dirSearcher.FindAll();
            foreach (SearchResult sr in searchResults)
            {
                result.Add(new Computer
                {
                    PlaceInAD = (string)sr.Properties["distinguishedName"][0],
                    Name = (string)sr.Properties["name"][0],
                    Place = (sr.Properties.Contains("location") ? (string)sr.Properties["location"][0] : "")
                });
            }
            return result;
        }
        // Загрузка плагинов из указанной дирректории
        private List<PluginData> LoadPlugins(string path, string login, string pass, DirectoryEntry entry, PrincipalContext context)
        {
            List<PluginData> plugins = new List<PluginData>();
            foreach (var file in Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    int index = file.ToString().LastIndexOf('\\');
                    string namespase = file.ToString().Substring(index, (file.ToString().Length - index)).TrimEnd(new char[] { '.', 'd', 'l', 'l' }).TrimStart('\\');
                    string configPath = file.ToString().Substring(0, index) + "\\config.ini";
                    if (!File.Exists(configPath))
                        configPath = "";
                    Assembly a = Assembly.LoadFile(file); // dll file
                    if (a == null) continue;
                    Type t = a.GetType(namespase + ".PlugIn"); // namespace - "MyPlayers" , class - "Player"
                    if (t == null) continue;
                    Object instance = Activator.CreateInstance(t);
                    if (instance == null) continue;
                    MethodInfo m = t.GetMethod("PlControl"); // method
                    if (m == null) continue;
                    PropertyInfo f = t.GetProperty("Number");
                    if (f == null) continue;
                    PropertyInfo n = t.GetProperty("DisplayPluginName");
                    if (n == null) continue;
                    string name = (string)n.GetValue(instance,null);
                    int number = (int)f.GetValue(instance,null);
                    UserControl control = (UserControl)m.Invoke(instance, new object[] { login, pass, configPath, entry, context });
                    if (control != null)
                    {
                        plugins.Add(new PluginData { IndexNumber = number, DisplayName = name, PLControl = control });
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return plugins;
        }
        // Создание вкладок на форме и добавление загруженых плагинов
        private bool AddPluginToForm(TabControl control, List<PluginData> plugins)
        {
            if (plugins.Count > 0)
            {
                try
                {
                    plugins.Sort(new PluginDataComparer());
                    for (int i = 0; i < plugins.Count; i++)
                    {
                        TabItem tab = new TabItem();
                        tab.Header = plugins[i].DisplayName;
                        tab.Content = plugins[i].PLControl;
                        control.Items.Add(tab);
                    }
                }
                catch (Exception exp)
                {
                    ShowErrorMessage(exp.Message);
                }
            }
            return true;
        }
        #endregion
    }
}
