using FileValidation.API.Models;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FileValidation.API.Services
{
    /// <summary>
    /// This class is responsible for validating the file.
    /// </summary>
    /// <seealso cref="FileValidation.API.Services.IFileValidator" />
    public class FileValidator(IAzureStorageService azureStorageService) : IFileValidator
    {
        /// <summary>
        /// The execution process
        /// </summary>
        public Process ExecutionProcess { get; private set; }

        /// <summary>
        /// The display process.
        /// </summary>
        public Process DisplayProcess { get; private set; }

        /// <summary>
        /// Validates the specified generation context.
        /// </summary>
        /// <param name="generationContext">The generation context.</param>
        /// <param name="fileValidationRequestDto">The file validation request dto.</param>
        public async Task Validate(GenerationContext generationContext, FileValidationRequestDto fileValidationRequestDto)
        {
            this.Initialize(generationContext);
            this.CopyCertificateToOutputLocation(generationContext);
            await this.EnrichFileAndCSIContent(generationContext, fileValidationRequestDto);
            this.StartValidationProcess(generationContext);
        }

        ///</inheritdoc>
        public void Initialize(GenerationContext generationContext)
        {
            if (Directory.Exists(generationContext.OutputLocation))
            {
                Directory.Delete(generationContext.OutputLocation, true);
            }

            Directory.CreateDirectory(generationContext.OutputLocation);
        }

        /// <summary>
        /// Copies the certificate to output location.
        /// </summary>
        /// <param name="generationContext">The generation context.</param>
        public void CopyCertificateToOutputLocation(GenerationContext generationContext)
        {
            File.Copy(generationContext.CerticicateLocation, Path.Combine(generationContext.OutputLocation, Path.GetFileName(generationContext.CerticicateLocation)));
        }

        /// <summary>
        /// Enriches the content of the file and csi.
        /// </summary>
        /// <param name="generationContext">The generation context.</param>
        /// <param name="fileValidationRequestDto">The file validation request dto.</param>
        public async Task EnrichFileAndCSIContent(GenerationContext generationContext, FileValidationRequestDto fileValidationRequestDto)
        {
            var certificateFile = File.Create(generationContext.CSIFileLocation);
            var contentFile = File.Create(generationContext.FileContentLocation);

            if (!fileValidationRequestDto.IsFileUploadRequest)
            {
                using (var httpClient = new HttpClient())
                {
                    await certificateFile.WriteAsync(await httpClient.GetByteArrayAsync(fileValidationRequestDto.CertificateUrl));
                    await contentFile.WriteAsync(await httpClient.GetByteArrayAsync(fileValidationRequestDto.FileUrl));

                    contentFile.Close();
                    certificateFile.Close();
                }
            }
            else
            {
                using (var fileStream = new FileStream(fileValidationRequestDto.FileUrl, FileMode.Open))
                {
                    await fileStream.CopyToAsync(contentFile);
                    fileStream.Close();
                    contentFile.Close();
                }

                using (var certificateStream = new FileStream(fileValidationRequestDto.CertificateUrl, FileMode.Open))
                {
                    await certificateStream.CopyToAsync(certificateFile);
                    certificateStream.Close();
                    certificateFile.Close();
                }
            }
        }

        /// <summary>
        /// Starts the validation process.
        /// </summary>
        /// <param name="generationContext">The generation context.</param>
        public void StartValidationProcess(GenerationContext generationContext)
        {
            this.RegisterFileWatcher(generationContext);
            var executionCommand = this.BuildExecutionCommand(generationContext);
            this.DisplayProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = "Xvfb",
                Arguments = ":99 -screen 0 1280x1024x24"
            });

            ExecutionProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = "java",
                Arguments = executionCommand,
                CreateNoWindow = true,
                ErrorDialog = false,
                WorkingDirectory = generationContext.OutputLocation,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false, //// This will cause the process object to get created.
                WindowStyle = ProcessWindowStyle.Hidden,
            });
        }

        /// <summary>
        /// Registers the file watcher.
        /// </summary>
        /// <param name="generationContext">The generation context.</param>
        private void RegisterFileWatcher(GenerationContext generationContext)
        {
            FileSystemWatcher errorWatcher = new()
            {
                Path = generationContext.OutputLocation,
                Filter = "*.err",
                EnableRaisingEvents = true
            };

            FileSystemWatcher successWatcher = new()
            {
                Path = generationContext.OutputLocation,
                Filter = "*.pdf",
                EnableRaisingEvents = true
            };

            errorWatcher.Changed += (sender, e) =>
            {
                if (!generationContext.IsCallbackExecutionInProgress)
                {
                    generationContext.SetIsCallbackExecutionInProgress();
                    this.DisposeFileSystemWatchers(successWatcher, errorWatcher);
                    RunPostExecutionAction(generationContext).Wait();
                }
            };

            successWatcher.Changed += (sender, e) =>
            {
                if (!generationContext.IsCallbackExecutionInProgress)
                {
                    generationContext.SetIsCallbackExecutionInProgress();
                    this.DisposeFileSystemWatchers(successWatcher, errorWatcher);
                    this.RunPostExecutionAction(generationContext).Wait();
                }
            };

            Task.Delay(100000).ContinueWith(interval =>
                    {
                        if (!generationContext.IsCallbackExecutionInProgress)
                        {
                            generationContext.SetIsCallbackExecutionInProgress();
                            this.DisposeFileSystemWatchers(successWatcher, errorWatcher);
                            this.RunPostExecutionAction(generationContext).Wait();
                        }
                    });
        }

        /// <summary>
        /// Disposes the file system watchers.
        /// </summary>
        /// <param name="successWatcher">The success watcher.</param>
        /// <param name="errorWatcher">The error watcher.</param>
        private void DisposeFileSystemWatchers(FileSystemWatcher successWatcher, FileSystemWatcher errorWatcher)
        {
            successWatcher.EnableRaisingEvents = false;
            errorWatcher.EnableRaisingEvents = false;
            successWatcher.Dispose();
            errorWatcher.Dispose();
        }

        /// <summary>
        /// Runs the post execution action.
        /// </summary>
        /// <param name="generationContext">The generation context.</param>
        private async Task RunPostExecutionAction(GenerationContext generationContext)
        {
            try
            {
                var blobClient = await azureStorageService.GetBlobClientAsync(generationContext.IdentificationNumber, $"{Guid.NewGuid()}.zip");
                if (this.ExecutionProcess is not null && !this.ExecutionProcess.HasExited)
                {
                    this.ExecutionProcess?.Kill();
                }
                Thread.Sleep(1000);
                var zipPath = this.GenerationZipFromOutputContent(generationContext);
                var blobResult = await blobClient.UploadAsync(zipPath);

                File.Delete(zipPath);
                generationContext.ReleaseResources();

                var zipUrl = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddHours(1)).AbsoluteUri;
                generationContext.NotifyGenerationCompleted(FileValidationResponseDto.CreateNew(zipUrl));
                this.DisplayProcess.Kill();
            }
            catch (Exception exception)
            {
                generationContext.NotifyGenerationCompleted(FileValidationResponseDto.CreateNew(new Exception("An error occurred while validation form 24 files:" + exception.Message)));
            }
        }

        /// <summary>
        /// Generations the content of the zip from output.
        /// </summary>
        /// <param name="generationContext">The generation context.</param>
        /// <returns>The file path of the generation zip file.</returns>
        private string GenerationZipFromOutputContent(GenerationContext generationContext)
        {
            var zipPath = Path.Combine(generationContext.TempPath, $"{Guid.NewGuid()}.zip");
            var zipFile = new ZipArchive(File.Create(zipPath), ZipArchiveMode.Create);
            foreach (string path in Directory.GetFiles(generationContext.OutputLocation))
            {
                zipFile.CreateEntryFromFile(path, Path.GetFileName(path));
            }
            zipFile.Dispose();

            return zipPath;
        }

        /// <summary>
        /// Builds the execution command.
        /// </summary>
        /// <param name="generationContext">The generation context.</param>
        /// <returns>The execution command</returns>
        private string BuildExecutionCommand(GenerationContext generationContext)
        {
            var command = new StringBuilder();

            //// 1. This is the path of the fvu jar file.
            command.Append($"-jar \"{Path.Combine(generationContext.ValidationUtilityLocation, "TDS_STANDALONE_FVU_8.5.jar")}\" ");

            //// 2. This is the path of the file to be validated.
            command.Append($"\"{generationContext.FileContentLocation}\" ");

            //// 3. This is the path of the error file.
            command.Append($"\"{Path.Combine(generationContext.OutputLocation, "24Q.err")}\" ");

            //// 4. This is the path of the FVU file.
            command.Append($"\"{Path.Combine(generationContext.OutputLocation, "24Q.fvu")}\" ");

            //// 5. This is the type of validation, 0 stands for regular validation.
            command.Append("\"0\" ");

            //// 6. This is the version of the FVU file.
            command.Append("\"8.5\" ");

            //// 7. This is the path of the certificate file.
            command.Append("\"1\" ");

            //// 8. This is the path of the certificate file.
            command.Append($"\"{generationContext.CSIFileLocation}\" ");

            return command.ToString();
        }
    }
}
