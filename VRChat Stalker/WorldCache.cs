using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRChatApi.Classes;

namespace VRChat_Stalker
{
    public class WorldCache
    {
        private readonly object m_lockCache = new object();
        private Dictionary<string, WorldResponse> m_cache = new Dictionary<string, WorldResponse>();
        private readonly object m_lockInstCache = new object();
        private Dictionary<string, WorldInstanceResponse> m_instCache = new Dictionary<string, WorldInstanceResponse>();

        public async Task<WorldResponse> GetWorld(VRChatApi.VRChatApi vrc, string id)
        {
            lock (m_lockCache)
            {
                if (m_cache.ContainsKey(id))
                {
                    return m_cache[id];
                }
            }

            
            for (int retry = 0; retry < 3; ++retry)
            {
                try
                {
                    var res = await vrc.WorldApi.Get(id);
                    
                    if (res != null)
                    {
                        lock (m_lockCache)
                        {
                            m_cache[id] = res;
                        }

                        return res;
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);

                    await Task.Delay(1000);
                }
            }


            return null;
        }

        public async Task<WorldInstanceResponse> GetInstance(VRChatApi.VRChatApi vrc, string worldId, string instanceId)
        {
            string key = worldId + "&" + instanceId;


            for (int retry = 0; retry < 3; ++retry)
            {
                try
                {
                    var res = await vrc.WorldApi.GetInstance(worldId, instanceId);

                    if (res != null)
                    {
                        lock (m_lockInstCache)
                        {
                            m_instCache[key] = res;
                        }

                        return res;
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);

                    await Task.Delay(1000);
                }
            }


            lock (m_lockInstCache)
            {
                if (m_instCache.ContainsKey(key))
                {
                    return m_instCache[key];
                }
            }


            return null;
        }
    }
}
