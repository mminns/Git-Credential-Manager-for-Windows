using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Bitbucket.Authentication.OAuth
{
    public class ProcessService : IProcessService
    {
        public Type ServiceType
            => typeof(IProcessService);

        public void StartProcess(string process)
        {
            Process.Start(process);
        }
    }
}
