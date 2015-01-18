using System;
using System.Management.Automation;

namespace GoogleStorage.Config
{
    /// <summary>
    /// Shows the current configuration
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "GoogleStorageConfig")]
    public class GetGoogleStorageConfig : GoogleStorageCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                var config = GetConfig();

                if (config == null)
                {
                    throw new ItemNotFoundException("Configuration not set. Call Set-GoogleStorageConfig");
                }

                WriteDynamicObject(config);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
    }
}
