﻿using System;
using System.Linq;
using System.IO;
using System.Web;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Management.Automation.Runspaces;

namespace GoogleStorage
{
    static class Extensions
    {
        public static void ThrowIfCancelled(this AggregateException e)
        {
            if (e.InnerExceptions.OfType<OperationCanceledException>().Any())
            {
                throw e.InnerExceptions.OfType<OperationCanceledException>().First();
            }
        }

        public static ErrorCategory GetCategory(this Exception e)
        {
            if (e is HaltCommandException || e is PipelineStoppedException || e is OperationCanceledException)
            {
                return ErrorCategory.OperationStopped;
            }
            else if (e is InvalidOperationException)
            {
                return ErrorCategory.InvalidOperation;
            }
            else if (e is ItemNotFoundException || e is FileNotFoundException)
            {
                return ErrorCategory.ObjectNotFound;
            }
            else if (e is AccessViolationException)
            {
                return ErrorCategory.PermissionDenied;
            }
            else
            {
                return ErrorCategory.NotSpecified;
            }
        }

        public static T WaitForResult<T>(this Task<T> task, CancellationToken cancelToken)
        {
            task.Wait(cancelToken);
            return task.Result;
        }

        public static string GetContentType(this FileInfo file)
        {
            return MimeMapping.GetMimeMapping(file.Name);
        }

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

        public static SecureString ToSecureString(this string s)
        {
            SecureString secure = new SecureString();
            Array.ForEach(s.ToArray(), secure.AppendChar);
            secure.MakeReadOnly();

            return secure;
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
