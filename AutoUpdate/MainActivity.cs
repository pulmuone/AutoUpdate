using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Java.IO;
using Android.Support.V4.Content;
using Android.Content;
using Android.Content.PM;
using System;
using Android.Database;
using System.Collections.Generic;
using Android.Support.V4.App;
using Android;

namespace AutoUpdate
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private DownloadReceiver receiver;
        public static long downloadId = -1L;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //var apkPath = FilesDir.AbsolutePath + "/app.apk";

            //using (Java.IO.File fileDel = new Java.IO.File(apkPath.ToString()))
            //{
            //    if (!fileDel.Exists())
            //    {
            //        CopyApkToAppFolder();
            //    }
            //}

            FileDelete();
            
            var btnDownload = FindViewById<TextView>(Resource.Id.button1);
            btnDownload.Click += BtnDownload_Click;
            //InstallApk();
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            FileDelete();

            var manager = DownloadManager.FromContext(this);

            var url = new System.Uri(string.Format(@"{0}{1}", "http://172.34.34.144:8080/barcodescanner", string.Format("{0}{1}{2}", "/", Application.Context.PackageName, ".apk")));
            //var url = new System.Uri(string.Format(@"{0}{1}", "http://172.34.34.144:8080/barcodescanner", string.Format("{0}{1}", "/", "com.daesangit.barcodescanner.apk")));

            try
            {
                var request = new DownloadManager.Request(Android.Net.Uri.Parse(url.ToString()));
                request.SetMimeType("application/vnd.android.package-archive");
                request.SetTitle("App Download");
                request.SetDescription("File Downloading...");
                request.SetAllowedNetworkTypes(DownloadNetwork.Wifi | DownloadNetwork.Mobile);
                request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
                request.SetAllowedOverMetered(true);
                request.SetAllowedOverRoaming(true);

                request.SetDestinationUri(Android.Net.Uri.FromFile(new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + string.Format("{0}{1}{2}", "/", Application.Context.PackageName, ".apk"))));

                System.Console.WriteLine(Android.OS.Environment.DirectoryDownloads);
                System.Console.WriteLine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath);

                downloadId = manager.Enqueue(request);
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }


        protected override void OnStart()
        {
            base.OnStart();
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M) //23이상부터
            {
                List<string> permissions = new List<string>();

                if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
                {
                    permissions.Add(Manifest.Permission.WriteExternalStorage);
                }

                if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
                {
                    permissions.Add(Manifest.Permission.ReadExternalStorage);
                }


                if (permissions.Count > 0)
                {
                    ActivityCompat.RequestPermissions(this, permissions.ToArray(), 1);
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Registers BroadcastReceiver to respond to completed downloads.
            var filter = new IntentFilter(DownloadManager.ActionDownloadComplete);
            receiver = new DownloadReceiver();
            receiver.DownloadCompleted += Receiver_DownloadCompleted;

            RegisterReceiver(receiver, filter);
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Unregister the BroadcastReceiver when app is destroyed.
            if (receiver != null)
            {
                UnregisterReceiver(receiver);
            }
        }

        private void Receiver_DownloadCompleted()
        {
            //var apkPath = FilesDir.AbsolutePath + "/app.apk";
            //var apkUri = Android.Support.V4.Content.FileProvider.GetUriForFile(this.ApplicationContext, string.Format("{0}{1}", Application.Context.PackageName, ".fileprovider"), new Java.IO.File(apkPath));

            Intent intent = new Intent(Intent.ActionView);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask | ActivityFlags.SingleTop | ActivityFlags.ClearTop);
            intent.PutExtra(Intent.ExtraNotUnknownSource, true);
            //intent.SetDataAndType(apkUri, this.ContentResolver.GetType(apkUri));

            var apkUri = Android.Support.V4.Content.FileProvider.GetUriForFile(
                                this.ApplicationContext,
                                string.Format("{0}{1}", Application.Context.PackageName, ".fileprovider"),
                                new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads) + string.Format("{0}{1}{2}", "/", Application.Context.PackageName, ".apk")));
            intent.SetDataAndType(apkUri, this.ContentResolver.GetType(apkUri));

            StartActivity(intent);
        }

        private void FileDelete()
        {
            Java.IO.File file = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).ToString());
            IFilenameFilter filter = new AutoUpdateFileFilter();
            Java.IO.File[] files = file.ListFiles(filter);

            if (files != null && files.Length > 0)
            {
                foreach (var item in files)
                {
                    if (item.IsFile)
                    {
                        if (item.Name.ToString().StartsWith(Application.Context.PackageName))
                        {
                            using (Java.IO.File fileDel = new Java.IO.File(item.ToString()))
                            {
                                if (fileDel.Exists())
                                {
                                    fileDel.Delete();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InstallApk()
        {
            var apkPath = FilesDir.AbsolutePath + "/app.apk";
            var apkUri = Android.Support.V4.Content.FileProvider.GetUriForFile(this.ApplicationContext, string.Format("{0}{1}", Application.Context.PackageName, ".fileprovider"), new Java.IO.File(apkPath));

            Intent intent = new Intent(Intent.ActionView);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask);
            intent.PutExtra(Intent.ExtraNotUnknownSource, true);
            intent.SetDataAndType(apkUri, this.ContentResolver.GetType(apkUri));

            StartActivity(intent);
        }

        private void CopyApkToAppFolder()
        {
            var inputStream = Assets.Open("app.apk");
            var outPath = FilesDir.AbsolutePath + "/app.apk";
            var outputStream = new Java.IO.FileOutputStream(outPath);

            byte[] buffer = new byte[1024];
            int n;
            while ((n = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                outputStream.Write(buffer, 0, n);
            }

            inputStream.Close();
            outputStream.Close();
        }


        public delegate void DownloadCompletedEventHandler();
        private class DownloadReceiver : BroadcastReceiver
        {
            public event DownloadCompletedEventHandler DownloadCompleted;

            public DownloadReceiver()
            {

            }

            public override void OnReceive(Context context, Intent intent)
            {
                long id = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);

                System.Console.WriteLine(string.Format("Received intent for {0}:\n", id));

                if (DownloadManager.ActionDownloadComplete.Equals(intent.Action))
                {
                    if (downloadId == id)
                    {
                        var manager = DownloadManager.FromContext(context);
                        var query = new DownloadManager.Query();
                        query.SetFilterById(id);
                        ICursor cursor = manager.InvokeQuery(query);
                        if (cursor.MoveToFirst())
                        {
                            // get the status
                            var columnIndex = cursor.GetColumnIndex(DownloadManager.ColumnStatus);
                            var status = (DownloadStatus)cursor.GetInt(columnIndex);

                            System.Console.WriteLine(string.Format("  Received status {0}\n", status));

                            if (status == Android.App.DownloadStatus.Successful)
                            {
                                DownloadCompleted?.Invoke();
                            }
                        }
                    }
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
    public class AutoUpdateFileFilter : Java.Lang.Object, Java.IO.IFilenameFilter
    {
        public bool Accept(Java.IO.File dir, string name)
        {
            if (name.StartsWith(Application.Context.PackageName) && name.EndsWith(".apk"))
            {
                return true;
            }
            return false;
        }
    }
}