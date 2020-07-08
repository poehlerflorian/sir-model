using System;
using System.Collections;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class CitizenAgent : Agent, ICitizen {

        public Material HealedMaterial;
        public Material InfectedMaterial;
        public Material InfectedNotContagiousMaterial;
        public Material InfectedSymptomaticMaterial;
        public Material RecoveredMaterial;
        public Material SusceptibleMaterial;

        protected bool IsContagious;
        protected MeshRenderer MeshRenderer;
        protected Rigidbody RigidBody;
        protected float Total;
        protected float Count;
        protected GameManager GameManager;

        public bool IsSymptomatic { get; private set; }
        public HealthStatus HealthStatus { get; private set; } = HealthStatus.Susceptible;
        public bool ReportStats { get; set; }

        public virtual void Infect(float chance) {
            if (HealthStatus != HealthStatus.Susceptible || Random.value > chance) {
                return;
            }

            AddReward(-1f);
            StartInfectionProcess();
        }

        public virtual void Kill() {
            float survivalLength = Count / Total;
            AddReward(2.5f * survivalLength * survivalLength);

            GameManager.RelativeSurvivalLength.Add(survivalLength);

            if (ReportStats) {
                StatsRecorder stats = Academy.Instance.StatsRecorder;
                float citizenCount = GameManager.NumberOfNormalCitizen + GameManager.NumberOfExtrovertedCitizen
                    + GameManager.NumberOfIntrovertedCitizen;
                stats.Add("Collisions/Collisions", GameManager.CollisionsTotal / citizenCount);
                stats.Add("Collisions/Symptomatic", GameManager.CollisionsSymptomatic / citizenCount);
                stats.Add("Collisions/Not Symptomatic",
                    (GameManager.CollisionsTotal - GameManager.CollisionsSymptomatic) / citizenCount);
                stats.Add("Citizen/Susceptible at end", GameManager.GetCount(HealthStatus.Susceptible));
                stats.Add("Citizen/Recovered at end", GameManager.GetCount(HealthStatus.Recovered));
                stats.Add("Citizen/Relative survival length", GameManager.RelativeSurvivalLength.Average());

                foreach (HealthStatus healthStatus in Enum.GetValues(typeof(HealthStatus))) {
                    stats.Add($"Collisions/{healthStatus}", GameManager.Collisions[healthStatus] / citizenCount);
                }
            }

            EndEpisode();
            Destroy(gameObject);
        }

        public void StopInfectionProcess() {
            StopAllCoroutines();
            MeshRenderer.material = HealedMaterial;
            HealthStatus = HealthStatus.Recovered;
            IsContagious = false;
            IsSymptomatic = false;
            gameObject.tag = GameManager.CITIZEN_TAG;
            gameObject.layer = 10;
        }

        // ReSharper disable once UnusedMember.Local
        private void Awake() {
            GameManager = FindObjectOfType<GameManager>();
            RigidBody = GetComponent<Rigidbody>();
            MeshRenderer = GetComponent<MeshRenderer>();

            MeshRenderer.material = SusceptibleMaterial;
            GameManager.OnRestart += (sender, e) => Kill();
            RigidBody.freezeRotation = true;
        }

        // ReSharper disable once UnusedMember.Local
        protected virtual void OnCollisionEnter(Collision collision) {
            ICitizen citizenAgent = collision.GetContact(0).otherCollider.GetComponent<ICitizen>();
            if (citizenAgent == null) {
                return;
            }

            GameManager.CollisionsTotal++;
            if (IsSymptomatic) {
                GameManager.CollisionsSymptomatic++;
            }

            GameManager.Collisions[HealthStatus]++;

            if (!IsContagious) {
                return;
            }

            citizenAgent.Infect(GameManager.ChanceOfInfection);
            AddReward(-0.1f);
        }


        protected void StartInfectionProcess() {
            HealthStatus = HealthStatus.Infected;
            MeshRenderer.material = InfectedNotContagiousMaterial;
            StartCoroutine(StartBecomingContagious());
            StartCoroutine(StartBecomingSymptomatic());
            StartCoroutine(StartRecovering());
        }

        protected IEnumerator StartBecomingContagious() {
            yield return new WaitForSeconds(GameManager.TimeUntilContagiousInSeconds);

            IsContagious = true;
            MeshRenderer.material = InfectedMaterial;
        }

        protected IEnumerator StartBecomingSymptomatic() {
            yield return new WaitForSeconds(GameManager.TimeUntilSymptomaticInSeconds);

            IsSymptomatic = true;
            MeshRenderer.material = InfectedSymptomaticMaterial;
            gameObject.tag = GameManager.CITIZEN_SYMPTOMATIC_TAG;
            gameObject.layer = 11;
        }

        protected IEnumerator StartRecovering() {
            yield return new WaitForSeconds(GameManager.InfectedDuration - GameManager.TimeUntilContagiousInSeconds);

            MeshRenderer.material = RecoveredMaterial;
            HealthStatus = HealthStatus.Recovered;
            IsContagious = false;
            IsSymptomatic = false;
            gameObject.tag = GameManager.CITIZEN_TAG;
            gameObject.layer = 10;
        }

        public override void CollectObservations(VectorSensor sensor) {
            sensor.AddObservation(IsSymptomatic);
        }

        public override void OnActionReceived(float[] vectorAction) {
            RigidBody.velocity = new Vector3(vectorAction[0], 0, vectorAction[1]) * GameManager.SpeedOfCitizen;

            if (HealthStatus == HealthStatus.Susceptible) {
                Count++;
            }

            Total++;
        }

        public override void Heuristic(float[] actionsOut) {
            actionsOut[0] = 0;
            actionsOut[1] = 0;
            if (Input.GetKey(KeyCode.W)) {
                actionsOut[1] = 1f;
            } else if (Input.GetKey(KeyCode.S)) {
                actionsOut[1] = -1f;
            } else if (Input.GetKey(KeyCode.A)) {
                actionsOut[0] = -1f;
            } else if (Input.GetKey(KeyCode.D)) {
                actionsOut[0] = 1f;
            }
        }
    }
}
