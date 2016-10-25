using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using ADWorkerSecondLine.Model;
using ADWorkerSecondLine.DataProvider;
using ADWorkerSecondLine.Converters;
using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для ViewAndEditUserGroups.xaml
    /// </summary>
    public partial class ViewAndEditUserGroups : Window
    {
        #region Поля
        private DirectoryEntry _sessionAD; //сессия АД
        private PrincipalContext _principalContext; // Контекст соединения с АД
        private string _distinguishedNameUser; // dn запись пользователя чьи группы отображаются
        private string _mode; // Режим работы формы
        #endregion

        #region Конструктор
        public ViewAndEditUserGroups(string dnUser, DirectoryEntry entry, PrincipalContext context, string mode = "user")
        {
            InitializeComponent();
            _sessionAD = entry;
            _principalContext = context;
            _distinguishedNameUser = dnUser;
            _mode = mode;
            if (mode == "comp")
            {
                Title = "Группы компьютера";
                tabItemsInGroup.Header = "Компьютеры состоящие в группе";
                headerItemsInGroup.Header = "Компьютер в АД";
                btAddUserInGroup_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/add_computer_in_groups.ico", UriKind.RelativeOrAbsolute));
                btDelUserFromGroup_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/remove_computer_from_groups.ico", UriKind.RelativeOrAbsolute));
                btAddUser_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/add_computer_in_group.ico", UriKind.RelativeOrAbsolute));
                btDelUser_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/remove_computer_from_group.ico", UriKind.RelativeOrAbsolute));
            }
            UpdateListGroupUser();
        }
        #endregion

        #region События
        // Фильтр групп
        private bool Groups_Filter(object item)
        {
            if (String.IsNullOrEmpty(filterUserGroups.Text))
                return true;

            var group = (Group)item;

            return (group.Name.ToUpper().Contains(filterUserGroups.Text.ToUpper()));
        }
        // Фильтр пользователей состоящих в текущей группе
        private bool UserInGroup_Filter(object item)
        {
            if (String.IsNullOrEmpty(userInGroupFilter.Text))
                return true;

            var user = (string)item;

            return (user.ToUpper().Contains(userInGroupFilter.Text.ToUpper()));
        }
        // Фильтр групп состоящих в текущей группе
        private bool GroupIn_Filter(object item)
        {
            if (String.IsNullOrEmpty(groupInFilter.Text))
                return true;

            var group = (string)item;

            return (group.ToUpper().Contains(groupInFilter.Text.ToUpper()));
        }
        // Изменился выбранный элемент в списке групп
        private void userGroupsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (userGroupsList.SelectedItem != null)
            {
                CollectionView view1 = (CollectionView)CollectionViewSource.GetDefaultView(groupInList.ItemsSource);
                view1.Filter = GroupIn_Filter;
                view1.SortDescriptions.Clear();
                view1.SortDescriptions.Add(new SortDescription());
                CollectionView view2 = (CollectionView)CollectionViewSource.GetDefaultView(userInGroupList.ItemsSource);
                view2.Filter = UserInGroup_Filter;
                view2.SortDescriptions.Clear();
                view2.SortDescriptions.Add(new SortDescription());
                switch (((Group)userGroupsList.SelectedItem).Type)
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
                gtypeDistribution.IsChecked = null;
                gtypeGlobal.IsChecked = null;
                gtypeLocalInDomain.IsChecked = null;
                gtypeSecurity.IsChecked = null;
                gtypeUnivers.IsChecked = null;
            }
        }
        // Изменено содержимое поля фильтрации групп
        private void filterUserGroups_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(userGroupsList.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                if (view.Count > 0)
                {
                    userGroupsList.SelectedIndex = 0;
                }
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }
        // Изменено содержимое поля фильтрации пользователей состоящих в текущей группе
        private void userInGroupFilter_TextChanged(object sender, TextChangedEventArgs e)
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
        // Нажата кнопка обновления информации о текущей группе
        private void btUpdateCurGroupInfo_Click(object sender, RoutedEventArgs e)
        {
            if (userGroupsList.SelectedItem != null)
            {
                string sAN = ((Group)userGroupsList.SelectedItem).Name;
                groupInList.SelectedItem = null;
                userInGroupList.SelectedItem = null;
                btAddUserInGroup.IsEnabled = false;
                btAddUser.IsEnabled = false;
                userInGroupList.IsEnabled = false;
                groupInList.IsEnabled = false;
                filterUserGroups.IsEnabled = false;
                userGroupsList.IsEnabled = false;

                btUpdateCurGroupInfo.IsEnabled = false;
                btEditGroupDescription.IsEnabled = false;
                btDelUserFromGroup.IsEnabled = false;
                new Thread(() =>
                {
                    Group groupData = GetGroupInfoFromAD(sAN, _sessionAD);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ((Group)userGroupsList.SelectedItem).copyData(groupData);
                        btAddUserInGroup.IsEnabled = true;
                        btAddUser.IsEnabled = true;
                        userInGroupList.IsEnabled = true;
                        groupInList.IsEnabled = true;
                        filterUserGroups.IsEnabled = true;
                        userGroupsList.IsEnabled = true;

                        Binding binding = new Binding();
                        binding.Source = userGroupsList; // установить в качестве source object значение ElementName
                        binding.Path = new PropertyPath(ListView.SelectedItemProperty);
                        binding.Mode = BindingMode.OneWay;
                        binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        binding.Converter = new NullToBooleanConverter();
                        BindingOperations.SetBinding(btEditGroupDescription, Button.IsEnabledProperty, binding);
                        BindingOperations.SetBinding(btDelUserFromGroup, Button.IsEnabledProperty, binding);
                        BindingOperations.SetBinding(btUpdateCurGroupInfo, Button.IsEnabledProperty, binding);

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
                    }));
                }).Start();
            }
        }
        // Нажата кнопка добавления текущего пользователя в группы
        private void btAddUserInGroup_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.AddUserToGroups _dwAUG = new DialogWindows.AddUserToGroups(_distinguishedNameUser, _sessionAD, _mode);
            _dwAUG.Owner = Application.Current.MainWindow;
            bool? result = _dwAUG.ShowDialog();
            if (result == true)
                UpdateListGroupUser();
        }
        // Нажата кнопка удаления пользователя из выбранных групп
        private void btDelUserFromGroup_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result;
            if (_mode == "comp")
            {
                result = MessageBox.Show("Вы уверены что хотите удалить текущий компьютер из выделенных групп?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            else
            {
                result = MessageBox.Show("Вы уверены что хотите удалить текущего пользователя из выделенных групп?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            if (result == MessageBoxResult.No)
            {
                return;
            }
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                dirSearcher.SearchScope = SearchScope.Subtree;
                foreach (var item in userGroupsList.SelectedItems)
                {
                    dirSearcher.Filter = string.Format("(&(objectClass=group)(distinguishedName={0}))", ((Group)item).PlaceInAD);
                    dirSearcher.PropertiesToLoad.Add("member");
                    SearchResult searchResult = dirSearcher.FindOne();
                    if (searchResult != null)
                    {
                        DirectoryEntry group = searchResult.GetDirectoryEntry();
                        group.Properties["member"].Remove(_distinguishedNameUser);
                        group.CommitChanges();
                    }
                }
                UpdateListGroupUser();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        // Нажата кнопка редактирования описания выбранной группы
        private void btEditGroupDescription_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.EditTextData _dwETD = new DialogWindows.EditTextData("Редактирование описания группы", "group", ((Group)userGroupsList.SelectedItem).Name, "description", description.Text, _sessionAD);
            _dwETD.Owner = Application.Current.MainWindow;
            bool? result = _dwETD.ShowDialog();
            if (result == true)
                btUpdateCurGroupInfo_Click(sender, e);
        }
        // Нажата кнопка добавления пользователей в группу
        private void btAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == "comp")
            {
                DialogWindows.AddComputersToGroup _dwACG = new DialogWindows.AddComputersToGroup(((Group)userGroupsList.SelectedItem).PlaceInAD, _sessionAD, _principalContext);
                _dwACG.Owner = Application.Current.MainWindow;
                bool? result = _dwACG.ShowDialog();
                if (result == true)
                    btUpdateCurGroupInfo_Click(sender, e);
            }
            else
            {
                DialogWindows.AddUsersToGroup _dwAUG = new DialogWindows.AddUsersToGroup(((Group)userGroupsList.SelectedItem).PlaceInAD, _sessionAD, _principalContext);
                _dwAUG.Owner = Application.Current.MainWindow;
                bool? result = _dwAUG.ShowDialog();
                if (result == true)
                    btUpdateCurGroupInfo_Click(sender, e);
            }
        }
        // Нажата кнопка удаления выделенного пользователя из группы
        private void btDelUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result;
            if (_mode == "comp")
            {
                result = MessageBox.Show("Вы уверены что хотите удалить выбранные компьютеры из текущей группы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            else
            {
                result = MessageBox.Show("Вы уверены что хотите удалить выбранных пользователей из текущей группы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            if (result == MessageBoxResult.No)
            {
                return;
            }
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=group)(distinguishedName={0}))", ((Group)userGroupsList.SelectedItem).PlaceInAD);
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
                    btUpdateCurGroupInfo_Click(sender,e);
                }
                else
                {
                    MessageBox.Show("Не удалось получить информацию по группе" + ((Group)userGroupsList.SelectedItem).PlaceInAD + "./r/nГруппа не найдена в домене!!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
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
        #endregion

        #region Методы
        // Получить информацию о группе из домена
        private Group GetGroupInfoFromAD(string sаmaccountname, DirectoryEntry entry)
        {
            Group result = new Group();
            DirectorySearcher dirSearcher = new DirectorySearcher(entry);
            dirSearcher.SearchScope = SearchScope.Subtree;
            dirSearcher.Filter = string.Format("(&(objectClass=group)(sAMAccountName={0}))", sаmaccountname);
            dirSearcher.PropertiesToLoad.Add("distinguishedName");
            dirSearcher.PropertiesToLoad.Add("sAMAccountName");
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
                Name = (string)searchResults.Properties["sAMAccountName"][0],
                Description = (searchResults.Properties.Contains("description") ? (string)searchResults.Properties["description"][0] : ""),
                Type = (int)searchResults.Properties["groupType"][0],
                Mail = (searchResults.Properties.Contains("mail") ? (string)searchResults.Properties["mail"][0] : ""),
                Users = member,
                InGroups = memberOf
            };
            return result;
        }
        // Обновление списка групп пользователя
        private void UpdateListGroupUser()
        {
            ReadOnlyCollection<Group> items;
            userGroupsList.ItemsSource = null;
            string errorMsg = "";
            new Thread(() =>
            {
                items = AsyncDataProvider.GetGroupItems(_distinguishedNameUser, _sessionAD, ref errorMsg);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    userGroupsList.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(userGroupsList.ItemsSource);
                    if (view != null)
                    {
                        view.Filter = Groups_Filter;
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                        if (view.Count > 0)
                        {
                            userGroupsList.SelectedIndex = 0;
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
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }).Start();
        }
        #endregion
    }
}
