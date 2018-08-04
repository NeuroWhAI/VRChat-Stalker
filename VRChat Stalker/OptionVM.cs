using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace VRChat_Stalker
{
    public class OptionVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public OptionVM()
        {
#if DEBUG
            Option = new ProgramOption();
#endif
        }

        private ProgramOption m_option = null;
        public ProgramOption Option
        {
            get { return m_option; }
            set
            {
                m_option = value;

                OnPropertyChanged("Option");
                OnPropertyChanged("Theme");
                OnPropertyChanged("StartWhenBoot");
                OnPropertyChanged("PlaySound");
                OnPropertyChanged("UpdateCycle");
                OnPropertyChanged("UpdateCycleText");
            }
        }

        private bool m_needRestart = false;
        public bool NeedRestart
        {
            get => m_needRestart;
            set
            {
                m_needRestart = value;

                OnPropertyChanged("NeedRestart");
            }
        }

        public string Theme
        {
            get => Option.Theme;
            set
            {
                if (value == "Light" || value == "Dark")
                {
                    if (Option.Theme != value)
                    {
                        Option.Theme = value;

                        NeedRestart = true;
                    }

                    OnPropertyChanged("Theme");
                }
            }
        }

        public bool StartWhenBoot
        {
            get => Option.StartWhenBoot;
            set
            {
                Option.StartWhenBoot = value;

                OnPropertyChanged("StartWhenBoot");
            }
        }

        public bool PlaySound
        {
            get => Option.PlaySound;
            set
            {
                Option.PlaySound = value;

                OnPropertyChanged("PlaySound");
            }
        }

        public int UpdateCycle
        {
            get => Option.UpdateCycle;
            set
            {
                Option.UpdateCycle = value;

                OnPropertyChanged("UpdateCycle");
                OnPropertyChanged("UpdateCycleText");
            }
        }

        public string UpdateCycleText => string.Format("{0}m {1}s", UpdateCycle / 60, UpdateCycle % 60);

        public void Close()
        {
            this.Option.Save();
        }
    }
}
