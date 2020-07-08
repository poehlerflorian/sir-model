namespace Assets.Scripts {
    public interface ICitizen {
        HealthStatus HealthStatus { get; }
        bool IsSymptomatic { get; }
        void Infect(float infectionChance);
        bool ReportStats { get; set; }
        void Kill();
        void StopInfectionProcess();
    }
}
