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

import argparse
import random
from collections import deque
from typing import Callable, Deque
import numpy as np
import gymnasium as gym

# ── Load .NET runtime ─────────────────────────────────────────────────────────
from pythonnet import load
load("coreclr")

import clr

DLL_PATH = r"..\Core\CoreEngine\bin\Debug\net8.0\CoreEngine"
clr.AddReference(DLL_PATH)

from CoreEngine.Game import GameManager, GameMode, Player, PlayerChoiceDelegate  # type: ignore
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
    for i, ch in enumerate(choices.Count):
        desc = getattr(ch, "Desc", str(ch))
        print(f"  [{i}] {desc}")

    while True:
        try:
            raw = input(f"Enter choice (0–{choices.Count-1}): ").strip()
            idx = int(raw)
            if 0 <= idx < len(choices.Count):
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
    cs_agents.Add(Player(None, 0))
    cs_agents.Add(Player(None, 1))
    cs_agents.Add(Player(None, 2))

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

# ── Agent factories ───────────────────────────────────────────────────────────

def human_agent_input(choices) -> int:
    """Prints the available choices and prompts the user."""
    print("\n── Your turn ──────────────────────────────────────────")
    items = list(choices)
    for i, ch in enumerate(items):
        desc = getattr(ch, "Desc", str(ch))
        print(f"  [{i}] {desc}")
    while True:
        try:
            raw = input(f"Enter choice (0–{len(items)-1}): ").strip()
            idx = int(raw)
            if 0 <= idx < len(items):
                return idx
            print(f"  ✗ Must be between 0 and {len(items)-1}.")
        except (ValueError, EOFError):
            print("  ✗ Enter an integer.")


def scripted_agent_factory(queue: Deque[int]) -> Callable:
    """Returns a delegate that pops the next index from the queue (loops on empty)."""
    def agent_input(choices) -> int:
        items = list(choices)
        if not queue:
            # Fallback: always pick index 0
            idx = 0
        else:
            idx = queue.popleft()
        idx = max(0, min(idx, len(items) - 1))  # clamp
        desc = getattr(items[idx], "Desc", str(items[idx])) if items else "?"
        print(f"  [Scripted] chose {idx}: {desc}")
        return idx
    return agent_input


def random_agent_factory(seed: int | None = None) -> Callable:
    """Returns a delegate that picks a random valid index."""
    rng = random.Random(seed)
    def agent_input(choices) -> int:
        items = list(choices)
        idx = rng.randrange(len(items)) if items else 0
        desc = getattr(items[idx], "Desc", str(items[idx])) if items else "?"
        print(f"  [Random] chose {idx}: {desc}")
        return idx
    return agent_input


def make_player(func: Callable, color=None) -> Player:
    delegate = PlayerChoiceDelegate(func)
    return Player(delegate)


# ── Scripted game sequences ───────────────────────────────────────────────────
# Each entry is a queue of choices for one player.
# Choice 0 is always a safe fallback (first available option).
# These sequences exercise the main code paths; update as the game evolves.

SCRIPTED_SEQUENCES: list[list[int]] = [
    # Player 0: mostly draw cards, occasionally claim, keep all tickets
    [
        # Setup: keep all 3 regular tickets (all choices = 0 = "Keep")
        0, 0, 0,
        # Turns: repeatedly draw cards (action index), then blind-draw twice
        *([0, 1, 1] * 30),   # pick "Draw Train Cards", then blind×2, repeat
    ],
    # Player 1: always claim routes when available, else draw cards
    [
        # Setup: keep at least 1 ticket
        0, 0, 1,  # keep 2, discard 1
        # Turns: try to claim (action 0 = BuildStation or Claim depending on order),
        # else draw; always pick first available route and first payment
        *([0, 0, 0] * 30),
    ],
]


# ── Entry point ───────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(description="Ticket to Ride Europe agent harness")
    parser.add_argument("--human",    type=int, default=1,    metavar="N",
                        help="Number of human (interactive) agents (default 1)")
    parser.add_argument("--scripted", action="store_true",
                        help="Run 2 scripted agents for deterministic smoke-testing")
    parser.add_argument("--random",   type=int, default=0,    metavar="N",
                        help="Number of random agents")
    parser.add_argument("--seed",     type=int, default=None, metavar="SEED",
                        help="RNG seed for deterministic game (passed to C# GameManager)")
    args = parser.parse_args()

    # Build player list
    agents: list = []

    if args.scripted:
        args.human = 0
        sequences = SCRIPTED_SEQUENCES
        for i, seq in enumerate(sequences):
            q: Deque[int] = deque(seq)
            agents.append(make_player(scripted_agent_factory(q)))
            print(f"  Added scripted agent {i} (queue length: {len(q)})")
    else:
        for _ in range(args.human):
            agents.append(make_player(human_agent_input))
        for i in range(args.random):
            agents.append(make_player(random_agent_factory(
                None if args.seed is None else args.seed + i)))

    total = len(agents)
    assert 2 <= total <= 5, f"Need 2–5 players, got {total}"

    # Convert to C# List[Player]
    cs_agents = List[Player]()
    for a in agents:
        cs_agents.Add(a)

    # Build GameManager — pass a seeded Random for reproducibility if requested
    if args.seed is not None:
        dot_net_rng = DotNetRandom(args.seed)
        gm = GameManager(cs_agents, GameMode.Live, dot_net_rng)
    else:
        gm = GameManager(cs_agents)

    print(f"\n🚂  Starting Ticket to Ride Europe with {total} player(s).\n")
    gm.Start()


if __name__ == "__main__":
    main()
