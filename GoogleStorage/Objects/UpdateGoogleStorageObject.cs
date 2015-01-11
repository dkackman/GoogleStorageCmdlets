using System;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    [Cmdlet(VerbsData.Update, "GoogleStorageObject", SupportsShouldProcess = true)]
    public class UpdateGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The bucket where the object exists
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        /// <summary>
        /// The name of the object
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        /// <summary>
        /// The name of the object's property to update
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string PropertyName { get; set; }

        /// <summary>
        /// The new value for the propety
        /// Omit to clear the current value
        /// </summary>
        [Parameter(Mandatory = false, Position = 3, ValueFromPipelineByPropertyName = true)]
        public string PropertyValue { get; set; }

        /// <summary>
        /// Flag indicating whetehr to update the property without prompting the user
        /// </summary>
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
                        using (var api = CreateApiWrapper())
                        {
                            var result = api.UpdateObjectMetaData(Bucket, ObjectName, PropertyName, PropertyValue).WaitForResult(GetCancellationToken());

                            WriteDynamicObject(result);
                            WriteVerbose(string.Format("Object {0}/{1} {2} property set to {3}", Bucket, ObjectName, PropertyName, PropertyValue));
                        }
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
