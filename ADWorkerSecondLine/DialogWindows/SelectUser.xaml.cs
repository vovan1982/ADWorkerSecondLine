using ADWorkerSecondLine.DataProvider;
using ADWorkerSecondLine.Model;
using System;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для SelectUser.xaml
    /// </summary>
    public partial class SelectUser : Window
    {
        // Выбранный пользователь
        public string  selectedUserDN { get; set; }

        // Конструктор
        public SelectUser(string user, string fielForSearch, DirectoryEntry entry, PrincipalContext context)
        {
            InitializeComponent();
            Title = "Найдено несколько пользователей " + user;
            selectedUserDN = "";
            ReadOnlyCollection<User> items;
            string errorMsg = "";

            ListUsersForSelected.ItemsSource = null;
            Filter.IsEnabled = false;
            Filter.Text = "";
            new Thread(() =>
            {
                items = AsyncDataProvider.GetUsersForSelected(fielForSearch, user, context, entry, ref errorMsg);
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
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }).Start();
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
        // Нажета кнопка выбора
        private void btSelect_Click(object sender, RoutedEventArgs e)
        {
            selectedUserDN = ((User)ListUsersForSelected.SelectedItem).PlaceInAD;
            DialogResult = true;
            Close();
        }
        // Нажата кнопка отмены
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        // Событие изменения текста в фильтре найденных пользователей
        private void Filter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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
        // Фильтр найденных пользователей
        private bool FindedUsers_Filter(object item)
        {
            if (String.IsNullOrEmpty(Filter.Text))
                return true;

            var user = (User)item;

            return (user.NameInAD.ToUpper().Contains(Filter.Text.ToUpper()));
        }
        // Выполненно двойное нажатие левой кнопки мышки на елементе списка пользователей для выбора
        private void ListUsersForSelectedItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btSelect_Click(sender, e);
        }
    }
}
