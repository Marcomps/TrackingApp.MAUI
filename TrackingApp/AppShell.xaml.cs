namespace TrackingApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(AlimentoGraficasPage),  typeof(AlimentoGraficasPage));
		Routing.RegisterRoute(nameof(CrecimientoPage),       typeof(CrecimientoPage));
		Routing.RegisterRoute(nameof(CrecimientoFormPage),   typeof(CrecimientoFormPage));
		Routing.RegisterRoute(nameof(PreferenciasPage),      typeof(PreferenciasPage));
		// RF-001: Perfiles
		Routing.RegisterRoute(nameof(PerfilesPage),          typeof(PerfilesPage));
		Routing.RegisterRoute(nameof(PerfilFormPage),        typeof(PerfilFormPage));
	}
}
