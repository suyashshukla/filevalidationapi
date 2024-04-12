using FileValidation.API.Models;
using FileValidation.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileValidation.API.Controllers
{
    /// <summary>
    /// This controller handles the validation of the file.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route("api/validation")]
    [ApiController]
    public class ValidationController(IAzureStorageService azureStorageService) : ControllerBase
    {
        /// <summary>
        /// Validates the file.
        /// </summary>
        /// <param name="fileValidationRequestService">The file validation request service.</param>
        /// <param name="fileValidationRequestDto">The file validation request dto.</param>
        /// <returns>The validation resposne dto.</returns>
        [HttpPost("url")]
        public async Task<IActionResult> ValidateFileFromUrl([FromServices] IFileValidationRequestService fileValidationRequestService, [FromBody] FileValidationRequestDto fileValidationRequestDto)
        {
            fileValidationRequestDto.IsFileUploadRequest = false;
            var response = await fileValidationRequestService.ValidateFile(fileValidationRequestDto);
            return Ok(response);
        }

        [HttpPost("files")]
        public async Task<IActionResult> ValidateFileFromContent([FromServices] IFileValidationRequestService fileValidationRequestService, [FromForm] FileValidationRequestDto fileValidationRequestDto)
        {
            fileValidationRequestDto.IsFileUploadRequest = true;
            fileValidationRequestDto.CertificateUrl = fileValidationRequestDto.FileUrl = null;
            var files = this.Request.Form.Files;

            if (files.Count == 0)
            {
                return BadRequest("No files were uploaded.");
            }

            foreach (var file in files)
            {
                var filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    stream.Close();
                }
                if (file.FileName.EndsWith(".csi"))
                {
                    fileValidationRequestDto.CertificateUrl = filePath;
                }
                else if (file.FileName.EndsWith(".txt"))
                {
                    fileValidationRequestDto.FileUrl = filePath;
                }
            }

            if (string.IsNullOrEmpty(fileValidationRequestDto.FileUrl) || string.IsNullOrEmpty(fileValidationRequestDto.CertificateUrl))
            {
                return BadRequest("Both file and certificate are required.");
            }

            var response = await fileValidationRequestService.ValidateFile(fileValidationRequestDto);
            return Ok(response);
        }
    }
}
