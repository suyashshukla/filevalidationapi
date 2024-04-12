namespace FileValidation.API.Models
{
    /// <summary>
    /// This class represents the file validation request DTO.
    /// </summary>
    public class FileValidationRequestDto
    {
        /// <summary>
        /// Gets or sets the identification number.
        /// </summary>
        /// <value>
        /// The identification number.
        /// </value>
        public string IdentificationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file URL.
        /// </summary>
        /// <value>
        /// The file URL.
        /// </value>
        public string? FileUrl { get; set; }

        /// <summary>
        /// Gets or sets the certificate URL.
        /// </summary>
        /// <value>
        /// The certificate URL.
        /// </value>
        public string? CertificateUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is file upload request.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is file upload request; otherwise, <c>false</c>.
        /// </value>
        public bool IsFileUploadRequest { get; set; }
    }
}
