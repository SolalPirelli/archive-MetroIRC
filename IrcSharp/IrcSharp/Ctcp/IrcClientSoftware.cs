// Copyright (C) 2012-2013, Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code or compiled versions of it must retain the above copyright notice and the licence.

namespace IrcSharp.Ctcp
{
    /// <summary>
    /// Represents an IRC client software.
    /// </summary>
    public sealed class IrcClientSoftware
    {
        /// <summary>
        /// Gets or sets the name of the <see cref="IrcClientSoftware"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of the <see cref="IrcClientSoftware"/>.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the name of the operating system the <see cref="IrcClientSoftware"/> is running on.
        /// </summary>
        public string OSName { get; set; }

        /// <summary>
        /// Gets or sets the download location of the <see cref="IrcClientSoftware"/>.
        /// </summary>
        public string DownloadLocation { get; set; }

        internal IrcClientSoftware()
        {
            this.Name = "IRC#";
            this.Version = "N/A";
            this.OSName = "N/A";
            this.DownloadLocation = "N/A";
        }

        /// <summary>
        /// Formats the <see cref="IrcClientSoftware"/> into a string containing the name, version and operating system name.
        /// </summary>
        /// <returns>The <see cref="IrcClientSoftware"/> as a string.</returns>
        public override string ToString()
        {
            return string.Format( "{0} {1} ({2})", this.Name, this.Version, this.OSName );
        }
    }
}