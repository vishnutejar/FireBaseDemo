using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace FireBaseDemo
{
    public partial class MainPage : ContentPage
    {
        FirebaseStorage firebaseStorage;
        public Stream Stream;
        public string FileName;
        public MainPage()
        {
            InitializeComponent();
        }

        private void SelectedImageFromFile(object sender, EventArgs e)
        {
            CheckAndRequestStoragePermission();
        }
        public async Task<PermissionStatus> CheckAndRequestStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            if (status == PermissionStatus.Granted)
            {

                var customFileType =
    new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.iOS, new[] { "public.my.comic.extension" } }, // or general UTType values
        { DevicePlatform.Android, new[] { "*/*" } },
        { DevicePlatform.UWP, new[] { ".cbr", ".cbz" } },
        { DevicePlatform.Tizen, new[] { "*/*" } },
        { DevicePlatform.macOS, new[] { "cbr", "cbz" } }, // or general UTType values
    });
                var options = new PickOptions
                {
                    PickerTitle = "Please select a comic file",
                    FileTypes = customFileType,
                };
                PickAndShow(options);
            }

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                return status;
            }

            if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
            {
                // Prompt the user with additional information as to why the permission is needed
            }

            status = await Permissions.RequestAsync<Permissions.StorageRead>();

            return status;
        }
        async Task<FileResult> PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    filename.Text = $"File Name: {result.FileName}";
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        var stream = await result.OpenReadAsync();
                        selectedImage.Source = ImageSource.FromStream(() => stream);
                        Stream = stream;
                        FileName = result.FileName;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }
        public async Task<string> UploadFile(Stream fileStream, string fileName)
        {
            firebaseStorage = new
            FirebaseStorage("fir-demo-2cc0a.appspot.com");
            //gs://fir-demo-2cc0a.appspot.com
            var imageurl =   firebaseStorage
                    .Child("FileUploading")
                    .Child(fileName)
                    .PutAsync(fileStream);
            var data = imageurl;
            return data.TargetUrl;

        }

        private void UploadAFile(object sender, EventArgs e)
        {
            UploadFile(Stream, FileName);

        }

        private void Downloaded(object sender, EventArgs e)
        {
            firebaseStorage.Child("XamarinMonkeys").Child(FileName).GetDownloadUrlAsync();
        }

        private void Deleted(object sender, EventArgs e)
        {
            firebaseStorage.Child("XamarinMonkeys").Child(FileName).DeleteAsync();
        }
    }
}
