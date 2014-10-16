using System.ComponentModel;
using System.Runtime.CompilerServices;
using RegawMOD.Android;

namespace de.sebastianrutofski.AndroidToolkit.Models
{
    public class DeviceModel : INotifyPropertyChanged
    {
        private ConfigModel _Config;
        private Device _Device;
        private VersionModel _Version;

        public ConfigModel Config
        {
            get { return _Config; }
            set
            {
                if (Config == value) return;
                _Config = value;
                OnPropertyChanged();
            }
        }

        public VersionModel Version
        {
            get { return _Version; }
            set
            {
                if (Version == value) return;
                _Version = value;
                OnPropertyChanged();
            }
        }

        public Device Device
        {
            get { return _Device; }
            set
            {
                if (Device == value) return;
                _Device = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}