using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class Citizen : MonoBehaviour, ICitizen {
        private GameManager _gameManager;
        private int _infectionDuration;
        public bool IsContagious;
        private MeshRenderer _meshRenderer;
        private Rigidbody _rigidBody;
        private int _timeUntilContagiousInSeconds;
        private int _timeUntilSymptomaticInSeconds;

        public Material InfectedMaterial;
        public Material InfectedNotContagiousMaterial;
        public Material InfectedSymptomaticMaterial;

        public bool IsSymptomatic { get; private set; }
        public Material HealedMaterial;
        public Material RecoveredMaterial;
        public Material SusceptibleMaterial;
        public HealthStatus HealthStatus { get; private set; }

        public void Infect(float chance) {
            if (HealthStatus != HealthStatus.Susceptible || Random.value > chance) {
                return;
            }

            StartInfectionProcess();
        }

        public bool ReportStats { get; set; }

        public void Kill() {
            Destroy(gameObject);
        }

        private void Awake() {
            _gameManager = FindObjectOfType<GameManager>();
            _rigidBody = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _rigidBody.velocity = RandomVector(-1f, 1f);
            _rigidBody.constraints = RigidbodyConstraints.FreezePositionY;
            _infectionDuration = Random.Range(_gameManager.InfectedDurationMinSeconds,
                _gameManager.InfectedDurationMaxSeconds);
            _timeUntilContagiousInSeconds = _gameManager.TimeUntilContagiousInSeconds;
            _timeUntilSymptomaticInSeconds = _gameManager.TimeUntilSymptomaticInSeconds;
            _meshRenderer.material = SusceptibleMaterial;
            _gameManager.OnRestart += (sender, e) => Kill();
        }

        private void Update() {
            if (IsSymptomatic && _gameManager.UseQuarantine) {
                Vector3 direction = (GetQuarantinePosition() - _rigidBody.position).normalized;
                _rigidBody.MovePosition(_rigidBody.position +
                    direction * _gameManager.SpeedOfCitizen * Time.deltaTime);

                _rigidBody.velocity = new Vector3(0, 0, 0);
            } else {
                _rigidBody.velocity = _gameManager.SpeedOfCitizen * _rigidBody.velocity.normalized;
            }
        }

        private Vector3 GetQuarantinePosition() {
            Vector3 currentPosition = _rigidBody.position;

            float distanceToWallX = 1 - Math.Abs(currentPosition.x);
            float distanceToWallZ = 1 - Math.Abs(currentPosition.z);

            if (distanceToWallX < distanceToWallZ) {
                if (currentPosition.x > 0) {
                    return new Vector3(_gameManager.SizeX / 2f, 0, currentPosition.z);
                }

                return new Vector3(_gameManager.SizeX / 2 * -1, 0, currentPosition.z);
            }

            if (currentPosition.z > 0) {
                return new Vector3(currentPosition.x, 0, _gameManager.SizeZ / 2f);
            }

            return new Vector3(currentPosition.x, 0, _gameManager.SizeZ / 2 * -1);
        }

        private void OnCollisionEnter(Collision collision) {
            if (!IsContagious) {
                return;
            }

            Citizen citizen = collision.GetContact(0).otherCollider.GetComponent<Citizen>();
            if (citizen != null) {
                citizen.Infect(_gameManager.ChanceOfInfection);
            }

            CitizenAgent citizenAgent = collision.GetContact(0).otherCollider.GetComponent<CitizenAgent>();
            if (citizenAgent != null) {
                citizenAgent.Infect(_gameManager.ChanceOfInfection);
            }
        }

        private static Vector3 RandomVector(float min, float max) {
            float x = Random.Range(min, max);
            float z = Random.Range(min, max);
            return new Vector3(x, 0, z);
        }

        protected void StartInfectionProcess() {
            HealthStatus = HealthStatus.Infected;
            _meshRenderer.material = InfectedNotContagiousMaterial;
            StartCoroutine(StartBecomingContagious());
            StartCoroutine(StartBecomingSymptomatic());
            StartCoroutine(StartRecovering());
        }
        public void StopInfectionProcess() {
            StopAllCoroutines();
            _meshRenderer.material = HealedMaterial;
            HealthStatus = HealthStatus.Recovered;
            IsContagious = false;
            IsSymptomatic = false;
            gameObject.tag = GameManager.CITIZEN_TAG;
            gameObject.layer = 10;
        }

        protected IEnumerator StartBecomingContagious() {
            yield return new WaitForSeconds(_timeUntilContagiousInSeconds);
            IsContagious = true;
            _meshRenderer.material = InfectedMaterial;
        }

        protected IEnumerator StartBecomingSymptomatic() {
            yield return new WaitForSeconds(_timeUntilSymptomaticInSeconds);
            IsSymptomatic = true;
            _meshRenderer.material = InfectedSymptomaticMaterial;
            gameObject.tag = GameManager.CITIZEN_SYMPTOMATIC_TAG;
            gameObject.layer = 11;
        }

        protected IEnumerator StartRecovering() {
            yield return new WaitForSeconds(_infectionDuration - _timeUntilContagiousInSeconds);
            _meshRenderer.material = RecoveredMaterial;
            HealthStatus = HealthStatus.Recovered;
            IsContagious = false;
            IsSymptomatic = false;
            gameObject.tag = GameManager.CITIZEN_TAG;
            gameObject.layer = 10;
        }
    }
}
