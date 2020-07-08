using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
    public class UserController : MonoBehaviour {
        private GameManager _gameManager;
        private GameObject _infectionGraph;
        private GameObject _simulationInstance;
        private List<Slider> _slidersToDisable = new List<Slider>();
        private List<Button> _buttonsToDisable = new List<Button>();

        public GameManager NewGameManager;
        public InfectionGraph NewInfectionGraph;

        public Text NumberOfCitizenText;
        public Text SpeedOfCitizenText;
        public Text SpeedOfMedicText;
        public Text SpeedOfAmbulanceText;
        public Text InfectedCitizenAtStartText;
        public Text ChanceOfInfectionText;
        public Text MedicChanceOfInfectionText;
        public Text InfectionDurationMinSecondsText;
        public Text InfectionDurationMaxSecondsText;
        public Text TimeUntilContagiousText;
        public Text TimeUntilSymptomaticText;
        public Text UseMLAgentsText;
        public Text UseMedicText;
        public Text UseQuarantaineText;

        public Slider NumberOfCitizenSlider;
        public Slider SpeedOfCitizenSlider;
        public Slider SpeedOfMedicSlider;
        public Slider SpeedOfAmbulanceSlider;
        public Slider InfectedCitizenAtStartSlider;
        public Slider ChanceOfInfectionSlider;
        public Slider MedicChanceOfInfectionSlider;
        public Slider InfectionDurationMinSecondsSlider;
        public Slider InfectionDurationMaxSecondsSlider;
        public Slider TimeUntilContagiousSlider;
        public Slider TimeUntilSymptomaticSlider;

        public Button ToggleUseMLAgentsButton;
        public Button ToggleUseMedicButton;
        public Button ToggleUseQuarantaineButton;
        public Button StartButton;
        public Button StopButton;

        private void Awake() {
            _gameManager = FindObjectOfType<GameManager>();
            _infectionGraph = GameObject.FindGameObjectWithTag("infectionGraph");
            _simulationInstance = GameObject.FindGameObjectWithTag("simulationInstance");

            _slidersToDisable.Add(NumberOfCitizenSlider);
            _slidersToDisable.Add(SpeedOfCitizenSlider);
            _slidersToDisable.Add(SpeedOfMedicSlider);
            _slidersToDisable.Add(SpeedOfAmbulanceSlider);
            _slidersToDisable.Add(InfectedCitizenAtStartSlider);
            _slidersToDisable.Add(ChanceOfInfectionSlider);
            _slidersToDisable.Add(MedicChanceOfInfectionSlider);
            _slidersToDisable.Add(InfectionDurationMinSecondsSlider);
            _slidersToDisable.Add(InfectionDurationMaxSecondsSlider);
            _slidersToDisable.Add(TimeUntilContagiousSlider);
            _slidersToDisable.Add(TimeUntilSymptomaticSlider);

            _buttonsToDisable.Add(ToggleUseMLAgentsButton);
            _buttonsToDisable.Add(ToggleUseMedicButton);
            _buttonsToDisable.Add(ToggleUseQuarantaineButton);
            _buttonsToDisable.Add(StartButton);

            NumberOfCitizenSlider.value = _gameManager.NumberOfNormalCitizen;
            SpeedOfCitizenSlider.value = _gameManager.SpeedOfCitizen;
            SpeedOfMedicSlider.value = _gameManager.SpeedOfMedic;
            SpeedOfAmbulanceSlider.value = _gameManager.SpeedOfAmbulance;
            InfectedCitizenAtStartSlider.value = _gameManager.InfectedNormalCitizenAtStart;
            ChanceOfInfectionSlider.value = _gameManager.ChanceOfInfection;
            MedicChanceOfInfectionSlider.value = _gameManager.MedicChanceOfInfection;
            InfectionDurationMinSecondsSlider.value = _gameManager.InfectedDurationMinSeconds;
            InfectionDurationMaxSecondsSlider.value = _gameManager.InfectedDurationMaxSeconds;
            TimeUntilContagiousSlider.value = _gameManager.TimeUntilContagiousInSeconds;
            TimeUntilSymptomaticSlider.value = _gameManager.TimeUntilSymptomaticInSeconds;

            StopButton.interactable = false;

            UseMLAgentsText.text = _gameManager.UseMlAgents ? "Yes" : "No";
            UseMedicText.text = _gameManager.NumberOfMedics >= 1 ? "Yes" : "No";
            UseQuarantaineText.text = _gameManager.UseQuarantine ? "Yes" : "No";
            _gameManager.enabled = !_gameManager.UseUserControl;
        }

        private void Update() {
            NumberOfCitizenText.text = NumberOfCitizenSlider.value.ToString();
            SpeedOfCitizenText.text = SpeedOfCitizenSlider.value.ToString();
            SpeedOfMedicText.text = SpeedOfMedicSlider.value.ToString();
            SpeedOfAmbulanceText.text = SpeedOfAmbulanceSlider.value.ToString();
            InfectedCitizenAtStartText.text = InfectedCitizenAtStartSlider.value.ToString();
            ChanceOfInfectionText.text = ChanceOfInfectionSlider.value.ToString();
            MedicChanceOfInfectionText.text = MedicChanceOfInfectionSlider.value.ToString();
            InfectionDurationMinSecondsText.text = InfectionDurationMinSecondsSlider.value.ToString();
            InfectionDurationMaxSecondsText.text = InfectionDurationMaxSecondsSlider.value.ToString();
            TimeUntilContagiousText.text = TimeUntilContagiousSlider.value.ToString();
            TimeUntilSymptomaticText.text = TimeUntilSymptomaticSlider.value.ToString();
        }

        public void ToggleUseMlAgents() {
            _gameManager.UseMlAgents = !_gameManager.UseMlAgents;
            UseMLAgentsText.text = _gameManager.UseMlAgents ? "No" : "Yes";
        }

        public void ToggleUseMedic() {
            if (_gameManager.NumberOfMedics >= 1) {
                _gameManager.NumberOfMedics = 0;
                UseMedicText.text = "No";
            } else {
                _gameManager.NumberOfMedics = 1;
                UseMedicText.text = "Yes";
            }
        }

        public void ToggleUseQuarantaine() {
            _gameManager.UseQuarantine = !_gameManager.UseQuarantine;
            UseQuarantaineText.text = _gameManager.UseQuarantine ? "No" : "Yes";
        }

        public void StartSimulation() {
            StopButton.interactable = true;

            foreach (Slider slider in _slidersToDisable) {
                slider.interactable = false;
            }

            foreach (Button button in _buttonsToDisable) {
                button.interactable = false;
            }

            if (_gameManager.UseUserControl) {
                _gameManager.NumberOfNormalCitizen = (int) NumberOfCitizenSlider.value;
                _gameManager.SpeedOfCitizen = (int) SpeedOfCitizenSlider.value;
                _gameManager.SpeedOfMedic = (int) SpeedOfMedicSlider.value;
                _gameManager.SpeedOfAmbulance = (int) SpeedOfAmbulanceSlider.value;
                _gameManager.InfectedNormalCitizenAtStart = (int) InfectedCitizenAtStartSlider.value;
                _gameManager.ChanceOfInfection = ChanceOfInfectionSlider.value;
                _gameManager.MedicChanceOfInfection = MedicChanceOfInfectionSlider.value;
                _gameManager.InfectedDurationMinSeconds = (int) InfectionDurationMinSecondsSlider.value;
                _gameManager.InfectedDurationMaxSeconds = (int) InfectionDurationMaxSecondsSlider.value;
                _gameManager.TimeUntilContagiousInSeconds = (int) TimeUntilContagiousSlider.value;
                _gameManager.TimeUntilSymptomaticInSeconds = (int) TimeUntilSymptomaticSlider.value;

                _gameManager.UseMlAgents = UseMLAgentsText.text == "Yes";

                _gameManager.NumberOfMedics = UseMedicText.text == "Yes" ? 1 : 0;

                _gameManager.UseQuarantine = UseQuarantaineText.text == "Yes";

                _gameManager.enabled = true;
            }
        }

        public void StopSimulation() {
            StopButton.interactable = false;

            foreach (Slider slider in _slidersToDisable) {
                slider.interactable = true;
            }

            foreach (Button button in _buttonsToDisable) {
                button.interactable = true;
            }

            if (_gameManager.UseUserControl) {
                Destroy(_gameManager.gameObject);
                Destroy(_infectionGraph);

                _gameManager = Instantiate(NewGameManager);
                _gameManager.transform.SetParent(_simulationInstance.transform);
                _gameManager.enabled = false;
                _infectionGraph = Instantiate(NewInfectionGraph.gameObject);
            }
        }
    }
}
