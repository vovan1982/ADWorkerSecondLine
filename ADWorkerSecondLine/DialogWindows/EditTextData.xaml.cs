using System;
using System.DirectoryServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для EditTextData.xaml
    /// </summary>
    public partial class EditTextData : Window
    {
        #region Поля
        private DirectoryEntry _entryToUpdate; // Запись для редактирования в АД 
        private SearchResult _searchResults; // Результат поиска в АД
        private string _propertyName; // параметр в АД для редактирования
        private string _currentValue; // текущее значение изменяемого параметра
        private string _mode; // режим работы
        #endregion

        #region Конструктор
        public EditTextData(string title, string objectClass, string sаmaccountname, string propertyName, string currentValue, DirectoryEntry entry, int maxLen = 64, string mode = "Default")
        {
            InitializeComponent();
            Title = title;
            _propertyName = propertyName;
            _currentValue = currentValue;
            _mode = mode;
            data.MaxLength = maxLen;
            data.Text = currentValue;
            DirectorySearcher dirSearcher = new DirectorySearcher(entry);
            dirSearcher.SearchScope = SearchScope.Subtree;
            dirSearcher.Filter = string.Format("(&(objectClass={1})(sAMAccountName={0}))", sаmaccountname, objectClass);
            dirSearcher.PropertiesToLoad.Add(propertyName);
            if(mode == "login")
                dirSearcher.PropertiesToLoad.Add("userPrincipalName");
            if(mode == "name")
                dirSearcher.PropertiesToLoad.Add("cn");
            _searchResults = dirSearcher.FindOne();
            if (_searchResults != null)
            {
                _entryToUpdate = _searchResults.GetDirectoryEntry();
            }
            else
            {
                MessageBox.Show("Не удалось получить информацию по текущему полю в АД!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                Close();
            }
            data.Focus();
        }
        #endregion

        #region События
        // Нажата кнопка сохранения
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(data.Text))
                {
                    if (_searchResults.Properties.Contains("" + _propertyName + ""))
                    {
                        if (_mode == "name")
                        {
                            _entryToUpdate.Rename("CN=" + data.Text);
                        }
                        else
                        {
                            _entryToUpdate.Properties["" + _propertyName + ""].Value = data.Text;
                        }
                        
                        if (_mode == "login")
                        {
                            string[] upn = ((string)_entryToUpdate.Properties["userPrincipalName"].Value).Split('@');
                            _entryToUpdate.Properties["userPrincipalName"].Value = data.Text + "@" + upn[1];
                        }
                    }
                    else
                    {
                        _entryToUpdate.Properties["" + _propertyName + ""].Add(data.Text);
                    }
                    _entryToUpdate.CommitChanges();
                    DialogResult = true;
                    Close();
                }
                else
                {
                    if (_mode != "login" && _mode != "name")
                    {
                        if (_searchResults.Properties.Contains("" + _propertyName + ""))
                        {
                            _entryToUpdate.Properties["" + _propertyName + ""].Clear();
                        }
                        _entryToUpdate.CommitChanges();
                        _entryToUpdate.Close();
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Поле не может быть пустым!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Нажата кнопка отмены
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        // Изменено значение текстового поля
        private void data_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (data.Text != _currentValue)
            {
                btSave.IsEnabled = true;
            }
            else
            {
                btSave.IsEnabled = false;
            }
        }
        // Проверка нажатых кнопок в поле data, если нажата Enter вызвать событие сохранения
        private void data_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btSave_Click(this, new RoutedEventArgs());
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

        
    }
}
