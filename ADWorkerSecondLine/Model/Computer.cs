using System.Collections.Generic;
using System.ComponentModel;

namespace ADWorkerSecondLine.Model
{
    public class Computer : INotifyPropertyChanged
    {
        private string _placeInAD;
        private string _name;
        private string _dnsName;
        private string _description;
        private string _place;
        private string _os;
        private bool _pcIsDisable;

        public Computer()
        {

        }

        public string PlaceInAD
        {
            get { return _placeInAD; }
            set 
            { 
                _placeInAD = value;
                OnPropertyChanged("PlaceInAD");
            }
        }

        public string Name
        {
            get { return _name; }
            set 
            { 
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string DnsName
        {
            get { return _dnsName; }
            set 
            { 
                _dnsName = value;
                OnPropertyChanged("DnsName");
            }
        }

        public string Description
        {
            get { return _description; }
            set 
            { 
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        public string Place
        {
            get { return _place; }
            set 
            { 
                _place = value;
                OnPropertyChanged("Place");
            }
        }

        public string Os
        {
            get { return _os; }
            set 
            { 
                _os = value;
                OnPropertyChanged("Place");
            }
        }

        public bool PcIsDisable
        {
            get { return _pcIsDisable; }
            set 
            { 
                _pcIsDisable = value;
                OnPropertyChanged("PcIsDisable");
            }
        }

        public void copyData(Computer compForCopy)
        {
            PlaceInAD = compForCopy.PlaceInAD;
            Name = compForCopy.Name;
            DnsName = compForCopy.DnsName;
            Description = compForCopy.Description;
            Place = compForCopy.Place;
            Os = compForCopy.Os;
            PcIsDisable = compForCopy.PcIsDisable;
        }

        #region реализация INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
