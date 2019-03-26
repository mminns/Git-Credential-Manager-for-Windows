using Microsoft.Win32;

namespace Microsoft.Alm.Authentication.Win32
{
    interface IRegistryStorage : IStorage
    {

        /// <summary>
        /// Opens `<paramref name="registryHive"/>` on the local machine with `<paramref name="registryView"/>`, and retrieves the value associated with `<paramref name="registryPath"/>` and `<paramref name="keyName"/>` provided.
        /// <para/>
        /// Returns the requested value as a `<see cref="string"/>` if possible; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="registryHive">The HKEY to open.</param>
        /// <param name="registryView">The registry view to use.</param>
        /// <param name="registryPath">The name or path of the subkey to open as read-only.</param>
        /// <param name="keyName">
        /// The name of the value to retrieve.
        /// <para/>
        /// This string is not case-sensitive.
        /// </param>
        string RegistryReadString(RegistryHive registryHive, RegistryView registryView, string registryPath, string keyName);

        /// <summary>
        /// Opens `<paramref name="registryHive"/>` on the local machine with the default view, and retrieves the value associated with `<paramref name="registryPath"/>` and `<paramref name="keyName"/>` provided.
        /// <para/>
        /// Returns the requested value as a `<see cref="string"/>` if possible; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="registryHive">The HKEY to open.</param>
        /// <param name="registryPath">The name or path of the subkey to open as read-only.</param>
        /// <param name="keyName">
        /// The name of the value to retrieve.
        /// <para/>
        /// This string is not case-sensitive.
        /// </param>
        string RegistryReadString(RegistryHive registryHive, string registryPath, string keyName);

        /// <summary>
        /// Opens Current User registry on the local machine with the default view, and retrieves the value associated with `<paramref name="registryPath"/>` and `<paramref name="keyName"/>` provided.
        /// <para/>
        /// Returns the requested value as a `<see cref="string"/>` if possible; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="registryPath">The name or path of the subkey to open as read-only.</param>
        /// <param name="keyName">
        /// The name of the value to retrieve.
        /// <para/>
        /// This string is not case-sensitive.
        /// </param>
        string RegistryReadString(string registryPath, string keyName);
    }
}
