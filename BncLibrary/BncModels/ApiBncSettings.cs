using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BncLibrary.BncModels
{
    public class ApiBncSettings
    {
        public string ClientGUID { get; set; } = string.Empty;
        public string MasterKey { get; set; } = string.Empty;
        public string BaseAddress { get; set; } = string.Empty;
    }
}
