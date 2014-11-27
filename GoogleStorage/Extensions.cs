using System;
using System.Security;
using System.Runtime.InteropServices;
using System.Management.Automation.Runspaces;
using System.Linq;
using System.Reflection;

namespace GoogleStorage
{
    static class Extensions
    {
        /// <summary>
        /// http://stackoverflow.com/questions/781205/getting-a-url-with-an-url-encoded-slash
        /// object media links may have eced path tokens
        /// </summary>
        /// <param name="uri"></param>
        public static void ForceCanonicalPathAndQuery(this Uri uri)
        {
            string paq = uri.PathAndQuery; // need to access PathAndQuery
            FieldInfo flagsFieldInfo = typeof(Uri).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            ulong flags = (ulong)flagsFieldInfo.GetValue(uri);
            flags &= ~((ulong)0x30); // Flags.PathNotCanonical|Flags.QueryNotCanonical
            flagsFieldInfo.SetValue(uri, flags);
        }

        public static string ToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static string ToEncyptedString(this SecureString s)
        {
            Command command = new Command("ConvertFrom-SecureString");
            command.Parameters.Add("SecureString", s);
            return command.InvokeCommand<string>();
        }

        public static SecureString FromEncyptedString(this string s)
        {
            Command command = new Command("ConvertTo-SecureString");
            command.Parameters.Add("String", s);
            return command.InvokeCommand<SecureString>();
        }

        public static T InvokeCommand<T>(this Command command)
        {
            using (Runspace runSpace = RunspaceFactory.CreateRunspace())
            {
                runSpace.Open();
                using (Pipeline pipeline = runSpace.CreatePipeline())
                {
                    pipeline.Commands.Add(command);
                    return pipeline.Invoke().Select(o => o.BaseObject).Cast<T>().First();
                }
            }
        }
    }
}
