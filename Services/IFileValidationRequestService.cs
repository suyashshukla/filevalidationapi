using FileValidation.API.Models;

namespace FileValidation.API.Services
{
    /// <summary>
    /// This interface represents the file validation request service.
    /// </summary>
    public interface IFileValidationRequestService
    {
        /// <summary>
        /// Validates the file.
        /// </summary>
        /// <param name="fileValidationRequestDto">The file validation request dto.</param>
        /// <returns>The file validation response dto.</returns>
        public Task<FileValidationResponseDto> ValidateFile(FileValidationRequestDto fileValidationRequestDto);
    }
}
