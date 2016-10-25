using ADWorkerSecondLine.DataProvider;
using ADWorkerSecondLine.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для AddUserToGroups.xaml
    /// </summary>
    public partial class AddUserToGroups : Window
    {
        #region Поля
        private DirectoryEntry _sessionAD; // Сессия АД для выполнения запросов
        private ObservableCollection<Group> _selectedGroups; // Список выбранных групп для добавления
        private string _distinguishedNameUser; // dn запись пользователя которого необходимо добавить в выбранные группы
        private string _mode; // Режим работы формы
        #endregion

        #region Конструктор
        public AddUserToGroups(string distinguishedNameUser, DirectoryEntry entry, string mode = "user")
        {
            InitializeComponent();
            _selectedGroups = new ObservableCollection<Group>();
            _sessionAD = entry;
            _distinguishedNameUser = distinguishedNameUser;
            _mode = mode;
            selectedGroups.ItemsSource = _selectedGroups;
            ReadOnlyCollection<Group> items;
            groupsForSelected.ItemsSource = null;
            string errorMsg = "";
            if (mode == "comp")
            {
                Title = "Добавление компьютера в группу";
                groupBoxSelectGroup.Header = "Выбор групп для добавления в них компьютера";
            }
            new Thread(() =>
            {
                items = AsyncDataProvider.GetGroupForSelected(_sessionAD, ref errorMsg);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    groupsForSelected.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(groupsForSelected.ItemsSource);
                    if (view != null)
                    {
                        view.Filter = Groups_Filter;
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                    }
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }).Start();
        }
        #endregion


        // Фильтр групп
        private bool Groups_Filter(object item)
        {
            if (String.IsNullOrEmpty(filterGroupsForSelected.Text))
                return true;

            var group = (Group)item;

            return (group.Name.ToUpper().Contains(filterGroupsForSelected.Text.ToUpper()));
        }
        // Изменено содержимое поля фильтрации групп
        private void filterGroupsForSelected_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(groupsForSelected.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                if (view.Count > 0)
                {
                    groupsForSelected.SelectedIndex = 0;
                }
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }
        // Нажата кнопка отмены
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        // Нажата кнопка добавления выбранной группы к списку выбранных для добавления в них пользователя
        private void btAddSelectedGroups_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in groupsForSelected.SelectedItems)
                {
                    bool isTake = false;
                    _selectedGroups.ToList().ForEach(x => 
                    {
                        if (x.Name == ((Group)item).Name) isTake = true;
                    });

                    if (!isTake)
                    {
                        _selectedGroups.Add(new Group{Name = ((Group)item).Name, PlaceInAD = ((Group)item).PlaceInAD});
                        selectedGroups.SelectedIndex = 0;
                        btAddUserToSelectedGroups.IsEnabled = true;
                    }
                }
                groupsForSelected.SelectedItems.Clear();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Нажата кнопка удаления выбранной группы из списка выбранных групп
        private void btDeleteSelectedGroups_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                while (selectedGroups.SelectedItems.Count > 0)
                {
                    _selectedGroups.Remove((Group)selectedGroups.SelectedItems[0]);
                }
                if (selectedGroups.Items.Count <= 0)
                {
                    btAddUserToSelectedGroups.IsEnabled = false;
                }
                else
                {
                    selectedGroups.SelectedIndex = 0;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Выполненно двойное нажатие левой кнопки мышки на елементе списка групп для выбора
        private void groupsForSelectedItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btAddSelectedGroups_Click(sender, e);
        }
        // Выполненно двойное нажатие левой кнопки мышки на елементе списка групп для добавления
        private void selectedGroupsItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btDeleteSelectedGroups_Click(sender, e);
        }
        // Нажата кнопка добавления пользователя в выбранные группы
        private void btAddUserToSelectedGroups_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                string gropsIsNotAddMessage = "";
                DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=group)(member={0}))", _distinguishedNameUser);
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                SearchResultCollection searchResults = dirSearcher.FindAll();
                foreach (SearchResult sr in searchResults)
                {
                    bool isTake = false;
                    Group itemForRemove = new Group();
                    _selectedGroups.ToList().ForEach(x =>
                    {
                        if (x.Name == (string)sr.Properties["sAMAccountName"][0])
                        {
                            isTake = true;
                            itemForRemove = x;
                        }
                    });
                    if (isTake)
                    {
                        _selectedGroups.Remove(itemForRemove);
                        gropsIsNotAddMessage += (string)sr.Properties["sAMAccountName"][0] + ", ";
                    }
                }
                _selectedGroups.ToList().ForEach(x =>
                {
                    dirSearcher.Filter = string.Format("(&(objectClass=group)(distinguishedName={0}))", x.PlaceInAD);
                    dirSearcher.PropertiesToLoad.Add("member");
                    SearchResult searchResult = dirSearcher.FindOne();
                    if (searchResult != null)
                    {
                        DirectoryEntry group = searchResult.GetDirectoryEntry();
                        group.Properties["member"].Add(_distinguishedNameUser);
                        group.CommitChanges();
                    }
                    
                    
                });
                if (!string.IsNullOrWhiteSpace(gropsIsNotAddMessage))
                {
                    if(_mode == "comp")
                        MessageBox.Show("Компьютер уже состоит в группах:\r\n" + gropsIsNotAddMessage, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    else
                        MessageBox.Show("Пользователь уже состоит в группах:\r\n" + gropsIsNotAddMessage, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                DialogResult = true;
                Close();
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
    }
}
