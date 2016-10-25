using ADWorkerSecondLine.DataProvider;
using ADWorkerSecondLine.Model;
using ADWorkerSecondLine.UISearchTextBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для AddComputersToGroup.xaml
    /// </summary>
    public partial class AddComputersToGroup : Window
    {
        #region Поля
        private Dictionary<string, string> fieldsPCInAD; // Сопоставление групп поиска и полей в АД
        private PrincipalContext _principalContext; // Контекст соединения с АД
        private DirectoryEntry _sessionAD; //сессия АД
        private ObservableCollection<string> _selectedPCs; // Список выбранных компьютеров для добавления
        private string _distinguishedNameGroup; // Группа в которую будут добавлены выбранные компьютеры
        private ListSortDirection _sortDirection; // Передыдущий режим сортировки вкладки Пользователи
        private GridViewColumnHeader _sortColumn; // Предыдущая колонка сортировки вкладки Пользователи
        #endregion

        #region Конструктор
        public AddComputersToGroup(string distinguishedNameGroup, DirectoryEntry entry, PrincipalContext context)
        {
            InitializeComponent();
            _selectedPCs = new ObservableCollection<string>();
            _principalContext = context;
            _sessionAD = entry;
            _distinguishedNameGroup = distinguishedNameGroup;
            ListSelectedPC.ItemsSource = _selectedPCs;
            #region Сопоставление групп поиска и полей в АД
            fieldsPCInAD = new Dictionary<string, string>();
            fieldsPCInAD.Add("По умолчанию", "Default");
            fieldsPCInAD.Add("Имя", "name");
            fieldsPCInAD.Add("DNS имя", "dNSHostName");
            fieldsPCInAD.Add("Размещение", "location");
            fieldsPCInAD.Add("Описание", "description");
            #endregion
            // Устанавливаем настройки поиска пользователя в домене, создаём группы поиска и выбираем стиль отображения групп
            List<string> sections = new List<string> { 
                "По умолчанию", 
                "Имя", 
                "DNS имя", 
                "Размещение",
                "Описание" };
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
            ReadOnlyCollection<Computer> items;
            string errorMsg = "";
            string ouForFindText = "";
            bool compInOU = false;
            if (findPCInOU.IsChecked == true)
            {
                compInOU = true;
                ouForFindText = OUForFindPC.Text;
            }
            ListPCForSelected.ItemsSource = null;
            Search.IsEnabled = false;
            findPCInOU.IsEnabled = false;
            OUForFindPC.IsEnabled = false;
            btSelectOUForFindPC.IsEnabled = false;
            Filter.IsEnabled = false;
            Filter.Text = "";
            // Убираем стрелку сортировки из предыдущей колонки
            if (_sortColumn != null)
            {
                _sortColumn.Column.HeaderTemplate = null;
                _sortColumn.Column.Width = _sortColumn.ActualWidth - 20;
                _sortColumn = null;
            }
            new Thread(() =>
            {
                if (compInOU)
                {
                    if (!string.IsNullOrWhiteSpace(ouForFindText))
                        items = AsyncDataProvider.GetPCItemsInOU(ouForFindText, _principalContext, _sessionAD, ref errorMsg);
                    else
                        items = AsyncDataProvider.GetPCItems();
                }
                else
                {
                    items = AsyncDataProvider.GetPCItems(fieldsPCInAD[searchArgs.Sections[0]], searchArgs.Keyword, _principalContext, _sessionAD, ref errorMsg);
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ListPCForSelected.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListPCForSelected.ItemsSource);
                    view.Filter = FindedPCs_Filter;
                    if (view.Count > 0)
                    {
                        ListPCForSelected.SelectedIndex = 0;
                    }
                    Filter.IsEnabled = true;
                    Search.IsEnabled = true;
                    findPCInOU.IsEnabled = true;
                    if (findPCInOU.IsChecked == true)
                    {
                        Search.IsEnabled = false;
                        OUForFindPC.IsEnabled = true;
                        btSelectOUForFindPC.IsEnabled = true;
                    }
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }).Start();
        }
        // Нажата кнопка обновления найденных компьютеров
        private void btUpdatePCFind_Click(object sender, RoutedEventArgs e)
        {
            string selectedSection = Search.SelectedSections[0];
            string filterStr = Search.Text;
            ReadOnlyCollection<Computer> items;
            string errorMsg = "";
            string ouForFindText = "";
            bool compInOU = false;
            if (findPCInOU.IsChecked == true)
            {
                compInOU = true;
                ouForFindText = OUForFindPC.Text;
            }
            ListPCForSelected.ItemsSource = null;
            Search.IsEnabled = false;
            findPCInOU.IsEnabled = false;
            OUForFindPC.IsEnabled = false;
            btSelectOUForFindPC.IsEnabled = false;
            Filter.IsEnabled = false;
            Filter.Text = "";
            // Убираем стрелку сортировки из предыдущей колонки
            if (_sortColumn != null)
            {
                _sortColumn.Column.HeaderTemplate = null;
                _sortColumn.Column.Width = _sortColumn.ActualWidth - 20;
                _sortColumn = null;
            }
            new Thread(() =>
            {
                if (compInOU)
                {
                    if (!string.IsNullOrWhiteSpace(ouForFindText))
                        items = AsyncDataProvider.GetPCItemsInOU(ouForFindText, _principalContext, _sessionAD, ref errorMsg);
                    else
                        items = AsyncDataProvider.GetPCItems();
                }
                else
                {
                    items = AsyncDataProvider.GetPCItems(fieldsPCInAD[selectedSection], filterStr, _principalContext, _sessionAD, ref errorMsg);
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ListPCForSelected.ItemsSource = items;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListPCForSelected.ItemsSource);
                    view.Filter = FindedPCs_Filter;
                    if (view.Count > 0)
                    {
                        ListPCForSelected.SelectedIndex = 0;
                    }
                    Filter.IsEnabled = true;
                    Search.IsEnabled = true;
                    findPCInOU.IsEnabled = true;
                    if (findPCInOU.IsChecked == true)
                    {
                        Search.IsEnabled = false;
                        OUForFindPC.IsEnabled = true;
                        btSelectOUForFindPC.IsEnabled = true;
                    }
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }).Start();
        }
        // Фильтр найденных пользователей
        private bool FindedPCs_Filter(object item)
        {
            if (String.IsNullOrEmpty(Filter.Text))
                return true;
            return (((Computer)item).Name.ToUpper().Contains(Filter.Text.ToUpper()));
        }
        // Событие изменения текста в фильтре найденных пользователей
        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListPCForSelected.ItemsSource);
            if (view != null)
            {
                view.Refresh();
                if (view.Count > 0)
                {
                    ListPCForSelected.SelectedIndex = 0;
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
        private void ListPCForSelectedItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btAddSelectedPCs_Click(sender, e);
        }
        // Нажата кнопка добавления выбранного пользователя к списку выбранных для добавления в группу
        private void btAddSelectedPCs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in ListPCForSelected.SelectedItems)
                {
                    bool isTake = false;
                    foreach (string PlaceInAD in _selectedPCs)
                    {
                        if (PlaceInAD == ((Computer)item).PlaceInAD) isTake = true;
                    }

                    if (!isTake)
                    {
                        _selectedPCs.Add(((Computer)item).PlaceInAD);
                        ListSelectedPC.SelectedIndex = 0;
                        btAddPCsToSelectedGroup.IsEnabled = true;
                    }
                }
                ListPCForSelected.SelectedItems.Clear();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Нажата кнопка удаления выбранных пользователей из списка выбранных пользователей для добавления
        private void btDeleteSelectedPCs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                while (ListSelectedPC.SelectedItems.Count > 0)
                {
                    _selectedPCs.Remove((string)ListSelectedPC.SelectedItems[0]);
                }
                if (ListSelectedPC.Items.Count <= 0)
                {
                    btAddPCsToSelectedGroup.IsEnabled = false;
                }
                else
                {
                    ListSelectedPC.SelectedIndex = 0;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Выполненно двойное нажатие левой кнопки мышки на елементе списка пользователей для добавления
        private void ListSelectedPCItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btDeleteSelectedPCs_Click(sender, e);
        }
        // Нажата кнопка добавления выбранных пользователей в группу
        private void btAddPCsToSelectedGroup_Click(object sender, RoutedEventArgs e)
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
                        foreach (string comp in searchResult.Properties["member"])
                        {
                            bool isTake = false;
                            string itemForRemove = "";
                            foreach (string selectedUser in _selectedPCs)
                            {
                                if (comp == selectedUser)
                                {
                                    isTake = true;
                                    itemForRemove = comp;
                                    break;
                                }
                            }
                            if (isTake)
                            {
                                _selectedPCs.Remove(itemForRemove);
                                userIsNotAddMessage += comp + ", ";
                            }
                        }
                    }

                    // Добавляем выбранных пользователей в группу
                    foreach (string userToAdd in _selectedPCs)
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
        // Выполнено нажатие на заголовок столбца с найденными компьютерами
        private void ListPCForSelected_Click(object sender, RoutedEventArgs e)
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

            ICollectionView resultDataView = CollectionViewSource.GetDefaultView(ListPCForSelected.ItemsSource);
            resultDataView.SortDescriptions.Clear();
            resultDataView.SortDescriptions.Add(new SortDescription(header, _sortDirection));
        }
        // Найти компьютеры из указанной OU
        private void findPCInOU_Click(object sender, RoutedEventArgs e)
        {
            if (findPCInOU.IsChecked == true)
            {
                Search.IsEnabled = false;
                OUForFindPC.IsEnabled = true;
                btSelectOUForFindPC.IsEnabled = true;
            }
            else
            {
                Search.IsEnabled = true;
                OUForFindPC.IsEnabled = false;
                btSelectOUForFindPC.IsEnabled = false;
                OUForFindPC.Text = "";
            }
        }
        // Нажата кнопка выбора OU для поиска
        private void btSelectOUForFindPC_Click(object sender, RoutedEventArgs e)
        {
            DialogWindows.MoveUserInAD _dwMUIAD = new DialogWindows.MoveUserInAD("", _sessionAD, "select");
            _dwMUIAD.Owner = Application.Current.MainWindow;
            bool? result = _dwMUIAD.ShowDialog();
            if (result == true)
                OUForFindPC.Text = _dwMUIAD.SelectedOU;
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
