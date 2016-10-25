using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ADWorkerSecondLine.Model;
using ADWorkerSecondLine.CustomEventArgs;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Windows.Documents;

namespace ADWorkerSecondLine.DataProvider
{
    public static class AsyncDataProvider
    {
        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        public static event MessageEventHandler messageEvent;

        #region Методы получения информации для вкладки "Пользователи"
        // Получить пустой список пользователей
        public static ReadOnlyCollection<User> GetItems()
        {
            return new ReadOnlyCollection<User>(new List<User>().AsReadOnly());
        }
        // Получить список пользователей согласно фильтра
        public static ReadOnlyCollection<User> GetItems(long maxPwdDay, string fieldForSearch, string searchStr, PrincipalContext context, DirectoryEntry entry, ref string errorMsg)
        {
            if (string.IsNullOrWhiteSpace(fieldForSearch) || string.IsNullOrWhiteSpace(searchStr))
            {
                return new ReadOnlyCollection<User>(new List<User>().AsReadOnly());
            }
            List<User> items = new List<User>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                if (fieldForSearch == "Default")
                    dirSearcher.Filter = string.Format("(&(objectClass=user)(!(objectClass=computer))(|(cn=*{0}*)(sn=*{0}*)(givenName=*{0}*)(sAMAccountName=*{0}*)))", searchStr.Trim());
                else
                    dirSearcher.Filter = string.Format("(&(objectClass=user)(!(objectClass=computer))(|({1}=*{0}*)))", searchStr.Trim(), fieldForSearch);
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                dirSearcher.PropertiesToLoad.Add("name");
                dirSearcher.PropertiesToLoad.Add("title");
                dirSearcher.PropertiesToLoad.Add("department");
                dirSearcher.PropertiesToLoad.Add("l");
                dirSearcher.PropertiesToLoad.Add("mobile");
                dirSearcher.PropertiesToLoad.Add("company");
                dirSearcher.PropertiesToLoad.Add("streetAddress");
                dirSearcher.PropertiesToLoad.Add("pwdLastSet");
                if (messageEvent != null)
                    messageEvent(null, new MessageEventArgs("Выполняется запрос к АД..."));
                SearchResultCollection searchResults = dirSearcher.FindAll();
                for (int i = 0; i < searchResults.Count; i++)
                {
                    SearchResult sr = searchResults[i];
                    if (MainWindow.StopSearchProcess)
                        break;
                    if (messageEvent != null)
                        messageEvent(null, new MessageEventArgs(string.Format("Найдено {0} записей. Обработка {1}/{0}. Для отмены обработки нажмите Esc...", searchResults.Count, i+1)));
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, (string)sr.Properties["sAMAccountName"][0]);
                    List<Computer> comps = GetCompUserFromAD((string)sr.Properties["sAMAccountName"][0], entry, ref errorMsg);
                    items.Add(new User
                    {
                        PlaceInAD = user.DistinguishedName,
                        Name = user.GivenName,
                        Surname = user.Surname,
                        DisplayName = user.DisplayName,
                        Login = (string)sr.Properties["sAMAccountName"][0],
                        NameInAD = (string)sr.Properties["name"][0],
                        Post = (sr.Properties.Contains("title") ? (string)sr.Properties["title"][0] : ""),
                        PhoneInt = user.VoiceTelephoneNumber,
                        PhoneMob = (sr.Properties.Contains("mobile") ? (string)sr.Properties["mobile"][0] : ""),
                        City = (sr.Properties.Contains("l") ? (string)sr.Properties["l"][0] : ""),
                        Department = (sr.Properties.Contains("department") ? (string)sr.Properties["department"][0] : ""),
                        Mail = user.EmailAddress,
                        Organization = (sr.Properties.Contains("company") ? (string)sr.Properties["company"][0] : ""),
                        Adress = (sr.Properties.Contains("streetAddress") ? (string)sr.Properties["streetAddress"][0] : ""),
                        PassExpireDate = user.PasswordNeverExpires ? new DateTime(1970, 01, 01, 00, 00, 00) : DateTime.FromFileTime((Int64)sr.Properties["PwdLastSet"][0]).AddDays(maxPwdDay),
                        AccountExpireDate = user.AccountExpirationDate,
                        PassMustBeChange = (user.LastPasswordSet == null),
                        AccountIsDisable = (user.Enabled == false ? true : false),
                        AccountIsLock = user.IsAccountLockedOut(),
                        Computers = comps
                    });
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return items.AsReadOnly();
        }
        // Получить список пользователей для выбора в окне добавления пользователей в группу
        public static ReadOnlyCollection<User> GetUsersForSelected(string fieldForSearch, string searchStr, PrincipalContext context, DirectoryEntry entry, ref string errorMsg)
        {
            if (string.IsNullOrWhiteSpace(fieldForSearch) || string.IsNullOrWhiteSpace(searchStr))
            {
                return new ReadOnlyCollection<User>(new List<User>().AsReadOnly());
            }
            List<User> items = new List<User>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                if (fieldForSearch == "Default")
                    dirSearcher.Filter = string.Format("(&(objectClass=user)(!(objectClass=computer))(|(cn=*{0}*)(sn=*{0}*)(givenName=*{0}*)(sAMAccountName=*{0}*)))", searchStr.Trim());
                else
                    dirSearcher.Filter = string.Format("(&(objectClass=user)(!(objectClass=computer))(|({1}=*{0}*)))", searchStr.Trim(), fieldForSearch);
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                dirSearcher.PropertiesToLoad.Add("name");
                dirSearcher.PropertiesToLoad.Add("title");
                dirSearcher.PropertiesToLoad.Add("department");
                dirSearcher.PropertiesToLoad.Add("l");
                dirSearcher.PropertiesToLoad.Add("mobile");
                dirSearcher.PropertiesToLoad.Add("company");
                dirSearcher.PropertiesToLoad.Add("streetAddress");
                dirSearcher.PropertiesToLoad.Add("pwdLastSet");
                SearchResultCollection searchResults = dirSearcher.FindAll();
                foreach (SearchResult sr in searchResults)
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, (string)sr.Properties["sAMAccountName"][0]);
                    items.Add(new User
                    {
                        PlaceInAD = user.DistinguishedName,
                        Name = user.GivenName,
                        Surname = user.Surname,
                        DisplayName = user.DisplayName,
                        Login = (string)sr.Properties["sAMAccountName"][0],
                        NameInAD = (string)sr.Properties["name"][0],
                        Post = (sr.Properties.Contains("title") ? (string)sr.Properties["title"][0] : ""),
                        PhoneInt = user.VoiceTelephoneNumber,
                        PhoneMob = (sr.Properties.Contains("mobile") ? (string)sr.Properties["mobile"][0] : ""),
                        City = (sr.Properties.Contains("l") ? (string)sr.Properties["l"][0] : ""),
                        Department = (sr.Properties.Contains("department") ? (string)sr.Properties["department"][0] : ""),
                        Mail = user.EmailAddress,
                        Organization = (sr.Properties.Contains("company") ? (string)sr.Properties["company"][0] : ""),
                        Adress = (sr.Properties.Contains("streetAddress") ? (string)sr.Properties["streetAddress"][0] : ""),
                        AccountIsDisable = (user.Enabled == false ? true : false),
                    });
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return items.AsReadOnly();
        }
        // Получить список только заблокированных пользователей
        public static ReadOnlyCollection<User> GetItemsOnlyLock(long maxPwdDay, PrincipalContext context, DirectoryEntry entry, ref string errorMsg)
        {
            List<User> items = new List<User>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = "(&(objectClass=User)(!(objectClass=computer))(LockoutTime>=1)(!(userAccountControl=514))(!(userAccountControl=546))(!(userAccountControl=66050))(!(userAccountControl=66082)))";
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                dirSearcher.PropertiesToLoad.Add("name");
                dirSearcher.PropertiesToLoad.Add("title");
                dirSearcher.PropertiesToLoad.Add("department");
                dirSearcher.PropertiesToLoad.Add("l");
                dirSearcher.PropertiesToLoad.Add("mobile");
                dirSearcher.PropertiesToLoad.Add("company");
                dirSearcher.PropertiesToLoad.Add("streetAddress");
                dirSearcher.PropertiesToLoad.Add("pwdLastSet");
                if (messageEvent != null)
                    messageEvent(null, new MessageEventArgs("Выполняется запрос к АД..."));
                SearchResultCollection searchResults = dirSearcher.FindAll();
                for (int i = 0; i < searchResults.Count; i++)
                {
                    SearchResult sr = searchResults[i];
                    if (MainWindow.StopSearchProcess)
                        break;
                    if (messageEvent != null)
                        messageEvent(null, new MessageEventArgs(string.Format("Найдено {0} записей. Обработка {1}/{0}. Для отмены обработки нажмите Esc...", searchResults.Count, i + 1)));
                    if (UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, (string)sr.Properties["sAMAccountName"][0]).IsAccountLockedOut())
                    {
                        UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, (string)sr.Properties["sAMAccountName"][0]);
                        List<Computer> comps = GetCompUserFromAD((string)sr.Properties["sAMAccountName"][0], entry, ref errorMsg);
                        items.Add(new User
                        {
                            PlaceInAD = user.DistinguishedName,
                            Name = user.GivenName,
                            Surname = user.Surname,
                            DisplayName = user.DisplayName,
                            Login = (string)sr.Properties["sAMAccountName"][0],
                            NameInAD = (string)sr.Properties["name"][0],
                            Post = (sr.Properties.Contains("title") ? (string)sr.Properties["title"][0] : ""),
                            PhoneInt = user.VoiceTelephoneNumber,
                            PhoneMob = (sr.Properties.Contains("mobile") ? (string)sr.Properties["mobile"][0] : ""),
                            City = (sr.Properties.Contains("l") ? (string)sr.Properties["l"][0] : ""),
                            Department = (sr.Properties.Contains("department") ? (string)sr.Properties["department"][0] : ""),
                            Mail = user.EmailAddress,
                            Organization = (sr.Properties.Contains("company") ? (string)sr.Properties["company"][0] : ""),
                            Adress = (sr.Properties.Contains("streetAddress") ? (string)sr.Properties["streetAddress"][0] : ""),
                            PassExpireDate = user.PasswordNeverExpires ? new DateTime(1970, 01, 01, 00, 00, 00) : DateTime.FromFileTime((Int64)sr.Properties["PwdLastSet"][0]).AddDays(maxPwdDay),
                            AccountExpireDate = user.AccountExpirationDate,
                            PassMustBeChange = (user.LastPasswordSet == null),
                            AccountIsDisable = (user.Enabled == false ? true : false),
                            AccountIsLock = user.IsAccountLockedOut(),
                            Computers = comps
                        });
                    }
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return items.AsReadOnly();
        }
        // Получить список пользователей из указанной OU
        public static ReadOnlyCollection<User> GetItemsInOU(long maxPwdDay, string dnOUForSearch, PrincipalContext context, DirectoryEntry entry, ref string errorMsg)
        {
            List<User> items = new List<User>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(|(objectClass=organizationalUnit)(objectClass=organization)(cn=Users)(cn=Computers))(distinguishedName=" + dnOUForSearch + "))");
                SearchResult searchResult = dirSearcher.FindOne();
                DirectoryEntry ouObjectForSearch = searchResult.GetDirectoryEntry();

                dirSearcher = new DirectorySearcher(ouObjectForSearch);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=user)(!(objectClass=computer)))"); ;
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                dirSearcher.PropertiesToLoad.Add("name");
                dirSearcher.PropertiesToLoad.Add("title");
                dirSearcher.PropertiesToLoad.Add("department");
                dirSearcher.PropertiesToLoad.Add("l");
                dirSearcher.PropertiesToLoad.Add("mobile");
                dirSearcher.PropertiesToLoad.Add("company");
                dirSearcher.PropertiesToLoad.Add("streetAddress");
                dirSearcher.PropertiesToLoad.Add("pwdLastSet");
                if (messageEvent != null)
                    messageEvent(null, new MessageEventArgs("Выполняется запрос к АД..."));
                SearchResultCollection searchResults = dirSearcher.FindAll();
                for (int i = 0; i < searchResults.Count; i++)
                {
                    SearchResult sr = searchResults[i];
                    if (MainWindow.StopSearchProcess)
                        break;
                    if (messageEvent != null)
                        messageEvent(null, new MessageEventArgs(string.Format("Найдено {0} записей. Обработка {1}/{0}. Для отмены обработки нажмите Esc...", searchResults.Count, i + 1)));
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, (string)sr.Properties["sAMAccountName"][0]);
                    List<Computer> comps = GetCompUserFromAD((string)sr.Properties["sAMAccountName"][0], entry, ref errorMsg);
                    items.Add(new User
                    {
                        PlaceInAD = user.DistinguishedName,
                        Name = user.GivenName,
                        Surname = user.Surname,
                        DisplayName = user.DisplayName,
                        Login = (string)sr.Properties["sAMAccountName"][0],
                        NameInAD = (string)sr.Properties["name"][0],
                        Post = (sr.Properties.Contains("title") ? (string)sr.Properties["title"][0] : ""),
                        PhoneInt = user.VoiceTelephoneNumber,
                        PhoneMob = (sr.Properties.Contains("mobile") ? (string)sr.Properties["mobile"][0] : ""),
                        City = (sr.Properties.Contains("l") ? (string)sr.Properties["l"][0] : ""),
                        Department = (sr.Properties.Contains("department") ? (string)sr.Properties["department"][0] : ""),
                        Mail = user.EmailAddress,
                        Organization = (sr.Properties.Contains("company") ? (string)sr.Properties["company"][0] : ""),
                        Adress = (sr.Properties.Contains("streetAddress") ? (string)sr.Properties["streetAddress"][0] : ""),
                        PassExpireDate = user.PasswordNeverExpires ? new DateTime(1970, 01, 01, 00, 00, 00) : DateTime.FromFileTime((Int64)sr.Properties["PwdLastSet"][0]).AddDays(maxPwdDay),
                        AccountExpireDate = user.AccountExpirationDate,
                        PassMustBeChange = (user.LastPasswordSet == null),
                        AccountIsDisable = (user.Enabled == false ? true : false),
                        AccountIsLock = user.IsAccountLockedOut(),
                        Computers = comps
                    });
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return items.AsReadOnly();
        }
        // Получить список компьютеров на которые заходил пользователь
        private static List<Computer> GetCompUserFromAD(string sаmaccountname, DirectoryEntry entry, ref string errorMsg)
        {
            List<Computer> result = new List<Computer>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=Computer)(description={0}))", sаmaccountname);
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
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return result;
        }
        // Получить пустой список групп
        public static ReadOnlyCollection<Group> GetGroupItems()
        {
            return new ReadOnlyCollection<Group>(new List<Group>().AsReadOnly());
        }
        // Получить список всех групп в домене для выбора
        public static ReadOnlyCollection<Group> GetGroupForSelected(DirectoryEntry entry, ref string errorMsg)
        {
            List<Group> result = new List<Group>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.SizeLimit = 10000;
                dirSearcher.PageSize = 10000;
                dirSearcher.Filter = string.Format("objectClass=group");
                dirSearcher.PropertiesToLoad.Add("distinguishedName");
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                dirSearcher.PropertiesToLoad.Add("description");
                SearchResultCollection searchResults = dirSearcher.FindAll();
                foreach (SearchResult sr in searchResults)
                {
                    result.Add(new Group
                    {
                        PlaceInAD = (string)sr.Properties["distinguishedName"][0],
                        Name = (string)sr.Properties["sAMAccountName"][0],
                        Description = (sr.Properties.Contains("description") ? (string)sr.Properties["description"][0] : ""),
                    });
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return result.AsReadOnly();
        }
        // Получить список групп пользователя
        public static ReadOnlyCollection<Group> GetGroupItems(string distinguishedName, DirectoryEntry entry, ref string errorMsg)
        {
            List<Group> result = new List<Group>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(objectClass=group)(member={0}))", distinguishedName);
                dirSearcher.PropertiesToLoad.Add("distinguishedName");
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                dirSearcher.PropertiesToLoad.Add("description");
                dirSearcher.PropertiesToLoad.Add("groupType");
                dirSearcher.PropertiesToLoad.Add("mail");
                dirSearcher.PropertiesToLoad.Add("member");
                dirSearcher.PropertiesToLoad.Add("memberOf");
                SearchResultCollection searchResults = dirSearcher.FindAll();
                foreach (SearchResult sr in searchResults)
                {
                    List<string> member = new List<string>();
                    List<string> memberOf = new List<string>();
                    if (sr.Properties.Contains("member"))
                    {
                        for (int i = 0; i < sr.Properties["member"].Count; i++) member.Add(sr.Properties["member"][i].ToString());
                    }
                    if (sr.Properties.Contains("memberOf"))
                    {
                        for (int i = 0; i < sr.Properties["memberOf"].Count; i++) memberOf.Add(sr.Properties["memberOf"][i].ToString());
                    }
                    result.Add(new Group
                    {
                        PlaceInAD = (string)sr.Properties["distinguishedName"][0],
                        Name = (string)sr.Properties["sAMAccountName"][0],
                        Description = (sr.Properties.Contains("description") ? (string)sr.Properties["description"][0] : ""),
                        Type = (int)sr.Properties["groupType"][0],
                        Mail = (sr.Properties.Contains("mail") ? (string)sr.Properties["mail"][0] : ""),
                        Users = member,
                        InGroups = memberOf
                    });
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return result.AsReadOnly();
        }
        // Получить пустое дерево подразделений домена
        public static ReadOnlyCollection<DomainTreeItem> GetDomainOUTree()
        {
            return new ReadOnlyCollection<DomainTreeItem>(new List<DomainTreeItem>().AsReadOnly());
        }
        // Получить дерево подразделений домена
        public static ReadOnlyCollection<DomainTreeItem> GetDomainOUTree(DirectoryEntry entry, ref string errorMsg)
        {
            List<DomainTreeItem> result = new List<DomainTreeItem>();
            DomainTreeItem rootNode = null;
            DomainTreeItem treeNode = null;
            DomainTreeItem curNode = null;
            string domain = "";
            string domainTag = "";
            string curTag = "";

            try
            {
                DirectorySearcher searchAD = new DirectorySearcher(entry);
                searchAD.Filter = "distinguishedName=*";
                SearchResult resultsAD = searchAD.FindOne();
                string domainDN = (string)resultsAD.Properties["distinguishedName"][0];

                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(|(objectClass=organizationalUnit)(objectClass=organization)(cn=Users)(cn=Computers))");
                dirSearcher.PropertiesToLoad.Add("distinguishedName");
                dirSearcher.PropertiesToLoad.Add("l");
                SearchResultCollection searchResults = dirSearcher.FindAll();

                #region Создане корневого элемента дерева
                foreach (string dom in domainDN.Split(','))
                {
                    if (domain.Length > 0)
                    {
                        domain += "." + dom.Split('=')[1];
                        domainTag += "," + dom;
                    }
                    else
                    {
                        domain += dom.Split('=')[1];
                        domainTag += dom;
                    }
                }
                rootNode = new DomainTreeItem() { Title = domain, Description = domainTag, Image = @"/ADWorkerSecondLine;component/Resources/ADServer.ico", IsSelected = true, IsExpanded = true };
                result.Add(rootNode);
                #endregion

                #region Обработка списка входных данных которые состоят из dn записей OU домена
                foreach (SearchResult sr in searchResults)
                {
                    // Разбиваем dn запись на элементы для дальнейшей обработки, в качестве разделителя используется запятая
                    string[] arrdata = ((string)sr.Properties["distinguishedName"][0]).Split(',');
                    // Получаем город dn записи
                    string city = (sr.Properties.Contains("l") ? (string)sr.Properties["l"][0] : "");
                    // Домен каждый раз получаем из dn записи
                    domain = "";
                    domainTag = "";

                    #region Получение домена из dn записи
                    foreach (string dom in arrdata)
                    {
                        if (dom.StartsWith("DC"))
                        {
                            if (domain.Length > 0)
                            {
                                domain += "." + dom.Split('=')[1];
                                domainTag += "," + dom;
                            }
                            else
                            {
                                domain += dom.Split('=')[1];
                                domainTag += dom;
                            }
                        }
                    }
                    #endregion

                    //Обнуляем дерево dn записи
                    treeNode = null;

                    #region Проверяем совпадает ли домен полученый из dn записи с доменом по умолчанию, совпадает - берем домен по умолчанию, не совпадает создаём дочерний домен
                    if (rootNode.Description == domainTag)
                    {
                        treeNode = rootNode;
                        curTag = rootNode.Description;
                    }
                    else
                    {
                        foreach (DomainTreeItem tvi in rootNode.Childs)
                        {
                            if (tvi.Description == domainTag)
                            {
                                treeNode = tvi;
                                curTag = tvi.Description;
                                break;
                            }
                        }
                        if (treeNode == null)
                        {
                            treeNode = new DomainTreeItem() { Title = domain, Description = domainTag, Image = @"/ADWorkerSecondLine;component/Resources/ADServer.ico", IsSelected = true, IsExpanded = true };
                            rootNode.Childs.Add(treeNode);
                            curTag = domainTag;
                        }
                    }
                    #endregion

                    //Обнуляем текущий элемент дерева
                    curNode = null;

                    #region Добавляем текущую dn запись в дерево домена, если какой либо ветки нет в домене она будет создана
                    //Получем количество элементов масива dn записи
                    int i = arrdata.Length - 1;
                    while (i >= 0)
                    {
                        //Если елемент массива начинается с DC, это часть имени домена, переходим к следующему элементу
                        if (arrdata[i].StartsWith("DC"))
                        {
                            i--;
                            continue;
                        }

                        //формируем текущий тег записи
                        curTag = arrdata[i] + "," + curTag;
                        //Обнуляем значение переменной нового элемента дерева
                        DomainTreeItem newNode = null;


                        if (curNode != null)
                        {
                            //Если текущий элемент дерева не пуст, значит ищем новый элемент дерева в нём
                            foreach (DomainTreeItem tvi in curNode.Childs)
                            {
                                //Если в текущем элементе дерева нашли новый элемент, значит делаем найденный элемент новым
                                if (tvi.Description == curTag)
                                {
                                    newNode = tvi;
                                    break;
                                }
                            }
                            //Если новый элемент дерева пуст значит создаём его
                            if (newNode == null)
                            {
                                newNode = new DomainTreeItem() { Title = arrdata[i].Split('=')[1], Description = curTag, City = city, Image = @"/ADWorkerSecondLine;component/Resources/ActiveDirectory.ico" };
                                curNode.Childs.Add(newNode);
                            }
                            //Делаем новый элемент текущим
                            curNode = newNode;
                        }
                        else
                        {
                            //Если текущий элемент дерева пуст, значит осущесвляем поиск нового элемента в корневом элементе дерева
                            foreach (DomainTreeItem tvi in treeNode.Childs)
                            {
                                //Если в корневом элементе дерева нашли новый элемент, значит делаем найденный элемент новым
                                if (tvi.Description == curTag)
                                {
                                    newNode = tvi;
                                    break;
                                }
                            }
                            //Если новый элемент дерева пуст значит создаём его
                            if (newNode == null)
                            {
                                newNode = new DomainTreeItem() { Title = arrdata[i].Split('=')[1], Description = curTag, City = city, Image = @"/ADWorkerSecondLine;component/Resources/ActiveDirectory.ico" };
                                treeNode.Childs.Add(newNode);
                            }
                            //Делаем новый элемент текущим
                            curNode = newNode;
                        }
                        //переходим к следующему элементу массива
                        i--;
                    }
                    #endregion

                }
                #endregion
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }

            return result.AsReadOnly();
        }
        #endregion

        #region Методы получения информации для вкладки "Компьютеры"
        // Получить пустой список компьютеров
        public static ReadOnlyCollection<Computer> GetPCItems()
        {
            return new ReadOnlyCollection<Computer>(new List<Computer>().AsReadOnly());
        }
        // Получить список компьютеров согласно фильтра
        public static ReadOnlyCollection<Computer> GetPCItems(string fieldForSearch, string searchStr, PrincipalContext context, DirectoryEntry entry, ref string errorMsg)
        {
            if (string.IsNullOrWhiteSpace(fieldForSearch) || string.IsNullOrWhiteSpace(searchStr))
            {
                return new ReadOnlyCollection<Computer>(new List<Computer>().AsReadOnly());
            }
            List<Computer> items = new List<Computer>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.SizeLimit = 10000;
                dirSearcher.PageSize = 10000;
                if (fieldForSearch == "Default")
                    dirSearcher.Filter = string.Format("(&(objectClass=computer)(|(cn=*{0}*)(name=*{0}*)(dNSHostName=*{0}*)))", searchStr.Trim());
                else
                    dirSearcher.Filter = string.Format("(&(objectClass=computer)(|({1}=*{0}*)))", searchStr.Trim(), fieldForSearch);
                dirSearcher.PropertiesToLoad.Add("distinguishedName");
                dirSearcher.PropertiesToLoad.Add("name");
                dirSearcher.PropertiesToLoad.Add("dNSHostName");
                dirSearcher.PropertiesToLoad.Add("description");
                dirSearcher.PropertiesToLoad.Add("location");
                dirSearcher.PropertiesToLoad.Add("operatingSystem");
                dirSearcher.PropertiesToLoad.Add("operatingSystemServicePack");
                dirSearcher.PropertiesToLoad.Add("operatingSystemVersion");
                if (messageEvent != null)
                    messageEvent(null, new MessageEventArgs("Выполняется запрос к АД..."));
                SearchResultCollection searchResults = dirSearcher.FindAll();
                for (int i = 0; i < searchResults.Count; i++)
                {
                    SearchResult sr = searchResults[i];
                    if (MainWindow.StopSearchProcess)
                        break;
                    if (messageEvent != null)
                        messageEvent(null, new MessageEventArgs(string.Format("Найдено {0} записей. Обработка {1}/{0}. Для отмены обработки нажмите Esc...", searchResults.Count, i + 1)));
                    ComputerPrincipal comp = ComputerPrincipal.FindByIdentity(context, IdentityType.DistinguishedName, (string)sr.Properties["distinguishedName"][0]);
                    items.Add(new Computer
                    {
                        PlaceInAD = comp.DistinguishedName,
                        Name = (string)sr.Properties["name"][0],
                        DnsName = (sr.Properties.Contains("dNSHostName") ? (string)sr.Properties["dNSHostName"][0] : ""),
                        Description = (sr.Properties.Contains("description") ? (string)sr.Properties["description"][0] : ""),
                        Place = (sr.Properties.Contains("location") ? (string)sr.Properties["location"][0] : ""),
                        Os = (sr.Properties.Contains("operatingSystem") ? (string)sr.Properties["operatingSystem"][0] : "") + " " +
                             (sr.Properties.Contains("operatingSystemServicePack") ? (string)sr.Properties["operatingSystemServicePack"][0] : "") + " " +
                             (sr.Properties.Contains("operatingSystemVersion") ? (string)sr.Properties["operatingSystemVersion"][0] : ""),
                        PcIsDisable = (comp.Enabled == false ? true : false)
                    });
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return items.AsReadOnly();
        }
        // Получить список компьютеров из указанной OU
        public static ReadOnlyCollection<Computer> GetPCItemsInOU(string dnOUForSearch, PrincipalContext context, DirectoryEntry entry, ref string errorMsg)
        {
            List<Computer> items = new List<Computer>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(|(objectClass=organizationalUnit)(objectClass=organization)(cn=Users)(cn=Computers))(distinguishedName=" + dnOUForSearch + "))");
                SearchResult searchResult = dirSearcher.FindOne();
                DirectoryEntry ouObjectForSearch = searchResult.GetDirectoryEntry();

                dirSearcher = new DirectorySearcher(ouObjectForSearch);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.SizeLimit = 10000;
                dirSearcher.PageSize = 10000;
                dirSearcher.Filter = string.Format("(&(objectClass=computer))"); ;
                dirSearcher.PropertiesToLoad.Add("distinguishedName");
                dirSearcher.PropertiesToLoad.Add("name");
                dirSearcher.PropertiesToLoad.Add("dNSHostName");
                dirSearcher.PropertiesToLoad.Add("description");
                dirSearcher.PropertiesToLoad.Add("location");
                dirSearcher.PropertiesToLoad.Add("operatingSystem");
                dirSearcher.PropertiesToLoad.Add("operatingSystemServicePack");
                dirSearcher.PropertiesToLoad.Add("operatingSystemVersion");
                if (messageEvent != null)
                    messageEvent(null, new MessageEventArgs("Выполняется запрос к АД..."));
                SearchResultCollection searchResults = dirSearcher.FindAll();
                for (int i = 0; i < searchResults.Count; i++)
                {
                    SearchResult sr = searchResults[i];
                    if (MainWindow.StopSearchProcess)
                        break;
                    if (messageEvent != null)
                        messageEvent(null, new MessageEventArgs(string.Format("Найдено {0} записей. Обработка {1}/{0}. Для отмены обработки нажмите Esc...", searchResults.Count, i + 1)));
                    ComputerPrincipal comp = ComputerPrincipal.FindByIdentity(context, IdentityType.DistinguishedName, (string)sr.Properties["distinguishedName"][0]);
                    items.Add(new Computer
                    {
                        PlaceInAD = comp.DistinguishedName,
                        Name = (string)sr.Properties["name"][0],
                        DnsName = (sr.Properties.Contains("dNSHostName") ? (string)sr.Properties["dNSHostName"][0] : ""),
                        Description = (sr.Properties.Contains("description") ? (string)sr.Properties["description"][0] : ""),
                        Place = (sr.Properties.Contains("location") ? (string)sr.Properties["location"][0] : ""),
                        Os = (sr.Properties.Contains("operatingSystem") ? (string)sr.Properties["operatingSystem"][0] : "") + " " +
                             (sr.Properties.Contains("operatingSystemServicePack") ? (string)sr.Properties["operatingSystemServicePack"][0] : "") + " " +
                             (sr.Properties.Contains("operatingSystemVersion") ? (string)sr.Properties["operatingSystemVersion"][0] : ""),
                        PcIsDisable = (comp.Enabled == false ? true : false)
                    });
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return items.AsReadOnly();
        }
        #endregion

        #region Методы получения информации для вкладки "Группы"
        // Получить пустой список групп
        public static ReadOnlyCollection<Group> GetGroupsItems()
        {
            return new ReadOnlyCollection<Group>(new List<Group>().AsReadOnly());
        }
        // Получить список групп согласно фильтра
        public static ReadOnlyCollection<Group> GetGroupsItems(string fieldForSearch, string searchStr, PrincipalContext context, DirectoryEntry entry, ref string errorMsg)
        {
            if (string.IsNullOrWhiteSpace(fieldForSearch) || string.IsNullOrWhiteSpace(searchStr))
            {
                return new ReadOnlyCollection<Group>(new List<Group>().AsReadOnly());
            }
            List<Group> items = new List<Group>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.SizeLimit = 10000;
                dirSearcher.PageSize = 10000;
                if (fieldForSearch == "Default")
                    dirSearcher.Filter = string.Format("(&(objectClass=group)(|(cn=*{0}*)(displayName=*{0}*)(name=*{0}*)(sAMAccountName=*{0}*)))", searchStr.Trim());
                else
                    dirSearcher.Filter = string.Format("(&(objectClass=group)(|({1}=*{0}*)))", searchStr.Trim(), fieldForSearch);
                dirSearcher.PropertiesToLoad.Add("distinguishedName");
                dirSearcher.PropertiesToLoad.Add("Name");
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                dirSearcher.PropertiesToLoad.Add("displayName");
                dirSearcher.PropertiesToLoad.Add("description");
                dirSearcher.PropertiesToLoad.Add("groupType");
                dirSearcher.PropertiesToLoad.Add("mail");
                dirSearcher.PropertiesToLoad.Add("member");
                dirSearcher.PropertiesToLoad.Add("memberOf");
                if (messageEvent != null)
                    messageEvent(null, new MessageEventArgs("Выполняется запрос к АД..."));
                SearchResultCollection searchResults = dirSearcher.FindAll();
                for (int i = 0; i < searchResults.Count; i++)
                {
                    SearchResult sr = searchResults[i];
                    if (MainWindow.StopSearchProcess)
                        break;
                    if (messageEvent != null)
                        messageEvent(null, new MessageEventArgs(string.Format("Найдено {0} записей. Обработка {1}/{0}. Для отмены обработки нажмите Esc...", searchResults.Count, i + 1)));
                    List<string> member = new List<string>();
                    List<string> memberOf = new List<string>();
                    if (sr.Properties.Contains("member"))
                    {
                        for (int j = 0; j < sr.Properties["member"].Count; j++) member.Add(sr.Properties["member"][j].ToString());
                    }
                    if (sr.Properties.Contains("memberOf"))
                    {
                        for (int j = 0; j < sr.Properties["memberOf"].Count; j++) memberOf.Add(sr.Properties["memberOf"][j].ToString());
                    }
                    items.Add(new Group
                    {
                        PlaceInAD = (string)sr.Properties["distinguishedName"][0],
                        Name = (string)sr.Properties["Name"][0],
                        NameWin = (string)sr.Properties["sAMAccountName"][0],
                        DisplayName = (sr.Properties.Contains("displayName") ? (string)sr.Properties["displayName"][0] : ""),
                        Description = (sr.Properties.Contains("description") ? (string)sr.Properties["description"][0] : ""),
                        Type = (int)sr.Properties["groupType"][0],
                        Mail = (sr.Properties.Contains("mail") ? (string)sr.Properties["mail"][0] : ""),
                        Users = member,
                        InGroups = memberOf
                    });
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return items.AsReadOnly();
        }
        // Получить список групп из указанной OU
        public static ReadOnlyCollection<Group> GetGroupsItemsInOU(string dnOUForSearch, PrincipalContext context, DirectoryEntry entry, ref string errorMsg)
        {
            List<Group> items = new List<Group>();
            try
            {
                DirectorySearcher dirSearcher = new DirectorySearcher(entry);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.Filter = string.Format("(&(|(objectClass=organizationalUnit)(objectClass=organization)(cn=Users)(cn=Computers))(distinguishedName=" + dnOUForSearch + "))");
                SearchResult searchResult = dirSearcher.FindOne();
                DirectoryEntry ouObjectForSearch = searchResult.GetDirectoryEntry();

                dirSearcher = new DirectorySearcher(ouObjectForSearch);
                dirSearcher.SearchScope = SearchScope.Subtree;
                dirSearcher.SizeLimit = 10000;
                dirSearcher.PageSize = 10000;
                dirSearcher.Filter = string.Format("(&(objectClass=group))"); ;
                dirSearcher.PropertiesToLoad.Add("distinguishedName");
                dirSearcher.PropertiesToLoad.Add("Name");
                dirSearcher.PropertiesToLoad.Add("sAMAccountName");
                dirSearcher.PropertiesToLoad.Add("displayName");
                dirSearcher.PropertiesToLoad.Add("description");
                dirSearcher.PropertiesToLoad.Add("groupType");
                dirSearcher.PropertiesToLoad.Add("mail");
                dirSearcher.PropertiesToLoad.Add("member");
                dirSearcher.PropertiesToLoad.Add("memberOf");
                if (messageEvent != null)
                    messageEvent(null, new MessageEventArgs("Выполняется запрос к АД..."));
                SearchResultCollection searchResults = dirSearcher.FindAll();
                for (int i = 0; i < searchResults.Count; i++)
                {
                    SearchResult sr = searchResults[i];
                    if (MainWindow.StopSearchProcess)
                        break;
                    if (messageEvent != null)
                        messageEvent(null, new MessageEventArgs(string.Format("Найдено {0} записей. Обработка {1}/{0}. Для отмены обработки нажмите Esc...", searchResults.Count, i + 1)));
                    List<string> member = new List<string>();
                    List<string> memberOf = new List<string>();
                    if (sr.Properties.Contains("member"))
                    {
                        for (int j = 0; j < sr.Properties["member"].Count; j++) member.Add(sr.Properties["member"][j].ToString());
                    }
                    if (sr.Properties.Contains("memberOf"))
                    {
                        for (int j = 0; j < sr.Properties["memberOf"].Count; j++) memberOf.Add(sr.Properties["memberOf"][j].ToString());
                    }
                    items.Add(new Group
                    {
                        PlaceInAD = (string)sr.Properties["distinguishedName"][0],
                        Name = (string)sr.Properties["Name"][0],
                        NameWin = (string)sr.Properties["sAMAccountName"][0],
                        DisplayName = (sr.Properties.Contains("displayName") ? (string)sr.Properties["displayName"][0] : ""),
                        Description = (sr.Properties.Contains("description") ? (string)sr.Properties["description"][0] : ""),
                        Type = (int)sr.Properties["groupType"][0],
                        Mail = (sr.Properties.Contains("mail") ? (string)sr.Properties["mail"][0] : ""),
                        Users = member,
                        InGroups = memberOf
                    });
                }
            }
            catch (Exception exp)
            {
                errorMsg = exp.ToString();
            }
            return items.AsReadOnly();
        }
        #endregion
    }
}
