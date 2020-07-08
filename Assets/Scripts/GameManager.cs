using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class GameManager : MonoBehaviour {
        public const string CITIZEN_SYMPTOMATIC_TAG = "citizenSymptomatic";
        public const string CITIZEN_TAG = "citizen";

        private readonly Dictionary<HealthStatus, int> _statusCount = new Dictionary<HealthStatus, int>();
        private ICitizen[] _citizenAgents;
        private Medic[] _medics;
        private bool _restartRequested;
        private List<(int, int)> _spawns = new List<(int, int)>();

        public float ChanceOfInfection = 0.2f;
        public float MedicChanceOfInfection = 0.2f;
        public Citizen Citizen;
        public CitizenAgent CitizenAgent;
        public ExtrovertedAgent ExtrovertedAgent;
        public IntrovertedAgent IntrovertedAgent;
        public Medic Medic;
        public Ambulance Ambulance;

        public int NumberOfNormalCitizen = 50;
        public int NumberOfExtrovertedCitizen = 25;
        public int NumberOfIntrovertedCitizen = 25;

        public int InfectedNormalCitizenAtStart = 1;
        public int InfectedExtrovertedCitizenAtStart = 0;
        public int InfectedIntrovertedCitizenAtStart = 0;

        public bool UseUserControl = true;

        public int InfectedDurationMaxSeconds = 10;
        public int InfectedDurationMinSeconds = 5;
        public float IntervalToDrawGraphInSeconds = 1;

        public int NumberOfMedics = 1;
        public float ReloadIntervalInSeconds = 2;
        public int SizeX = 100;
        public int SizeZ = 100;
        public int SpeedOfCitizen = 25;
        public int SpeedOfMedic = 30;
        public int SpeedOfAmbulance = 60;
        public int TimeUntilContagiousInSeconds = 2;
        public int TimeUntilSymptomaticInSeconds = 5;
        public bool UseMlAgents = false;
        public bool UseQuarantine = true;

        [HideInInspector] public Dictionary<HealthStatus, int> Collisions;
        [HideInInspector] public int CollisionsSymptomatic;
        [HideInInspector] public int CollisionsTotal;
        [HideInInspector] public List<float> RelativeSurvivalLength = new List<float>();
        [HideInInspector] public int HealedCounter;
        public int InfectedDuration => Random.Range(InfectedDurationMinSeconds, InfectedDurationMaxSeconds);

        public event EventHandler OnRefresh;
        public event EventHandler OnRestart;

        // ReSharper disable once UnusedMember.Local
        private void Start() {
            foreach (HealthStatus healthStatus in Enum.GetValues(typeof(HealthStatus))) {
                _statusCount.Add(healthStatus, 0);
            }

            SpawnCitizens();
            HealedCounter = 0;
            InvokeRepeating(nameof(Refresh), IntervalToDrawGraphInSeconds, IntervalToDrawGraphInSeconds);
        }

        private void SpawnCitizens() {
            InitializeLogging();

            _citizenAgents = new ICitizen[NumberOfNormalCitizen + NumberOfExtrovertedCitizen + NumberOfIntrovertedCitizen];
            for (int i = 0; i < NumberOfNormalCitizen - InfectedNormalCitizenAtStart; i++) {
                _citizenAgents[i] = SpawnNormalCitizen();
            }
            for (int i = 0; i < NumberOfExtrovertedCitizen - InfectedExtrovertedCitizenAtStart; i++) {
                _citizenAgents[i + NumberOfNormalCitizen] = SpawnExtrovertedCitizen();
            }
            for (int i = 0; i < NumberOfIntrovertedCitizen - InfectedIntrovertedCitizenAtStart; i++) {
                _citizenAgents[i + NumberOfExtrovertedCitizen + NumberOfNormalCitizen] = SpawnIntrovertedCitizen();
            }

            for (int i = 0; i < InfectedNormalCitizenAtStart; i++) {
                ICitizen citizenToInfect = SpawnNormalCitizen();
                citizenToInfect.Infect(1);
                _citizenAgents[NumberOfNormalCitizen - InfectedNormalCitizenAtStart + i] = citizenToInfect;
            }
            for (int i = NumberOfNormalCitizen; i < NumberOfNormalCitizen + InfectedExtrovertedCitizenAtStart; i++) {
                ICitizen citizenToInfect = SpawnExtrovertedCitizen();
                citizenToInfect.Infect(1);
                _citizenAgents[NumberOfExtrovertedCitizen - InfectedExtrovertedCitizenAtStart + i] = citizenToInfect;
            }
            for (int i = NumberOfNormalCitizen + NumberOfExtrovertedCitizen; i < NumberOfNormalCitizen + NumberOfExtrovertedCitizen + InfectedIntrovertedCitizenAtStart; i++) {
                ICitizen citizenToInfect = SpawnIntrovertedCitizen();
                citizenToInfect.Infect(1);
                _citizenAgents[NumberOfIntrovertedCitizen - InfectedIntrovertedCitizenAtStart + i] = citizenToInfect;
            }

            _citizenAgents[_citizenAgents.Length - 1].ReportStats = true;

            _medics = new Medic[NumberOfMedics];
            for (int i = 0; i < NumberOfMedics; i++)
            {
                _medics[i] = SpawnMedic();
            }
        }

        private void InitializeLogging()
        {
            Collisions = new Dictionary<HealthStatus, int>();
            foreach (HealthStatus healthStatus in Enum.GetValues(typeof(HealthStatus)))
            {
                Collisions.Add(healthStatus, 0);
            }

            CollisionsTotal = 0;
            CollisionsSymptomatic = 0;

            RelativeSurvivalLength = new List<float>();
        }

        private ICitizen SpawnNormalCitizen() {
            if (UseMlAgents) {
                CitizenAgent citizenAgent = Instantiate(CitizenAgent, GetNextSpawnPosition(), Quaternion.identity);
                citizenAgent.transform.SetParent(transform);
                return citizenAgent;
            } else {
                Citizen citizenAgent = Instantiate(Citizen, GetNextSpawnPosition(), Quaternion.identity);
                citizenAgent.transform.SetParent(transform);
                return citizenAgent;
            }
        }

        private ICitizen SpawnExtrovertedCitizen() {
            if (UseMlAgents) {
                CitizenAgent extrovertedAgent = Instantiate(ExtrovertedAgent, GetNextSpawnPosition(), Quaternion.identity);
                extrovertedAgent.transform.SetParent(transform);
                return extrovertedAgent;
            } else {
                Citizen extrovertedAgent = Instantiate(Citizen, GetNextSpawnPosition(), Quaternion.identity);
                extrovertedAgent.transform.SetParent(transform);
                return extrovertedAgent;
            }
        }

        private ICitizen SpawnIntrovertedCitizen() {
            if (UseMlAgents) {
                CitizenAgent introvertedCitizen = Instantiate(IntrovertedAgent, GetNextSpawnPosition(), Quaternion.identity);
                introvertedCitizen.transform.SetParent(transform);
                return introvertedCitizen;
            } else {
                Citizen introvertedCitizen = Instantiate(Citizen, GetNextSpawnPosition(), Quaternion.identity);
                introvertedCitizen.transform.SetParent(transform);
                return introvertedCitizen;
            }
        }

        private Medic SpawnMedic() {
            Medic medic = Instantiate(Medic, GetNextSpawnPosition(), Quaternion.Euler(0f, 0f, 0f));
            medic.transform.SetParent(transform);
            return medic;
        }

        private Vector3 GetNextSpawnPosition() {
            (int, int) position;
            int x, z;
            do {
                x = (int) (Random.Range(-1f, 1f) * SizeX / 2);
                z = (int) (Random.Range(-1f, 1f) * SizeZ / 2);
                position = (x, z);
            } while (_spawns.Contains(position));

            _spawns.Add(position);
            Vector3 globalPosition = gameObject.transform.parent.position;
            return new Vector3(x, 1, z) + globalPosition;
        }

        private IEnumerator Restart() {
            _restartRequested = true;
            yield return new WaitForSeconds(ReloadIntervalInSeconds);
            _spawns = new List<(int, int)>();

            OnRestart?.Invoke(this, EventArgs.Empty);
            OnRestart = null;

            OnRestart += (sender, e) => GameObject.FindGameObjectWithTag("infectionGraph").GetComponent<InfectionGraph>().Reset();

            SpawnCitizens();
            HealedCounter = 0;
            _restartRequested = false;
        }

        public void CountInfections() {
            foreach (HealthStatus healthStatus in Enum.GetValues(typeof(HealthStatus))) {
                _statusCount[healthStatus] = 0;
            }

            foreach (ICitizen citizen in _citizenAgents) {
                _statusCount[citizen.HealthStatus]++;
            }
        }

        public int GetCount(HealthStatus healthStatus) {
            return _statusCount[healthStatus];
        }

        private void Refresh() {
            if (_restartRequested) {
                return;
            }

            CountInfections();

            OnRefresh?.Invoke(this, EventArgs.Empty);

            if (_statusCount[HealthStatus.Infected] == 0) {
                StartCoroutine(Restart());
            }
        }
    }
}
