using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;

namespace GoogleDriveAPI_v03.Models
{
    public class FilesDriveRepository
    {
        //defined scope. Drive - View and manage the files in my Google Drive.
        public static string[] Scopes =
            { DriveService.Scope.Drive };

        //create Drive API service.
        [Obsolete] //asks first credential [Obsolete("Мой метод устарел. Не используйте!!!", false)] - warning, but if 'true' - error
        public static DriveService GetService()
        {
            //get Credentials from client_secret.json file 
            UserCredential credential;
            using (var stream = new FileStream(@"D:\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                String FolderPath = @"D:\";
                String FilePath = Path.Combine(FolderPath, "GoogleDriveServiceCredentials.json"); //storing in a new file

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;
            }

            //creating Drive API service. Data service constructor
            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential, //нужно для авторизации вызовов любого google cloud api
                ApplicationName = "GoogleDriveAPI_v03",
            });
            return service;
        }

        //get all files from Google Drive.
        [Obsolete]
        public static List<FilesFromDrive> GetDriveFiles()
        {
            DriveService service = GetService();

            // define parameters of request.
            FilesResource.ListRequest FileListRequest = service.Files.List();

            //listRequest.PageSize = 10; można ograniczyć listę

            FileListRequest.Fields = "nextPageToken, files(id, name, size, version, createdTime)"; //properties of files Im using

            //get file list.
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            List<FilesFromDrive> fileList = new List<FilesFromDrive>();

            //инициализация каждого свойства (property)
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    FilesFromDrive File = new FilesFromDrive
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime
                    };
                    fileList.Add(File);
                }
            }
            return fileList;
        }

        //file Upload to the Google Drive.
        [Obsolete]
        public static void FileUpload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                DriveService service = GetService();

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/GoogleDriveFiles"),
                Path.GetFileName(file.FileName));
                file.SaveAs(path);

                //opis pliku dla google api
                var FileMetaData = new Google.Apis.Drive.v3.Data.File();
                FileMetaData.Name = Path.GetFileName(file.FileName);
                FileMetaData.MimeType = MimeMapping.GetMimeMapping(path);

                FilesResource.CreateMediaUpload request;
                
                //wrzucenie pliku do google drive
                using (var stream = new FileStream(path, FileMode.Open)) //za pomocą biblioteki System.IO
                {
                    request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                    request.Fields = "id";
                    request.Upload();
                }
            }
        }

        //pobiranie pliku z Google Drive z wykorzystaniem fileId
        [Obsolete]
        public static string DownloadGoogleFile(string fileId)
        {
            DriveService service = GetService();

            string FolderPath = HttpContext.Current.Server.MapPath("/GoogleDriveFiles/");
            FilesResource.GetRequest request = service.Files.Get(fileId);

            string FileName = request.Execute().Name;
            string FilePath = Path.Combine(FolderPath, FileName);

            MemoryStream stream1 = new MemoryStream();

            request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                    {
                        Console.WriteLine(progress.BytesDownloaded);
                        break;
                    }
                    case DownloadStatus.Completed:
                    {
                        Console.WriteLine("Download complete.");
                        SaveStream(stream1, FilePath);
                        break;
                    }
                    case DownloadStatus.Failed:
                    {
                        Console.WriteLine("Download failed.");
                        break;
                    }
                }
            };
            request.Download(stream1);
            return FilePath;
        }

        // file save to server path
        private static void SaveStream(MemoryStream stream, string FilePath)
        {
            using (FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                stream.WriteTo(file);
            }
        }

        //Delete file from the Google drive
        [Obsolete]
        public static void DeleteFile(FilesFromDrive files)
        {
            DriveService service = GetService();
            try
            {
                // Sprawdzenie
                if (service == null)
                    throw new ArgumentNullException("service"); // передаче нулевой ссылки методу, метод не принимает ее в качестве допустимого аргумента. 

                if (files == null)
                    throw new ArgumentNullException(files.Id);

                // Make the request.
                service.Files.Delete(files.Id).Execute();
            }
            catch (Exception ex)
            {
                throw new Exception("Request Files: Delete failed.", ex);
            }
        }
    }
}