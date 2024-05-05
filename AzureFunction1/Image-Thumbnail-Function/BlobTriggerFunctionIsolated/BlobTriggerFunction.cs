using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace BlobTriggerImageThumbnail
{
    public class BlobTriggerFunction
    {
        private readonly ILogger<BlobTriggerFunction> _logger;

        public BlobTriggerFunction(ILogger<BlobTriggerFunction> logger)
        {
            _logger = logger;
        }

        [Function("BlobTriggerFunction")]
        [BlobOutput("image-output/thumbnail-{name}", Connection = "STORAGE_ACCOUNT_CONNECTION_STRING")]
        public static async Task<Stream> Run([BlobTrigger("image-input/{name}", Connection = "STORAGE_ACCOUNT_CONNECTION_STRING")] Stream inBlob,
        string name,
        ILogger log, FunctionContext context)
        {
            log.LogInformation($"New image uploaded in image-input container: {name}");
            Stream blob;

            using var image = Image.Load(inBlob);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size
                {
                    Height = 100,
                    Width = 100
                },
                Mode = ResizeMode.Crop
            }));

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            blob = ms;
            return blob;

        }
    }
}
