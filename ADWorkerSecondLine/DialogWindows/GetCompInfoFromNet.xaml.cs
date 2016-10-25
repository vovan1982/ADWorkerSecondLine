using System;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ADWorkerSecondLine.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для GetCompInfoFromNet.xaml
    /// </summary>
    public partial class GetCompInfoFromNet : Window
    {
        private bool _stopThread; // Признак завершения основного потока

        // Конструктор
        public GetCompInfoFromNet(string compName, string domain, string user, string pass)
        {
            InitializeComponent();
            namePC.Text = compName;
            _stopThread = false;
            Thread t = new Thread(() =>
            {
                try
                {
                    if (_stopThread) return;
                    #region Получение IP адреса
                    Dispatcher.BeginInvoke(new Action(() => { statusText.Text = "Отправка DNS запроса для получения IP Адреса..."; }));
                    IPAddress ip = new IPAddress(0);
                    try
                    {
                        foreach (IPAddress currrentIPAddress in Dns.GetHostAddresses(compName))
                        {
                            if (_stopThread) return;
                            if (currrentIPAddress.AddressFamily.ToString() == System.Net.Sockets.AddressFamily.InterNetwork.ToString())
                            {
                                ip = currrentIPAddress;
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    ipAdress.Text = ip.ToString();
                                }));
                                break;
                            }
                        }
                    }
                    catch
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ipAdress.Foreground = new SolidColorBrush(Colors.Red);
                            ipAdress.Text = "Не удалось получить IP адрес";
                            statusText.Text = "";
                        }));
                        return;
                    }
                    #endregion
                    
                    if (_stopThread) return;
                    #region Проверка состояния
                    Dispatcher.BeginInvoke(new Action(() => { statusText.Text = "Проверка состояния..."; }));
                    if (_stopThread) return;
                    if (!PingHost(ip.ToString()))
                    {
                        if (_stopThread) return;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            state.Foreground = new SolidColorBrush(Colors.Red);
                            state.Text = "Не в сети";
                            statusText.Text = "";
                        }));
                    }
                    else
                    {
                        if (_stopThread) return;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            state.Foreground = new SolidColorBrush(Colors.Green);
                            state.Text = "В сети";
                        }));
                    #endregion
                        if (_stopThread) return;
                        #region Получение модели
                        Dispatcher.BeginInvoke(new Action(() => { statusText.Text = "Получение модели компьютера..."; }));
                        ConnectionOptions connection = new ConnectionOptions();
                        connection.Username = user;
                        connection.Password = pass;
                        connection.Authority = "ntlmdomain:" + domain;
                        ManagementScope scope;
                        string localPCname = Environment.MachineName;
                        try
                        {
                            if (_stopThread) return;
                            if (localPCname == compName)
                            {
                                scope = new ManagementScope("\\\\.\\root\\CIMV2");
                            }
                            else
                            {
                                scope = new ManagementScope("\\\\" + compName + "\\root\\CIMV2", connection);
                            }
                            scope.Connect();

                            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem ");

                            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

                            ManagementObject queryObj = searcher.Get().OfType<ManagementObject>().FirstOrDefault();

                            if (_stopThread) return;
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                model.Text = queryObj["Model"].ToString(); ;
                                statusText.Text = "";
                            }));
                        }
                        catch
                        {
                            if (_stopThread) return;
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                model.Foreground = new SolidColorBrush(Colors.Red);
                                model.Text = "Не удалось получить модель компьютера";
                                statusText.Text = "";
                            }));
                            return;
                        }
                        #endregion
                        if (_stopThread) return;
                        #region Получение UpTime
                        Dispatcher.BeginInvoke(new Action(() => { statusText.Text = "Получение времени включения компьютера..."; }));
                        try
                        {
                            if (_stopThread) return;
                            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem ");
                            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                            ManagementObject queryObj = searcher.Get().OfType<ManagementObject>().FirstOrDefault();
                            if (_stopThread) return;
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                upTime.Text = (ManagementDateTimeConverter.ToDateTime(queryObj["LastBootUpTime"].ToString())).ToString("dd.MM.yyyy HH:mm:ss"); ;
                                statusText.Text = "";
                            }));
                        }
                        catch
                        {
                            if (_stopThread) return;
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                model.Foreground = new SolidColorBrush(Colors.Red);
                                model.Text = "Не удалось получить время последней загрузки компьютера";
                                statusText.Text = "";
                            }));
                            return;
                        }
                        #endregion
                        if (_stopThread) return;
                        #region Получение залогиненых пользователей
                        Dispatcher.BeginInvoke(new Action(() => { statusText.Text = "Получение загруженых пользователей..."; }));
                        try
                        {
                            ManagementClass registry = new ManagementClass(scope, new ManagementPath("StdRegProv"), null);
                            ManagementBaseObject inParams = registry.GetMethodParameters("EnumKey");
                            inParams["hDefKey"] = 0x80000003;//HKEY_USERS
                            inParams["sSubKeyName"] = "";

                            ManagementBaseObject outParams = registry.InvokeMethod("EnumKey", inParams, null);
                            string[] namesU = outParams["sNames"] as string[];

                            foreach (string s in namesU)
                            {
                                inParams = registry.GetMethodParameters("EnumKey");
                                inParams["hDefKey"] = 0x80000003;//HKEY_USERS
                                inParams["sSubKeyName"] = s;

                                outParams = registry.InvokeMethod("EnumKey", inParams, null);
                                string[] key = outParams["sNames"] as string[];
                                if (key != null)
                                {
                                    foreach (string k in key)
                                    {
                                        //Если писутствует ветка Volatile Environment значит это активный пользователь
                                        if (k == "Volatile Environment")
                                        {
                                            inParams = registry.GetMethodParameters("GetStringValue");
                                            inParams["hDefKey"] = 0x80000003;//HKEY_USERS
                                            inParams["sSubKeyName"] = s + @"\" + k;
                                            inParams["sValueName"] = "USERNAME";
                                            outParams = registry.InvokeMethod("GetStringValue", inParams, null);

                                            if (outParams.Properties["sValue"].Value != null)
                                            {
                                                Dispatcher.BeginInvoke(new Action(() =>
                                                {
                                                    try
                                                    {
                                                        listLoadedUsers.Items.Add(outParams.Properties["sValue"].Value.ToString());
                                                    }
                                                    catch
                                                    {
                                                        listLoadedUsers.Items.Add("Не удалось получить список загруженых пользователей...");
                                                        statusText.Text = "";
                                                    }
                                                }));
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                statusText.Text = "";
                            }));
                        }
                        catch
                        {
                            if (_stopThread) return;
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                listLoadedUsers.Items.Add("Не удалось получить список загруженых пользователей...");
                                statusText.Text = "";
                            }));
                            return;
                        }
                        #endregion
                    }
                }
                catch (Exception e)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        statusText.Text = "";
                        MessageBox.Show("Ошибка:" + e.ToString() + "!!!", "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                }
            });
            t.IsBackground = true;
            t.Start();
        }
        // Событие закрытия окна
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _stopThread = true;
        }
        // Метод проверки доступности PC путём пингования
        private bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                return pingable;
            }
            return pingable;
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
        // Выбран пункт меню копирования в буфер
        private void CopyBuffSelectUser_Click(object sender, RoutedEventArgs e)
        {
            if (listLoadedUsers.SelectedItem != null)
                Clipboard.SetDataObject((string)listLoadedUsers.SelectedItem);
        }
        // Нажата комбинация клавиш Ctrl+C для копирования в буфер
        private void listLoadedUsers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (listLoadedUsers.SelectedItem != null)
                    Clipboard.SetDataObject((string)listLoadedUsers.SelectedItem);
            }
        }
    }
}
