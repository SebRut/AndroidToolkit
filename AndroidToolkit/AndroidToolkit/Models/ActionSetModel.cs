using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AndroidDeviceConfig;

namespace de.sebastianrutofski.AndroidToolkit.Models
{
    public sealed class ActionSetModel : INotifyPropertyChanged
    {
        private readonly ActionSet _ActionSet;

        public ActionSetModel(ActionSet actionSet)
        {
            _ActionSet = actionSet;
        }

        public List<Action> Actions
        {
            get { return _ActionSet.Actions; }
        }

        public string Description
        {
            get { return _ActionSet.Description; }
            set
            {
                if (_ActionSet.Description == value) return;
                _ActionSet.Description = value;
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