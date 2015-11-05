using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Sopme.Random.Namespace.Logging;
using Sopme.Random.Namespace.Resources;

namespace Some.Random.Namespace
{
    /// <summary>    
	/// Monitor a web service method call. Take some action to measure the time the call took, 
	/// and if a Timeout exception or SocketException occurs.
    /// See more about socket error codes here https://msdn.microsoft.com/en-us/library/ee433496.aspx
    /// </summary>
    public abstract class Monitor
    {
        private readonly ILogger logger;
        const int SocketTimeoutErrorCode = 10060;

        protected Monitor(ILogger logger)
        {
            this.logger = logger;
        }

        protected TResult Method<T1, TResult>(Func<T1, TResult> func, T1 t1, Action<string, long> execTimeLambda,
            Action<string> timeoutLambda, Action<string> socketTimeoutLambda)
        {
            string methodName = func.Method.Name;

            try
            {
                var watch = new Stopwatch();
                watch.Start();
                TResult result = func(t1);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;
                execTimeLambda(methodName, elapsedMs);
                return result;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    timeoutLambda(methodName);
                }

                var inner = e.InnerException as SocketException;
                if (inner != null)
                {
                    string message = string.Format(StringRes.SocketException, inner.SocketErrorCode, inner.ErrorCode, inner.Message);
                    logger.Log(message, LogLevel.Error);

                    if (inner.ErrorCode == SocketTimeoutErrorCode)
                    {
                        logger.Log(string.Format(StringRes.SocketTimeoutException, inner.SocketErrorCode), LogLevel.Error);
                        socketTimeoutLambda(methodName);
                    }
                }

                throw;
            }
        } 
    }
}