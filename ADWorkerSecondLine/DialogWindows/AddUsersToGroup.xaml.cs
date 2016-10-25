using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using ADWorkerSecondLine.UISearchTextBox;
using System.Collections.ObjectModel;
using ADWorkerSecondLine.Model;
using ADWorkerSecondLine.DataProvider;
using System.Threading;
using System.DirectoryServices.AccountManagement;
using System.Windows.Input;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для AddUsersToGroup.xaml
    /// </summary>
    public partial class AddUsersToGroup : Window
    {
        #region Поля
        private Dictionary<string, string> fieldsInAD; // Сопоставление групп поиска и полей в АД
        private PrincipalContext _principalContext; // Контекст соединения с АД
        private DirectoryEntry _sessionAD; //сессия АД
        private ObservableCollection<string> _selectedUsers; // Список выбранных пользователей для добавления
        private string _distinguishedNameGroup; // Группа в которую будут добавлены выбранные пользователи
        #endregion

        #region Конструктор
        public AddUsersToGroup(string distinguishedNameGroup, DirectoryEntry entry, PrincipalContext context)
        {
            InitializeComponent();
            _selectedUsers = new ObservableCollection<string>();
            _principalContext = context;
            _sessionAD = entry;
            _distinguishedNameGroup = distinguishedNameGroup;
            ListSelectedUsers.ItemsSource = _selectedUsers;
            #region Сопоставление групп поиска и полей в АД
            fieldsInAD = new Dictionary<string, string>();
            fieldsInAD.Add("По умолчанию", "Default");
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
            Search.SectionsList = sections;
            Search.SectionsStyle = SearchTextBox.SectionsStyles.RadioBoxStyle;
            // Определяем обработчик поиска
            Search.OnSearch += new RoutedEventHandler(search_OnSearch);
        }
        #endregion

        // Запущен процесс поиска пользователя в АД
        private void search_OnSearch(object sender, RoutedEventArgs e)
        {
            SearchEventArgs searchArgs = e as SearchEventArgs;
            ReadOnlyCollection<User> items;
            string errorMsg = "";

            ListUsersForSelected.ItemsSource = null;
            Search.IsEnabled = false;
            Filter.IsEnabled = false;
            Filter.Text = "";
            new Thread(() =>
            {
                items = AsyncDataProvider.GetUsersForSelected(fieldsInAD[searchArgs.Sections[0]], searchArgs.Keyword, _principalContext, _sessionAD, ref errorMsg);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ListUsersForSelected.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListUsersForSelected.ItemsSource);
                    view.Filter = FindedUsers_Filter;
                    if (view.Count > 0)
                    {
                        ListUsersForSelected.SelectedIndex = 0;
                    }
                    Filter.IsEnabled = true;
                    Search.IsEnabled = true;
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }).Start();
        }
        // Фильтр найденных пользователей
        private bool FindedUsers_Filter(object item)
        {
            if (String.IsNullOrEmpty(Filter.Text))
                return true;

            var user = (User)item;

            return (user.NameInAD.ToUpper().Contains(Filter.Text.ToUpper()));
        }
        // Событие изменения текста в фильтре найденных пользователей
        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListUsersForSelected.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                if (view.Count > 0)
                {
                    ListUsersForSelected.SelectedIndex = 0;
                }
            }
        }
        // Нажата кнопка отмены
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        // Выполненно двойное нажатие левой кнопки мышки на елементе списка групп для выбора
        private void ListUsersForSelectedItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btAddSelectedUsers_Click(sender, e);
        }
        // Нажата кнопка добавления выбранного пользователя к списку выбранных для добавления в группу
        private void btAddSelectedUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in ListUsersForSelected.SelectedItems)
                {
                    bool isTake = false;
                    foreach (string PlaceInAD in _selectedUsers)
                    {
                        if (PlaceInAD == ((User)item).PlaceInAD) isTake = true;
                    }

                    if (!isTake)
                    {
                        _selectedUsers.Add(((User)item).PlaceInAD);
                        ListSelectedUsers.SelectedIndex = 0;
                        btAddUsersToSelectedGroup.IsEnabled = true;
                    }
                }
                ListUsersForSelected.SelectedItems.Clear();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Нажата кнопка удаления выбранных пользователей из списка выбранных пользователей для добавления
        private void btDeleteSelectedUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                while (ListSelectedUsers.SelectedItems.Count > 0)
                {
                    _selectedUsers.Remove((string)ListSelectedUsers.SelectedItems[0]);
                }
                if (ListSelectedUsers.Items.Count <= 0)
                {
                    btAddUsersToSelectedGroup.IsEnabled = false;
                }
                else
                {
                    ListSelectedUsers.SelectedIndex = 0;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Выполненно двойное нажатие левой кнопки мышки на елементе списка пользователей для добавления
        private void ListSelectedUsersItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btDeleteSelectedUsers_Click(sender, e);
        }
        // Нажата кнопка добавления выбранных пользователей в группу
        private void btAddUsersToSelectedGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string userIsNotAddMessage = "";

                // Получаем группу в домене и список её участников
                DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=group)(distinguishedName={0}))", _distinguishedNameGroup);
                dirSearcher.PropertiesToLoad.Add("member");
                SearchResult searchResult = dirSearcher.FindOne();

                if (searchResult != null)
                {
                    // Если группа в домене найдена получаем её запись
                    DirectoryEntry group = searchResult.GetDirectoryEntry();
                    // Сравниваем участников группы и выбранных пользователей, исключаем совпадения из списка выбранных пользователей
                    if (searchResult.Properties.Contains("member"))
                    {
                        foreach (string user in searchResult.Properties["member"])
                        {
                            bool isTake = false;
                            string itemForRemove = "";
                            foreach (string selectedUser in _selectedUsers)
                            {
                                if (user == selectedUser)
                                {
                                    isTake = true;
                                    itemForRemove = user;
                                    break;
                                }
                            }
                            if (isTake)
                            {
                                _selectedUsers.Remove(itemForRemove);
                                userIsNotAddMessage += user + ", ";
                            }
                        }
                    }

                    // Добавляем выбранных пользователей в группу
                    foreach (string userToAdd in _selectedUsers)
                    {
                        group.Properties["member"].Add(userToAdd);
                        group.CommitChanges();
                    }

                    // Если были найдены совпадения, показываем их
                    if (!string.IsNullOrWhiteSpace(userIsNotAddMessage))
                    {
                        MessageBox.Show("Следующие пользователи уже состоят в данной группе:\r\n" + userIsNotAddMessage, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    DialogResult = true;
                    Close();
                }
                else
                {
                    // Если группа в домене не найдена выводим ошибку
                    MessageBox.Show("Не удалось получить информацию по группе" + _distinguishedNameGroup + "./r/nГруппа не найдена в домене!!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        // Нажата кнопка загрузки списка пользователей
        private void btLoadDataFromList_Click(object sender, RoutedEventArgs e)
        {
            string[] separator = { Environment.NewLine };
            LoadListUsers _dwLLU = new LoadListUsers(_sessionAD,_principalContext);
            _dwLLU.Owner = this;
            bool? res = _dwLLU.ShowDialog();
            if (res == true)
            {
                string[] loadArr = _dwLLU.dataForLoad.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < loadArr.Length; j++)
                {
                    bool isTake = false;
                    foreach (string PlaceInAD in _selectedUsers)
                    {
                        if (PlaceInAD == loadArr[j]) isTake = true;
                    }

                    if (!isTake)
                    {
                        _selectedUsers.Add(loadArr[j]);
                        ListSelectedUsers.SelectedIndex = 0;
                        btAddUsersToSelectedGroup.IsEnabled = true;
                    }
                }
            }
        }
    }
}
