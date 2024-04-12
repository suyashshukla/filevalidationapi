using FileValidation.API.Models;

namespace FileValidation.API.Services
{
    /// <summary>
    /// This class represents the service that handles the file validation request.
    /// </summary>
    /// <seealso cref="FileValidation.API.Services.IFileValidationRequestService" />
    public class FileValidationRequestService(IFileValidator fileValidator) : IFileValidationRequestService
    {
        ///</inheritdoc>
        public async Task<FileValidationResponseDto> ValidateFile(FileValidationRequestDto fileValidationRequestDto)
        {
            var generationContext = new GenerationContext(fileValidationRequestDto.IdentificationNumber);
            await fileValidator.Validate(generationContext, fileValidationRequestDto);

            FileValidationResponseDto? fileValidationResponseDto = null;
            generationContext.OnGenerationCompleted += (sender, response) =>
            {
                fileValidationResponseDto = response;
                if (fileValidationRequestDto.IsFileUploadRequest)
                {
                    File.Delete(fileValidationRequestDto.FileUrl);
                    File.Delete(fileValidationRequestDto.CertificateUrl);
                }
                generationContext.SetIsExecutionCompleted();
            };

        checkGenerationStatus: if (!generationContext.IsExecutionCompleted)
            {
                Thread.Sleep(2000);
                goto checkGenerationStatus;
            }

            if (fileValidationResponseDto is not null)
            {
                return fileValidationResponseDto;
            }

            throw new Exception("Validation process failed.");
        }
    }
}
