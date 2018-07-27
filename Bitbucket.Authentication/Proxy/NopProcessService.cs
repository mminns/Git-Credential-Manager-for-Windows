using Atlassian.Bitbucket.Authentication.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Bitbucket.Authentication.Test
{
    public class NopProcessService : IProcessService
    {
        public Type ServiceType
            => typeof(IProcessService);

        public void StartProcess(string process)
        {
        }
    }
}
