using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Components.Forms;
using Syncfusion.Presentation;
using System.Text;
using Syncfusion.PresentationRenderer;
using Shared.Models.Enums;
using System.IO;
using Hangfire.MemoryStorage.Database;
using Syncfusion.Blazor.DocumentEditor;

namespace Domain.Services.ThirdParty
{
    public interface IFileService
    {
        Task<Tuple<object, DocumentType>> GetFile(string id, string container);
        Task<List<BlobItem>> GetFiles(string dealId);
        Task UploadFile(IBrowserFile file, string containerName, string fileId, int size = 1000000, IProgress<int> progressHandler = null);
        Task UploadFile(byte[] contents, string contentType, string containerName, string fileId);
        Task<bool> DeleteFile(string dealId, string fileId);
    }

    public class FileService : IFileService
    {
        private readonly BlobServiceClient blobServiceClient;

        public FileService(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        public async Task UploadFile(IBrowserFile file, string containerName, string fileId, int BufferSize = 1000000, IProgress<int> progressHandler = null)
        {
            long filesize = file.Size;
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync();

            // Method to create a new Blob client.
            var blob = container.GetBlockBlobClient(fileId);
            // If a blob with the same name exists, then we delete the Blob and its snapshots.
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            byte[] buffer = new byte[BufferSize];
            using (var bufferedStream = new BufferedStream(file.OpenReadStream(30000000), BufferSize))
            {
                int readCount = 0;
                int bytesRead;
                long TotalBytesSent = 0;
                // track the current block number as the code iterates through the file
                int blockNumber = 0;

                // Create list to track blockIds, it will be needed after the loop
                List<string> blockList = new List<string>();

                while ((bytesRead = await bufferedStream.ReadAsync(buffer, 0, BufferSize)) > 0)
                {
                    blockNumber++;
                    // set block ID as a string and convert it to Base64 which is the required format
                    string blockId = $"{blockNumber:0000000}";
                    string base64BlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(blockId));

                    Console.WriteLine($"Read:{readCount++} {bytesRead / (double)BufferSize} MB");

                    // Do work on the block of data
                    await blob.StageBlockAsync(base64BlockId, new MemoryStream(buffer, 0, bytesRead));
                    // add the current blockId into our list
                    blockList.Add(base64BlockId);
                    TotalBytesSent += bytesRead;
                    int PercentageSent = (int)(TotalBytesSent * 100 / filesize);
                    if (progressHandler != null)
                    {
                        progressHandler.Report(PercentageSent);
                    }
                }

                await blob.CommitBlockListAsync(blockList, new BlobHttpHeaders { ContentType = file.ContentType }, metadata: new Dictionary<string, string>() { { "format", file.ContentType } });

                // make sure to dispose the stream once your are done
                bufferedStream.Dispose();   // Belt and braces
            }
        }

        public async Task UploadFile(byte[] contents, string contentType, string containerName, string fileId)
        {
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync();

            // Method to create a new Blob client.
            var blob = container.GetBlockBlobClient(fileId);
            // If a blob with the same name exists, then we delete the Blob and its snapshots.
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            using (var ms = new MemoryStream(contents, false))
            {
                await blob.UploadAsync(ms, new BlobHttpHeaders { ContentType = contentType }, metadata: new Dictionary<string, string>() { { "format", contentType } });
            }
        }

        public async Task<bool> DeleteFile(string dealId, string fileId)
        {
            try
            {
                var container = blobServiceClient.GetBlobContainerClient(dealId);

                // Method to create a new Blob client.
                var blob = container.GetBlockBlobClient(fileId);
                await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR");
                return false;
            }
        }

        public async Task<List<BlobItem>> GetFiles(string dealId)
        {
            List<BlobItem> files = new List<BlobItem>();
            if (dealId == null)
            {
                return files;
            }

            var containerClient = blobServiceClient.GetBlobContainerClient(dealId);

            try
            {
                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
                {
                    files.Add(blobItem);
                }
            }
            catch (RequestFailedException)
            {
                Console.WriteLine("ERROR");
                return files;
            }

            return files;
        }

        public async Task<Tuple<object, DocumentType>> GetFile(string id, string container)
        {
            var client = blobServiceClient.GetBlobContainerClient(container);

            BlockBlobClient blockBlobClient = client.GetBlockBlobClient(id);
            var fileContent = (await blockBlobClient.GetPropertiesAsync()).Value;
            var file = await blockBlobClient.OpenReadAsync();
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var type = fileContent.Metadata.FirstOrDefault(x => x.Key == "format").Value.ToLower();

            if (type.Contains("pdf"))
            {
                var resp = new Tuple<object, DocumentType>(memoryStream, DocumentType.Pdf);
                return resp;
            }
            else if (type.Contains("word"))
            {
                var resp = new Tuple<object, DocumentType>(memoryStream, DocumentType.Word);
                return resp;
            }
            else if (type.Contains("presentation"))
            {
                IPresentation pptxDoc = Presentation.Open(memoryStream);
                var pdf = PresentationToPdfConverter.Convert(pptxDoc);
                var strm = new MemoryStream();
                pdf.Save(strm);
                memoryStream.Dispose();
                memoryStream = null;
                file.Dispose();
                file = null;
                var resp = new Tuple<object, DocumentType>(strm, DocumentType.Powerpoint);
                return resp;
            }
            else
            {
                return null;
            }
        }
    }
}
