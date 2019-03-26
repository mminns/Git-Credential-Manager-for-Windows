﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Alm.Authentication.Test;
using Microsoft.Alm.Authentication.Win32;
using Xunit;

namespace Microsoft.Alm.Authentication.Git.Test
{
    public class ConfigurationTests : UnitTestBase
    {
        public ConfigurationTests(Xunit.Abstractions.ITestOutputHelper output)
            : base(XunitHelper.Convert(output))
        { }

        public static object[][] GitConfig_Parse_Data
        {
            get
            {
                var data = new List<object[]>()
                {
                    new object[] { "\n[core]\n    autocrlf = false\n", "core.autocrlf", "false", true },
                    new object[] { "\n[core]\n    autocrlf = true\n    autocrlf = ThisShouldBeInvalidButIgnored\n    autocrlf = false\n", "core.autocrlf", "false", true },
                    new object[] { "\n[core \"oneQuote]\n    autocrlf = \"false\n", "core.oneQuote.autocrlf", "false", true },
                };

                return data.ToArray();
            }
        }

        [Theory]
        [MemberData(nameof(GitConfig_Parse_Data), DisableDiscoveryEnumeration = true)]
        public async Task GitConfig_Parse(string input, string expectedName, string expected, bool ignoreCase)
        {
            var values = await TestParseGitConfig(input);
            Assert.NotNull(values);

            Assert.Equal(expected, values[expectedName], ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        [Fact]
        public async Task GitConfig_ParseSampleFile()
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var type = GetType();
            var assembly = type.Assembly;

            using (var rs = assembly.GetManifestResourceStream("Microsoft.Alm.Authentication.Test.Git.sample.gitconfig"))
            using (var sr = new StreamReader(rs))
            {
                await Configuration.ParseGitConfig(Win32RuntimeContext.Default, sr, values);
            }

            Assert.Equal(36, values.Count);
            Assert.Equal("\\\"C:/Utils/Compare It!/wincmp3.exe\\\" \\\"$(cygpath -w \\\"$LOCAL\\\")\\\" \\\"$(cygpath -w \\\"$REMOTE\\\")\\\"", values["difftool.cygcompareit.cmd"], StringComparer.OrdinalIgnoreCase);
            Assert.Equal("!f() { git fetch origin && git checkout -b $1 origin/master --no-track; }; f", values["alias.cob"], StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GitConfig_ReadThrough()
        {
            const string input = "\n" +
                    "[core]\n" +
                    "    autocrlf = false\n" +
                    "[credential \"microsoft.visualstudio.com\"]\n" +
                    "    authority = AAD\n" +
                    "[credential \"visualstudio.com\"]\n" +
                    "    authority = MSA\n" +
                    "[credential \"https://ntlm.visualstudio.com\"]\n" +
                    "    authority = NTLM\n" +
                    "[credential]\n" +
                    "    helper = manager\n" +
                    "";
            Configuration cut;

            using (var reader = new StringReader(input))
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                await Configuration.ParseGitConfig(Win32RuntimeContext.Default, reader, dict);

                var values = new Dictionary<ConfigurationLevel, Dictionary<string, string>>();

                foreach (var level in Configuration.Levels)
                {
                    values[level] = dict;
                }

                cut = new Configuration(Win32RuntimeContext.Default, values);
            }

            Assert.True(cut.ContainsKey("CoRe.AuToCrLf"));
            Assert.Equal("false", cut["CoRe.AuToCrLf"], StringComparer.OrdinalIgnoreCase);

            Configuration.Entry entry;
            Assert.True(cut.TryGetEntry("core", (string)null, "autocrlf", out entry));
            Assert.Equal("false", entry.Value, StringComparer.OrdinalIgnoreCase);

            Assert.True(cut.TryGetEntry("credential", new Uri("https://microsoft.visualstudio.com"), "authority", out entry));
            Assert.Equal("AAD", entry.Value, StringComparer.OrdinalIgnoreCase);

            Assert.True(cut.TryGetEntry("credential", new Uri("https://mseng.visualstudio.com"), "authority", out entry));
            Assert.Equal("MSA", entry.Value, StringComparer.OrdinalIgnoreCase);

            Assert.True(cut.TryGetEntry("credential", new Uri("https://ntlm.visualstudio.com"), "authority", out entry));
            Assert.Equal("NTLM", entry.Value, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GitConfig_Read()
        {
            InitializeTest();

            var configuration = new Configuration(Context);
            await configuration.LoadGitConfiguration(SolutionDirectory, ConfigurationLevel.All);
        }

        private static async Task<Dictionary<string, string>> TestParseGitConfig(string input)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (var sr = new StringReader(input))
            {
                await Configuration.ParseGitConfig(Win32RuntimeContext.Default, sr, values);
            }

            return values;
        }
    }
}
