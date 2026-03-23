namespace TrackingApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(AlimentoGraficasPage),  typeof(AlimentoGraficasPage));
		Routing.RegisterRoute(nameof(CrecimientoPage),       typeof(CrecimientoPage));
		Routing.RegisterRoute(nameof(CrecimientoFormPage),   typeof(CrecimientoFormPage));
	}
}
