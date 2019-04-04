﻿/**** Git Credential Manager for Windows ****
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
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Alm.Authentication;
using static Microsoft.Alm.NativeMethods;
using Git = Microsoft.Alm.Authentication.Git;

namespace Microsoft.Alm.Cli
{
    internal static class DialogFunctions
    {
        public static bool DisplayModal(Git.Trace trace, ref CredentialUiInfo credUiInfo,
                                        ref CredentialPackFlags authPackage,
                                        IntPtr packedAuthBufferPtr,
                                        uint packedAuthBufferSize,
                                        IntPtr inBufferPtr,
                                        int inBufferSize,
                                        bool saveCredentials,
                                        CredentialUiWindowsFlags flags,
                                        out string username,
                                        out string password)
        {
            if (trace is null)
                throw new ArgumentNullException(nameof(trace));

            int error;

            try
            {
                // open a standard Windows authentication dialog to acquire username + password credentials
                if ((error = CredUIPromptForWindowsCredentials(credInfo: ref credUiInfo,
                                                                            authError: 0,
                                                                          authPackage: ref authPackage,
                                                                         inAuthBuffer: inBufferPtr,
                                                                     inAuthBufferSize: (uint)inBufferSize,
                                                                        outAuthBuffer: out packedAuthBufferPtr,
                                                                    outAuthBufferSize: out packedAuthBufferSize,
                                                                      saveCredentials: ref saveCredentials,
                                                                                flags: flags)) != Win32Error.Success)
                {
                    trace.WriteLine($"credential prompt failed ('{Win32Error.GetText(error)}').");

                    username = null;
                    password = null;

                    return false;
                }

                // use `StringBuilder` references instead of string so that they can be written to
                var usernameBuffer = new StringBuilder(512);
                var domainBuffer = new StringBuilder(256);
                var passwordBuffer = new StringBuilder(512);
                int usernameLen = usernameBuffer.Capacity;
                int passwordLen = passwordBuffer.Capacity;
                int domainLen = domainBuffer.Capacity;

                // unpack the result into locally useful data
                if (!CredUnPackAuthenticationBuffer(flags: authPackage,
                                               authBuffer: packedAuthBufferPtr,
                                           authBufferSize: packedAuthBufferSize,
                                                 username: usernameBuffer,
                                           maxUsernameLen: ref usernameLen,
                                               domainName: domainBuffer,
                                         maxDomainNameLen: ref domainLen,
                                                 password: passwordBuffer,
                                           maxPasswordLen: ref passwordLen))
                {
                    username = null;
                    password = null;

                    error = Marshal.GetLastWin32Error();
                    trace.WriteLine($"failed to unpack buffer ('{Win32Error.GetText(error)}').");

                    return false;
                }

                trace.WriteLine("successfully acquired credentials from user.");

                username = usernameBuffer.ToString();
                password = passwordBuffer.ToString();

                return true;
            }
            finally
            {
                if (packedAuthBufferPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(packedAuthBufferPtr);
                }
            }
        }

        public static Credential CredentialPrompt(Git.Trace trace, string programTitle, IntPtr parentHwnd, TargetUri targetUri, string message)
        {
            if (trace is null)
                throw new ArgumentNullException(nameof(trace));
            if (programTitle is null)
                throw new ArgumentNullException(nameof(programTitle));
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var credUiInfo = new CredentialUiInfo
            {
                BannerArt = IntPtr.Zero,
                CaptionText = programTitle,
                Parent = parentHwnd,
                MessageText = message,
                Size = Marshal.SizeOf(typeof(CredentialUiInfo))
            };
            var flags = CredentialUiWindowsFlags.Generic;
            var authPackage = CredentialPackFlags.None;
            var packedAuthBufferPtr = IntPtr.Zero;
            var inBufferPtr = IntPtr.Zero;
            uint packedAuthBufferSize = 0;
            bool saveCredentials = false;
            int inBufferSize = 0;
            string username;
            string password;

            if (DisplayModal(trace, ref credUiInfo,
                                                 ref authPackage,
                                                 packedAuthBufferPtr,
                                                 packedAuthBufferSize,
                                                 inBufferPtr,
                                                 inBufferSize,
                                                 saveCredentials,
                                                 flags,
                                                 out username,
                                                 out password))
            {
                return new Credential(username, password);
            }

            return null;
        }

        public static Credential PasswordPrompt(Git.Trace trace, string programTitle, IntPtr parentHwnd, TargetUri targetUri, string message, string username)
        {
            if (trace is null)
                throw new ArgumentNullException(nameof(trace));
            if (programTitle is null)
                throw new ArgumentNullException(nameof(programTitle));
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (username is null)
                throw new ArgumentNullException(nameof(username));

            var credUiInfo = new CredentialUiInfo
            {
                BannerArt = IntPtr.Zero,
                CaptionText = programTitle,
                MessageText = message,
                Parent = parentHwnd,
                Size = Marshal.SizeOf(typeof(CredentialUiInfo))
            };
            var flags = CredentialUiWindowsFlags.Generic;
            var authPackage = CredentialPackFlags.None;
            var packedAuthBufferPtr = IntPtr.Zero;
            var inBufferPtr = IntPtr.Zero;
            uint packedAuthBufferSize = 0;
            bool saveCredentials = false;
            int inBufferSize = 0;
            string password = null;

            try
            {
                int error;

                // Execute with `null` to determine buffer size always returns false when determining
                // size, only fail if `inBufferSize` looks bad.
                CredPackAuthenticationBuffer(flags: authPackage,
                                          username: username,
                                          password: string.Empty,
                                 packedCredentials: IntPtr.Zero,
                             packedCredentialsSize: ref inBufferSize);
                if (inBufferSize <= 0)
                {
                    error = Marshal.GetLastWin32Error();
                    trace.WriteLine($"unable to determine credential buffer size ('{Win32Error.GetText(error)}').");

                    return null;
                }

                inBufferPtr = Marshal.AllocHGlobal(inBufferSize);

                if (!CredPackAuthenticationBuffer(flags: authPackage,
                                               username: username,
                                               password: string.Empty,
                                      packedCredentials: inBufferPtr,
                                  packedCredentialsSize: ref inBufferSize))
                {
                    error = Marshal.GetLastWin32Error();
                    trace.WriteLine($"unable to write to credential buffer ('{Win32Error.GetText(error)}').");

                    return null;
                }

                if (DisplayModal(trace, ref credUiInfo,
                                                     ref authPackage,
                                                     packedAuthBufferPtr,
                                                     packedAuthBufferSize,
                                                     inBufferPtr,
                                                     inBufferSize,
                                                     saveCredentials,
                                                     flags,
                                                     out username,
                                                     out password))
                {
                    return new Credential(username, password);
                }
            }
            finally
            {
                if (inBufferPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(inBufferPtr);
                }
            }

            return null;
        }
    }
}
