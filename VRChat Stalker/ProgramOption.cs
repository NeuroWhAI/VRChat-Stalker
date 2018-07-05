using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VRChat_Stalker
{
    public class ProgramOption
    {
        public bool StartWhenBoot { get; set; } = false;
        public bool PlaySound { get; set; } = true;
        public int UpdateCycle { get; set; } = 8;

        public void Load()
        {
            if (File.Exists("option.dat") == false)
            {
                return;
            }

            try
            {
                using (var br = new BinaryReader(new FileStream("option.dat", FileMode.Open)))
                {
                    Version fileVersion = Version.Parse(br.ReadString());

                    StartWhenBoot = br.ReadBoolean();
                    PlaySound = br.ReadBoolean();
                    UpdateCycle = br.ReadInt32();


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
