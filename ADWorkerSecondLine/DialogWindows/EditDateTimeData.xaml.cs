using System;
using System.DirectoryServices.AccountManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для EditDateTimeData.xaml
    /// </summary>
    public partial class EditDateTimeData : Window
    {
        #region Поля
        private DateTime? _currentValue; // текущее значение изменяемого параметра
        private UserPrincipal userToModify; // Запись для редактирования в АД
        #endregion

        #region Конструктор
        public EditDateTimeData(string title, string sаmaccountname, DateTime? currentValue, PrincipalContext context)
        {
            InitializeComponent();
            Title = title;
            _currentValue = currentValue;
            data.SelectedDate = currentValue;
            userToModify = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, sаmaccountname);
            if (userToModify == null)
            {
                MessageBox.Show("Не удалось получить информацию по текущему пользователю в АД!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
        #endregion

        #region События
        // Изменено значение выбранной даты
        private void data_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (data.SelectedDate != _currentValue)
            {
                btSave.IsEnabled = true;
            }
            else
            {
                btSave.IsEnabled = false;
            }
        }
        // Нажата кнопка отмены
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        // Нажата кнопка сохранения
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (data.SelectedDate != null)
                {
                    userToModify.AccountExpirationDate = new DateTime(
                        data.SelectedDate.Value.Year,
                        data.SelectedDate.Value.Month,
                        data.SelectedDate.Value.Day,
                        21, 0, 0, 0);
                    userToModify.Save();
                }
                else
                {
                    userToModify.AccountExpirationDate = null;
                    userToModify.Save();
                }
                DialogResult = true;
                Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
