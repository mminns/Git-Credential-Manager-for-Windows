using Microsoft.Alm.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Bitbucket.Authentication.OAuth
{
    public interface IProcessService : IRuntimeService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        void StartProcess(string process);
    }
}
