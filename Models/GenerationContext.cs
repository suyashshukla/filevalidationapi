using System.Reflection;

namespace FileValidation.API.Models
{
    /// <summary>
    /// This class represents the generation context.
    /// </summary>
    public class GenerationContext
    {
        /// <summary>
        /// Gets or sets the output location.
        /// </summary>
        /// <value>
        /// The output location.
        /// </value>
        public string OutputLocation { get; set; }

        /// <summary>
        /// Gets or sets the application root.
        /// </summary>
        /// <value>
        /// The application root.
        /// </value>
        public string AppRoot { get; set; }

        /// <summary>
        /// Gets or sets the temporary path.
        /// </summary>
        /// <value>
        /// The temporary path.
        /// </value>
        public string TempPath { get; set; }

        /// <summary>
        /// Gets or sets the validation utility location.
        /// </summary>
        /// <value>
        /// The validation utility location.
        /// </value>
        public string ValidationUtilityLocation { get; set; }

        /// <summary>
        /// Gets or sets the identification number.
        /// </summary>
        /// <value>
        /// The identification number.
        /// </value>
        public string IdentificationNumber { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is execution completed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is execution completed; otherwise, <c>false</c>.
        /// </value>
        public bool IsExecutionCompleted { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is callback execution in progress.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is callback execution in progress; otherwise, <c>false</c>.
        /// </value>
        public bool IsCallbackExecutionInProgress { get; private set; }

        /// <summary>
        /// Occurs when [on generation completed].
        /// </summary>
        public event EventHandler<FileValidationResponseDto> OnGenerationCompleted = (sender, response) => { };

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationContext"/> class.
        /// </summary>
        /// <param name="identificationNumber">The identification number.</param>
        public GenerationContext(string identificationNumber)
        {
            this.TempPath = Path.GetTempPath();
            this.AppRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.OutputLocation = Path.Combine(this.TempPath, Guid.NewGuid().ToString().Replace("-", string.Empty));
            this.ValidationUtilityLocation = Path.Combine(this.AppRoot, "ValidationUtility");
            this.IdentificationNumber = identificationNumber;
        }

        /// <summary>
        /// Gets the file content location.
        /// </summary>
        /// <value>
        /// The file content location.
        /// </value>
        public string FileContentLocation => Path.Combine(this.OutputLocation, "24Q.txt");

        /// <summary>
        /// Gets the csi file location.
        /// </summary>
        /// <value>
        /// The csi file location.
        /// </value>
        public string CSIFileLocation => Path.Combine(this.OutputLocation, $"{this.IdentificationNumber}.csi");

        /// <summary>
        /// Gets the certicicate location.
        /// </summary>
        /// <value>
        /// The certicicate location.
        /// </value>
        public string CerticicateLocation => Path.Combine(this.ValidationUtilityLocation, "e-mudhra.cer");

        /// <summary>
        /// Releases the resources.
        /// </summary>
        public void ReleaseResources()
        {
            File.Delete(this.FileContentLocation);
            File.Delete(this.CSIFileLocation);
            Directory.Delete(this.OutputLocation, true);
        }

        /// <summary>
        /// Notifies the generation completed.
        /// </summary>
        /// <param name="response">The response.</param>
        public void NotifyGenerationCompleted(FileValidationResponseDto response)
        {
            this.OnGenerationCompleted(this, response);
        }

        /// <summary>
        /// Sets the is execution completed.
        /// </summary>
        public void SetIsExecutionCompleted()
        {
            this.IsExecutionCompleted = true;
        }

        /// <summary>
        /// Sets the is callback execution in progress.
        /// </summary>
        public void SetIsCallbackExecutionInProgress()
        {
            this.IsCallbackExecutionInProgress = true;
        }
    }
}
