using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Azureblob
{
    public partial class MainPage : ContentPage
    {
        MediaFile file;
        static string _storageConnection = "DefaultEndpointsProtocol=https;AccountName=xamblob;AccountKey=NNCjJFGAM2ooeClsX9aSwe25MF2xfxFPHUEe4c2dxTneTpKmJtduXWcfJzvqiX+i/nt47eJsdTWwJcOGknJh5g==;EndpointSuffix=core.windows.net";
        static CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnection);
        static CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        static CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

        
        public MainPage()
        {
            
            InitializeComponent();
        }

        private async void btnPick_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            try
            {
                file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (file == null)
                    return;
                imgchoosed.Source = ImageSource.FromStream(() =>
                {
                    var imageStram = file.GetStream();
                    return imageStram;
                });
            }
            catch(Exception ex) 
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void btnStore_clicked(object sender, EventArgs e)
        {
            if (file != null)
            {
                string filePath = file.Path;
                string fileName = Path.GetFileName(filePath);
                
                await cloudBlobContainer.CreateIfNotExistsAsync();

                await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
                var blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                var blobUrl = blockBlob.Uri.AbsoluteUri;
                await UploadImage(blockBlob, filePath);
            }
            else
            {
                await DisplayAlert("Error", "No file found to Upload", "Ok");
            }
            
        }

        private  async Task UploadImage(CloudBlockBlob blockBlob, string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {

                await blockBlob.UploadFromStreamAsync(fileStream);
                await DisplayAlert("Alert", "File Uploaded successfully", "Ok");
            }
            
        }

        private async void btnGet_clicked(object sender, EventArgs e)
        {   
            if (file != null)
            {
              
                string filePath = file.Path;
                string fileName = Path.GetFileName(filePath);
                var blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                
                await DownloadImage(blockBlob, filePath);
              
            }
            else 
            {
                await DisplayAlert("Error", "No file found to Download", "Ok");
            }
        } 

        private static async Task DownloadImage(CloudBlockBlob blockBlob, string filePath)
        {
            // MemoryStream memoryStream = new MemoryStream();

            if (blockBlob.ExistsAsync().Result)
            {
               // memoryStream.Read(filePath);

                await blockBlob.DownloadToFileAsync(filePath, FileMode.CreateNew);
            }
        }

        private async void btnDelete_clicked(object sender, EventArgs e)
        {
            if (file != null)
            {
                string filePath = file.Path;
                string fileName = Path.GetFileName(filePath);
                var blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                var result = await DeleteImage(blockBlob);
                if (result)
                {
                    await DisplayAlert("Alert", "file successfully deleted", "Ok");
                }
                else {
                    await DisplayAlert("Alert", "failed to delete", "Ok");
                }
            }
            else
            {
                 await DisplayAlert("Error","No file found to delete","Ok");
            }
            
        }

        private static async Task<bool> DeleteImage(CloudBlockBlob blockBlob)
        {
            if (blockBlob.ExistsAsync().Result)
            {
               return await blockBlob.DeleteIfExistsAsync();

            }
            return false;
        }
    }
}
