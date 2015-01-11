using System;
using System.Management.Automation;

namespace GoogleStorage.Config
{
    [Cmdlet(VerbsCommon.Get, "GoogleStorageConfig")]
    public class GetGoogleStorageConfig : GoogleStorageCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                var config = GetConfig();

                if (config != null)
                {
                    WriteObject(config);
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "GetGoogleStorageConfig", ErrorCategory.NotSpecified, "config"));
            }
        }
    }
}
