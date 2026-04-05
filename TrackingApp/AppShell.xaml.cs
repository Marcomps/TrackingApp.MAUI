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
		// RF-001: Perfiles (PerfilesPage is a tab; PerfilFormPage is a modal route)
		Routing.RegisterRoute(nameof(PerfilFormPage),        typeof(PerfilFormPage));
		// RF-003: Alimentos
		Routing.RegisterRoute(nameof(AlimentoListPage),      typeof(AlimentoListPage));
		Routing.RegisterRoute(nameof(AlimentoFormPage),      typeof(AlimentoFormPage));
		// RF-004: Citas Médicas
		Routing.RegisterRoute(nameof(CitasListPage),         typeof(CitasListPage));
		Routing.RegisterRoute(nameof(CitaFormPage),          typeof(CitaFormPage));
		// Gráficas unificadas
		Routing.RegisterRoute(nameof(GraficasPage),          typeof(GraficasPage));
		// RF-002: Gráficas de crecimiento
		Routing.RegisterRoute(nameof(CrecimientoGraficasPage), typeof(CrecimientoGraficasPage));
		// Salud: medicamentos y confirmación de dosis
		Routing.RegisterRoute(nameof(SaludPage),             typeof(SaludPage));
	}
}
