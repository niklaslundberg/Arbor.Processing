using System.IO;
using Arbor.Aesculus.Core;
using NCrunch.Framework;

namespace Arbor.Processing.Tests.Integration
{
    internal static class VcsTestPathHelper
    {
        public static string FindVcsRootPath()
        {
            if (NCrunchEnvironment.NCrunchIsResident())
            {
                var solutionDir = new FileInfo(NCrunchEnvironment.GetOriginalSolutionPath())?.Directory;
                return VcsPathHelper.FindVcsRootPath(solutionDir?.FullName);
            }

            return VcsPathHelper.FindVcsRootPath();
        }
    }
}