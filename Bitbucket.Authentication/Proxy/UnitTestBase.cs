using System.Runtime.CompilerServices;
using Atlassian.Bitbucket.Authentication.OAuth;
using GitHub.Shared.Api;
using Microsoft.Alm.Authentication.Test;

namespace Atlassian.Bitbucket.Authentication.Test
{
    public class UnitTestBase : Microsoft.Alm.Authentication.Test.UnitTestBase
    {
        protected UnitTestBase(IUnitTestTrace output, string projectDirectory, [CallerFilePath] string filePath = "")
            : base(output, projectDirectory, filePath)
        {
            if (GetService<IGui>() is null)
            {
                SetService(new Gui(Context));
            }

            if (GetService<IProcessService>() is null)
            {
                //TODO
                SetService(new ProcessService());
            }
        }

        protected UnitTestBase(IUnitTestTrace output, [CallerFilePath] string filePath = "")
            : this(output, null, filePath)
        { }

        protected override void InitializeTest(int iteration = -1, [CallerMemberName] string testName = "")
        {
            switch (TestMode)
            {
                case UnitTestMode.Capture:
                    {
                        var serviceGui = GetService<IGui>();
                        var captureGui = new CaptureGui(Context, serviceGui);

                        SetService(captureGui);
                    }
                    break;

                case UnitTestMode.NoProxy: break;

                case UnitTestMode.Replay:
                    {
                        var replayGui = new ReplayGui(Context);

                        SetService(replayGui);
                    }
                    break;
            }

            base.InitializeTest(iteration, testName);
        }
    }
}
