"""
Ticket to Ride Europe – Python agent harness
=============================================

Uses pythonnet (coreclr) to load the compiled CoreEngine DLL.
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

# ── RL Imports ────────────────────────────────────────────────────────────────
from sb3_contrib import MaskablePPO
from sb3_contrib.common.wrappers import ActionMasker

MAX_POSSIBLE_ACTIONS = 100

# ── Helper for SB3 Action Masking ─────────────────────────────────────────────
def mask_fn(env: gym.Env) -> np.ndarray:
    return env.unwrapped._get_info()["action_mask"]

class TicketToRideEnv(gym.Env):
    def __init__(self, training_mode: bool = False, rl_agent_idx: int = 0):
        super().__init__()
        self.training_mode = training_mode
        self.rl_agent_idx = rl_agent_idx

        self.action_space = gym.spaces.Discrete(MAX_POSSIBLE_ACTIONS)

        self.observation_space = gym.spaces.Box(low=0, high=1, shape=(390,), dtype=np.float32)

        self.gm = None

    def reset(self, seed=None, options=None):
        current_seed = seed if seed is not None else random.randint(0, 10000)

        cs_agents = List[Player]()
        cs_agents.Add(Player(None, PlayerColor(0)))
        cs_agents.Add(Player(None, PlayerColor(1)))
        cs_agents.Add(Player(None, PlayerColor(2)))

        self.gm = GameManager(cs_agents, GameMode.Live, DotNetRandom(current_seed))

        self.gm.BeginGame()

        # If agent is trained and is not the first player, first do other players steps
        if self.training_mode:
            self._fast_forward_to_rl_agent()

        return self._get_obs(), self._get_info()

    def step(self, action: int):
        # Record the score and state BEFORE the move
        rl_player = self.gm.Players[self.rl_agent_idx]
        score_before = self.gm.CalculateCurrentScore(rl_player)

        # Advance the C# simulation by one step
        self.gm.Step(int(action))

        # If agent is trained, first do other players steps
        if self.training_mode and not self.gm.IsGameOver:
            self._fast_forward_to_rl_agent()

        done = self.gm.IsGameOver

        # Record the state AFTER the move
        score_after = self.gm.CalculateCurrentScore(rl_player) if not done else rl_player.Score

        reward = 0.0
        # CURRENT GAINS:
        points_gained = score_after - score_before
        reward += points_gained * 0.1

        # END-GAME REWARDS (Sparse rewards):
        if done:
            # Determine the winner (the one with the highest score)
            best_score = max(p.Score for p in self.gm.Players)
            if rl_player.Score == best_score:
                reward += 10.0  # Large bonus for winning the game
            else:
                reward -= 5.0  # Penalty for losing

        # Return the updated observation vector, reward, done flag, truncated flag, and info dict
        return self._get_obs(), reward, done, False, self._get_info()

    def _fast_forward_to_rl_agent(self):
        while not self.gm.IsGameOver and self.gm.Players.IndexOf(self.gm.CurrentPlayer) != self.rl_agent_idx:
            choices = self.gm.CurrentChoices
            if choices.Count > 0:
                opp_action = random.randrange(choices.Count)
                self.gm.Step(opp_action)
            else:
                break # fallback

    def _get_obs(self):
        rl_player = self.gm.Players[self.rl_agent_idx]
        state_array = self.gm.GetGameStateVector(rl_player)
        return np.array(list(state_array), dtype=np.float32)

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
    """Randomly choose one of an available action."""
    choices = gm.CurrentChoices
    idx = random.randrange(choices.Count)
    print(f"\n[Random agent {gm.CurrentPlayer.Color}] choose: {choices[idx].Desc}")
    return idx

def rl_agent(obs, info, gm: GameManager, model) -> int:
    """Neural network agent PPO."""
    # Mask of actions which cannot be executed
    action, _states = model.predict(obs, action_masks=info["action_mask"], deterministic=True)

    idx = int(action)
    print(f"\n[DRL Agent {gm.CurrentPlayer.Color}] choose: {gm.CurrentChoices[idx].Desc}")
    return idx


# ── Training Loop ─────────────────────────────────────────────────────────────
def train_loop():
    print("\n🚂 Starting Training Mode (Fast-Forwarding opponents)...")

    env = TicketToRideEnv(training_mode=True, rl_agent_idx=0)

    env = ActionMasker(env, mask_fn)

    model = MaskablePPO("MlpPolicy", env, verbose=1, learning_rate=0.0003, device="cuda")

    model.learn(total_timesteps=10000)

    model.save("ppo_ticket_to_ride")
    print("✅ Model trained and saved as 'ppo_ticket_to_ride.zip'")


# ── Evaluation / Play Loop ────────────────────────────────────────────────────
def game_loop():
    print("\n🚂 Starting Ticket to Ride Europe (Evaluation Mode)...\n")

    try:
        model = MaskablePPO.load("ppo_ticket_to_ride")
    except FileNotFoundError:
        print("No model file. Run train_loop() first!")
        return

    env = TicketToRideEnv(training_mode=False, rl_agent_idx=0)

    obs, info = env.reset(seed=42)  # Stały seed, żebyś mógł analizować grę
    done = False

    gm = env.gm

    print(f"\n🚂 Game started! RL Agent is {gm.Players[0].Color}. You are {gm.Players[1].Color}.\n")

    agent_behaviors = {
        0: lambda obs, info: rl_agent(obs, info, gm, model),
        1: lambda obs, info: human_agent(obs, info, gm),
        2: lambda obs, info: random_agent(obs, info, gm)
    }

    while not done:
        current_idx = info["current_player_idx"]

        action = agent_behaviors[current_idx](obs, info)

        obs, _, done, _, info = env.step(action)

    print("\n🏆 Game finished! 🏆")

    for p in gm.Players:
        print(f"  {p.Color}: {p.Score} pts")


if __name__ == "__main__":
    train_loop()
    # game_loop()
