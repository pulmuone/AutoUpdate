using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Java.IO;
using Android.Support.V4.Content;
using Android.Content;
using Android.Content.PM;

namespace AutoUpdate
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        int REQUEST_INSTALL_PERMISSION = 10;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var apkPath = FilesDir.AbsolutePath + "/app.apk";

            using (File fileDel = new File(apkPath.ToString()))
            {
                if (!fileDel.Exists())
                {
                    CopyApkToAppFolder();
                }
            }

            InstallApk();

            if (PackageManager.CanRequestPackageInstalls())
            {
                InstallApk();
            }
           
        }
        
        private void InstallApk()
        {
            //var apkPath = FilesDir.AbsolutePath + "/app.apk";
            var apkUri = FileProvider.GetUriForFile(
                                                    this.ApplicationContext,
                                                    "com.gwise.autoupdate.fileprovider",
                                                    new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads) + "app.apk")
                                                    );

            Intent intent = new Intent(Intent.ActionView);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask);
            intent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
            
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

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            //if (requestCode == OPEN_DIRECTORY_REQUEST_CODE && resultCode == Result.Ok)
            //{
            //    this.ContentResolver.TakePersistableUriPermission(data.Data, ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            //}
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}