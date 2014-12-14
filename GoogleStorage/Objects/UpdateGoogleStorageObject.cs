using System;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    [Cmdlet(VerbsData.Update, "GoogleStorageObject", SupportsShouldProcess = true)]
    public class UpdateGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string PropertyName { get; set; }

        /// <summary>
        /// The new value for the propety
        /// Omit to clear the current value
        /// </summary>
        [Parameter(Mandatory = false, Position = 3, ValueFromPipelineByPropertyName = true)]
        public string PropertyValue { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess(string.Format("{0}/{1}", Bucket, ObjectName), string.Format("Set {0} to {1}", PropertyName, PropertyValue)))
                {
                    if (Force || ShouldContinue(string.Format("Set object {0}/{1} {2} to {3}?", Bucket, ObjectName, PropertyName, PropertyValue), "Update Object?"))
                    {
                        var api = CreateApiWrapper();
                        var t = api.UpdateObjectMetaData(Bucket, ObjectName, PropertyName, PropertyValue);
                        dynamic result = t.Result;

                        WriteDynamicObject(result);
                        WriteVerbose(string.Format("Object {0}/{1} {2} property set to {3}", Bucket, ObjectName, PropertyName, PropertyValue));
                    }
                }          
            }
            catch (HaltCommandException)
            {
            }
            catch (PipelineStoppedException)
            {
            }
            catch (AggregateException e)
            {
                WriteAggregateException(e);
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, e.Message, ErrorCategory.ReadError, null));
            }
        }
    }
}
