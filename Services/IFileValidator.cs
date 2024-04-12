using FileValidation.API.Models;

namespace FileValidation.API.Services
{
    /// <summary>
    /// The file validator interface.
    /// </summary>
    public interface IFileValidator
    {
        ///</inheritdoc>
        public Task Validate(GenerationContext generationContext, FileValidationRequestDto fileValidationRequestDto);
    }
}
