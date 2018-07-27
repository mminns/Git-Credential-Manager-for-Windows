using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Atlassian.Bitbucket.Authentication.Test
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    public struct CapturedGuiData
    {
        [JsonProperty(PropertyName = "Operations", NullValueHandling = NullValueHandling.Ignore)]
        public List<CapturedGuiOperation> Operations { get; set; }

        internal string DebuggerDisplay
        {
            get { return Invariant($"{nameof(CapturedGuiData)}: {nameof(Operations)}[{Operations?.Count}]"); }
        }

        public static bool TryDeserialize(object serializedData, out CapturedGuiData guiData)
        {
            if (serializedData is JObject jGuiData)
            {
                guiData = jGuiData.ToObject<CapturedGuiData>();

                return true;
            }

            guiData = default(CapturedGuiData);
            return false;
        }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    public struct CapturedGuiOperation
    {
        [JsonProperty(PropertyName = "Output", NullValueHandling = NullValueHandling.Ignore)]
        public CapturedGuiOutput Output { get; set; }

        [JsonProperty(PropertyName = "DialogType", NullValueHandling = NullValueHandling.Ignore)]
        public string DialogType { get; set; }

        internal string DebuggerDisplay
        {
            get { return Invariant($"{nameof(CapturedGuiOperation)}: {nameof(DialogType)} = {DialogType}"); }
        }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    public struct CapturedGuiOutput
    {
        [JsonProperty(PropertyName = "Login", NullValueHandling = NullValueHandling.Ignore)]
        public string Login { get; set; }

        [JsonProperty(PropertyName = "IsValid", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsValid { get; set; }

        [JsonProperty(PropertyName = "UsesOAuth", NullValueHandling = NullValueHandling.Ignore)]
        public bool UsesOAuth { get; set; }

        [JsonProperty(PropertyName = "Password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "Result", NullValueHandling = NullValueHandling.Ignore)]
        public int Result { get; set; }

        [JsonProperty(PropertyName = "Success", NullValueHandling = NullValueHandling.Ignore)]
        public bool Success { get; set; }

        internal string DebuggerDisplay
        {
            get { return Invariant($"{nameof(CapturedGuiOutput)}: {ToString()}"); }
        }

        public override string ToString()
        {
            return Invariant($"{nameof(Result)} = {Result}, {nameof(Success)} = {Success}");
        }
    }
}
