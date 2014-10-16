using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AndroidDeviceConfig;

namespace de.sebastianrutofski.AndroidToolkit.Models
{
    public sealed class VersionModel : INotifyPropertyChanged
    {
        private ObservableCollection<ActionSetModel> _ActionSets;
        private ObservableCollection<RecoveryModel> _Recoveries;
        private DeviceVersion version;

        public VersionModel(DeviceVersion version)
        {
            this.version = version;
            _Recoveries = new ObservableCollection<RecoveryModel>();
            _Recoveries.CollectionChanged += recoveries_CollectionChanged;

            foreach (Recovery recovery in this.version.Recoveries)
            {
                _Recoveries.Add(new RecoveryModel(recovery));
            }

            ActionSets = new ObservableCollection<ActionSetModel>();
            ActionSets.CollectionChanged += actionSets_CollectionChanged;

            foreach (ActionSet actionSet in this.version.PossibleActions)
            {
                ActionSets.Add(new ActionSetModel(actionSet));
            }
        }

        public ObservableCollection<RecoveryModel> Recoveries
        {
            get { return _Recoveries; }
            set
            {
                if (Recoveries == value) return;
                _Recoveries = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ActionSetModel> ActionSets
        {
            get { return _ActionSets; }
            set
            {
                if (ActionSets == value) return;
                _ActionSets = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void actionSets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("ActionSets");
        }

        private void recoveries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Recoveries");
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}