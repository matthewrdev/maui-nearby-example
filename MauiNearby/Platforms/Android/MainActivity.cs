using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using MauiNearby.Services;

namespace MauiNearby;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private const int REQUEST_CODE_REQUIRED_PERMISSIONS = 1;


    protected override void OnCreate(Bundle savedInstanceState)
    {
        NearbySyncService.Instance.Initialise(this, "com.nearby.sync.sample", (DeviceInfo.Manufacturer + DeviceInfo.Model).Replace(" ", ""));

        base.OnCreate(savedInstanceState);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}

