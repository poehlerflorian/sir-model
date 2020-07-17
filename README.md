# SIR Model
This project was a university group project carried out by [David Ferneding](https://github.com/fernedingd), [Nils Neukirch](https://github.com/NeukirchN), and [Florian Pöhler](https://github.com/poehlerflorian). The original goal was to create a [SIR model](https://en.wikipedia.org/wiki/Compartmental_models_in_epidemiology#The_SIR_model) as a multi-agent system. We actually extended this model to a [SEIR model](https://en.wikipedia.org/wiki/Compartmental_models_in_epidemiology#The_SEIR_model), and then even further to allow individuals to have five different states: _Susceptible, Exposed, Infectious, Symptomatic, and Recovered_. Then we use the [ML-Agents](https://github.com/Unity-Technologies/ml-agents) framework to train our citizens to try to not get infected.

The code is currently still a bit messy, sorry. It allows us to play around with it and see interesting results though.
## Getting Started
This project can't be run (yet), so to see it in action or develop it further, there are some prerequisites.
### Prerequisites
* [Unity](https://unity.com/)
* [ML-Agents](https://github.com/Unity-Technologies/ml-agents)
  * The Unity package is included as a submodule at ref/ml-agents
  * The python package is only needed if you want to train your own model

### Running a simulation
To run a simulation you need to open either the [LearningScene](Assets/Scenes/LearningScene.unity) or the [GraphScene](Assets/Scenes/GraphScene.unity). Using the GameManager you can adjust some variables like the number of citizens, the infection chance etc. You should then add a model to the [CitizenAgent Prefab](Assets/Prefabs/CitizenAgent.prefab) by changing the model located under _Behavior Parameters_.

## Ideas/Outlook/Todo
The following list represents a collection of ideas that would be interesting to see implemented:

* [Reproduction number R](https://en.wikipedia.org/wiki/Basic_reproduction_number) - show R in realtime while running a simulation
* Infection without contact - allow citizens to infect others while being near them without touching
* Hotspots
  * Events
  * Supermarkets - visiting once in a specified time period could be mandatory
* Age - give the citizens an age
* Medical preconditions - citizens could have some medical issues like diabetes, heart problems etc.
* Death - citizens have a chance of dieing, could be based on age and/or medical conditions
* Quarantine - allow citizens to quarantine themselves
* Standalone application - create a standalone application where the user can change some configurations and run a simulation without needing the unity editor. Development on this has begun, but is on hold and far from finished. See [Assets/Scenes/MainScene.unity](Assets/Scenes/MainScene.unity)
* Regions - add different regions to represent countries. Citizens have a small chance to travel to another region
* Improve the arena - currently the arena is just a square. This could be improved with buildings, streets etc. to better resemble a city

## Authors
* David Ferneding - [@fernedingd](https://github.com/fernedingd)
* Nils Neukirch - [@NeukirchN](https://github.com/NeukirchN)
* Florian Pöhler - [@poehlerflorian](https://github.com/poehlerflorian)

## License
The project is licensed under the MIT license - see the [LICENSE](LICENSE) for details.