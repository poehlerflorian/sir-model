using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Assets.Scripts {
    public class Medic : CitizenAgent {
        private Ambulance _ambulance;
        private int _driveCount;
        private bool _isDrivingAmbulance;
        private StatsRecorder _stats;
        private float _timer;
        private int _walkCount;

        public RayPerceptionSensorComponent3D AmbulanceRay;

        private void Awake() {
            _timer = 0.0f;
            GameManager = FindObjectOfType<GameManager>();
            RigidBody = GetComponent<Rigidbody>();
            MeshRenderer = GetComponent<MeshRenderer>();

            MeshRenderer.material = SusceptibleMaterial;
            GameManager.OnRestart += (sender, e) => Kill();
            _stats = Academy.Instance.StatsRecorder;
            _ambulance = Instantiate(GameManager.Ambulance, transform.position, Quaternion.Euler(0f, 0f, 0f));
            _ambulance.transform.SetParent(GameManager.transform);
            RigidBody.freezeRotation = true;
            //HealedCounter = 0;
            _driveCount = 0;
            _walkCount = 0;
        }

        protected override void OnCollisionEnter(Collision collision) {
            if (!_isDrivingAmbulance) {
                ICitizen citizenAgent = collision.GetContact(0).otherCollider.GetComponent<ICitizen>();
                if (citizenAgent != null) {
                    if (citizenAgent.HealthStatus == HealthStatus.Infected && citizenAgent.IsSymptomatic) {
                        citizenAgent.StopInfectionProcess();
                        GameManager.HealedCounter++;
                        AddReward(0.4f);
                    }
                    if (IsContagious) {
                        citizenAgent.Infect(GameManager.ChanceOfInfection);
                    }
                }
            }
        }

        private void EnterAmbulance() {
            if(Vector3.Distance(transform.position, _ambulance.transform.position) < 5) {
                _isDrivingAmbulance = true;
                transform.position = new Vector3(_ambulance.transform.position.x, transform.position.y, _ambulance.transform.position.z);
                _ambulance.transform.SetParent(transform);
                AmbulanceRay.RayLength = 40f;
            }
        }

        private void GetOutOfAmbulance() {
            _isDrivingAmbulance = false;
            _ambulance.transform.SetParent(GameManager.transform);
            AmbulanceRay.RayLength = 0f;
        }

        public override void Infect(float chance) {
            chance = GameManager.MedicChanceOfInfection;
            if (HealthStatus != HealthStatus.Susceptible || Random.value > chance) {
                return;
            }
            StartInfectionProcess();
        }
        public override void Kill() {
            _stats.Add("Healed", GameManager.HealedCounter);
            _stats.Add("RelativeDriveTime", _driveCount/_timer);
            _stats.Add("RelativeWalkTime", _walkCount/_timer);
            AddReward(-_timer/180);
            EndEpisode();
            Destroy(_ambulance.gameObject);
            Destroy(gameObject);
        }
        public override void CollectObservations(VectorSensor sensor) {
            base.CollectObservations(sensor);
            sensor.AddObservation(IsSymptomatic);
            sensor.AddObservation(_isDrivingAmbulance);
        }
        public override void OnActionReceived(float[] vectorAction) {
            base.OnActionReceived(vectorAction);

            if (vectorAction[2] > 0) {
                if (!_isDrivingAmbulance) {
                    EnterAmbulance();
                }
            }
            if (vectorAction[2] <= 0) {
                if (_isDrivingAmbulance) {
                    GetOutOfAmbulance();
                }
            }

            if (_isDrivingAmbulance) {
                RigidBody.velocity = new Vector3(vectorAction[0], 0, vectorAction[1]) * GameManager.SpeedOfAmbulance;
            } else {
                RigidBody.velocity = new Vector3(vectorAction[0], 0, vectorAction[1]) * GameManager.SpeedOfMedic;
            }
            
            if (HealthStatus == HealthStatus.Susceptible) {
                Count++;
            }

            Total++;
        }

        public override void Heuristic(float[] actionsOut) {
            base.Heuristic(actionsOut);
            actionsOut[0] = 0;
            actionsOut[1] = 0;
            actionsOut[2] = 0.5f;
            if (Input.GetKey(KeyCode.W)) {
                actionsOut[1] = 1f;
            } else if (Input.GetKey(KeyCode.S)) {
                actionsOut[1] = -1f;
            } else if (Input.GetKey(KeyCode.A)) {
                actionsOut[0] = -1f;
            } else if (Input.GetKey(KeyCode.D)) {
                actionsOut[0] = 1f;
            } else if (Input.GetKey(KeyCode.E)) {
                actionsOut[2] = -0.5f;
            }
        }
        void Update() {
            _timer += Time.deltaTime;
            if (_isDrivingAmbulance) {
                _driveCount++;
            } else {
                _walkCount++;
            }
        }
    }
}
