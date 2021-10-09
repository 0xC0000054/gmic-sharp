////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp, a .NET wrapper for G'MIC.
//
// Copyright (c) 2020, 2021 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace GmicSharp
{
    /// <summary>
    /// The exception that is thrown for G'MIC errors.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public sealed class GmicException : Exception
    {
        private const string GmicCommandSerializationName = "GmicCommand";

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicException"/> class.
        /// </summary>
        public GmicException() : this("An unspecified error occurred.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public GmicException(string message) : base(message)
        {
            GmicCommand = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public GmicException(string message, Exception innerException) : base(message, innerException)
        {
            GmicCommand = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="command">The G'MIC command.</param>
        internal GmicException(string message, string command) : base(message)
        {
            GmicCommand = command;
        }

        private GmicException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            GmicCommand = info.GetString(GmicCommandSerializationName);
        }

        /// <summary>
        /// Gets the G'MIC command that is associated with this error.
        /// </summary>
        /// <value>
        /// The G'MIC command that is associated with this error.
        /// </value>
        public string GmicCommand { get; }

        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(GmicCommandSerializationName, GmicCommand, typeof(string));
        }
    }
}
