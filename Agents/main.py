from pythonnet import load

# IMPORTANT for .NET Core / .NET 5+
load("coreclr")

import clr

# Add reference to DLL
clr.AddReference(r"..\Core\CoreEngine\bin\Debug\net8.0\CoreEngine")

from CoreEngine import Player, GameManager, PlayerChoiceDelegate
from System.Collections.Generic import List

HUMAN_AGENTS = 1
HEURISTIC_AGENTS = 0
DRL_AGENTS = 0


def GetAgentInstance(pythonDelegate):
    delegate = PlayerChoiceDelegate(pythonDelegate)
    player = Player(delegate)
    return player


def HumanAgentInputDelegate(someIEnumerable):
    print(someIEnumerable)
    return 0


if __name__ == '__main__':

    assert HUMAN_AGENTS >= 0 and HEURISTIC_AGENTS >= 0 and DRL_AGENTS >= 0, "Liczba graczy nie może być ujemna"
    assert HUMAN_AGENTS + HEURISTIC_AGENTS + DRL_AGENTS <= 5, "Liczba graczy musi być mniejsza niż 5"

    cs_agents = List[Player]()

    for _ in range(HUMAN_AGENTS):
        cs_agents.Add(GetAgentInstance(HumanAgentInputDelegate))

    for _ in range(HEURISTIC_AGENTS):
        pass

    for _ in range(DRL_AGENTS):
        pass

    print(cs_agents)
    gm = GameManager(cs_agents)
    gm.Start()
