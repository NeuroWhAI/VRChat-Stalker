using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;

namespace VRChat_Stalker
{
    public class LoginVM : INotifyPropertyChanged
    {
        private readonly string LoginFile = "login.dat";
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public LoginVM()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);


            AutoLogin = File.Exists(LoginFile);
        }

        private bool m_autoLogin = false;
        public bool AutoLogin
        {
            get { return m_autoLogin; }
            set
            {
                m_autoLogin = value;

                OnPropertyChanged("AutoLogin");

                if (value == false)
                {
                    File.Delete(LoginFile);
                }
            }
        }

        public async Task<VRChatApi.VRChatApi> Login()
        {
            if (File.Exists(LoginFile))
            {
                string id, pwd;

                // Load login data.
                using (var br = new BinaryReader(new FileStream(LoginFile, FileMode.Open)))
                {
                    id = br.ReadString();

                    int keyBytesLength = br.ReadInt32();
                    byte[] key = br.ReadBytes(keyBytesLength);

                    int pwdBytesLength = br.ReadInt32();
                    byte[] encryptedPwdBytes = br.ReadBytes(pwdBytesLength);

                    byte[] pwdBytes = ProtectedData.Unprotect(encryptedPwdBytes, key,
                        DataProtectionScope.CurrentUser);

                    pwd = Encoding.UTF8.GetString(pwdBytes);

                    br.Close();
                }

                return await Login(id, pwd);
            }


            return null;
        }

        public async Task<VRChatApi.VRChatApi> Login(string id, string pwd)
        {
            if (AutoLogin)
            {
                // Save login data.
                using (var bw = new BinaryWriter(new FileStream(LoginFile, FileMode.Create)))
                {
                    bw.Write(id);

                    byte[] pwdBytes = Encoding.UTF8.GetBytes(pwd);
                    byte[] key = new byte[20];
                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(key);
                    }

                    byte[] encryptedPwd = ProtectedData.Protect(pwdBytes, key,
                        DataProtectionScope.CurrentUser);

                    bw.Write(key.Length);
                    bw.Write(key);

                    bw.Write(encryptedPwd.Length);
                    bw.Write(encryptedPwd);

                    bw.Close();
                }
            }


            // Auth API
            var vrc = new VRChatApi.VRChatApi(id, pwd);
            await vrc.RemoteConfig.Get();

            // Login
            var loginResult = await vrc.UserApi.Login();


            if (loginResult == null)
            {
                return null;
            }

            return vrc;
        }
    }
}
