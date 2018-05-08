namespace Plugins.Common
{
    using Microsoft.Xrm.Sdk;

    public enum ErrorCodes
    {
        InvalidTargetEntitySpecified = 72,
        PostImageNotFound = 73,
        PreImageNotFound = 74,
        NullTarget = 1615,
        NoApprovalRequestOnAction = 3001,
        TestError = 9999
    }

    public static class ErrorCode {

        public static string GetErrorCodeString(int errorCode)
        {
            switch (errorCode)
            {
                case (int)ErrorCodes.InvalidTargetEntitySpecified:
                    return "Invalid Target Entity Specified.  {0} was excepected, and {1} was found.";
                case (int)ErrorCodes.PostImageNotFound:
                    return "Plugin post-image '{0}' was not found.";
                case (int)ErrorCodes.PreImageNotFound:
                    return "Plugin pre-image '{0}' was not found.";
                case (int)ErrorCodes.NullTarget:
                    return "Target was null.";
                case (int)ErrorCodes.NoApprovalRequestOnAction:
                    return "The action does not have an approval request.";
                case (int)ErrorCodes.TestError:
                    return "Testing... {0}";
                default: return "An unknown error occurred.";
            }
        }

        /// <summary>
        /// Creates an InvalidPluginExecutionException using the specified error code.
        /// </summary>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="args">
        /// The arguments used when building the error string.
        /// </param>
        /// <returns>
        /// The <see cref="InvalidPluginExecutionException"/>.
        /// </returns>
        public static InvalidPluginExecutionException Exception(ErrorCodes errorCode, params object[] args) =>
            InternalException(OperationStatus.Failed, (int)errorCode, args);

        /// <summary>
        /// Creates an InvalidPluginExecutionException using the specified error code.
        /// </summary>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="args">
        /// The arguments used when building the error string.
        /// </param>
        /// <returns>
        /// The <see cref="InvalidPluginExecutionException"/>.
        /// </returns>
        public static InvalidPluginExecutionException Exception(int errorCode, params object[] args) =>
            InternalException(OperationStatus.Failed, errorCode, args);

        /// <summary>
        /// Creates an InvalidPluginExecutionException using the specified error code.
        /// </summary>
        /// <param name="status">
        /// The operation status.
        /// </param>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="args">
        /// The arguments used when building the error string.
        /// </param>
        /// <returns>
        /// The <see cref="InvalidPluginExecutionException"/>.
        /// </returns>
        public static InvalidPluginExecutionException Exception(
            OperationStatus status, 
            ErrorCodes errorCode,
            params object[] args) => InternalException(status, (int)errorCode, args);

        /// <summary>
        /// Creates an InvalidPluginExecutionException using the specified error code.
        /// </summary>
        /// <param name="status">
        /// The operation status.
        /// </param>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="args">
        /// The arguments used when building the error string.
        /// </param>
        /// <returns>
        /// The <see cref="InvalidPluginExecutionException"/>.
        /// </returns>
        public static InvalidPluginExecutionException Exception(OperationStatus status, int errorCode, params object[] args) => 
            InternalException(status, errorCode, args);

        /// <summary>
        /// Creates an InvalidPluginExecutionException using the specified error code.
        /// </summary>
        /// <param name="status">
        /// The operation status.
        /// </param>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="args">
        /// The arguments used when building the error string.
        /// </param>
        /// <returns>
        /// The <see cref="InvalidPluginExecutionException"/>.
        /// </returns>
        private static InvalidPluginExecutionException InternalException(
            OperationStatus status,
            int errorCode,
            object[] args)
        {
            var message = (args == null || args.Length==0) ? GetErrorCodeString(errorCode) : string.Format(GetErrorCodeString(errorCode), args);
            return new InvalidPluginExecutionException(status, errorCode, message);
        }
    }
}