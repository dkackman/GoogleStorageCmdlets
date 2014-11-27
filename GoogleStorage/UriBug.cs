using System;
using System.Management.Automation;
using System.Net;
using System.Reflection;

namespace GoogleStorage
{
    [Cmdlet(VerbsDiagnostic.Debug, "UriFormatting")]

    public class UriBug : Cmdlet
    {        
        protected override void ProcessRecord()
        {
            try
            {
                var uri = new Uri("https://www.googleapis.com/download/storage/v1/b/uspto-pair/o/applications%2F05900002.zip?generation=1370956749027000&alt=media");
                ForceCanonicalPathAndQuery(uri);
                WebClient c = new WebClient();
                var b = c.DownloadData(uri);
                WriteObject(string.Format("Got {0} bytes", b.Length));
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "UriBug", ErrorCategory.ReadError, null));
            }
        }

        void ForceCanonicalPathAndQuery(Uri uri)
        {
            string paq = uri.PathAndQuery; // need to access PathAndQuery
            FieldInfo flagsFieldInfo = typeof(Uri).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            ulong flags = (ulong)flagsFieldInfo.GetValue(uri);
            flags &= ~((ulong)0x30); // Flags.PathNotCanonical|Flags.QueryNotCanonical
            flagsFieldInfo.SetValue(uri, flags);
        }
    }
}
