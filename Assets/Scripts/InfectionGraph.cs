using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
    public class InfectionGraph : MonoBehaviour {
        private List<GameObject> _bars;
        private GameManager _gameManager;
        private float _infectedBarSize;
        private float _recoveredBarSize;
        private float _susceptibleBarSize;
        private float _healedBarSize;
        private Transform _xAxis;
        private float _xAxisPosition;
        private float _xAxisPositionAddition;
        private Transform _yAxis;
        private float _yAxisPosition;
        private float _yAxisPositionAddition;

        public GameObject BarObject;
        public Material HealedMaterial;
        public Material InfectedBarMaterial;
        public Material RecoveredBarMaterial;
        public Material SusceptibleBarMaterial;

        public void Start() {
            _gameManager = FindObjectOfType<GameManager>();
            _gameManager.OnRefresh += (sender, e) => DrawGraph(_gameManager.GetCount(HealthStatus.Susceptible),
                _gameManager.GetCount(HealthStatus.Infected), _gameManager.GetCount(HealthStatus.Recovered), _gameManager.HealedCounter);

            _gameManager.OnRestart += (sender, e) => Reset();
            _xAxis = transform.GetChild(0);
            _yAxis = transform.GetChild(1);
            _bars = new List<GameObject>();
        }

        public void Reset() {
            _bars.ForEach(Destroy);
            _bars = new List<GameObject>();
            _xAxisPositionAddition = 0;
        }

        public void DrawGraph(int susceptibleCount, int infectedCount, int recoveredCount, int healedCount) {
            _xAxisPosition = _yAxis.position.x + 1 - (1 - _gameManager.IntervalToDrawGraphInSeconds) / 2;
            _xAxisPosition += _xAxisPositionAddition;

            _yAxisPosition = _xAxis.position.z + 0.5f;
            _yAxisPositionAddition = 0;

            if (healedCount != 0) {
                recoveredCount -= healedCount;
            }

            _susceptibleBarSize = 37.5f * susceptibleCount / (_gameManager.NumberOfNormalCitizen + _gameManager.NumberOfExtrovertedCitizen + _gameManager.NumberOfIntrovertedCitizen);
            _infectedBarSize = 37.5f * infectedCount / (_gameManager.NumberOfNormalCitizen + _gameManager.NumberOfExtrovertedCitizen + _gameManager.NumberOfIntrovertedCitizen);
            _recoveredBarSize = 37.5f * recoveredCount / (_gameManager.NumberOfNormalCitizen + _gameManager.NumberOfExtrovertedCitizen + _gameManager.NumberOfIntrovertedCitizen);
            _healedBarSize = 37.5f * healedCount / (_gameManager.NumberOfNormalCitizen + _gameManager.NumberOfExtrovertedCitizen + _gameManager.NumberOfIntrovertedCitizen);

            GenerateBar(_infectedBarSize, InfectedBarMaterial);
            GenerateBar(_healedBarSize, HealedMaterial);
            GenerateBar(_recoveredBarSize, RecoveredBarMaterial);
            GenerateBar(_susceptibleBarSize, SusceptibleBarMaterial);

            _xAxisPositionAddition += _gameManager.IntervalToDrawGraphInSeconds;
        }

        private void GenerateBar(float barSize, Material barMaterial) {
            BarObject.transform.localScale = new Vector3(_gameManager.IntervalToDrawGraphInSeconds, 1, barSize);
            BarObject.GetComponent<MeshRenderer>().material = barMaterial;
            GameObject generatedBar = Instantiate(BarObject,
                new Vector3(_xAxisPosition, 1, _yAxisPosition + _yAxisPositionAddition + barSize / 2),
                Quaternion.identity);
            generatedBar.transform.parent = transform;
            _yAxisPositionAddition += barSize;
            _bars.Add(generatedBar);
        }
    }
}
