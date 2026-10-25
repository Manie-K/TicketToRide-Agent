"""
Ticket to Ride Europe – Python agent harness
=============================================

Uses pythonnet (coreclr) to load the compiled CoreEngine DLL.

Agent types
-----------
HumanAgent        — prints choices and reads from stdin
ScriptedAgent     — plays a pre-queued list of int indices (deterministic testing)
RandomAgent       — picks a random valid index (quick smoke-test)

Quick start
-----------
  python main.py                  # 1 human agent (interactive)
  python main.py --scripted       # 2 scripted agents (deterministic smoke-test)
  python main.py --random 3       # 3 random agents
  python main.py --seed 42        # fix RNG seed for reproducible games
"""

from __future__ import annotations

import random
import numpy as np
import gymnasium as gym

# ── Load .NET runtime ─────────────────────────────────────────────────────────
from pythonnet import load
load("coreclr")

import clr

DLL_PATH = r"..\Core\CoreEngine\bin\Debug\net8.0\CoreEngine"
clr.AddReference(DLL_PATH)

from CoreEngine.Game import GameManager, GameMode, Player, PlayerChoiceDelegate  # type: ignore
from CoreEngine import PlayerColor
from System.Collections.Generic import List  # type: ignore
from System import Random as DotNetRandom  # type: ignore

# Environment

MAX_POSSIBLE_ACTIONS = 100

class TicketToRideEnv(gym.Env):
    def __init__(self, gm: GameManager):
        super().__init__()
        self.gm = gm

        self.action_space = gym.spaces.Discrete(MAX_POSSIBLE_ACTIONS)

        self.observation_space = gym.spaces.Box(low=0, high=1, shape=(256,), dtype=np.float32)

    def reset(self, seed=None, options=None):
        self.gm.BeginGame()
        return self._get_obs(), self._get_info()

    def step(self, action: int):
        self.gm.Step(action)

        #TODO: calculate partial rewards
        done = self.gm.IsGameOver
        reward = 1.0 if done else 0.0

        return self._get_obs(), reward, done, False, self._get_info()

    def _get_obs(self):
        return np.zeros(self.observation_space.shape, dtype=np.float32)

    def _get_info(self):
        num_choices = self.gm.CurrentChoices.Count
        mask = np.zeros(MAX_POSSIBLE_ACTIONS, dtype=np.int8)
        if num_choices > 0:
            mask[:num_choices] = 1

        current_idx = self.gm.Players.IndexOf(self.gm.CurrentPlayer) if not self.gm.IsGameOver else -1

        return {
            "action_mask": mask,
            "current_player_idx": current_idx
        }

# Agents
def human_agent(obs, info, gm: GameManager) -> int:
    """Prints the available choices and prompts the user."""
    print(f"\n[Your turn: {gm.CurrentPlayer.Color}] :")

    choices = gm.CurrentChoices
    for i in range(choices.Count):
        print(f"  [{i}] {choices[i].Desc}")

    while True:
        try:
            idx = int(input(f"Enter choice (0–{choices.Count-1}): ").strip())
            if 0 <= idx < choices.Count:
                return idx
            print(f"  ✗ Must be between 0 and {choices.Count-1}.")
        except (ValueError, EOFError):
            print("  ✗ Enter an integer.")

def random_agent(obs, info, gm: GameManager) -> int:
    """Randomly choose one of a available action."""
    choices = gm.CurrentChoices
    idx = random.randrange(choices.Count)
    print(f"\n[Random agent {gm.CurrentPlayer.Color}] choose: {choices[idx].Desc}")
    return idx

#TODO: implement RL agent (Stable-Baselines3 for example)
def rl_agent(obs, info, gm: GameManager, model) -> int:
    """Neural network agent PPO."""
    # Mask of actions which cannot be execute
    action, _states = model.predict(obs, action_masks=info["action_mask"], deterministic=True)

    idx = int(action)
    print(f"\n[DRL Agent {gm.CurrentPlayer.Color}] choose: {gm.CurrentChoices[idx].Desc}")
    return idx

#Game loop
def game_loop():
    cs_agents = List[Player]()
    cs_agents.Add(Player(None, PlayerColor(0)))
    cs_agents.Add(Player(None, PlayerColor(1)))
    cs_agents.Add(Player(None, PlayerColor(2)))

    gm = GameManager(cs_agents, GameMode.Live, DotNetRandom(42))

    env = TicketToRideEnv(gm)

    agent_behaviors = {
        0: lambda obs, info: human_agent(obs, info, gm),
        1: lambda obs, info: random_agent(obs, info, gm),
        # 1: lambda obs, info: rl_agent(obs, info, gm, ppo_model),
        2: lambda obs, info: random_agent(obs, info, gm)
    }

    # game start
    obs, info = env.reset()
    done = False

    print(f"\n🚂  Starting Ticket to Ride Europe with {len(cs_agents)} player(s).\n")

    while not done:
        current_idx = info["current_player_idx"]

        action = agent_behaviors[current_idx](obs, info)

        obs, reward, done, truncated, info = env.step(action)

    print("\n🏆 Game finished! 🏆")


if __name__ == "__main__":
    game_loop()
