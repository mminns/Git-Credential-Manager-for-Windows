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
using System.IO;
using System.Runtime.InteropServices;
using static System.Diagnostics.Debug;

namespace Microsoft.Alm.Authentication
{
    public abstract class Storage : Base, IStorage
    {
        public Storage(RuntimeContext context)
            : base(context)
        { }

        public Type ServiceType
            => typeof(IStorage);

        public void CreateDirectory(string path)
            => Directory.CreateDirectory(path);

        public bool DirectoryExists(string path)
            => Directory.Exists(path);

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string pattern, SearchOption options)
            => Directory.EnumerateFileSystemEntries(path, pattern, options);

        public IEnumerable<string> EnumerateFileSystemEntries(string path)
            => EnumerateFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly);

        public IEnumerable<SecureData> EnumerateSecureData(string prefix)
        {
            string filter = prefix ?? string.Empty + "*";

            if (NativeMethods.CredEnumerate(filter, 0, out int count, out IntPtr credentialArrayPtr))
            {
                Trace.WriteLine($"{count} credentials enumerated from secret store.");

                try
                {
                    for (int i = 0; i < count; i += 1)
                    {
                        int offset = i * Marshal.SizeOf(typeof(IntPtr));
                        IntPtr credentialPtr = Marshal.ReadIntPtr(credentialArrayPtr, offset);

                        if (credentialPtr != IntPtr.Zero)
                        {
                            NativeMethods.Credential credStruct = Marshal.PtrToStructure<NativeMethods.Credential>(credentialPtr);
                            int passwordLength = credStruct.CredentialBlobSize;

                            byte[] data = new byte[credStruct.CredentialBlobSize];
                            Marshal.Copy(credStruct.CredentialBlob, data, 0, credStruct.CredentialBlobSize);

                            string name = credStruct.UserName ?? string.Empty;
                            string key = credStruct.TargetName;

                            yield return new SecureData(key, name, data);
                        }
                    }
                }
                finally
                {
                    NativeMethods.CredFree(credentialArrayPtr);
                }
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                if (error != NativeMethods.Win32Error.FileNotFound
                    && error != NativeMethods.Win32Error.NotFound)
                {
                    Fail($"Failed with error code 0x{error.ToString("X8")}.");
                }
            }

            yield break;
        }

        public void FileCopy(string sourcePath, string destinationPath)
            => FileCopy(sourcePath, destinationPath, false);

        public void FileCopy(string sourcePath, string destinationPath, bool overwrite)
            => File.Copy(sourcePath, destinationPath, overwrite);

        public void FileDelete(string path)
            => File.Delete(path);

        public bool FileExists(string path)
            => File.Exists(path);

        public Stream FileOpen(string path, FileMode mode, FileAccess access, FileShare share)
            => File.Open(path, mode, access, share);

        public byte[] FileReadAllBytes(string path)
            => File.ReadAllBytes(path);

        public string FileReadAllText(string path, System.Text.Encoding encoding)
            => File.ReadAllText(path, encoding);

        public string FileReadAllText(string path)
            => FileReadAllText(path, System.Text.Encoding.UTF8);

        public void FileWriteAllBytes(string path, byte[] bytes)
            => File.WriteAllBytes(path, bytes);

        public void FileWriteAllText(string path, string contents, System.Text.Encoding encoding)
            => File.WriteAllText(path, contents, encoding);

        public void FileWriteAllText(string path, string contents)
            => FileWriteAllText(path, contents, System.Text.Encoding.UTF8);

        public string[] GetDriveRoots()
        {
            var drives = DriveInfo.GetDrives();
            var paths = new string[drives.Length];

            for (int i = 0; i < drives.Length; i += 1)
            {
                paths[i] = drives[i].RootDirectory.FullName;
            }

            return paths;
        }

        public string GetFileName(string path)
            => Path.GetFileName(path);

        public string GetFullPath(string path)
            => Path.GetFullPath(path);

        public string GetParent(string path)
            => Path.GetDirectoryName(path);

        public int TryPurgeSecureData(string prefix)
        {
            if (prefix is null)
                throw new ArgumentNullException(nameof(prefix));

            string filter = prefix ?? string.Empty + "*";
            int purgeCount = 0;

            if (NativeMethods.CredEnumerate(filter, 0, out int count, out IntPtr credentialArrayPtr))
            {
                try
                {
                    for (int i = 0; i < count; i += 1)
                    {
                        int offset = i * Marshal.SizeOf(typeof(IntPtr));
                        IntPtr credentialPtr = Marshal.ReadIntPtr(credentialArrayPtr, offset);

                        if (credentialPtr != IntPtr.Zero)
                        {
                            NativeMethods.Credential credential = Marshal.PtrToStructure<NativeMethods.Credential>(credentialPtr);

                            if (NativeMethods.CredDelete(credential.TargetName, credential.Type, 0))
                            {
                                purgeCount += 1;
                            }
                            else
                            {
                                int error = Marshal.GetLastWin32Error();
                                if (error != NativeMethods.Win32Error.FileNotFound)
                                {
                                    Fail("Failed with error code " + error.ToString("X"));
                                }
                            }
                        }
                    }
                }
                finally
                {
                    NativeMethods.CredFree(credentialArrayPtr);
                }
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                if (error != NativeMethods.Win32Error.FileNotFound
                    && error != NativeMethods.Win32Error.NotFound)
                {
                    Fail("Failed with error code " + error.ToString("X"));
                }
            }

            return purgeCount;
        }

        public abstract bool TryReadSecureData(string key, out string name, out byte[] data);

        public abstract bool TryWriteSecureData(string key, string name, byte[] data);
    }
}
