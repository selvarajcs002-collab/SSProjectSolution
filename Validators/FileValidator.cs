using System;
using System.IO;

namespace SSProjectSolution.Validators
{
    public class FileValidator : IFileValidator
    {
        public (bool IsValid, string ErrorMessage) ValidateFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return (false, "Folder path cannot be empty.");
            }

            try
            {
                string root = Path.GetPathRoot(folderPath);
                if (!string.IsNullOrEmpty(root))
                {
                    DriveInfo drive = new DriveInfo(root);
                    if (!drive.IsReady)
                    {
                        return (false, $"Drive {root} is not ready or does not exist.");
                    }
                    
                    if (drive.AvailableFreeSpace < 10485760) // Require at least 10MB free
                    {
                        return (false, $"Insufficient disk space on {root}.");
                    }
                }
                
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Folder validation failed: {ex.Message}");
            }
        }

        public (bool IsValid, string ErrorMessage) ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return (false, "File name cannot be empty.");
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return (false, "File name contains invalid characters.");
            }

            return (true, string.Empty);
        }
    }
}
