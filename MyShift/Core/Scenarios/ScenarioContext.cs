using MyShift.Core.Scenarios.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Scenarios
{
    internal class ScenarioContext
    {
        public ScenarioType CurrentScenario { get; set; }//Тут тип сценария. В данный момент видимо только добавление задачи "AddTask"
        public string? CurrentStep { get; set; }//Текущий шаг
        public Dictionary<string, object> Data { get; set; }
        public ScenarioContext(ScenarioType scenario)
        {
            CurrentScenario = scenario;
            CurrentStep = null;
            Data = new Dictionary<string, object>();
        }
    }
}