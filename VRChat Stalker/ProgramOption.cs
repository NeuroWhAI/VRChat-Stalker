using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;

namespace VRChat_Stalker
{
    public class ProgramOption
    {
        private bool m_startWhenBoot = false;
        public bool StartWhenBoot
        {
            get => m_startWhenBoot;
            set
            {
                try
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                    if (value)
                    {
                        key.SetValue("VRChat Stalker", System.Reflection.Assembly.GetExecutingAssembly().Location);
                    }
                    else
                    {
                        key.DeleteValue("VRChat Stalker", false);
                    }

                    m_startWhenBoot = value;
                }
                catch (Exception)
                {
#if DEBUG
                    throw;
#endif
                }
            }
        }

        public bool PlaySound { get; set; } = true;
        public int UpdateCycle { get; set; } = 60;
        public FilterTypes FilterType { get; set; } = FilterTypes.All;
        public string Theme { get; set; } = "Dark";

        public void Load()
        {
            if (File.Exists("option.dat") == false)
            {
                return;
            }

            try
            {
                var filterVersion = new Version(1, 0, 9, 0);
                var themeVersion = new Version(1, 1, 0, 0);

                using (var br = new BinaryReader(new FileStream("option.dat", FileMode.Open)))
                {
                    Version fileVersion = Version.Parse(br.ReadString());

                    StartWhenBoot = br.ReadBoolean();
                    PlaySound = br.ReadBoolean();
                    UpdateCycle = br.ReadInt32();

                    if (UpdateCycle < 60)
                    {
                        UpdateCycle = 60;
                    }

                    if (fileVersion >= filterVersion)
                    {
                        FilterType = (FilterTypes)br.ReadInt32();
                    }

                    if (fileVersion >= themeVersion)
                    {
                        Theme = br.ReadString();
                    }


                    br.Close();
                }
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        public void Save()
        {
            try
            {
                using (var bw = new BinaryWriter(new FileStream("_option.dat", FileMode.Create)))
                {
                    bw.Write(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

                    bw.Write(StartWhenBoot);
                    bw.Write(PlaySound);
                    bw.Write(UpdateCycle);
                    bw.Write((int)FilterType);
                    bw.Write(Theme);


                    bw.Close();
                }

                File.Copy("_option.dat", "option.dat", true);
                File.Delete("_option.dat");
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }
    }
}
