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

        public async Task<WorldResponse> GetWorld(VRChatApi.VRChatApi vrc, string id)
        {
            lock (m_lockCache)
            {
                if (m_cache.ContainsKey(id))
                {
                    return m_cache[id];
                }
            }

            WorldResponse res = await vrc.WorldApi.Get(id);

            if (res != null)
            {
                lock (m_lockCache)
                {
                    m_cache[id] = res;
                }
            }

            return res;
        }

        public async Task<WorldInstanceResponse> GetInstance(VRChatApi.VRChatApi vrc, string worldId, string instanceId)
        {
            WorldInstanceResponse res = await vrc.WorldApi.GetInstance(worldId, instanceId);

            return res;
        }
    }
}
