using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Win32;
using Microsoft.Win32;
using static System.Diagnostics.Debug;

namespace Microsoft.Alm.Authentication.Win32
{
    internal class Win32Storage : Storage, IRegistryStorage
    {
        public Win32Storage(RuntimeContext context)
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
                                    System.Diagnostics.Debug.Fail("Failed with error code " + error.ToString("X"));
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
        #region Registry
        public string RegistryReadString(RegistryHive registryHive, RegistryView registryView, string registryPath, string keyName)
        {
            if (registryPath is null)
                throw new ArgumentNullException(nameof(registryPath));
            if (keyName is null)
                throw new ArgumentNullException(nameof(keyName));

            using (var baseKey = RegistryKey.OpenBaseKey(registryHive, registryView))
            using (var dataKey = baseKey?.OpenSubKey(registryPath))
            {
                return dataKey?.GetValue(keyName, null) as string;
            }
        }

        public string RegistryReadString(RegistryHive registryHive, string registryPath, string keyName)
            => RegistryReadString(registryHive, RegistryView.Default, registryPath, keyName);

        public string RegistryReadString(string registryPath, string keyName)
            => RegistryReadString(RegistryHive.CurrentUser, RegistryView.Default, registryPath, keyName);

        #endregion



        #region SecureData

        public bool TryReadSecureData(string key, out string name, out byte[] data)
        {
            const string NoSuchSessionMessage = "The logon session does not exist or there is no credential set associated with this logon session. Network logon sessions do not have an associated credential set.";

            if (key is null)
                throw new ArgumentNullException(nameof(key));

            var credPtr = IntPtr.Zero;

            if (NativeMethods.CredRead(key, NativeMethods.CredentialType.Generic, 0, out credPtr))
            {
                try
                {
                    var credStruct = (NativeMethods.Credential)Marshal.PtrToStructure(credPtr, typeof(NativeMethods.Credential));
                    if (credStruct.CredentialBlob != null && credStruct.CredentialBlobSize > 0)
                    {
                        int size = credStruct.CredentialBlobSize;
                        data = new byte[size];
                        Marshal.Copy(credStruct.CredentialBlob, data, 0, size);
                        name = credStruct.UserName;

                        return true;
                    }
                }
                finally
                {
                    if (credPtr != IntPtr.Zero)
                    {
                        NativeMethods.CredFree(credPtr);
                    }
                }
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                var errorCode = (ErrorCode)error;

                if (errorCode == ErrorCode.NoSuchLogonSession)
                    throw new InvalidOperationException(NoSuchSessionMessage);
            }

            data = null;
            name = null;

            return false;
        }

        public bool TryWriteSecureData(string key, string name, byte[] data)
        {
            const string BadUsernameMessage = "The UserName member of the passed in Credential structure is not valid.";
            const string NoSuchSessionMessage = "The logon session does not exist or there is no credential set associated with this logon session. Network logon sessions do not have an associated credential set.";

            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var credential = new NativeMethods.Credential
            {
                Type = NativeMethods.CredentialType.Generic,
                TargetName = key,
                CredentialBlob = Marshal.AllocCoTaskMem(data.Length),
                CredentialBlobSize = data.Length,
                Persist = NativeMethods.CredentialPersist.LocalMachine,
                AttributeCount = 0,
                UserName = name,
            };

            try
            {
                Marshal.Copy(data, 0, credential.CredentialBlob, data.Length);

                if (NativeMethods.CredWrite(ref credential, 0))
                    return true;

                int error = Marshal.GetLastWin32Error();
                var errorCode = (ErrorCode)error;

                switch (errorCode)
                {
                    case ErrorCode.NoSuchLogonSession:
                        throw new InvalidOperationException(NoSuchSessionMessage);

                    case ErrorCode.BadUserName:
                        throw new ArgumentException(BadUsernameMessage, nameof(name));
                }
            }
            finally
            {
                if (credential.CredentialBlob != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(credential.CredentialBlob);
                }
            }

            return false;
        }
    
        #endregion
    }
}
