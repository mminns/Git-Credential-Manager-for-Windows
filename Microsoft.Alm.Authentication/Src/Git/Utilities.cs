/**** Git Credential Manager for Windows ****
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the """"Software""""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
**/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Alm.Authentication.Git
{
    public interface IUtilities : IRuntimeService
    {
        /// <summary>
        /// Enumerates all processes visible by the current user, and builds process parentage chain from the current process back to System.
        /// <para/>
        /// Then walks up the chain from the current process, inspecting each parent process, looking for "git-remote-https" or "git-remote-http".
        /// <para/>
        /// When it find the target process, it then reads the process' memory and extracts the command line used to start the process and the image path of its executable.
        /// <para/>
        /// Returns `<see langword="true"/>` if able to extract information from the desired process; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="commandLine">The command line used to create the target process if successful; otherwise `<see langword="null"/>`.</param>
        /// <param name="imagePath">The path to the image used to create the process if successful; otherwise `<see langword="null"/>`.</param>
        bool TryReadGitRemoteHttpDetails(out string commandLine, out string imagePath);
    }

    internal abstract class Utilities : Base, IUtilities
    {
        public Utilities(RuntimeContext context)
            : base(context)
        { }

        public Type ServiceType
            => typeof(IUtilities);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals",
            MessageId = "error")]
        public abstract bool TryReadGitRemoteHttpDetails(out string commandLine, out string imagePath);
    }
}
