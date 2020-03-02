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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var apkPath = FilesDir.AbsolutePath + "/app.apk";

            using (Java.IO.File fileDel = new Java.IO.File(apkPath.ToString()))
            {
                if (!fileDel.Exists())
                {
                    CopyApkToAppFolder();
                }
            }

            InstallApk();

            //if (PackageManager.CanRequestPackageInstalls())
            //{
            //    InstallApk();
            //}
           
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
    }
}