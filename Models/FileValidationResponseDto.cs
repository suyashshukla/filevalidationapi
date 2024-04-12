namespace FileValidation.API.Models
{
    /// <summary>
    /// This class represents the file validation response DTO.
    /// </summary>
    public class FileValidationResponseDto
    {
        /// <summary>
        /// Gets or sets the validation result URL.
        /// </summary>
        /// <value>
        /// The validation result URL.
        /// </value>
        public string ValidationResultUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Creates the new.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The instance of file validation response dto.</returns>
        public static FileValidationResponseDto CreateNew(string url) => new FileValidationResponseDto { ValidationResultUrl = url };

        /// <summary>
        /// Creates the new.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The file validation response dto.</returns>
        public static FileValidationResponseDto CreateNew(Exception exception) => new FileValidationResponseDto { Exception = exception };
    }
}
