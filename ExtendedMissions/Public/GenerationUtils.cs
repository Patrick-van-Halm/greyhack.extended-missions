namespace ExtendedMissions.Utils 
{
    /// <summary>
    /// Helpers for creating generated mission content.
    /// </summary>
    public static class GenerationUtils
    {
        /// <summary>
        /// Creates a generic file attachment for mission mail.
        /// </summary>
        /// <param name="name">The attachment file name.</param>
        /// <param name="content">The attachment content.</param>
        /// <param name="isBinary">Whether the attachment should be marked as binary.</param>
        /// <returns>The generated file attachment.</returns>
        public static FileSystem.Archivo CreateMailAttachment(string name, string content, bool isBinary)
        {
            var file = new FileSystem.Archivo(name, content, "Unknown", FileSystem.Fichero.TypeFile.Generic, true);
            file.SetBinario(isBinary);
            return file;
        }
    }
}