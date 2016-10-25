using ADWorkerSecondLine.DataProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ADWorkerSecondLine.Model;
using ADWorkerSecondLine.Converters;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для MoveUserInAD.xaml
    /// </summary>
    public partial class MoveUserInAD : Window
    {
        #region Поля
        private DirectoryEntry _sessionAD; //сессия АД
        private string _distinguishedNameUser; // dn запись пользователя/компьютера/группы которого необходимо переместить
        private string _mode; // Режим работы формы
        #endregion

        #region Свойства
        // Возвращает DN зачение выбранной OU
        public string SelectedOU
        {
            get { return ((DomainTreeItem)DomainOUTreeView.SelectedItem).Description; }
        }
        #endregion

        #region Конструктор
        public MoveUserInAD(string dnUser, DirectoryEntry entry, string mode = "move")
        {
            InitializeComponent();
            btMove.IsEnabled = false;
            _mode = mode;
            _sessionAD = entry;
            _distinguishedNameUser = dnUser;
            if (mode == "select")
            {
                Title = "Выбор OU для поиска";
                btMove_text.Text = "Выбрать";
                btMove_image.Source = new BitmapImage(new Uri(@"/ADWorkerSecondLine;component/Resources/select.ico", UriKind.RelativeOrAbsolute));
            }
            if (mode == "movePC" || mode == "moveListPC")
                Title = "Перемещение компьютера";
            if (mode == "moveGroup")
                Title = "Перемещение группы";

            CreateDomainOUTree();
        }
        #endregion

        #region События
        // Нажата кнопка отмены
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        // Нажата кнопка переместить
        private void btMove_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == "move")
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены что хотите переместить текущего пользователя в указанное подразделение?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    DomainOUTreeView.Focus();
                    return;
                }
                try
                {
                    DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                    dirSearcher.SearchScope = SearchScope.Subtree;
                    dirSearcher.Filter = string.Format("(&(objectClass=user)(distinguishedName=" + _distinguishedNameUser + "))");
                    SearchResult searchResults = dirSearcher.FindOne();
                    DirectoryEntry theObjectToMove = searchResults.GetDirectoryEntry();

                    dirSearcher.Filter = string.Format("(&(|(objectClass=organizationalUnit)(objectClass=organization)(cn=Users)(cn=Computers))(distinguishedName=" + ((DomainTreeItem)DomainOUTreeView.SelectedItem).Description + "))");
                    searchResults = dirSearcher.FindOne();
                    DirectoryEntry theNewParent = searchResults.GetDirectoryEntry();

                    theObjectToMove.MoveTo(theNewParent);

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    DomainOUTreeView.Focus();
                }
            }
            if (_mode == "movePC")
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены что хотите переместить текущий компьютер в указанное подразделение?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    DomainOUTreeView.Focus();
                    return;
                }
                try
                {
                    DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                    dirSearcher.SearchScope = SearchScope.Subtree;
                    dirSearcher.Filter = string.Format("(&(objectClass=computer)(distinguishedName=" + _distinguishedNameUser + "))");
                    SearchResult searchResults = dirSearcher.FindOne();
                    DirectoryEntry theObjectToMove = searchResults.GetDirectoryEntry();

                    dirSearcher.Filter = string.Format("(&(|(objectClass=organizationalUnit)(objectClass=organization)(cn=Users)(cn=Computers))(distinguishedName=" + ((DomainTreeItem)DomainOUTreeView.SelectedItem).Description + "))");
                    searchResults = dirSearcher.FindOne();
                    DirectoryEntry theNewParent = searchResults.GetDirectoryEntry();

                    theObjectToMove.MoveTo(theNewParent);

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    DomainOUTreeView.Focus();
                }
            }
            if (_mode == "moveGroup")
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены что хотите переместить текущую группу в указанное подразделение?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    DomainOUTreeView.Focus();
                    return;
                }
                try
                {
                    DirectorySearcher dirSearcher = new DirectorySearcher(_sessionAD);
                    dirSearcher.SearchScope = SearchScope.Subtree;
                    dirSearcher.Filter = string.Format("(&(objectClass=group)(distinguishedName=" + _distinguishedNameUser + "))");
                    SearchResult searchResults = dirSearcher.FindOne();
                    DirectoryEntry theObjectToMove = searchResults.GetDirectoryEntry();

                    dirSearcher.Filter = string.Format("(&(|(objectClass=organizationalUnit)(objectClass=organization)(cn=Users)(cn=Computers))(distinguishedName=" + ((DomainTreeItem)DomainOUTreeView.SelectedItem).Description + "))");
                    searchResults = dirSearcher.FindOne();
                    DirectoryEntry theNewParent = searchResults.GetDirectoryEntry();

                    theObjectToMove.MoveTo(theNewParent);

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    DomainOUTreeView.Focus();
                }
            }
            if (_mode == "select")
            {
                DialogResult = true;
                Close();
            }
            if (_mode == "moveListPC")
            {
                DialogResult = true;
                Close();
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
        // Нажата клавиша Enter на элементе дерева
        private void DomainOUTreeViewItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DomainOUTreeView.SelectedItem != null && !((DomainTreeItem)DomainOUTreeView.SelectedItem).Description.StartsWith("DC"))
            {
                e.Handled = true;
                btMove_Click(sender, new RoutedEventArgs());
            }
        }
        #endregion

        #region Методы
        // Создание дерева подразделений домена
        private void CreateDomainOUTree()
        {
            ReadOnlyCollection<DomainTreeItem> items;
            DomainOUTreeView.ItemsSource = null;
            string errorMsg = "";
            new Thread(() =>
            {
                items = AsyncDataProvider.GetDomainOUTree(_sessionAD, ref errorMsg);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DomainOUTreeView.ItemsSource = items;

                    Binding binding = new Binding();
                    binding.Source = DomainOUTreeView; // установить в качестве source object значение ElementName
                    binding.Path = new PropertyPath("SelectedItem.Description");
                    binding.Mode = BindingMode.OneWay;
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    binding.Converter = new MoveUserTreeBtEnableConverter();
                    BindingOperations.SetBinding(btMove, Button.IsEnabledProperty, binding);

                    
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        DomainOUTreeView.Focus();
                    }
                }));
            }).Start();
        }
        #endregion

        
    }
}
