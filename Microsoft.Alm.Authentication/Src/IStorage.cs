using System.Collections.Generic;
using System.IO;

namespace Microsoft.Alm.Authentication
{
    public interface IStorage : IRuntimeService
    {
        /// <summary>
        /// Creates all directories and subdirectories in the specified path.
        /// </summary>
        /// <param name="path">The directory path to create.</param>
        void CreateDirectory(string path);

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// <para/>
        /// Returns `<see langword="true"/>` if path refers to an existing directory; otherwise, `<see langword="false"/>`.
        /// </summary>
        /// <param name="path">The path to test.</param>
        bool DirectoryExists(string path);

        /// <summary>
        ///  Returns an enumerable collection of file names and directory names that match a search pattern in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The directory to search.</param>
        /// <param name="pattern">
        /// The search string to match against the names of directories in path
        /// </param>
        /// <param name="options">
        /// Include only the current directory or should include all subdirectories.
        /// <para/>
        /// The default value is `<see cref="SearchOption.TopDirectoryOnly"/>`.
        /// </param>
        IEnumerable<string> EnumerateFileSystemEntries(string path, string pattern, SearchOption options);

        /// <summary>
        /// Returns an enumerable collection of file-system entries in a specified path.
        /// </summary>
        /// <param name="path">The directory to search.</param>
        IEnumerable<string> EnumerateFileSystemEntries(string path);

        /// <summary>
        /// Returns an enumerable collection of decrypted secrets, from the operating system's secure store, filtered by `<paramref name="prefix"/>`.
        /// </summary>
        /// <param name="prefix">
        /// Value that any secure store entry's key must start with.
        /// <para/>
        /// Value can be `<see langword="null"/>`.
        /// </param>
        IEnumerable<SecureData> EnumerateSecureData(string prefix);

        /// <summary>
        /// Copies an existing file to a new file.
        /// <para/>
        /// Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourcePath">The file to copy.</param>
        /// <param name="targetPath">
        /// The name of the destination file.
        /// <para/>
        /// This cannot be a directory or an existing file.
        /// </param>
        void FileCopy(string sourcePath, string targetPath, bool overwrite);

        /// <summary>
        /// Copies an existing file to a new file.
        /// </summary>
        /// <param name="sourcePath">The file to copy.</param>
        /// <param name="targetPath">
        /// The name of the destination file.
        /// <para/>
        /// This cannot be a directory.
        /// </param>
        /// <param name="overwrite">
        /// `<see langword="true"/>` if the destination file can be overwritten; otherwise, `<see langword="false"/>`.
        /// </param>
        void FileCopy(string sourcePath, string targetPath);

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">
        /// The name of the file to be deleted.
        /// <para/>
        /// Wildcard characters are not supported.
        /// </param>
        void FileDelete(string path);

        /// <summary>
        /// Determines whether the specified file exists.
        /// <para/>
        /// `<see langword="true"/>` if the caller has the required permissions and path contains the name of an existing file; otherwise, `<see langword="false"/>`.
        /// <para/>
        /// Returns `<see langword="false"/>` if `<paramref name="path"/>` is `<see langword="null"/>`, an invalid path, or a zero-length `<see langword="string"/>`.
        /// <para/>
        /// Returns `<see langword="false"/>` if the caller does not have sufficient permissions to read the specified file, no exception is thrown.
        /// <para/>
        /// </summary>
        /// <param name="path">The file to check.</param>
        bool FileExists(string path);

        /// <summary>
        /// Returns a `<see cref="Stream"/>` specified by `<paramref name="path"/>`, having the specified mode with read, write, or read/write access and the specified sharing option.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">
        /// A `<see cref="FileMode"/>` value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.
        /// </param>
        /// <param name="access">
        /// A `<see cref="FileAccess"/>` value that specifies the operations that can be performed on the file.
        /// </param>
        /// <param name="share">
        /// A `<see cref="FileShare value"/>` specifying the type of access other threads have to the file.
        /// </param>
        Stream FileOpen(string path, FileMode mode, FileAccess access, FileShare share);

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// <para/>
        /// Returns a byte array containing the contents of the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        byte[] FileReadAllBytes(string path);

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// <para/>
        /// Returns all of text contents of the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        string FileReadAllText(string path, System.Text.Encoding encoding);

        /// <summary>
        /// Opens a file, reads all lines of the file with UTF-8 encoding, and then closes the file.
        /// <para/>
        /// Returns all of text contents of the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        string FileReadAllText(string path);

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file.
        /// <para/>
        /// If `<paramref name="path"/>` already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        void FileWriteAllBytes(string path, byte[] data);

        /// <summary>
        /// Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file.
        /// <para/>
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        void FileWriteAllText(string path, string contents, System.Text.Encoding encoding);

        /// <summary>
        /// Creates a new file, writes the specified string to the file using UTF-8 encoding, and then closes the file.
        /// <para/>
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        void FileWriteAllText(string path, string contents);

        /// <summary>
        /// Returns an array of paths to all known drive roots.
        /// </summary
        string[] GetDriveRoots();

        /// <summary>
        /// Returns the file name and extension of the specified path string.
        /// <para/>
        /// If the last character f path is a directory or volume separator character, this method returns `<see cref="string.Empty"/>`.
        /// <para/>
        /// If `<paramref name="path"/>` is `<see langword="null"/>`, this method returns `<see langword="null"/>`.
        /// </summary>
        /// <param name="path">
        /// The path string from which to obtain the file name and extension.
        /// </param>
        string GetFileName(string path);

        /// <summary>
        /// Returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="path">
        /// The file or directory for which to obtain absolute path information.
        /// </param>
        string GetFullPath(string path);

        /// <summary>
        /// Returns the directory information for the specified path string.
        /// <para/>
        /// Returns `<see cref="string.Empty"/>` if `<paramref name="path"/>` does not contain directory information.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        string GetParent(string path);

        /// <summary>
        /// Deletes any, and all, entries from the operating system's secure storage with keys starting with `<paramref name="prefix"/>`.
        /// <para/>
        /// Returns the count of entries deleted.
        /// </summary>
        /// <param name="prefix">
        /// Value that any secure store entry's key must start with.
        /// <para/>
        /// Value can be `<see langword="null"/>`.
        /// </param>
        int TryPurgeSecureData(string prefix);

        /// <summary>
        /// Reads data from the operating system's secure storage.
        /// <para/>
        /// Data written to the store is uniquely identified by the associated `<paramref name="key"/>`.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="key">The key used to uniquely identify the data in the secure store.</param>
        /// <param name="name">
        /// The 'name' associated with the key, often a username.
        /// <para/>
        /// This information might not be encrypted by the operating system.
        /// </param>
        /// <param name="data">
        /// The encrypted data, to be decrypted, to be read from the operating systems secure storage.
        /// <para/>
        /// This information is encrypted, and will be decrypted, by the operating system.
        /// </param>
        bool TryReadSecureData(string key, out string name, out byte[] data);

        /// <summary>
        /// Writes data to the operating system's secure storage.
        /// <para/>
        /// Data written to the store is uniquely identified by the associated `<paramref name="key"/>`.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="key">The key used to uniquely identify the data in the secure store.</param>
        /// <param name="name">
        /// The 'name' associated with the key, often a username.
        /// <para/>
        /// This information might not be encrypted by the operating system.
        /// </param>
        /// <param name="data">
        /// The data, to be encrypted, to be written to the operating systems secure storage.
        /// <para/>
        /// This information will be encrypted by the operating system.
        /// </param>
        bool TryWriteSecureData(string key, string name, byte[] data);
    }
}