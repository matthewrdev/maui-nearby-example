using MauiNearby.Services;

namespace MauiNearby;

public partial class MainPage : ContentPage
{
	int count = 0;

    MainViewModel ViewModel => BindingContext as MainViewModel;

    public MainPage()
	{
		InitializeComponent();
		BindingContext = new MainViewModel();
	}

    bool hasStarted = false;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!hasStarted)
        {
            hasStarted = true;
            if (!NearbySyncService.Instance.VerifyPermissions())
            {
                Task.Run(async () =>
                {
                    await Task.Delay(1200);
                    NearbySyncService.Instance.TryRequestPermissions();
                });
            }
            else
            {
                NearbySyncService.Instance.Start();
            }

            ViewModel.RaiseEvents();
        }
    }
}


